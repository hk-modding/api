using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using Modding.Utils;
using Newtonsoft.Json;

namespace Modding;

internal class Preloader : MonoBehaviour
{
    private ProgressBar progressBar;

    private void Start() {
        progressBar = gameObject.AddComponent<ProgressBar>();
    }

    public IEnumerator Preload
    (
        Dictionary<string, List<(ModLoader.ModInstance, List<string>)>> toPreload,
        Dictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<string, List<Func<IEnumerator>>> sceneHooks
    )
    {
        var stopwatch = Stopwatch.StartNew();
        MuteAllAudio();

        bool usesSceneHooks = sceneHooks.Sum(kvp => kvp.Value.Count) > 0;

        Logger.APILogger.Log($"Preloading using mode {ModHooks.GlobalSettings.PreloadMode}");
        switch (ModHooks.GlobalSettings.PreloadMode) {
            case PreloadMode.FullScene:
                yield return DoPreloadScenes(toPreload, preloadedObjects, sceneHooks);
                break;
            case PreloadMode.RepackScene:
                yield return DoPreloadRepackedScenes(toPreload, preloadedObjects, sceneHooks);
                break;
            case PreloadMode.RepackAssets:
                if (usesSceneHooks) {
                    Logger.APILogger.LogWarn($"Some mods ({string.Join(", ", sceneHooks.Keys)}) use scene hooks, falling back to \"{nameof(PreloadMode.RepackScene)}\" preload mode");
                    yield return DoPreloadRepackedScenes(toPreload, preloadedObjects, sceneHooks);
                    break;
                }
                
                yield return DoPreloadAssetbundle(toPreload, preloadedObjects);
                break;
            default:
                Logger.APILogger.LogError($"Unknown preload mode {ModHooks.GlobalSettings.PreloadMode}. Expected one of: full-scene, repack-scene, repack-assets");
                break;
        }
        
        yield return CleanUpPreloading();

        UnmuteAllAudio();
        Logger.APILogger.LogError($"Finished preloading in {stopwatch.ElapsedMilliseconds/1000:F2}s");
    }

    /// <summary>
    ///     Mutes all audio from AudioListeners.
    /// </summary>
    private static void MuteAllAudio() => AudioListener.pause = true;
    
    private IEnumerator DoPreloadAssetbundle
    (
        Dictionary<string, List<(ModLoader.ModInstance Mod, List<string> Preloads)>> toPreload,
        IDictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects
    ) {
        const string PreloadBundleName = "modding_api_asset_bundle";
        
        string preloadJson = JsonConvert.SerializeObject(toPreload.ToDictionary(
                                                             k => k.Key,
                                                             v => v.Value.SelectMany(x => x.Preloads).Distinct() ));
        byte[] bundleData = null;
        try {
            (bundleData, RepackStats repackStats) = UnitySceneRepacker.Repack(PreloadBundleName, Application.dataPath, preloadJson, UnitySceneRepacker.Mode.AssetBundle);
            Logger.APILogger.Log($"Repacked {toPreload.Count} preload scenes from {repackStats.objectsBefore} to {repackStats.objectsAfter} objects ({bundleData.Length / 1024f / 1024f:F2}MB)");
        } catch (Exception e) {
            Logger.APILogger.LogError($"Error trying to repack preloads into assetbundle: {e}");
        }
        AssetBundleCreateRequest op = AssetBundle.LoadFromMemoryAsync(bundleData);

        if (op == null) {
            progressBar.Progress = 1;
            yield break;
        }

        yield return op;
        var bundle = op.assetBundle;

        var queue = new HashSet<AssetBundleRequest>();

        foreach (var (sceneName, sceneToPreload) in toPreload) {
            foreach (var (mod, toPreloadPaths) in sceneToPreload) {
                if (!preloadedObjects.TryGetValue(mod, out var modPreloads)) {
                    modPreloads = new Dictionary<string, Dictionary<string, GameObject>>();
                    preloadedObjects[mod] = modPreloads;
                }
                if (!modPreloads.TryGetValue(sceneName, out var modScenePreloads)) {
                    modScenePreloads = new Dictionary<string, GameObject>();
                    modPreloads[sceneName] = modScenePreloads;
                }
                
                foreach (string path in toPreloadPaths) {
                    if (modScenePreloads.ContainsKey(path)) continue;
                            
                    string assetName = $"{sceneName}/{path}.prefab";
                    AssetBundleRequest request = bundle.LoadAssetAsync<GameObject>(assetName);
                    request.completed += _ => {
                        queue.Remove(request);
                        
                        var go = (GameObject) request.asset;
                        if (!go) {
                            Logger.APILogger.LogError($"    could not load '{assetName}'");
                            return;
                        }
                        if (modScenePreloads.ContainsKey(path)) {
                            Logger.APILogger.LogWarn($"Duplicate preload by {mod.Name}: '{path}' in '{sceneName}'");
                        } else {
                            modScenePreloads.Add(path, go);
                        }
                    };
                    queue.Add(request);
                }
            }
        }
        int total = queue.Count;

        while (queue.Count > 0) {
            float progress = (total - queue.Count) / (float)total;
            progressBar.Progress = progress;
            yield return null;
        }
    }
    
    /// <summary>
    ///     Preload using `DoPreloadScenes`, but first preprocess them to only contain relevant objects
    /// </summary>
    private IEnumerator DoPreloadRepackedScenes
    (
        Dictionary<string, List<(ModLoader.ModInstance Mod, List<string> Preloads)>> toPreload,
        IDictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<string, List<Func<IEnumerator>>> sceneHooks
    ) {
        const string PreloadBundleName = "modding_api_scene_bundle";
        
        string preloadJson = JsonConvert.SerializeObject(
            toPreload.ToDictionary(k => k.Key, v => v.Value.SelectMany(x => x.Preloads).Distinct())
        );
        byte[] bundleData = null;
        Task task = Task.Run(() => {
            try {
                (bundleData, RepackStats repackStats) = UnitySceneRepacker.Repack(PreloadBundleName, Application.dataPath, preloadJson, UnitySceneRepacker.Mode.SceneBundle);
                Logger.APILogger.Log($"Repacked {toPreload.Count} preload scenes from {repackStats.objectsBefore} to {repackStats.objectsAfter} objects ({bundleData.Length / 1024f / 1024f:F2}MB)");
            } catch (Exception e) {
                Logger.APILogger.LogError($"Error trying to repack preloads into assetbundle: {e}");
            }
        });
        yield return new WaitUntil(() => task.IsCompleted);
        if (bundleData == null) {
            yield return DoPreloadScenes(toPreload, preloadedObjects, sceneHooks);
            yield break;
        } 
        
        AssetBundle repackBundle = AssetBundle.LoadFromMemory(bundleData);
        if (repackBundle == null) {
            Logger.APILogger.LogWarn($"Scene repacking during preloading produced an unloadable asset bundle");
            yield return DoPreloadScenes(toPreload, preloadedObjects, sceneHooks);
            yield break;
        }
        
        const string scenePrefix = $"{PreloadBundleName}_";
        yield return DoPreloadScenes(toPreload, preloadedObjects, sceneHooks, scenePrefix);
        repackBundle.Unload(true);
    }

    /// <summary>
    ///     Preload original scenes using a queue bounded by GlobalSettings.PreloadBatchSize
    /// </summary>
    private IEnumerator DoPreloadScenes
    (
        Dictionary<string, List<(ModLoader.ModInstance Mod, List<string> Preloads)>> toPreload,
        IDictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<string, List<Func<IEnumerator>>> sceneHooks,
        string scenePrefix = ""
    ) {
        List<string> sceneNames = toPreload.Keys.Union(sceneHooks.Keys).ToList();
        Dictionary<string, int> scenePriority = new();
        Dictionary<string, (AsyncOperation load, AsyncOperation unload)> sceneAsyncOperationHolder = new();
        
        foreach (string sceneName in sceneNames)
        {
            int priority = 0;
            
            if (toPreload.TryGetValue(sceneName, out var requests)) 
                priority += requests.Select(x => x.Preloads.Count).Sum();
            
            scenePriority[sceneName] = priority;
            sceneAsyncOperationHolder[sceneName] = (null, null);
        }

        Dictionary<string, GameObject> GetModScenePreloadedObjects(ModLoader.ModInstance mod, string sceneName)
        {
            if (!preloadedObjects.TryGetValue
            (
                mod,
                out Dictionary<string, Dictionary<string, GameObject>> modPreloadedObjects
            ))
            {
                preloadedObjects[mod] = modPreloadedObjects = new Dictionary<string, Dictionary<string, GameObject>>();
            }
            
            // ReSharper disable once InvertIf
            if (!modPreloadedObjects.TryGetValue
            (
                sceneName,
                out Dictionary<string, GameObject> modScenePreloadedObjects
            ))
            {
                modPreloadedObjects[sceneName] = modScenePreloadedObjects = new Dictionary<string, GameObject>();
            }
            
            return modScenePreloadedObjects;
        }

        var preloadOperationQueue = new List<AsyncOperation>(ModHooks.GlobalSettings.PreloadBatchSize);

        IEnumerator GetPreloadObjectsOperation(string sceneName)
        {
            Scene scene = USceneManager.GetSceneByName(scenePrefix + sceneName);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            
            foreach (var go in rootObjects)
                go.SetActive(false);

            if (sceneHooks.TryGetValue(scene.name, out List<Func<IEnumerator>> hooks))
            {
                // ToArray to force a strict select, that way we start them all simultaneously
                foreach (IEnumerator hook in hooks.Select(x => x()).ToArray())
                    yield return hook;
            }

            if (!toPreload.TryGetValue(sceneName, out var sceneObjects)) 
                yield break;
            
            // Fetch object names to preload
            foreach ((ModLoader.ModInstance mod, List<string> objNames) in sceneObjects)
            {
                Logger.APILogger.LogFine($"Fetching objects for mod \"{mod.Mod.GetName()}\"");

                Dictionary<string, GameObject> scenePreloads = GetModScenePreloadedObjects(mod, sceneName);

                foreach (string objName in objNames)
                {
                    Logger.APILogger.LogFine($"Fetching object \"{objName}\"");

                    GameObject obj;

                    try
                    {
                        obj = UnityExtensions.GetGameObjectFromArray(rootObjects, objName);
                    }
                    catch (ArgumentException)
                    {
                        Logger.APILogger.LogWarn($"Invalid GameObject name {objName}");
                        continue;
                    }

                    if (obj == null)
                    {
                        Logger.APILogger.LogWarn(
                            $"Could not find object \"{objName}\" in scene \"{sceneName}\"," + $" requested by mod `{mod.Mod.GetName()}`"
                        );
                        continue;
                    }

                    // Create inactive duplicate of requested object
                    obj = Instantiate(obj);
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);

                    // Set object to be passed to mod
                    scenePreloads[objName] = obj;
                }
            }
        }

        void CleanupPreloadOperation(string sceneName)
        {
            Logger.APILogger.LogFine($"Unloading scene \"{sceneName}\"");

            AsyncOperation unloadOp = USceneManager.UnloadSceneAsync(scenePrefix + sceneName);
            
            sceneAsyncOperationHolder[sceneName] = (sceneAsyncOperationHolder[sceneName].load, unloadOp);
            
            unloadOp.completed += _ => preloadOperationQueue.Remove(unloadOp);
            
            preloadOperationQueue.Add(unloadOp);
        }

        void StartPreloadOperation(string sceneName)
        {
            Logger.APILogger.LogFine($"Loading scene \"{sceneName}\"");

            AsyncOperation loadOp = USceneManager.LoadSceneAsync(scenePrefix + sceneName, LoadSceneMode.Additive);

            StartCoroutine(DoLoad(loadOp));
            
            sceneAsyncOperationHolder[sceneName] = (loadOp, null);

            loadOp.priority = scenePriority[sceneName];
            
            preloadOperationQueue.Add(loadOp);
            
            return;
            
            IEnumerator DoLoad(AsyncOperation load)
            {
                yield return load;
                
                preloadOperationQueue.Remove(load);
                yield return GetPreloadObjectsOperation(sceneName);
                CleanupPreloadOperation(sceneName);
            }
        }

        int i = 0;
        
        float sceneProgressAverage = 0;
        
        while (sceneProgressAverage < 1.0f)
        {
            while (
                preloadOperationQueue.Count < ModHooks.GlobalSettings.PreloadBatchSize &&
                i < sceneNames.Count &&
                sceneProgressAverage < 1.0f
            )
            {
                StartPreloadOperation(sceneNames[i++]);
            }
            
            yield return null;
            
            sceneProgressAverage = sceneAsyncOperationHolder
                                   .Values
                                   .Select(x => (x.load?.progress ?? 0) * 0.5f + (x.unload?.progress ?? 0) * 0.5f)
                                   .Average();
            
            progressBar.Progress = sceneProgressAverage;
        }

        progressBar.Progress = 1;
    }

    /// <summary>
    ///     Clean up everything from preloading.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CleanUpPreloading()
    {
        // Reload the main menu to fix the music/shaders
        Logger.APILogger.LogDebug("Preload done, returning to main menu");

        ModLoader.LoadState |= ModLoader.ModLoadState.Preloaded;

        yield return USceneManager.LoadSceneAsync("Quit_To_Menu");

        while (USceneManager.GetActiveScene().name != Constants.MENU_SCENE)
        {
            yield return new WaitForEndOfFrame();
        }

        Destroy(progressBar);
    }

    /// <summary>
    ///     Unmutes all audio from AudioListeners.
    /// </summary>
    private static void UnmuteAllAudio()
    {
        AudioListener.pause = false;
    }
}
