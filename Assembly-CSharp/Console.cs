using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{
    internal class Console : MonoBehaviour
    {
        private static GameObject _overlayCanvas;
        private static GameObject _textPanel;
        private static Font _arial;
        private readonly List<string> _messages = new List<string>(25);
        private bool _enabled = true;

        [PublicAPI]
        public void Start()
        {
            _arial = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            DontDestroyOnLoad(gameObject);

            if (_overlayCanvas != null)
            {
                return;
            }

            CanvasUtil.CreateFonts();
            _overlayCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            _overlayCanvas.name = "ModdingApiConsoleLog";
            DontDestroyOnLoad(_overlayCanvas);

            GameObject background = CanvasUtil.CreateImagePanel(_overlayCanvas,
                CanvasUtil.NullSprite(new byte[] { 0x80, 0x00, 0x00, 0x00}),
                new CanvasUtil.RectData(new Vector2(500, 800), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0,0)));

            _textPanel = CanvasUtil.CreateTextPanel(background, string.Join(string.Empty, _messages.ToArray()), 12, TextAnchor.LowerLeft,
                new CanvasUtil.RectData(new Vector2(-5, -5), new Vector2(0, 0), new Vector2(0, 0), new Vector2(1, 1)), _arial);

            _textPanel.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        [PublicAPI]
        public void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F10))
            {
                return;
            }

            StartCoroutine(_enabled
                ? CanvasUtil.FadeOutCanvasGroup(_overlayCanvas.GetComponent<CanvasGroup>())
                : CanvasUtil.FadeInCanvasGroup(_overlayCanvas.GetComponent<CanvasGroup>()));
            _enabled = !_enabled;
        }

        public void AddText(string message)
        {
            if (_messages.Count > 24)
            {
                _messages.RemoveAt(0);
            }

            _messages.Add(message);

            if (_textPanel != null)
            {
                _textPanel.GetComponent<Text>().text = string.Join(string.Empty, _messages.ToArray());
            }
        }
    }
}
