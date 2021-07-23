using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Modding
{
    /// <summary>
    ///     Handles loading of mods.
    /// </summary>
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    [PublicAPI]
    internal static class ModLoader
    {
        /// <summary>
        ///     Checks if the mod loads are done.
        /// </summary>
        public static bool Loaded;

        /// <summary>
        ///     Checks if the mod preloads are done
        /// </summary>
        public static bool Preloaded;

        public static Dictionary<Type, ModInstance> ModInstanceTypeMap { get; private set; } = new();
        public static Dictionary<string, ModInstance> ModInstanceNameMap { get; private set; } = new();
        public static HashSet<ModInstance> ModInstances { get; private set; } = new();
        
        private static void AddModInstance(Type ty, ModInstance mod)
        {
            ModInstanceTypeMap[ty] = mod;
            ModInstanceNameMap[mod.Name] = mod;
            ModInstances.Add(mod);
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
            if (Loaded || Preloaded)
            {
                UObject.Destroy(coroutineHolder);
                yield break;
            }
            Logger.APILogger.Log("Starting mod loading");
            
            string managed_path = SystemInfo.operatingSystemFamily switch
            {
                OperatingSystemFamily.Windows => Path.Combine(Application.dataPath, "Managed"),
                OperatingSystemFamily.MacOSX => Path.Combine(Application.dataPath, "Resources", "Data", "Managed"),
                OperatingSystemFamily.Linux => Path.Combine(Application.dataPath, "Managed"),
                
                OperatingSystemFamily.Other => null,
                
                _ => throw new ArgumentOutOfRangeException()
            };

            if (managed_path is null)
            {
                Loaded = true;
                
                UObject.Destroy(coroutineHolder);

                yield break;
            }
            
            ModHooks.LoadGlobalSettings();
            
            Logger.APILogger.LogDebug($"Loading assemblies and constructing mods");

            string mods = Path.Combine(managed_path, "Mods");

            string[] files = Directory.GetDirectories(mods)
                                      .SelectMany(d => Directory.GetFiles(d, "*.dll"))
                                      .ToArray();
            
            Logger.APILogger.LogDebug(string.Join(",\n", files));
            
            Assembly Resolve(object sender, ResolveEventArgs args)
            {
                var asm_name = new AssemblyName(args.Name);
                
                if (files.FirstOrDefault(x => x.EndsWith($"{asm_name.Name}.dll")) is string path)
                    return Assembly.LoadFrom(path);

                return null;
            }
            
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
            
            foreach (string assemblyPath in files)
            {
                Logger.APILogger.LogDebug($"Loading assembly `{assemblyPath}`");
                try
                {
                    foreach (Type ty in Assembly.LoadFrom(assemblyPath).GetTypes())
                    {
                        if (!ty.IsClass || ty.IsAbstract || !ty.IsSubclassOf(typeof(Mod))) 
                            continue;    
                        
                        Logger.APILogger.LogDebug($"Constructing mod `{ty.FullName}`");
                        
                        try
                        {
                            if (ty.GetConstructor(new Type[0])?.Invoke(new object[0]) is Mod mod)
                            {
                                AddModInstance(
                                    ty,
                                    new ModInstance
                                    {
                                        Mod = mod,
                                        Enabled = false,
                                        Error = null,
                                        Name = mod.GetName()
                                    }
                                );
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.APILogger.LogError(e);
                            
                            AddModInstance(
                                ty,
                                new ModInstance
                                {
                                    Mod = null,
                                    Enabled = false,
                                    Error = ModErrorState.Construct,
                                    Name = ty.Name
                                }
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.APILogger.LogError(e);
                }
            }

            var scenes = new List<string>();
            for (int i = 0; i < USceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                scenes.Add(Path.GetFileNameWithoutExtension(scenePath));
            }

            ModInstance[] orderedMods = ModInstanceTypeMap.Values
                .OrderBy(x => x.Mod.LoadPriority())
                .ToArray();

            // dict<scene name, list<(mod, list<objectNames>)>
            var toPreload = new Dictionary<string, List<(ModInstance, List<string> objectNames)>>();
            // dict<mod, dict<scene, dict<objName, object>>>
            var preloadedObjects = new Dictionary<ModInstance, Dictionary<string, Dictionary<string, GameObject>>>();

            Logger.APILogger.Log("Creating mod preloads");
            // Setup dict of scene preloads
            GetPreloads(orderedMods, scenes, toPreload);
            if (toPreload.Count > 0)
            {
                yield return PreloadScenes(coroutineHolder, toPreload, preloadedObjects);
            }

            foreach (ModInstance mod in orderedMods)
            {
                try
                {
                    preloadedObjects.TryGetValue(mod, out Dictionary<string, Dictionary<string, GameObject>> preloads);
                    LoadMod(mod, false, preloads);
                    if (!ModHooks.GlobalSettings.ModEnabledSettings.TryGetValue(mod.Name, out var enabled)) {
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
            
            Logger.APILogger.LogDebug("Updated mod text.");

            Loaded = true;

            new ModListMenu().InitMenuCreation();

            UObject.Destroy(coroutineHolder.gameObject);
        }

        private static void GetPreloads(
            ModInstance[] orderedMods,
            List<string> scenes,
            Dictionary<string, List<(ModInstance, List<string> objectNames)>> toPreload
        )
        {
            foreach (var mod in orderedMods)
            {
                if (mod.Error != null)
                {
                    continue;
                }
                Logger.APILogger.LogDebug($"Checking preloads for mod \"{mod.Mod.GetName()}\"");

                List<(string, string)> preloadNames = mod.Mod.GetPreloadNames();
                if (preloadNames == null)
                {
                    continue;
                }

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

                    scenePreloads.Add((mod, objects));
                    toPreload[scene] = scenePreloads;
                }
            }
        }

        private static IEnumerator PreloadScenes(
            GameObject coroutineHolder,
            Dictionary<string, List<(ModInstance, List<string>)>> toPreload,
            Dictionary<ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects
        )
        {
            // Mute all audio
            AudioListener.pause = true;

            // Create a blanker so the preloading is invisible
            GameObject blanker = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            UObject.DontDestroyOnLoad(blanker);

            var nb = coroutineHolder.GetComponent<NonBouncer>();

            CanvasUtil.CreateImagePanel(
                    blanker,
                    CanvasUtil.NullSprite(new byte[] { 0x00, 0x00, 0x00, 0xFF }),
                    new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one)
                )
                .GetComponent<Image>()
                .preserveAspect = false;

            // Create loading bar background
            CanvasUtil.CreateImagePanel(
                    blanker,
                    CanvasUtil.NullSprite(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
                    new CanvasUtil.RectData
                    (
                        new Vector2(1000, 100),
                        Vector2.zero,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f)
                    )
                )
                .GetComponent<Image>()
                .preserveAspect = false;

            // Create actual loading bar
            GameObject loadingBar = CanvasUtil.CreateImagePanel(
                blanker,
                CanvasUtil.NullSprite(new byte[] { 0x99, 0x99, 0x99, 0xFF }),
                new CanvasUtil.RectData(
                    new Vector2(0, 75),
                    Vector2.zero,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f)
                )
            );

            loadingBar.GetComponent<Image>().preserveAspect = false;
            RectTransform loadingBarRect = loadingBar.GetComponent<RectTransform>();

            // Preload all needed objects
            int progress = 0;

            void updateLoadingBarProgress()
            {
                loadingBarRect.sizeDelta = new Vector2(
                    progress / (float)toPreload.Count * 975,
                    loadingBarRect.sizeDelta.y
                );
            }

            IEnumerator PreloadScene(string s)
            {
                Logger.APILogger.LogFine($"Loading scene \"{s}\"");

                updateLoadingBarProgress();
                yield return USceneManager.LoadSceneAsync(s, LoadSceneMode.Additive);
                updateLoadingBarProgress();

                Scene scene = USceneManager.GetSceneByName(s);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (var go in rootObjects)
                {
                    go.SetActive(false);
                }

                // Fetch object names to preload
                List<(ModInstance, List<string>)> sceneObjects = toPreload[s];

                foreach ((ModInstance mod, List<string> objNames) in sceneObjects)
                {
                    Logger.APILogger.LogFine($"Fetching objects for mod \"{mod.Mod.GetName()}\"");

                    foreach (string objName in objNames)
                    {
                        Logger.APILogger.LogFine($"Fetching object \"{objName}\"");

                        // Split object name into root and child names based on '/'
                        string rootName;
                        string childName = null;

                        int slashIndex = objName.IndexOf('/');
                        if (slashIndex == -1)
                        {
                            rootName = objName;
                        }
                        else if (slashIndex == 0 || slashIndex == objName.Length - 1)
                        {
                            Logger.APILogger.LogWarn(
                                $"Invalid preload object name given by mod `{mod.Mod.GetName()}`: \"{objName}\""
                            );
                            continue;
                        }
                        else
                        {
                            rootName = objName.Substring(0, slashIndex);
                            childName = objName.Substring(slashIndex + 1);
                        }

                        // Get root object
                        GameObject obj = rootObjects.FirstOrDefault(o => o.name == rootName);
                        if (obj == null)
                        {
                            Logger.APILogger.LogWarn(
                                $"Could not find object \"{objName}\" in scene \"{s}\","
                                + $" requested by mod `{mod.Mod.GetName()}`"
                            );
                            continue;
                        }

                        // Get child object
                        if (childName != null)
                        {
                            Transform t = obj.transform.Find(childName);
                            if (t == null)
                            {
                                Logger.APILogger.LogWarn(
                                    $"Could not find object \"{objName}\" in scene \"{s}\","
                                    + $" requested by mod `{mod.Mod.GetName()}`"
                                );
                                continue;
                            }

                            obj = t.gameObject;
                        }

                        // Create all sub-dictionaries if necessary (Yes, it's terrible)
                        if (!preloadedObjects.TryGetValue
                        (
                            mod,
                            out Dictionary<string, Dictionary<string, GameObject>> modPreloadedObjects
                        ))
                        {
                            modPreloadedObjects = new Dictionary<string, Dictionary<string, GameObject>>();
                            preloadedObjects[mod] = modPreloadedObjects;
                        }

                        if (!modPreloadedObjects.TryGetValue
                        (
                            s,
                            out Dictionary<string, GameObject> modScenePreloadedObjects
                        ))
                        {
                            modScenePreloadedObjects = new Dictionary<string, GameObject>();
                            modPreloadedObjects[s] = modScenePreloadedObjects;
                        }

                        // Create inactive duplicate of requested object
                        obj = UObject.Instantiate(obj);
                        UObject.DontDestroyOnLoad(obj);
                        obj.SetActive(false);

                        // Set object to be passed to mod
                        modScenePreloadedObjects[objName] = obj;
                    }
                }

                // Update loading progress
                progress++;

                updateLoadingBarProgress();
                yield return USceneManager.UnloadSceneAsync(scene);
                updateLoadingBarProgress();
            }

            List<IEnumerator> batch = new();
            int maxKeys = toPreload.Keys.Count;

            foreach (string sceneName in toPreload.Keys)
            {
                int batchCount = Math.Min(ModHooks.GlobalSettings.PreloadBatchSize, maxKeys);

                batch.Add(PreloadScene(sceneName));

                if (batch.Count < batchCount)
                    continue;

                Coroutine[] coros = batch.Select(nb.StartCoroutine).ToArray();

                foreach (var coro in coros)
                    yield return coro;

                batch.Clear();

                maxKeys -= batchCount;
            }

            // Reload the main menu to fix the music/shaders
            Logger.APILogger.LogDebug("Preload done, returning to main menu");

            Preloaded = true;

            yield return USceneManager.LoadSceneAsync("Quit_To_Menu");

            while (USceneManager.GetActiveScene().name != Constants.MENU_SCENE)
            {
                yield return new WaitForEndOfFrame();
            }

            // Remove the black screen
            UObject.Destroy(blanker);

            // Restore the audio
            AudioListener.pause = false;
        }

        private static void UpdateModText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Modding API: " + ModHooks.ModVersion);
            foreach (ModInstance mod in ModInstances)
            {
                if (mod.Error is not ModErrorState err)
                {
                    if (mod.Enabled) builder.AppendLine($"{mod.Name} : {mod.Mod.GetVersion()}");
                }
                else
                {
                    switch (err)
                    {
                        case ModErrorState.Construct:
                            builder.AppendLine($"{mod.Name} : Failed to call constructor! Check ModLog.txt");
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
                if (mod is ModInstance { Enabled: false, Error: null })
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
                if (mod is ModInstance { Mod: ITogglableMod itmod, Enabled: true, Error: null })
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
            Initialize,
            Unload
        }
    }
}