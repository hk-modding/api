using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{
    internal class Console : MonoBehaviour
    {
        public static GameObject OverlayCanvas;
        private static GameObject _textPanel;
        public static Font Arial;
        private readonly List<string> _messages = new List<string>(25);
        private bool _enabled = true;

        public void Start()
        {
            Arial = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            DontDestroyOnLoad(gameObject);

            if (OverlayCanvas != null) return;
             
            CanvasUtil.CreateFonts();
            OverlayCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            OverlayCanvas.name = "ModdingApiConsoleLog";
            DontDestroyOnLoad(OverlayCanvas);

            GameObject background = CanvasUtil.CreateImagePanel(OverlayCanvas,
                CanvasUtil.NullSprite(new byte[] { 0x80, 0x00, 0x00, 0x00}),
                new CanvasUtil.RectData(new Vector2(500, 800), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0,0)));

            _textPanel = CanvasUtil.CreateTextPanel(background, string.Join(string.Empty, _messages.ToArray()), 12, TextAnchor.LowerLeft,
                new CanvasUtil.RectData(new Vector2(-5, -5), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1)), Arial);

            _textPanel.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                StartCoroutine(_enabled
                    ? CanvasUtil.FadeOutCanvasGroup(OverlayCanvas.GetComponent<CanvasGroup>())
                    : CanvasUtil.FadeInCanvasGroup(OverlayCanvas.GetComponent<CanvasGroup>()));
                _enabled = !_enabled;
            }
        }

        public void AddText(string message)
        {
            if (_messages.Count > 24)
                _messages.RemoveAt(0);

            _messages.Add(message);

            if (_textPanel != null)
            {
                _textPanel.GetComponent<Text>().text = string.Join(string.Empty, _messages.ToArray());
            }
        }
    }
}
