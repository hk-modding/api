using UnityEngine;
using UnityEngine.UI;

namespace Modding;

internal class ProgressBar : MonoBehaviour
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



    /// <summary>
    ///     Updates the progress of the loading bar to the given progress.
    /// </summary>
    /// <param name="value">The progress that should be displayed. 0.0f-1.0f</param>
    public float Progress {
        get => _commandedProgress;
        set => _commandedProgress = value;
    }

    public void Start()
    {
        CreateBlanker();

        CreateLoadingBarBackground();

        CreateLoadingBar();
    }

    private static float ExpDecay(float a, float b, float decay) => b + (a - b) * Mathf.Exp(-decay * Time.deltaTime);

    public void Update()
    {
        // https://youtu.be/LSNQuFEDOyQ?si=GmrFzX94CRqDdVqO&t=2976
        const float decay = 16;
        _shownProgress = ExpDecay(_shownProgress, _commandedProgress, decay);
    }

    public void LateUpdate()
    {
        _loadingBarRect.sizeDelta = new Vector2(
            _shownProgress * LoadingBarWidth,
            _loadingBarRect.sizeDelta.y
        );
    }
    
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

    private void OnDestroy()
    {
        Destroy(_loadingBar);
        Destroy(_loadingBarBackground);
        Destroy(_blanker);
    }
}
