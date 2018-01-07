using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Modding
{
    internal class Console : MonoBehaviour
    {
        public static GameObject OverlayCanvas;
        private static GameObject _textPanel;
        private List<string> messages = new List<string>(20);

        public void Start()
        {
            Debug.Log("Console.Start");
            DontDestroyOnLoad(gameObject);

            if (OverlayCanvas == null) { 
                CanvasUtil.CreateFonts();
                OverlayCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));
                OverlayCanvas.name = "ModdingApiConsoleLog";
                DontDestroyOnLoad(OverlayCanvas);
                _textPanel = CanvasUtil.CreateTextPanel(OverlayCanvas, string.Empty, 12, TextAnchor.UpperLeft,
                    new CanvasUtil.RectData(new Vector2(1920, 400), new Vector2(0, 0)), false);
                Debug.Log("Overlay Created");
            }
        }

        public void AddText(string message)
        {
            Debug.Log("Attempting to log to console:" + message);

            if (_textPanel != null)
            {
                if (messages.Count > 19)
                    messages.RemoveAt(0);
                messages.Add(message);
                _textPanel.GetComponent<UnityEngine.UI.Text>().text = string.Join(string.Empty, messages.ToArray());
            }
        }
    }
}
