using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
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

        /// <summary>
        ///     List of loaded mods.
        /// </summary>
        public static List<IMod> LoadedMods = new List<IMod>();

        private static readonly List<string> Errors = new List<string>();

        private static ModVersionDraw _draw;

        private static readonly Dictionary<string, string> ModVersionsCache = new Dictionary<string, string>();

        // Event subscription cache
        private static readonly Dictionary<string, EventInfo> ModHooksEvents =
            typeof(ModHooks).GetEvents().ToDictionary(e => e.Name);

        private static readonly List<FieldInfo> EventSubscribers = new List<FieldInfo>();

        // Hook name, method hooked, ITogglableMod used by hook
        private static readonly List<(EventInfo, MethodInfo, Type)> EventSubscriptions = new List<(EventInfo, MethodInfo, Type)>();

        /// <summary>
        ///     Loads the mod by searching for assemblies in hollow_knight_Data\Managed\Mods\
        /// </summary>
        public static IEnumerator LoadMods(GameObject coroutineHolder)
        {
            if (Preloaded || Loaded)
            {
                Object.Destroy(coroutineHolder);
                yield break;
            }

            Logger.APILogger.Log("Trying to load mods");
            string path = string.Empty;
            if (SystemInfo.operatingSystem.Contains("Windows"))
            {
                path = Application.dataPath + "\\Managed\\Mods";
            }
            else if (SystemInfo.operatingSystem.Contains("Mac"))
            {
                path = Application.dataPath + "/Resources/Data/Managed/Mods/";
            }
            else if (SystemInfo.operatingSystem.Contains("Linux"))
            {
                path = Application.dataPath + "/Managed/Mods";
            }

            if (string.IsNullOrEmpty(path))
            {
                Logger.LogWarn($"Operating system of {SystemInfo.operatingSystem} is not known.  Unable to load mods.");

                Loaded = true;
                Object.Destroy(coroutineHolder);
                yield break;
            }

            Logger.APILogger.Log("Attempting to determine type of mod assemblies (library/mod/addon)");
            Dictionary<int, string> modTypeNames = typeof(AssemblyTypeAttribute)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(f => (int)f.GetValue(null), f => f.Name);

            Dictionary<int, List<string>> modTypes = new Dictionary<int, List<string>>();
            foreach (string modPath in Directory.GetFiles(path, "*.dll"))
            {
                using AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(modPath);
                CustomAttribute attr = asmDef.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.FullName == typeof(AssemblyTypeAttribute).FullName);

                bool hasAttr = attr != null;
                int type = attr == null
                    ? AssemblyContainsMod(asmDef)
                        ? AssemblyTypeAttribute.MOD
                        : AssemblyTypeAttribute.LIBRARY
                    : (int)attr.ConstructorArguments[0].Value;

                if (!modTypeNames.TryGetValue(type, out string typeName))
                {
                    Logger.APILogger.LogWarn($"Invalid type on assembly '{Path.GetFileNameWithoutExtension(modPath)}'! Defaulting to MOD.");
                    type = AssemblyTypeAttribute.MOD;
                    hasAttr = false;
                }

                if (!modTypes.TryGetValue(type, out List<string> typePaths))
                {
                    typePaths = new List<string>();
                    modTypes[type] = typePaths;
                }

                // Place explicitly defined mods in the front of the queue, since we're more sure of those ones
                if (hasAttr)
                {
                    typePaths.Insert(0, modPath);
                }
                else
                {
                    typePaths.Add(modPath);
                }

                Logger.APILogger.Log($"{Path.GetFileNameWithoutExtension(modPath)} - {modTypeNames[type]}");
            }

            foreach (string modPath in modTypes.OrderBy(p => p.Key).SelectMany(p => p.Value))
            {
                Logger.APILogger.LogDebug("Loading assembly: " + modPath);
                try
                {
                    foreach (Type type in Assembly.LoadFile(modPath).GetTypes())
                    {
#pragma warning disable 618 // Backwards compatibility
                        if (IsSubclassOfRawGeneric(typeof(Mod<>), type))
#pragma warning restore 618
                        {
                            Logger.APILogger.LogDebug("Trying to instantiate mod<T>: " + type);

                            if (!(Activator.CreateInstance(type) is IMod mod))
                            {
                                continue;
                            }

                            LoadedMods.Add((Mod) mod);
                        }
                        else if (!type.IsGenericType && type.IsClass && type.IsSubclassOf(typeof(Mod)))
                        {
                            Logger.APILogger.LogDebug("Trying to instantiate mod: " + type);
                            if (!(type.GetConstructor(new Type[0])?.Invoke(new object[0]) is Mod mod))
                            {
                                continue;
                            }

                            LoadedMods.Add(mod);
                        }

                        // Search for method annotations
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                        {
                            SubscribeEventAttribute[] attributes;

                            try
                            {
                                attributes = (SubscribeEventAttribute[]) method.GetCustomAttributes(typeof(SubscribeEventAttribute), false);
                            }
                            catch (FileNotFoundException)
                            {
                                // Tried to load an assembly which doesn't exist.
                                Logger.APILogger.LogWarn($"Skipping method {method.Name} of type {type} for attribute events as it tried to load a non-existent assembly.");
                                
                                continue;
                            }
                            
                            foreach (SubscribeEventAttribute attr in attributes)
                            {
                                if (attr.ModType != null && !attr.ModType.IsSubclassOf(typeof(Mod)))
                                {
                                    Logger.APILogger.LogWarn($"Mod type '{attr.ModType.FullName}' on '{type.FullName}.{method.Name}' is not a Mod.");
                                    continue;
                                }

                                if (string.IsNullOrEmpty(attr.HookName))
                                {
                                    Logger.APILogger.LogWarn($"Null hook specified on method '{type.FullName}.{method.Name}'.");
                                    continue;
                                }

                                if (method.ContainsGenericParameters)
                                {
                                    Logger.APILogger.LogWarn($"Cannot subscribe method '{type.FullName}.{method.Name}', it contains generic parameters.");
                                    continue;
                                }

                                if (!ModHooksEvents.TryGetValue(attr.HookName, out EventInfo e))
                                {
                                    Logger.APILogger.LogWarn($"Cannot subscribe method '{type.FullName}.{method.Name}' to nonexistent event '{attr.HookName}'.");
                                    continue;
                                }

                                MethodInfo invoke = e.EventHandlerType.GetMethod("Invoke");

                                if (invoke == null)
                                {
                                    // This should never happen
                                    Logger.APILogger.LogWarn($"Event '{attr.HookName}' has no public method 'Invoke'.");
                                    continue;
                                }

                                if (invoke.ReturnType != method.ReturnType)
                                {
                                    Logger.APILogger.LogWarn($"Cannot subscribe method '{type.FullName}.{method.Name}' to event '{attr.HookName}', return types do not match.");
                                    continue;
                                }

                                ParameterInfo[] invokeParams = invoke.GetParameters();
                                ParameterInfo[] subscriberParams = method.GetParameters();

                                if (invokeParams.Length != subscriberParams.Length
                                    || invokeParams.Where((param, index) => param.ParameterType != subscriberParams[index].ParameterType).Any())
                                {
                                    Logger.APILogger.LogWarn($"Cannot subscribe method '{type.FullName}.{method.Name}' to event '{attr.HookName}', parameters do not match.");
                                    continue;
                                }

                                EventSubscriptions.Add((e, method, attr.ModType));
                            }
                        }

                        // Search for field annotations
                        foreach (FieldInfo field in type.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                        {
                            if (!field.GetCustomAttributes(typeof(EventSubscriberAttribute), false).Any())
                            {
                                continue;
                            }

                            if (!field.IsStatic && !type.IsSubclassOf(typeof(Mod)))
                            {
                                Logger.APILogger.LogWarn($"'{type.FullName}.{field.Name}' cannot be an event subscriber, it is an instance method on a non-Mod.");
                                continue;
                            }

                            EventSubscribers.Add(field);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.APILogger.LogError("Error: " + ex);
                    Errors.Add(modPath + ": FAILED TO LOAD! Check ModLog.txt.");
                }
            }

            List<string> scenes = new List<string>();
            for (int i = 0; i < USceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                scenes.Add(Path.GetFileNameWithoutExtension(scenePath));
            }

            IMod[] orderedMods = LoadedMods.OrderBy(x => x.LoadPriority()).ToArray();

            // dict<scene name, list<(mod, list<objectNames>)>
            Dictionary<string, List<(IMod, List<string>)>> toPreload =
                new Dictionary<string, List<(IMod, List<string>)>>();

            // dict<mod, dict<scene, dict<objName, object>>>
            Dictionary<IMod, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects =
                new Dictionary<IMod, Dictionary<string, Dictionary<string, GameObject>>>();

            Logger.APILogger.Log("Preloading");

            // Setup dict of scene preloads
            foreach (IMod mod in orderedMods)
            {
                Logger.APILogger.Log($"Checking preloads for mod \"{mod.GetName()}\"");

                List<(string, string)> preloadNames = mod.GetPreloadNames();
                if (preloadNames == null)
                {
                    continue;
                }

                // dict<scene, list<objects>>
                Dictionary<string, List<string>> modPreloads = new Dictionary<string, List<string>>();

                foreach (var (scene, obj) in preloadNames)
                {
                    if (string.IsNullOrEmpty(scene) || string.IsNullOrEmpty(obj))
                    {
                        Logger.APILogger.LogWarn($"Mod \"{mod.GetName()}\" passed null values to preload");
                        continue;
                    }

                    if (!scenes.Contains(scene))
                    {
                        Logger.APILogger.LogWarn($"Mod \"{mod.GetName()}\" attempted preload from non-existent scene \"{scene}\"");
                        continue;
                    }

                    if (!modPreloads.TryGetValue(scene, out List<string> objects))
                    {
                        objects = new List<string>();
                        modPreloads[scene] = objects;
                    }

                    Logger.APILogger.Log($"Found object \"{scene}.{obj}\"");

                    objects.Add(obj);
                }

                foreach (KeyValuePair<string, List<string>> pair in modPreloads)
                {
                    if (!toPreload.TryGetValue(pair.Key, out List<(IMod, List<string>)> scenePreloads))
                    {
                        scenePreloads = new List<(IMod, List<string>)>();
                        toPreload[pair.Key] = scenePreloads;
                    }

                    scenePreloads.Add((mod, pair.Value));
                    toPreload[pair.Key] = scenePreloads;
                }
            }

            if (toPreload.Count > 0)
            {
                yield return PreloadScenes(coroutineHolder, toPreload, preloadedObjects);
            }
            
            ModHooks.Instance.LoadGlobalSettings();

            foreach (IMod mod in orderedMods)
            {
                try
                {
                    preloadedObjects.TryGetValue(mod, out Dictionary<string, Dictionary<string, GameObject>> preloads);
                    LoadMod(mod, false, false, preloads);
                }
                catch (Exception ex)
                {
                    Errors.Add(string.Concat(mod.GetName(), ": FAILED TO LOAD! Check ModLog.txt."));
                    Logger.APILogger.LogError("Error: " + ex);
                }
            }

            // Subscribe events without a parent Mod
            SubscribeEvents(null, true);

            // Clean out the ModEnabledSettings for any mods that don't exist.
            LoadedMods.RemoveAll(mod =>
                !ModHooks.Instance.GlobalSettings.ModEnabledSettings.ContainsKey(mod.GetName()));

            // Get previously disabled mods and disable them.
            foreach (KeyValuePair<string, bool> modPair in ModHooks.Instance.GlobalSettings.ModEnabledSettings.Where(
                x => !x.Value))
            {
                IMod mod = LoadedMods.FirstOrDefault(x => x.GetName() == modPair.Key);
                if (!(mod is ITogglableMod togglable))
                {
                    continue;
                }

                togglable.Unload();
                Logger.LogDebug($"Mod {modPair.Key} was unloaded.");
            }

            // Create version text
            GameObject gameObject = new GameObject();
            _draw = gameObject.AddComponent<ModVersionDraw>();
            Object.DontDestroyOnLoad(gameObject);
            UpdateModText();

            Loaded = true;

            ModHooks.Instance.SaveGlobalSettings();

            Object.Destroy(coroutineHolder.gameObject);
        }

        private static IEnumerator PreloadScenes
        (
            GameObject coroutineHolder,
            Dictionary<string, List<(IMod, List<string>)>> toPreload,
            Dictionary<IMod, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects
        )
        {
            // Mute all audio
            AudioListener.pause = true;

            // Create a blanker so the preloading is invisible
            GameObject blanker = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            Object.DontDestroyOnLoad(blanker);
            
            var nb = coroutineHolder.GetComponent<NonBouncer>();

            CanvasUtil.CreateImagePanel
                      (
                          blanker,
                          CanvasUtil.NullSprite(new byte[] { 0x00, 0x00, 0x00, 0xFF }),
                          new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one)
                      )
                      .GetComponent<Image>()
                      .preserveAspect = false;

            // Create loading bar background
            CanvasUtil.CreateImagePanel
                      (
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
            GameObject loadingBar = CanvasUtil.CreateImagePanel
            (
                blanker,
                CanvasUtil.NullSprite(new byte[] { 0x99, 0x99, 0x99, 0xFF }),
                new CanvasUtil.RectData
                (
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

            IEnumerator PreloadScene(string s)
            {
                Logger.APILogger.Log($"Loading scene \"{s}\"");

                AsyncOperation load = USceneManager.LoadSceneAsync(s, LoadSceneMode.Additive);

                while (!load.isDone)
                {
                    loadingBarRect.sizeDelta =
                        new Vector2
                        (
                            progress / (float) toPreload.Count * 975,
                            loadingBarRect.sizeDelta.y
                        );
                    yield return new WaitForEndOfFrame();
                }

                Scene scene = USceneManager.GetSceneByName(s);
                GameObject[] rootObjects = scene.GetRootGameObjects();

                // Fetch object names to preload
                List<(IMod, List<string>)> sceneObjects = toPreload[s];

                foreach ((IMod mod, List<string> objNames) in sceneObjects)
                {
                    Logger.APILogger.Log($"Fetching objects for mod \"{mod.GetName()}\"");

                    foreach (string objName in objNames)
                    {
                        Logger.APILogger.Log($"Fetching object \"{objName}\"");

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
                            Logger.APILogger.LogWarn($"Invalid preload object name given by mod \"{mod.GetName()}\": \"{objName}\"");
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
                            Logger.APILogger.LogWarn($"Could not find object \"{objName}\" in scene \"{s}\", requested by mod \"{mod.GetName()}\"");
                            continue;
                        }

                        // Get child object
                        if (childName != null)
                        {
                            Transform t = obj.transform.Find(childName);
                            if (t == null)
                            {
                                Logger.APILogger.LogWarn($"Could not find object \"{objName}\" in scene \"{s}\", requested by mod \"{mod.GetName()}\"");
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
                        obj = Object.Instantiate(obj);
                        Object.DontDestroyOnLoad(obj);
                        obj.SetActive(false);

                        // Set object to be passed to mod
                        modScenePreloadedObjects[objName] = obj;
                    }
                }

                // Update loading progress
                progress++;

                yield return USceneManager.UnloadSceneAsync(scene);
            }

            List<IEnumerator> batch = new List<IEnumerator>();
            int maxKeys = toPreload.Keys.Count;

            foreach (string sceneName in toPreload.Keys)
            {
                int batchCount = Math.Min(ModHooks.Instance.GlobalSettings.PreloadBatchSize, maxKeys);

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
            Logger.APILogger.Log("Preload done, returning to main menu");

            Preloaded = true;
            
            yield return USceneManager.LoadSceneAsync("Quit_To_Menu");
            
            while (USceneManager.GetActiveScene().name != Constants.MENU_SCENE)
            {
                yield return new WaitForEndOfFrame();
            }

            // Remove the black screen
            Object.Destroy(blanker);

            // Restore the audio
            AudioListener.pause = false;
        }

        private static void UpdateModText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Modding API: " + ModHooks.Instance.ModVersion +
                               (ModHooks.Instance.IsCurrent ? "" : " - New Version Available!"));
            foreach (string error in Errors)
            {
                builder.AppendLine(error);
            }

            // 56 you made me do this, I hope you're happy
            Dictionary<string, List<IMod>> modsByNamespace = new Dictionary<string, List<IMod>>();

            foreach (IMod mod in LoadedMods)
            {
                try
                {
                    if (!ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()])
                    {
                        continue;
                    }

                    if (!ModVersionsCache.ContainsKey(mod.GetName()))
                    {
                        ModVersionsCache.Add(mod.GetName(),
                            mod.GetVersion() + (mod.IsCurrent() ? string.Empty : " - New Version Available!"));
                    }

                    string ns = mod.GetType().Namespace;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (!modsByNamespace.TryGetValue(ns, out List<IMod> nsMods))
                    {
                        nsMods = new List<IMod>();
                        modsByNamespace.Add(ns, nsMods);
                    }

                    nsMods.Add(mod);
                }
                catch (Exception e)
                {
                    Logger.APILogger.LogError($"Failed to obtain mod namespace:\n{e}");
                }
            }

            foreach (string ns in modsByNamespace.Keys)
            {
                try
                {
                    List<IMod> nsMods = modsByNamespace[ns];

                    if (nsMods == null || nsMods.Count == 0)
                    {
                        Logger.APILogger.LogWarn("Namespace mod list empty, ignoring");
                    }
                    else if (nsMods.Count == 1)
                    {
                        builder.AppendLine($"{nsMods[0].GetName()} : {ModVersionsCache[nsMods[0].GetName()]}");
                    }
                    else
                    {
                        builder.Append($"{ns} : ");
                        for (int i = 0; i < nsMods.Count; i++)
                        {
                            builder.Append(nsMods[i].GetName() + (i == nsMods.Count - 1 ? Environment.NewLine : ", "));
                            if ((i + 1) % 4 == 0 && i < nsMods.Count - 1)
                            {
                                builder.Append(Environment.NewLine + "\t");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.APILogger.LogError($"Failed to append mod name text:\n{e}");
                }
            }

            _draw.drawString = builder.ToString();
        }

        internal static void LoadMod(IMod mod, bool updateModText, bool changeSettings = true,
            Dictionary<string, Dictionary<string, GameObject>> preloadedObjects = null)
        {
            if (changeSettings || !ModHooks.Instance.GlobalSettings.ModEnabledSettings.ContainsKey(mod.GetName()))
            {
                ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] = true;
            }

            mod.Initialize(preloadedObjects);
            SubscribeEvents(mod, true);

            if (!ModHooks.Instance.LoadedModsWithVersions.ContainsKey(mod.GetType().Name))
            {
                ModHooks.Instance.LoadedModsWithVersions.Add(mod.GetType().Name, mod.GetVersion());
            }
            else
            {
                ModHooks.Instance.LoadedModsWithVersions[mod.GetType().Name] = mod.GetVersion();
            }

            if (ModHooks.Instance.LoadedMods.All(x => x != mod.GetType().Name))
            {
                ModHooks.Instance.LoadedMods.Add(mod.GetType().Name);
            }

            if (updateModText)
            {
                UpdateModText();
            }
        }

        internal static void UnloadMod(ITogglableMod mod)
        {
            try
            {
                ModHooks.Instance.GlobalSettings.ModEnabledSettings[mod.GetName()] = false;
                ModHooks.Instance.LoadedModsWithVersions.Remove(mod.GetType().Name);
                ModHooks.Instance.LoadedMods.Remove(mod.GetType().Name);

                SubscribeEvents(mod, false);
                mod.Unload();
            }
            catch (Exception ex)
            {
                Logger.APILogger.LogError($"Failed to unload Mod - {mod.GetName()} - {Environment.NewLine} - {ex} ");
            }

            UpdateModText();
        }

        /// <summary>
        ///     Checks to see if a class is a subclass of a generic class.
        /// </summary>
        /// <param name="generic">Generic to compare against.</param>
        /// <param name="toCheck">Type to check</param>
        /// <returns></returns>
        internal static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == type)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        private static bool AssemblyContainsMod(AssemblyDefinition asm)
        {
            foreach (TypeDefinition type in asm.Modules.SelectMany(m => m.Types))
            {
                try
                {
                    TypeDefinition parent = type.BaseType?.Resolve();
                    while (parent != null)
                    {
                        if (parent.FullName == typeof(Mod).FullName)
                        {
                            return true;
                        }

                        parent = parent.BaseType?.Resolve();
                    }
                }
                catch (AssemblyResolutionException) { }
            }

            return false;
        }

        // TODO: Cache delegates
        private static void SubscribeEvents([CanBeNull] IMod mod, bool subscribe)
        {
            if (!subscribe && !(mod is ITogglableMod))
            {
                Logger.APILogger.LogWarn($"Cannot unsubscribe events for non-togglable mod '{mod?.GetName()}'");
                return;
            }

            foreach ((EventInfo e, MethodInfo method, Type modType) in EventSubscriptions)
            {
                if (mod == null && modType != null)
                {
                    continue;
                }

                if (mod != null && modType != mod.GetType())
                {
                    continue;
                }

                Delegate del = null;
                if (method.IsStatic)
                {
                    try
                    {
                        del = Delegate.CreateDelegate(e.EventHandlerType, method);
                    }
                    catch (Exception exception)
                    {
                        Logger.APILogger.LogError($"Could not create delegate for event subscriber '{method.DeclaringType?.FullName}.{method.Name}':\n{exception}");
                        continue;
                    }
                }
                else if (mod != null && method.DeclaringType == mod.GetType())
                {
                    try
                    {
                        del = Delegate.CreateDelegate(e.EventHandlerType, mod, method);
                    }
                    catch (Exception exception)
                    {
                        Logger.APILogger.LogError($"Could not create delegate for event subscriber '{method.DeclaringType.FullName}.{method.Name}':\n{exception}");
                        continue;
                    }
                }
                else
                {
                    foreach (FieldInfo field in EventSubscribers)
                    {
                        if (field.FieldType != method.DeclaringType)
                        {
                            continue;
                        }

                        object target;
                        if (field.IsStatic)
                        {
                            target = field.GetValue(null);
                        }
                        else
                        {
                            IMod fieldMod = LoadedMods.FirstOrDefault(imod => imod.GetType() == field.DeclaringType);

                            if (fieldMod == null)
                            {
                                // Shouldn't ever happen
                                Logger.APILogger.LogWarn($"Cannot find Mod '{field.DeclaringType}', requested by '{method.DeclaringType.FullName}.{method.Name}'");
                                break;
                            }

                            target = field.GetValue(fieldMod);
                        }

                        if (target == null)
                        {
                            Logger.APILogger.LogWarn($"Event subscriber '{field.DeclaringType?.FullName}.{field.Name}' returned null.");
                            continue;
                        }

                        try
                        {
                            del = Delegate.CreateDelegate(e.EventHandlerType, target, method);
                        }
                        catch (Exception exception)
                        {
                            Logger.APILogger.LogError($"Could not create delegate for event subscriber '{method.DeclaringType.FullName}.{method.Name}':\n{exception}");
                            continue;
                        }

                        break;
                    }
                }

                if (del == null)
                {
                    Logger.APILogger.LogWarn($"Could not handle event subscription for '{method.DeclaringType?.FullName}.{method.Name}'.");
                    continue;
                }

                try
                {
                    if (subscribe)
                    {
                        e.AddEventHandler(ModHooks.Instance, del);
                    }
                    else
                    {
                        e.RemoveEventHandler(ModHooks.Instance, del);
                    }
                }
                catch (Exception exception)
                {
                    Logger.APILogger.LogError($"Could not {(subscribe ? "subscribe" : "unsubscribe")} event '{method.DeclaringType?.FullName}.{method.Name}':\n{exception}");
                }
            }
        }
    }
}
