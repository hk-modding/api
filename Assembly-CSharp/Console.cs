using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Modding.Patches;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{
    internal class Console : MonoBehaviour
    {
        private static GameObject _overlayCanvas;
        private static GameObject _textPanel;
        private static Font _font;
        private readonly List<string> _messages = new(25);
        private bool _enabled = true;
        private static InGameConsoleSettings _consoleSettings;
        private static readonly string SettingsPath = Path.Combine(Application.persistentDataPath, "ModdingApi.ConsoleSettings.json");

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

            ModHooks.ApplicationQuitHook += SaveGlobalSettings;
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

        public void AddText(string message, LogLevel level)
        {
            IEnumerable<string> chunks = Chunks(message, MSG_LENGTH);

            string color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.DefaultColor}>";

            if (ModHooks.GlobalSettings.ConsoleSettings.UseLogColors)
            {
                switch (level)
                {
                    case LogLevel.Fine:
                        color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.FineColor}>";
                        break;
                    case LogLevel.Info:
                        color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.InfoColor}>";
                        break;
                    case LogLevel.Debug:
                        color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.DebugColor}>";
                        break;
                    case LogLevel.Warn:
                        color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.WarningColor}>";
                        break;
                    case LogLevel.Error:
                        color = $"<color={ModHooks.GlobalSettings.ConsoleSettings.ErrorColor}>";
                        break;
                }
            }

            foreach (string s in chunks)
                _messages.Add(color + s + "</color>");

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

        internal static InGameConsoleSettings ConsoleSettings
        {
            get
            {
                if (_consoleSettings != null)
                {
                    return _consoleSettings;
                }

                _consoleSettings = new InGameConsoleSettings();

                LoadConsoleSettings();

                if (_consoleSettings == null)
                    throw new NullReferenceException(nameof(_consoleSettings));

                return _consoleSettings;
            }
        }

        /// <summary>
        ///     Loads console settings from disk (if they exist)
        /// </summary>
        internal static void LoadConsoleSettings()
        {
            Logger.APILogger.Log("Loading ModdingApi Console Settings.");

            if (!File.Exists(SettingsPath))
            {
                _consoleSettings = new InGameConsoleSettings();

                return;
            }

            try
            {
                using FileStream fileStream = File.OpenRead(SettingsPath);
                using StreamReader reader = new StreamReader(fileStream);

                string json = reader.ReadToEnd();

                try
                {
                    _consoleSettings = JsonConvert.DeserializeObject<InGameConsoleSettings>
                    (
                        json,
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    );
                }
                catch (Exception e)
                {
                    Logger.APILogger.LogError("Failed to deserialize settings using Json.NET, falling back.");
                    Logger.APILogger.LogError(e);

                    _consoleSettings = JsonUtility.FromJson<InGameConsoleSettings>(json);
                }
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError("Failed to load console settings, creating new settings file:\n" + e);

                if (File.Exists(SettingsPath))
                {
                    File.Move(SettingsPath, SettingsPath + ".error");
                }

                _consoleSettings = new InGameConsoleSettings();
            }
        }

        /// <summary>
        ///     Save InGameConsoleSettings to disk. (backs up the current console settings if it exists)
        /// </summary>
        internal static void SaveGlobalSettings()
        {
            Logger.APILogger.Log("Saving console Settings");

            if (File.Exists(SettingsPath + ".bak"))
            {
                File.Delete(SettingsPath + ".bak");
            }

            if (File.Exists(SettingsPath))
            {
                File.Move(SettingsPath, SettingsPath + ".bak");
            }

            using FileStream fileStream = File.Create(SettingsPath);

            using StreamWriter writer = new StreamWriter(fileStream);

            try
            {
                string json = JsonConvert.SerializeObject
                (
                    ConsoleSettings,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ContractResolver = ShouldSerializeContractResolver.Instance,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = JsonConverterTypes.ConverterTypes
                    }
                );

                writer.Write(json);
            }
            catch (Exception e)
            {
                Logger.APILogger.LogError("Failed to save console settings using Json.NET.");
                Logger.APILogger.LogError(e);

                string json = JsonUtility.ToJson(ConsoleSettings, true);

                writer.Write(json);
            }
        }
    }
}