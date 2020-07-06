using System;
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
        private static Font _font;
        private readonly List<string> _messages = new List<string>(25);
        private bool _enabled = true;

        private const int MSG_LENGTH = 80;

        private static readonly string[] OSFonts =
        {
            // Windows
            "Consolas",
            // Mac
            "Menlo",
            // Linux
            "Courier New",
            "DejaVu Mono"
        };

        [PublicAPI]
        public void Start()
        {
            foreach (string font in OSFonts)
            {
                _font = Font.CreateDynamicFontFromOSFont(font, 12);

                // Found a monospace OS font.
                if (_font != null)
                    break;

                Logger.APILogger.Log($"Font {font} not found.");
            }

            // Fallback
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            }

            DontDestroyOnLoad(gameObject);

            if (_overlayCanvas != null)
            {
                return;
            }

            CanvasUtil.CreateFonts();

            _overlayCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            _overlayCanvas.name = "ModdingApiConsoleLog";

            DontDestroyOnLoad(_overlayCanvas);

            GameObject background = CanvasUtil.CreateImagePanel
            (
                _overlayCanvas,
                CanvasUtil.NullSprite(new byte[] {0x80, 0x00, 0x00, 0x00}),
                new CanvasUtil.RectData(new Vector2(500, 800), Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero)
            );

            _textPanel = CanvasUtil.CreateTextPanel
            (
                background,
                string.Join(string.Empty, _messages.ToArray()),
                12,
                TextAnchor.LowerLeft,
                new CanvasUtil.RectData(new Vector2(-5, -5), Vector2.zero, Vector2.zero, Vector2.one),
                _font
            );

            _textPanel.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        [PublicAPI]
        public void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F10))
            {
                return;
            }

            StartCoroutine
            (
                _enabled
                    ? CanvasUtil.FadeOutCanvasGroup(_overlayCanvas.GetComponent<CanvasGroup>())
                    : CanvasUtil.FadeInCanvasGroup(_overlayCanvas.GetComponent<CanvasGroup>())
            );

            _enabled = !_enabled;
        }

        public void AddText(string message)
        {
            IEnumerable<string> chunks = Chunks(message, MSG_LENGTH);

            foreach (string s in chunks)
                _messages.Add(s);

            while (_messages.Count > 24)
            {
                _messages.RemoveAt(0);
            }

            if (_textPanel != null)
            {
                _textPanel.GetComponent<Text>().text = string.Join(string.Empty, _messages.ToArray());
            }
        }

        private static IEnumerable<string> Chunks(string str, int maxChunkSize) 
        {
            for (int i = 0; i < str.Length; i += maxChunkSize) 
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length-i));
        }
    }
}