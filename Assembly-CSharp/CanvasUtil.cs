using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modding
{
    /// <summary>
    /// 
    /// </summary>
    public static class CanvasUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static Font TrajanBold;

        /// <summary>
        /// 
        /// </summary>
        public static Font TrajanNormal;

        private static readonly Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

        /// <summary>
        /// 
        /// </summary>
        public class RectData
        {
            /// <summary>
            /// 
            /// </summary>
            public Vector2 RectSize;

            /// <summary>
            /// 
            /// </summary>
            public Vector2 Position;

            /// <summary>
            /// 
            /// </summary>
            public Vector2 AnchorMin;

            /// <summary>
            /// 
            /// </summary>
            public Vector2 AnchorMax;

            /// <summary>
            /// 
            /// </summary>
            public Vector2 AnchorPivot;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="size"></param>
            /// <param name="pos"></param>
            public RectData(Vector2 size, Vector2 pos)
            {
                RectSize = size;
                Position = pos;
                AnchorMin = new Vector2(0.5f, 0.5f);
                AnchorMax = new Vector2(0.5f, 0.5f);
                AnchorPivot = new Vector2(0.5f, 0.5f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="size"></param>
            /// <param name="pos"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            public RectData(Vector2 size, Vector2 pos, Vector2 min, Vector2 max)
            {
                RectSize = size;
                Position = pos;
                AnchorMin = min;
                AnchorMax = max;
                AnchorPivot = new Vector2(0.5f, 0.5f);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="size"></param>
            /// <param name="pos"></param>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <param name="pivot"></param>
            public RectData(Vector2 size, Vector2 pos, Vector2 min, Vector2 max, Vector2 pivot)
            {
                RectSize = size;
                Position = pos;
                AnchorMin = min;
                AnchorMax = max;
                AnchorPivot = pivot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CreateFonts()
        {

            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold")
                {
                    TrajanBold = f;
                }

                if (f != null && f.name == "TrajanPro-Regular")
                {
                    TrajanNormal = f;
                }
            }
        }


        /// <summary>
        /// Fetches the cached font if it exists.
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns>Font if found, null if not.</returns>
        public static Font GetFont(string fontName)
        {
            if (Fonts.ContainsKey(fontName))
                return Fonts[fontName];

            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == fontName)
                {
                    Fonts.Add(fontName, f);
                }
            }

            return Fonts.ContainsKey(fontName) ? Fonts[fontName] : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Sprite NullSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadRawTextureData(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Sprite CreateSprite(byte[] data, int x, int y, int width, int height)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            tex.anisoLevel = 0;
            return Sprite.Create(tex, new Rect(x, y, width, height), Vector2.zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="rd"></param>
        /// <returns></returns>
        public static GameObject CreateBasePanel(GameObject parent, RectData rd)
        {
            GameObject basePanel = new GameObject();
            if (parent != null)
            {
                basePanel.transform.SetParent(parent.transform);
                basePanel.transform.localScale = new Vector3(1, 1, 1);
            }
            basePanel.AddComponent<CanvasRenderer>();
            AddRectTransform(basePanel, rd);
            return basePanel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="rd"></param>
        public static void AddRectTransform(GameObject go, RectData rd)
        {
            //Create a rectTransform
            //Set the total size of the content
            //all you need to know is, 
            //--

            //sizeDelta is size of the difference of the anchors multiplied by screen size so
            //the sizeDelta width is actually = ((anchorMax.x-anchorMin.x)*screenWidth) + sizeDelta.x
            //so assuming a streched horizontally rectTransform on a 1920 screen, this would be
            //((1-0)*1920)+sizeDelta.x
            //1920 + sizeDelta.x
            //so if you wanted a 100pixel wide box in the center of the screen you'd do -1820, height as 1920+-1820 = 100
            //and if you wanted a fullscreen wide box, its just 0 because 1920+0 = 1920
            //the same applies for height

            //anchorPosition is basically an offset to the center of the anchors multiplies by screen size so
            //a 0.5,0.5 min and 0.5,0.5 max, would put the anchor in the middle of the screen but anchorPosition just offsets that 
            //i.e on a 1920x1080 screen
            //anchorPosition 100,100 would do (1920*0.5)+100,(1080*0.5)+100, so 1060,640

            //ANCHOR MIN / MAX
            //--
            //0,0 = bottom left
            //0,1 = top left
            //1,0 = bottom right
            //1,1 = top right
            //--


            //The only other rects I'd use are
            //anchorMin = 0.0, yyy anchorMax = 1.0, yyy (strech horizontally) y = 0.0 is bottom, y = 0.5 is center, y = 1.0 is top
            //anchorMin = xxx, 0.0 anchorMax = xxx, 1.0 (strech vertically) x = 0.0 is left, x = 0.5 is center, x = 1.0 is right
            //anchorMin = 0.0, 0.0 anchorMax = 1.0, 1.0 (strech to fill)
            //--
            //technically you can anchor these anywhere on the screen
            //you can even use negative values to float something offscreen

            //as for the pivot, the pivot determines where the "center" of the rect is which is useful if you want to rotate something by its corner, note that this DOES offset the anchor positions
            //i.e. with a 100x100 square, setting the pivot to be 1,1 will put the top right of the square at the anchor position (-50,-50 from its starting position)

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMax = rd.AnchorMax;
            rt.anchorMin = rd.AnchorMin;
            rt.pivot = rd.AnchorPivot;
            rt.sizeDelta = rd.RectSize;
            rt.anchoredPosition = rd.Position;
        }

        /*  
         *  ██████╗ █████╗ ███╗   ██╗██╗   ██╗ █████╗ ███████╗
         * ██╔════╝██╔══██╗████╗  ██║██║   ██║██╔══██╗██╔════╝
         * ██║     ███████║██╔██╗ ██║██║   ██║███████║███████╗
         * ██║     ██╔══██║██║╚██╗██║╚██╗ ██╔╝██╔══██║╚════██║
         * ╚██████╗██║  ██║██║ ╚████║ ╚████╔╝ ██║  ██║███████║
         *  ╚═════╝╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═══╝  ╚═╝  ╚═╝╚══════╝           
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderMode"></param>
        /// <param name="ppu"></param>
        /// <returns></returns>
        public static GameObject CreateCanvas(RenderMode renderMode, int ppu)
        {
            GameObject c = new GameObject();
            c.AddComponent<Canvas>().renderMode = renderMode;
            CanvasScaler cs = c.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            cs.referencePixelsPerUnit = ppu;
            c.AddComponent<GraphicRaycaster>();
            c.AddComponent<CanvasGroup>();
            c.GetComponent<CanvasGroup>().blocksRaycasts = false;
            c.GetComponent<CanvasGroup>().interactable = false;
            return c;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderMode"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static GameObject CreateCanvas(RenderMode renderMode, Vector2 size)
        {
            GameObject c = new GameObject();
            c.AddComponent<Canvas>().renderMode = renderMode;
            CanvasScaler cs = c.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = size;
            c.AddComponent<GraphicRaycaster>();
            c.AddComponent<CanvasGroup>();
            c.GetComponent<CanvasGroup>().blocksRaycasts = false;
            c.GetComponent<CanvasGroup>().interactable = false;
            return c;
        }

        /*
         * ████████╗███████╗██╗  ██╗████████╗
         * ╚══██╔══╝██╔════╝╚██╗██╔╝╚══██╔══╝
         *    ██║   █████╗   ╚███╔╝    ██║   
         *    ██║   ██╔══╝   ██╔██╗    ██║   
         *    ██║   ███████╗██╔╝ ██╗   ██║   
         *    ╚═╝   ╚══════╝╚═╝  ╚═╝   ╚═╝  
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textAnchor"></param>
        /// <param name="rectData"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public static GameObject CreateTextPanel(GameObject parent, string text, int fontSize, TextAnchor textAnchor, RectData rectData, Font font)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            Text textObj = panel.AddComponent<Text>();
            textObj.font = font;

            textObj.text = text;
            textObj.supportRichText = true;
            textObj.fontSize = fontSize;
            textObj.alignment = textAnchor;
            return panel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textAnchor"></param>
        /// <param name="rectData"></param>
        /// <param name="bold"></param>
        /// <returns></returns>
        public static GameObject CreateTextPanel(GameObject parent, string text, int fontSize, TextAnchor textAnchor, RectData rectData, bool bold = true)
        {
            return CreateTextPanel(parent, text, fontSize, textAnchor, rectData, bold ? TrajanBold : TrajanNormal);
        }


        /*
         * ██╗███╗   ███╗ █████╗  ██████╗ ███████╗
         * ██║████╗ ████║██╔══██╗██╔════╝ ██╔════╝
         * ██║██╔████╔██║███████║██║  ███╗█████╗  
         * ██║██║╚██╔╝██║██╔══██║██║   ██║██╔══╝  
         * ██║██║ ╚═╝ ██║██║  ██║╚██████╔╝███████╗
         * ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="spr"></param>
        /// <param name="rectData"></param>
        /// <returns></returns>
        public static GameObject CreateImagePanel(GameObject parent, Sprite spr, RectData rectData)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            Image img = panel.AddComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;
            return panel;
        }

        /*
         * ██████╗ ██╗   ██╗████████╗████████╗ ██████╗ ███╗   ██╗
         * ██╔══██╗██║   ██║╚══██╔══╝╚══██╔══╝██╔═══██╗████╗  ██║
         * ██████╔╝██║   ██║   ██║      ██║   ██║   ██║██╔██╗ ██║
         * ██╔══██╗██║   ██║   ██║      ██║   ██║   ██║██║╚██╗██║
         * ██████╔╝╚██████╔╝   ██║      ██║   ╚██████╔╝██║ ╚████║
         * ╚═════╝  ╚═════╝    ╚═╝      ╚═╝    ╚═════╝ ╚═╝  ╚═══╝
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        /// <param name="id"></param>
        /// <param name="spr"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textAnchor"></param>
        /// <param name="rectData"></param>
        /// <param name="bold"></param>
        /// <param name="extraSprites"></param>
        /// <returns></returns>
        public static GameObject CreateButton(GameObject parent, Action<int> action, int id, Sprite spr, string text, int fontSize, TextAnchor textAnchor, RectData rectData, bool bold = true, params Sprite[] extraSprites)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            CreateTextPanel(panel, text, fontSize, textAnchor, rectData, bold);

            Image img = panel.AddComponent<Image>();
            img.sprite = spr;

            Button button = panel.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(delegate
            {
                action(id);
            });

            if (extraSprites.Length == 3)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                button.targetGraphic = img;
                SpriteState sprState = new SpriteState
                {
                    highlightedSprite = extraSprites[0],
                    pressedSprite = extraSprites[1],
                    disabledSprite = extraSprites[2]
                };

                button.spriteState = sprState;
            }
            else
            {
                button.transition = Selectable.Transition.None;
            }

            return panel;
        }

        /*
             ██████╗██╗  ██╗███████╗ ██████╗██╗  ██╗██████╗  ██████╗ ██╗  ██╗
            ██╔════╝██║  ██║██╔════╝██╔════╝██║ ██╔╝██╔══██╗██╔═══██╗╚██╗██╔╝
            ██║     ███████║█████╗  ██║     █████╔╝ ██████╔╝██║   ██║ ╚███╔╝ 
            ██║     ██╔══██║██╔══╝  ██║     ██╔═██╗ ██╔══██╗██║   ██║ ██╔██╗ 
            ╚██████╗██║  ██║███████╗╚██████╗██║  ██╗██████╔╝╚██████╔╝██╔╝ ██╗
             ╚═════╝╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝  ╚═╝╚═════╝  ╚═════╝ ╚═╝  ╚═╝                                                     
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="action"></param>
        /// <param name="boxBgSprite"></param>
        /// <param name="boxFgSprite"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="textAnchor"></param>
        /// <param name="rectData"></param>
        /// <param name="bold"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public static GameObject CreateToggle(GameObject parent, Action<bool> action, Sprite boxBgSprite, Sprite boxFgSprite, string text, int fontSize, TextAnchor textAnchor, RectData rectData, bool bold = true, bool isOn = false)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            GameObject boxBg = CreateImagePanel(panel, boxBgSprite, rectData);
            GameObject boxFg = CreateImagePanel(boxBg, boxFgSprite, rectData);
            //GameObject label = CreateTextPanel(panel, text, fontSize, TextAnchor.UpperLeft, rectData, bold);

            Toggle toggle = panel.AddComponent<Toggle>();
            toggle.isOn = isOn;

            toggle.targetGraphic = boxBg.GetComponent<Image>();
            toggle.graphic = boxFg.GetComponent<Image>();

            toggle.transition = Selectable.Transition.ColorTint;

            ColorBlock cb = new ColorBlock
            {
                normalColor = new Color(1, 1, 1, 1),
                highlightedColor = new Color(1, 1, 1, 1),
                pressedColor = new Color(0.8f, 0.8f, 0.8f, 1.0f),
                disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f),
                fadeDuration = 0.1f
            };
            toggle.colors = cb;

            toggle.onValueChanged.AddListener(delegate (bool b)
            {
                action(b);
            });

            ToggleGroup group = parent.GetComponent<ToggleGroup>();

            if (group != null)
                toggle.group = group;

            return panel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static GameObject CreateToggleGroup()
        {
            GameObject panel = new GameObject();

            AddRectTransform(panel, new RectData(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)));

            panel.AddComponent<ToggleGroup>();

            return panel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="rectData"></param>
        /// <returns></returns>
        public static GameObject CreateRectMask2DPanel(GameObject parent, RectData rectData)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            panel.AddComponent<RectMask2D>();
            return panel;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cg"></param>
        /// <returns></returns>
        public static IEnumerator FadeInCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.alpha = 0f;
            cg.gameObject.SetActive(true);
            while (cg.alpha < 1f)
            {
                cg.alpha += Time.unscaledDeltaTime * 3.2f;
                loopFailsafe += Time.unscaledDeltaTime;
                if (cg.alpha >= 0.95f)
                {
                    cg.alpha = 1f;
                    break;
                }
                if (loopFailsafe >= 2f)
                {
                    break;
                }
                yield return null;
            }
            cg.alpha = 1f;
            cg.interactable = true;
            cg.gameObject.SetActive(true);
            yield return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cg"></param>
        /// <returns></returns>
        public static IEnumerator FadeOutCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.interactable = false;
            while (cg.alpha > 0.05f)
            {
                cg.alpha -= Time.unscaledDeltaTime * 3.2f;
                loopFailsafe += Time.unscaledDeltaTime;
                if (cg.alpha <= 0.05f)
                {
                    break;
                }
                if (loopFailsafe >= 2f)
                {
                    break;
                }
                yield return null;
            }
            cg.alpha = 0f;
            cg.gameObject.SetActive(false);
            yield return null;
        }
    }
}
