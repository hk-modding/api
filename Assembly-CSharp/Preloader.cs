using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using Modding.Utils;

namespace Modding;

internal class Preloader : MonoBehaviour
{
    private const int CanvasResolutionWidth = 1920;
    private const int CanvasResolutionHeight = 1080;
    private const int LoadingBarBackgroundWidth = 1000;
    private const int LoadingBarBackgroundHeight = 100;
    private const int LoadingBarMargin = 12;
    private const int LoadingBarWidth = LoadingBarBackgroundWidth - 2 * LoadingBarMargin;
    private const int LoadingBarHeight = LoadingBarBackgroundHeight - 2 * LoadingBarMargin;

    private GameObject _blanker;
    private GameObject _loadingBarBackground;
    private GameObject _loadingBar;
    private RectTransform _loadingBarRect;

    private float _commandedProgress;
    private float _shownProgress;
    private float _secondsSinceLastSet;

    public IEnumerator Preload
    (
        Dictionary<string, List<(ModLoader.ModInstance, List<string>)>> toPreload,
        Dictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<int, Dictionary<Type, List<(ModLoader.ModInstance, List<string>)>>> preloadAssets,
        IDictionary<ModLoader.ModInstance, Dictionary<int, Dictionary<string, UnityEngine.Object>>> preloadedAssets
    )
    {
        MuteAllAudio();

        CreateBlanker();

        CreateLoadingBarBackground();

        CreateLoadingBar();

        yield return DoPreload(toPreload, preloadedObjects, preloadAssets, preloadedAssets);

        yield return CleanUpPreloading();

        UnmuteAllAudio();
    }

    public void Update()
    {
        _secondsSinceLastSet += Time.unscaledDeltaTime;
        _shownProgress = Mathf.Lerp(_shownProgress, _commandedProgress, _secondsSinceLastSet / 10.0f);
    }

    public void LateUpdate()
    {
        _loadingBarRect.sizeDelta = new Vector2(
            _shownProgress * LoadingBarWidth,
            _loadingBarRect.sizeDelta.y
        );
    }

    /// <summary>
    ///     Mutes all audio from AudioListeners.
    /// </summary>
    private static void MuteAllAudio() => AudioListener.pause = true;

    /// <summary>
    ///     Creates the canvas used to show the loading progress.
    ///     It is centered on the screen.
    /// </summary>
    private void CreateBlanker()
    {
        _blanker = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(CanvasResolutionWidth, CanvasResolutionHeight));

        DontDestroyOnLoad(_blanker);

        GameObject panel = CanvasUtil.CreateImagePanel
        (
            _blanker,
            CanvasUtil.NullSprite(new byte[] { 0x00, 0x00, 0x00, 0xFF }),
            new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one)
        );

        panel
            .GetComponent<Image>()
            .preserveAspect = false;
    }

    /// <summary>
    ///     Creates the background of the loading bar.
    ///     It is centered in the canvas.
    /// </summary>
    private void CreateLoadingBarBackground()
    {
        _loadingBarBackground = CanvasUtil.CreateImagePanel
        (
            _blanker,
            CanvasUtil.NullSprite(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
            new CanvasUtil.RectData
            (
                new Vector2(LoadingBarBackgroundWidth, LoadingBarBackgroundHeight),
                Vector2.zero,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f)
            )
        );

        _loadingBarBackground.GetComponent<Image>().preserveAspect = false;
    }

    /// <summary>
    ///     Creates the loading bar with an initial width of 0.
    ///     It is centered in the canvas.
    /// </summary>
    private void CreateLoadingBar()
    {
        _loadingBar = CanvasUtil.CreateImagePanel
        (
            _blanker,
            CanvasUtil.NullSprite(new byte[] { 0x99, 0x99, 0x99, 0xFF }),
            new CanvasUtil.RectData
            (
                new Vector2(0, LoadingBarHeight),
                Vector2.zero,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f)
            )
        );

        _loadingBar.GetComponent<Image>().preserveAspect = false;
        _loadingBarRect = _loadingBar.GetComponent<RectTransform>();
    }

    /// <summary>
    ///     Updates the progress of the loading bar to the given progress.
    /// </summary>
    /// <param name="progress">The progress that should be displayed. 0.0f - 1.0f</param>
    private void UpdateLoadingBarProgress(float progress)
    {
        if (Mathf.Abs(_commandedProgress - progress) < float.Epsilon)
            return;

        _commandedProgress = progress;
        _secondsSinceLastSet = 0.0f;
    }

    /// <summary>
    ///     This is the actual preloading process.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoPreload
    (
        Dictionary<string, List<(ModLoader.ModInstance, List<string>)>> toPreload,
        IDictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        IDictionary<int, Dictionary<Type, List<(ModLoader.ModInstance, List<string>)>>> preloadAssets,
        IDictionary<ModLoader.ModInstance, Dictionary<int, Dictionary<string, UnityEngine.Object>>> preloadedAssets
    )
    {
        List<string> sceneNames = toPreload.Keys.ToList();
        Dictionary<string, int> scenePriority = new();
        Dictionary<string, (AsyncOperation load, AsyncOperation unload)> sceneAsyncOperationHolder = new();

        Dictionary<string, int> assetsCount = preloadAssets.Select(x =>
            (
                Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(x.Key)), x.Value.SelectMany(x2 => x2.Value).Select(x3 => x3.Item2.Count).Sum()
            )
            ).ToDictionary(x => x.Item1, x => x.Item2);

        sceneNames.AddRange(preloadAssets.Select(x => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(x.Key))));

        foreach (string sceneName in sceneNames)
        {
            if (sceneName == "resources") continue;
            int priority = 0;

            if (toPreload.TryGetValue(sceneName, out var preloadObjs0))
                priority += preloadObjs0.Select(x => x.Item2.Count).Sum();

            if (assetsCount.TryGetValue(sceneName, out var assetpriority))
                priority += assetpriority;

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

        var preloadOperationQueue = new List<AsyncOperation>(5);

        void PreloadAssets(
                IDictionary<int, Dictionary<Type, List<(ModLoader.ModInstance, List<string>)>>> preloadAssets,
                IDictionary<ModLoader.ModInstance, Dictionary<int, Dictionary<string, UObject>>> preloadedAssets,
                IDictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
                int sceneId,
                Func<Type, UnityEngine.Object[]> assetsgetter)
        {
            if (preloadAssets.TryGetValue(sceneId, out var assets))
            {
                preloadAssets.Remove(sceneId);
                foreach ((Type type, List<(ModLoader.ModInstance, List<string>)> asset) in assets)
                {
                    List<string> allObjects = asset.SelectMany(x => x.Item2).Select(x => x.StartsWith("prefab:") ? x.Substring(7) : x).ToList();
                    Dictionary<string, UnityEngine.Object> objectsMap = new();

                    foreach (
                        var obj in assetsgetter(type)
                    )
                    {
                        if(obj.GetInstanceID() < 0) continue; //The InstanceId of all objects created through code is negative
                        if (obj is GameObject go)
                        {
                            if (go.scene.IsValid() || go.transform.parent != null) continue;
                        }
                        if (obj is Component component)
                        {
                            if (component.gameObject.scene.IsValid()) continue;
                        }
                        if (!allObjects.Contains(obj.name)) continue;
                        Logger.APILogger.LogFine($"Found object {obj.name}({type.FullName})");
                        objectsMap[obj.name] = obj;
                        allObjects.Remove(obj.name);
                    }

                    foreach (var v in asset)
                    {

                        if (!preloadedAssets.TryGetValue(v.Item1, out var dict))
                        {
                            dict = new();
                            preloadedAssets.Add(v.Item1, dict);
                        }
                        if (!dict.TryGetValue(sceneId, out var dict2))
                        {
                            dict2 = new();
                            dict.Add(sceneId, dict2);
                        }
                        foreach (var name in v.Item2)
                        {
                            Logger.APILogger.LogFine($"Fetching object({type.FullName}) \"{name}\"");
                            string assetname;
                            bool saveToPrelaodGO = false;
                            if (name.StartsWith("prefab:") && type == typeof(GameObject))
                            {
                                assetname = name.Substring(7);
                                saveToPrelaodGO = true;
                            }
                            else
                            {
                                assetname = name;
                            }
                            if (!objectsMap.TryGetValue(assetname, out var obj))
                            {
                                Logger.APILogger.LogWarn(
                                    $"Could not find object \"{name}\"({type.FullName})," + $" requested by mod `{v.Item1.Mod.GetName()}`"
                                );
                                continue;
                            }
                            if (saveToPrelaodGO)
                            {
                                GetModScenePreloadedObjects(v.Item1, sceneId == 0 ? "resources" : ("sharedassets" + sceneId))[assetname] = (GameObject)obj;
                            }
                            else
                            {
                                dict2[assetname] = obj;
                            }
                        }
                    }
                }
            }
        }




        void GetPreloadObjectsOperation(string sceneName)
        {
            Scene scene = USceneManager.GetSceneByName(sceneName);

            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (var go in rootObjects)
                go.SetActive(false);

            PreloadAssets(preloadAssets, preloadedAssets, preloadedObjects, scene.buildIndex, t => Resources.FindObjectsOfTypeAll(t));
            //PreloadPrefab(preloadPrefabs, preloadSceneNameMap, preloadedObjects, sceneName, () => Resources.FindObjectsOfTypeAll<GameObject>());

            if (!toPreload.TryGetValue(sceneName, out var sceneObjects))
                return;

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

            AsyncOperation unloadOp = USceneManager.UnloadSceneAsync(sceneName);

            sceneAsyncOperationHolder[sceneName] = (sceneAsyncOperationHolder[sceneName].load, unloadOp);

            unloadOp.completed += _ => preloadOperationQueue.Remove(unloadOp);

            preloadOperationQueue.Add(unloadOp);
        }

        void StartPreloadOperation(string sceneName)
        {
            Logger.APILogger.LogFine($"Loading scene \"{sceneName}\"");

            AsyncOperation loadOp = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            sceneAsyncOperationHolder[sceneName] = (loadOp, null);

            loadOp.priority = scenePriority[sceneName];
            loadOp.completed += _ =>
            {
                preloadOperationQueue.Remove(loadOp);
                GetPreloadObjectsOperation(sceneName);
                CleanupPreloadOperation(sceneName);
            };

            preloadOperationQueue.Add(loadOp);
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

            UpdateLoadingBarProgress(sceneProgressAverage);
        }


        PreloadAssets(preloadAssets, preloadedAssets, preloadedObjects, 0, t => {
            //Resources.LoadAll("", t);
            return Resources.FindObjectsOfTypeAll(t);
    });
        //PreloadPrefab(preloadPrefabs, preloadSceneNameMap, preloadedObjects, "resources", () => Resources.LoadAll<GameObject>(""));

        UpdateLoadingBarProgress(1.0f);
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

        // Remove the black screen
        Destroy(_loadingBar);
        Destroy(_loadingBarBackground);
        Destroy(_blanker);
    }

    /// <summary>
    ///     Unmutes all audio from AudioListeners.
    /// </summary>
    private static void UnmuteAllAudio()
    {
        AudioListener.pause = false;
    }
}
