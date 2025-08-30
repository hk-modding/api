using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using Modding.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Modding
{
    internal class ModLoaderObject: MonoBehaviour
    {
        private void Update()
        {
            ModLoader.HandleHotReloadEvents();
        }
    }
    
    /// <summary>
    ///     Handles loading of mods.
    /// </summary>
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    [PublicAPI]
    internal static class ModLoader
    {
        [Flags]
        public enum ModLoadState
        {
            NotStarted = 0,
            Started = 1,
            Preloaded = 2,
            Loaded = 4,
        }

        public static ModLoadState LoadState = ModLoadState.NotStarted;


        public static Dictionary<Type, ModInstance> ModInstanceTypeMap { get; private set; } = new();
        public static Dictionary<string, ModInstance> ModInstanceNameMap { get; private set; } = new();
        public static HashSet<ModInstance> ModInstances { get; private set; } = new();

        private static Dictionary<string, List<ModInstance>> ModInstancesByAssembly = new();
        
        private static DefaultAssemblyResolver hotReloadAssemblyResolver;
        
        private static string ManagedPath = SystemInfo.operatingSystemFamily switch
        {
            OperatingSystemFamily.Windows => Path.Combine(Application.dataPath, "Managed"),
            OperatingSystemFamily.MacOSX => Path.Combine(Application.dataPath, "Resources", "Data", "Managed"),
            OperatingSystemFamily.Linux => Path.Combine(Application.dataPath, "Managed"),
            OperatingSystemFamily.Other => null,
            _ => throw new ArgumentOutOfRangeException(),
        };

        /// <summary>
        /// Try to add a ModInstance to the internal dictionaries.
        /// </summary>
        /// <param name="ty">The type of the mod.</param>
        /// <param name="mod">The ModInstance.</param>
        /// <returns>True if the ModInstance was successfully added; false otherwise.</returns>
        private static bool TryAddModInstance(Type ty, ModInstance mod)
        {
            if (ModInstanceNameMap.ContainsKey(mod.Name))
            {
                Logger.APILogger.LogWarn($"Found multiple mods with name {mod.Name}.");
                mod.Error = ModErrorState.Duplicate;
                ModInstanceNameMap[mod.Name].Error = ModErrorState.Duplicate;
                ModInstanceTypeMap[ty] = mod;
                ModInstances.Add(mod);
                return false;
            }

            ModInstanceTypeMap[ty] = mod;
            ModInstanceNameMap[mod.Name] = mod;
            ModInstances.Add(mod);
            return true;
        }

        private static ModVersionDraw modVersionDraw;

        /// <summary>
        /// Starts the main loading of all mods.
        /// This loads assemblies, constructs and initializes mods, and creates the mod list menu.<br/>
        /// This method should only be called once in the lifetime of the game.
        /// </summary>
        /// <param name="coroutineHolder"></param>
        /// <returns></returns>
        public static IEnumerator LoadModsInit(GameObject coroutineHolder)
        {
            try
            {
                Logger.InitializeFileStream();
            }
            catch (Exception e)
            {
                // We can still log to the console at least, if that's enabled.
                Logger.APILogger.LogError(e);
            }

            Logger.APILogger.Log("Starting mod loading");

            if (ManagedPath is null)
            {
                LoadState |= ModLoadState.Loaded;
                UObject.Destroy(coroutineHolder);
                yield break;
            }

            ModHooks.LoadGlobalSettings();
            Logger.ClearOldModlogs();

            Logger.APILogger.LogDebug($"Loading assemblies and constructing mods");

            string mods = Path.Combine(ManagedPath, "Mods");
            
            string[] modDirectories = Directory.GetDirectories(mods)
                                               .Except([Path.Combine(mods, "Disabled")])
                                               .ToArray();
            string[] modFiles = modDirectories
                                .SelectMany(d => Directory.GetFiles(d, "*.dll"))
                                .ToArray();
            hotReloadAssemblyResolver = new DefaultAssemblyResolver();
            foreach (string modDirectory in modDirectories)
            {
                hotReloadAssemblyResolver.AddSearchDirectory(modDirectory);
            }
            
            List<(string, Assembly)> modAssemblies = GetModAssemblies(modFiles);

            foreach ((string path, Assembly asm) in modAssemblies)
            {
                Logger.APILogger.LogDebug($"Loading mods in assembly `{asm.FullName}`");
                // ReSharper disable once InconsistentlySynchronizedField the watcher hasn't started yet
                ModInstancesByAssembly[path] = InstantiateMods(asm);
            }

            var scenes = new List<string>();
            for (int i = 0; i < USceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                scenes.Add(Path.GetFileNameWithoutExtension(scenePath));
            }

            ModInstance[] orderedMods = ModInstanceTypeMap.Values
                .OrderBy(x => x.Mod?.LoadPriority() ?? 0)
                .ToArray();

            // dict<scene name, list<(mod, list<objectNames>)>
            var toPreload = new Dictionary<string, List<(ModInstance, List<string> objectNames)>>();
            // dict<mod, dict<scene, dict<objName, object>>>
            var preloadedObjects = new Dictionary<ModInstance, Dictionary<string, Dictionary<string, GameObject>>>();
            // scene -> respective hooks
            var sceneHooks = new Dictionary<string, List<Func<IEnumerator>>>();
            
            Logger.APILogger.Log("Creating mod preloads");
            
            // Setup dict of scene preloads
            GetPreloads(orderedMods, scenes, toPreload, sceneHooks);
            
            if (toPreload.Count > 0 || sceneHooks.Count > 0)
            {
                Preloader pld = coroutineHolder.GetOrAddComponent<Preloader>();
                yield return pld.Preload(toPreload, preloadedObjects, sceneHooks);
                UObject.Destroy(pld);
            }

            foreach (ModInstance mod in orderedMods)
            {
                if (mod.Error is not null)
                {
                    Logger.APILogger.LogWarn($"Not loading mod {mod.Name}: error state {mod.Error}");
                    continue;
                }

                try
                {
                    preloadedObjects.TryGetValue(mod, out Dictionary<string, Dictionary<string, GameObject>> preloads);
                    LoadMod(mod, false, preloads);
                    if (!ModHooks.GlobalSettings.ModEnabledSettings.TryGetValue(mod.Name, out var enabled))
                    {
                        enabled = true;
                    }

                    if (mod.Error == null && mod.Mod is ITogglableMod && !enabled)
                    {
                        UnloadMod(mod, false);
                    }
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("Error: " + ex);
                }
            }

            // Create version text
            GameObject version = new GameObject();
            modVersionDraw = version.AddComponent<ModVersionDraw>();
            UObject.DontDestroyOnLoad(version);

            UpdateModText();
            
            if (ModHooks.GlobalSettings.EnableHotReload)
            {
                StartFileSystemWatcher(mods);
            }

            // Adding version nums to the modlog by default to make debugging significantly easier
            Logger.APILogger.Log("Finished loading mods:\n" + modVersionDraw.drawString);

            ModHooks.OnFinishedLoadingMods();
            LoadState |= ModLoadState.Loaded;

            new ModListMenu().InitMenuCreation();
        }

        private static List<ModInstance> InstantiateMods(Assembly asm)
        {
            bool foundMod = false;

            List<ModInstance> modInstances = [];

            try
            {
                foreach (Type ty in asm.GetTypesSafely())
                {
                    if (!ty.IsClass || ty.IsAbstract || !ty.IsSubclassOf(typeof(Mod)))
                        continue;

                    foundMod = true;

                    Logger.APILogger.LogDebug($"Constructing mod `{ty.FullName}`");

                    try
                    {
                        if (ty.GetConstructor(Type.EmptyTypes)?.Invoke([]) is Mod mod)
                        {
                            var instance = new ModInstance {
                                Mod = mod,
                                Enabled = false,
                                Error = null,
                                Name = mod.GetName(),
                            };
                            modInstances.Add(instance);
                            TryAddModInstance(ty, instance);
                        }
                    } catch (Exception e)
                    {
                        Logger.APILogger.LogError(e);
                        var instance = new ModInstance {
                            Mod = null,
                            Enabled = false,
                            Error = ModErrorState.Construct,
                            Name = ty.Name,
                        };
                        modInstances.Add(instance);
                        TryAddModInstance(ty, instance);
                    }
                }
            } catch (Exception e)
            {
                Logger.APILogger.LogError(e);
            }

            if (!foundMod)
            {
                AssemblyName info = asm.GetName();
                Logger.APILogger.Log($"Assembly {info.Name} ({info.Version}) loaded with 0 mods");
            }

            return modInstances;
        }

        private static List<(string, Assembly)> GetModAssemblies(string[] files)
        {
            Logger.APILogger.LogDebug(string.Join(",\n", files));

            Assembly Resolve(object sender, ResolveEventArgs args)
            {
                var asm_name = new AssemblyName(args.Name);

                if (files.FirstOrDefault(x => x.EndsWith($"{asm_name.Name}.dll")) is {} path)
                    return Assembly.LoadFrom(path);

                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            List<(string, Assembly)> asms = new(files.Length);

            // Load all the assemblies first to avoid dependency issues
            // Dependencies are lazy-loaded, so we won't have attempted loads
            // until the mod initialization.
            foreach (string path in files)
            {
                Logger.APILogger.LogDebug($"Loading assembly `{path}`");

                try {
                    asms.Add((path, Assembly.LoadFrom(path)));
                }
                catch (FileLoadException e)
                {
                    Logger.APILogger.LogError($"Unable to load assembly - {e}");
                }
                catch (BadImageFormatException e)
                {
                    Logger.APILogger.LogError($"Assembly is bad image. {e}");
                }
                catch (PathTooLongException)
                {
                    Logger.APILogger.LogError("Unable to load, path to dll is too long!");
                }
            }

            return asms;
        }

        #region Hot Reloading
        
        private static ConcurrentQueue<FileSystemEventArgs> hotReloadEvents = new();

        internal static void HandleHotReloadEvents()
        {
            while (hotReloadEvents.TryDequeue(out var e))
            {
                Logger.APILogger.LogDebug($"Got file system event {e.ChangeType} at {e.FullPath}");
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        Logger.APILogger.Log($"Loading mods from {e.FullPath}");
                        HotLoadModAssembly(e.FullPath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        Logger.APILogger.Log($"Unloading mods from {e.FullPath}");
                        HotUnloadModAssembly(e.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        Logger.APILogger.Log($"Reloading mods from {e.FullPath}");
                        HotUnloadModAssembly(e.FullPath);
                        HotLoadModAssembly(e.FullPath);
                        break;
                    case WatcherChangeTypes.Renamed:
                        var renamedEvent = (RenamedEventArgs) e;
                        Logger.APILogger.Log($"Reloading mods from {e.FullPath}");
                        HotUnloadModAssembly(renamedEvent.OldFullPath);
                        HotLoadModAssembly(renamedEvent.FullPath);
                        break;
                    
                    case WatcherChangeTypes.All:
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void StartFileSystemWatcher(string mods)
        {
            var fileSystemWatcher = new FileSystemWatcher(mods)
            {
                IncludeSubdirectories = true,
            };
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            fileSystemWatcher.Filter = "*.dll";
            fileSystemWatcher.Created += (_, e) => hotReloadEvents.Enqueue(e);
            fileSystemWatcher.Deleted += (_, e) => hotReloadEvents.Enqueue(e);
            fileSystemWatcher.Changed += (_, e) => hotReloadEvents.Enqueue(e);
            fileSystemWatcher.Renamed += (_, e) => hotReloadEvents.Enqueue(e);
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private static void HotUnloadModAssembly(string assemblyPath)
        {
            try {
                if (ModInstancesByAssembly.TryGetValue(assemblyPath, out List<ModInstance> assemblyMods))
                {
                    foreach (ModInstance mod in assemblyMods)
                    {
                        if (mod.Mod is not ITogglableMod)
                        {
                            Logger.APILogger.LogError("Hot reloaded mod contains non-togglable mods");
                            return;
                        }
                        UnloadMod(mod);
                        ModInstances.Remove(mod);
                        ModInstanceNameMap.Remove(mod.Name);
                        ModInstanceTypeMap.Remove(mod.Mod.GetType());
                    }
                    ModInstancesByAssembly.Remove(assemblyPath);
                } else {
                    Logger.APILogger.LogWarn($"No mods loaded for changed assembly '{assemblyPath}'");
                }
            } catch (Exception e)
            {
                Logger.APILogger.LogError($"Error trying to unload mods in {assemblyPath}:\n{e}");
            }
        }

        private static void HotLoadModAssembly(string assemblyPath)
        {
            if (ModInstancesByAssembly.TryGetValue(assemblyPath, out _))
            {
                Logger.APILogger.LogError($"Did not hot reload mods because they old ones were still loaded {assemblyPath}");
                return;
            }
            
            try {
                // Renames sometimes emit [Changed, Created, Deleted]
                if (!File.Exists(assemblyPath))
                {
                    return;
                }
                var assembly = LoadHotReloadDll(assemblyPath);
                List<ModInstance> newAssemblyMods = InstantiateMods(assembly);
                ModInstancesByAssembly[assemblyPath] = newAssemblyMods;
                foreach (var mod in newAssemblyMods)
                {
                    LoadMod(mod, preloadedObjects: null); // TODO preloadedObjects
                }
            } catch (Exception e)
            {
                Logger.APILogger.LogError($"Error trying to load mods in {assemblyPath}:\n{e}");
            }
        }
            

        private static List<(string, Assembly)> GetHotReloadModAssemblies(string hotReloadMods)
        {
            string[] files = Directory.GetDirectories(hotReloadMods)
                                      .Except([Path.Combine(hotReloadMods, "Disabled")])
                                      .SelectMany(d => Directory.GetFiles(d, "*.dll"))
                                      .ToArray();
            Logger.APILogger.LogDebug("Hot reload: " + string.Join(",\n", files));

            List<(string, Assembly)> asms = new();
            foreach (string path in files)
            {
                asms.Add((path, Assembly.LoadFrom(path)));
            }

            return asms;
        }

        public class MemorySymbolWriterProvider : ISymbolWriterProvider
        {
            public MemoryStream stream = new();
            public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
            {
                return module.SymbolReader.GetWriterProvider().GetSymbolWriter(module, stream);
            }
            public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream) => throw new NotImplementedException();
        }

        private static Assembly LoadHotReloadDll(string path)
        {
            using var dll = AssemblyDefinition.ReadAssembly(path, new ReaderParameters {
                AssemblyResolver = hotReloadAssemblyResolver,
                ReadSymbols = true,
            });
            dll.Name.Name = $"{dll.Name.Name}-{DateTime.Now.Ticks}";
            
            using var ms = new MemoryStream();
            var symbolWriter = new MemorySymbolWriterProvider();
            dll.Write(ms, new WriterParameters {
                SymbolWriterProvider = symbolWriter,
            });
            
            byte[] symbols = symbolWriter.stream.ToArray();
            return Assembly.Load(ms.ToArray(), symbols);
        }
        
        #endregion

        private static void GetPreloads
        (
            ModInstance[] orderedMods,
            List<string> scenes,
            Dictionary<string, List<(ModInstance, List<string> objectNames)>> toPreload,
            Dictionary<string, List<Func<IEnumerator>>> sceneHooks
        )
        {
            foreach (var mod in orderedMods)
            {
                if (mod.Error != null)
                {
                    continue;
                }

                Logger.APILogger.LogDebug($"Checking preloads for mod \"{mod.Mod.GetName()}\"");

                List<(string, string)> preloadNames = null;
                try
                {
                    preloadNames = mod.Mod.GetPreloadNames();
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError($"Error getting preload names for mod {mod.Name}\n" + ex);
                }

                try
                {
                    foreach (var (scene, hook) in mod.Mod.PreloadSceneHooks())
                    {
                        if (!sceneHooks.TryGetValue(scene, out var hooks))
                            sceneHooks[scene] = hooks = new List<Func<IEnumerator>>();

                        hooks.Add(hook);
                    }
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError($"Error getting preload hooks for mod {mod.Name}\n" + ex);
                }
                
                if (preloadNames == null)
                    continue;

                // dict<scene, list<objects>>
                Dictionary<string, List<string>> modPreloads = new();

                foreach ((string scene, string obj) in preloadNames)
                {
                    if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(obj))
                    {
                        Logger.APILogger.LogWarn($"Mod `{mod.Mod.GetName()}` passed null values to preload");
                        continue;
                    }

                    if (!scenes.Contains(scene))
                    {
                        Logger.APILogger.LogWarn(
                            $"Mod `{mod.Mod.GetName()}` attempted preload from non-existent scene `{scene}`"
                        );
                        continue;
                    }

                    if (!modPreloads.TryGetValue(scene, out List<string> objects))
                    {
                        objects = new List<string>();
                        modPreloads[scene] = objects;
                    }

                    Logger.APILogger.LogFine($"Found object `{scene}.{obj}`");

                    objects.Add(obj);
                }

                foreach ((string scene, List<string> objects) in modPreloads)
                {
                    if (!toPreload.TryGetValue(scene, out List<(ModInstance, List<string>)> scenePreloads))
                    {
                        scenePreloads = new List<(ModInstance, List<string>)>();
                        toPreload[scene] = scenePreloads;
                    }

                    Logger.APILogger.LogFine($"`{mod.Name}` preloads {objects.Count} objects in the `{scene}` scene");

                    scenePreloads.Add((mod, objects));
                    toPreload[scene] = scenePreloads;
                }
            }
        }

        private static void UpdateModText()
        {
            StringBuilder builder = new StringBuilder();
            
            builder.AppendLine("Modding API: " + ModHooks.ModVersion);
            
            foreach (ModInstance mod in ModInstances)
            {
                if (mod.Error is not ModErrorState err)
                {
                    if (mod.Enabled) builder.AppendLine($"{mod.Name} : {mod.Mod.GetVersionSafe(returnOnError: "ERROR")}");
                }
                else
                {
                    switch (err)
                    {
                        case ModErrorState.Construct:
                            builder.AppendLine($"{mod.Name} : Failed to call constructor! Check ModLog.txt");
                            break;
                        case ModErrorState.Duplicate:
                            builder.AppendLine($"{mod.Name} : Failed to load! Duplicate mod detected");
                            break;
                        case ModErrorState.Initialize:
                            builder.AppendLine($"{mod.Name} : Failed to initialize! Check ModLog.txt");
                            break;
                        case ModErrorState.Unload:
                            builder.AppendLine($"{mod.Name} : Failed to unload! Check ModLog.txt");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            modVersionDraw.drawString = builder.ToString();
        }

        internal static void LoadMod
        (
            ModInstance mod,
            bool updateModText = true,
            Dictionary<string, Dictionary<string, GameObject>> preloadedObjects = null
        )
        {
            try
            {
                if (mod is { Enabled: false, Error: null })
                {
                    mod.Enabled = true;
                    mod.Mod.Initialize(preloadedObjects);
                }
            }
            catch (Exception e)
            {
                mod.Error = ModErrorState.Initialize;
                Logger.APILogger.LogError($"Failed to load Mod `{mod.Mod.GetName()}`\n{e}");
            }

            if (updateModText) UpdateModText();
        }

        internal static void UnloadMod(ModInstance mod, bool updateModText = true)
        {
            try
            {
                if (mod is { Mod: ITogglableMod itmod, Enabled: true, Error: null })
                {
                    mod.Enabled = false;
                    itmod.Unload();
                }
            }
            catch (Exception ex)
            {
                mod.Error = ModErrorState.Unload;
                Logger.APILogger.LogError($"Failed to unload Mod `{mod.Name}`\n{ex}");
            }

            if (updateModText) UpdateModText();
        }

        // Essentially the state of a loaded **mod**. The assembly has nothing to do directly with mods.
        public class ModInstance
        {
            // The constructed instance of the mod. If Error is `Construct` this will be null.
            // Generally if Error is anything this value should not be referred to.
            public IMod Mod;

            public string Name;

            public ModErrorState? Error;

            // If the mod is "Enabled" (in the context of ITogglableMod)
            public bool Enabled;
        }

        public enum ModErrorState
        {
            Construct,
            Duplicate,
            Initialize,
            Unload
        }
    }
}