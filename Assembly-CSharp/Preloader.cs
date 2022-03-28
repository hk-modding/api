using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public IEnumerator Preload(
        Dictionary<string, List<(ModLoader.ModInstance, List<string>)>> toPreload,
        Dictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects
        )
    {
        MuteAllAudio();

        CreateBlanker();
        
        CreateLoadingBarBackground();
        
        CreateLoadingBar();
        
        yield return DoPreload(toPreload, preloadedObjects);
        
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
    private void MuteAllAudio()
    {
        AudioListener.pause = true;
    }

    /// <summary>
    ///     Creates the canvas used to show the loading progress.
    ///     It is centered on the screen.
    /// </summary>
    private void CreateBlanker()
    {
        _blanker = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(CanvasResolutionWidth, CanvasResolutionHeight));
        UObject.DontDestroyOnLoad(_blanker);
        CanvasUtil.CreateImagePanel(
                _blanker,
                CanvasUtil.NullSprite(new byte[] {0x00, 0x00, 0x00, 0xFF}),
                new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one)
            )
            .GetComponent<Image>()
            .preserveAspect = false;
    }

    /// <summary>
    ///     Creates the background of the loading bar.
    ///     It is centered in the canvas.
    /// </summary>
    private void CreateLoadingBarBackground()
    {
        _loadingBarBackground = CanvasUtil.CreateImagePanel(
                _blanker,
                CanvasUtil.NullSprite(new byte[] {0xFF, 0xFF, 0xFF, 0xFF}),
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
        _loadingBar = CanvasUtil.CreateImagePanel(
            _blanker,
            CanvasUtil.NullSprite(new byte[] {0x99, 0x99, 0x99, 0xFF}),
            new CanvasUtil.RectData(
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
        _commandedProgress = progress;
        _secondsSinceLastSet = 0.0f;
    }

    /// <summary>
    ///     This is the actual preloading process.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoPreload(
        Dictionary<string, List<(ModLoader.ModInstance, List<string>)>> toPreload,
        Dictionary<ModLoader.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects
        )
    {
        List<string> sceneNames = toPreload.Keys.ToList();
        Dictionary<string, int> scenePriority = new();
        Dictionary<string, float> sceneProgress = new();
            
        foreach (var sceneName in sceneNames)
        {
            scenePriority[sceneName] = toPreload[sceneName].Select(x => x.Item2.Count).Sum();
            sceneProgress[sceneName] = 0.0f;
        }
        
        List<AsyncOperation> preloadOperationQueue = new List<AsyncOperation>(5);

        void GetPreloadObjectsOperation(string sceneName)
        {
            Scene scene = USceneManager.GetSceneByName(sceneName);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var go in rootObjects)
            {
                go.SetActive(false);
            }

            // Fetch object names to preload
            List<(ModLoader.ModInstance, List<string>)> sceneObjects = toPreload[sceneName];

            foreach ((ModLoader.ModInstance mod, List<string> objNames) in sceneObjects)
            {
                Logger.APILogger.LogFine($"Fetching objects for mod \"{mod.Mod.GetName()}\"");

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
                            sceneName,
                            out Dictionary<string, GameObject> modScenePreloadedObjects
                        ))
                    {
                        modScenePreloadedObjects = new Dictionary<string, GameObject>();
                        modPreloadedObjects[sceneName] = modScenePreloadedObjects;
                    }

                    // Create inactive duplicate of requested object
                    obj = Instantiate(obj);
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);

                    // Set object to be passed to mod
                    modScenePreloadedObjects[objName] = obj;
                }
            }
        }

        void CleanupPreloadOperation(string sceneName)
        {
            Logger.APILogger.LogFine($"Unloading scene \"{sceneName}\"");
            var unloadOperation = USceneManager.UnloadSceneAsync(sceneName);
            unloadOperation.completed += _ =>
            {
                sceneProgress[sceneName] = 1.0f;
                preloadOperationQueue.Remove(unloadOperation);
            };
            preloadOperationQueue.Add(unloadOperation);
        }

        void StartPreloadOperation(string sceneName)
        {
            Logger.APILogger.LogFine($"Loading scene \"{sceneName}\"");
            sceneProgress[sceneName] = 0.0f;
            var preloadOperation = USceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            preloadOperation.priority = scenePriority[sceneName];
            preloadOperation.completed += _ =>
            {
                sceneProgress[sceneName] = 0.5f;
                preloadOperationQueue.Remove(preloadOperation);
                GetPreloadObjectsOperation(sceneName);
                CleanupPreloadOperation(sceneName);
            };
            preloadOperationQueue.Add(preloadOperation);
        }

        int i = 0;
        float sceneProgressAverage = sceneProgress.Values.Average();
        while (sceneProgressAverage < 1.0f)
        {
            while (
                preloadOperationQueue.Count < ModHooks.GlobalSettings.PreloadBatchSize &&
                i < sceneNames.Count &&
                sceneProgressAverage < 1.0f
            )
            {
                StartPreloadOperation(sceneNames[i++]);
                UpdateLoadingBarProgress(sceneProgress.Values.Average());
                sceneProgressAverage = sceneProgress.Values.Average();
            }
            yield return null;
            sceneProgressAverage = sceneProgress.Values.Average();
        }
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