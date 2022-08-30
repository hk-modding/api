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
        
        private bool _enabled = true;
        
        private readonly List<string> _messages = new(25);
        
        private KeyCode _toggleKey = KeyCode.F10;
        private int _maxMessageCount = 25;
        private int _fontSize = 12;

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
            LoadSettings();

            if (_font == null)
            {
                _font = Font.CreateDynamicFontFromOSFont(OSFonts, _fontSize);
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

            _overlayCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
            _overlayCanvas.name = "ModdingApiConsoleLog";
            CanvasGroup cg = _overlayCanvas.GetComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
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
                _fontSize,
                TextAnchor.LowerLeft,
                new CanvasUtil.RectData(new Vector2(-5, -5), Vector2.zero, Vector2.zero, Vector2.one),
                _font
            );

            _textPanel.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void LoadSettings()
        {
            InGameConsoleSettings settings = ModHooks.GlobalSettings.ConsoleSettings;
            
            _toggleKey = settings.ToggleHotkey;
            
            if (_toggleKey == KeyCode.Escape)
            {
                Logger.APILogger.LogError("Esc cannot be used as hotkey for console togging");
                
                _toggleKey = settings.ToggleHotkey = KeyCode.F10;
            }

            _maxMessageCount = settings.MaxMessageCount;
            
            if (_maxMessageCount <= 0)
            {
                Logger.APILogger.LogError($"Specified max console message count {_maxMessageCount} is invalid");
                
                _maxMessageCount = settings.MaxMessageCount = 24;
            }

            _fontSize = settings.FontSize;
            
            if (_fontSize <= 0)
            {
                Logger.APILogger.LogError($"Specified console font size {_fontSize} is invalid");
                
                _fontSize = settings.FontSize = 12;
            }

            string userFont = settings.Font;
            
            if (string.IsNullOrEmpty(userFont)) 
                return;
            
            _font = Font.CreateDynamicFontFromOSFont(userFont, _fontSize);

            if (_font == null)
                Logger.APILogger.LogError($"Specified font {userFont} not found.");
        }

        [PublicAPI]
        public void Update()
        {
            if (!Input.GetKeyDown(_toggleKey))
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

        public void AddText(string message, LogLevel level)
        {
            IEnumerable<string> chunks = Chunks(message, MSG_LENGTH);
            
            string color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.DefaultColor}>";

            if (ModHooks.GlobalSettings.ConsoleSettings.UseLogColors)
            {
                color = level switch
                {
                    LogLevel.Fine => $"<color={ModHooks.GlobalSettings.ConsoleSettings.FineColor}>",
                    LogLevel.Info => $"<color={ModHooks.GlobalSettings.ConsoleSettings.InfoColor}>",
                    LogLevel.Debug => $"<color={ModHooks.GlobalSettings.ConsoleSettings.DebugColor}>",
                    LogLevel.Warn => $"<color={ModHooks.GlobalSettings.ConsoleSettings.WarningColor}>",
                    LogLevel.Error => $"<color={ModHooks.GlobalSettings.ConsoleSettings.ErrorColor}>",
                    _ => color
                };
            }

            foreach (string s in chunks)
                _messages.Add(color + s + "</color>");

            while (_messages.Count > _maxMessageCount)
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
