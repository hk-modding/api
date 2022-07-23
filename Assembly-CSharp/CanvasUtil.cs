using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable file UnusedMember.Global

namespace Modding
{
    /// <summary>
    ///     Utility with helpful functions for drawing canvas elements on screen.
    /// </summary>
    [PublicAPI]
    public static class CanvasUtil
    {
        private static Font _trajanBold;
        private static Font _trajanNormal;
        
        /// <summary>
        ///     Access to the TrajanBold Font
        /// </summary>
        public static Font TrajanBold 
        {
            get
            {
                if (!_trajanBold)
                    CreateFonts();
                
                return _trajanBold;
            }
            
        }

        /// <summary>
        ///     Access to the TrajanNormal Font
        /// </summary>
        public static Font TrajanNormal
        {
            get
            {
                if (!_trajanNormal)
                    CreateFonts();

                return _trajanNormal;
            }
        }

        private static readonly Dictionary<string, Font> Fonts = new();

        /// <summary>
        ///     Fetches the Trajan fonts to be cached and used.
        /// </summary>
        private static void CreateFonts()
        {
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == "TrajanPro-Bold")
                {
                    _trajanBold = f;
                }

                if (f != null && f.name == "TrajanPro-Regular")
                {
                    _trajanNormal = f;
                }
            }
        }


        /// <summary>
        ///     Fetches the cached font if it exists.
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns>Font if found, null if not.</returns>
        public static Font GetFont(string fontName)
        {
            if (Fonts.ContainsKey(fontName))
            {
                return Fonts[fontName];
            }

            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (f != null && f.name == fontName)
                {
                    Fonts.Add(fontName, f);
                    break;
                }
            }

            return Fonts.ContainsKey(fontName) ? Fonts[fontName] : null;
        }

        /// <summary>
        ///     Creates a 1px * 1px sprite of a single color.
        /// </summary>
        /// <param name="data">Optional value to control the single null sprite</param>
        /// <returns></returns>
        public static Sprite NullSprite(byte[] data = null)
        {
            Texture2D tex = new Texture2D(1, 1);
            
            data ??= new byte[] { 0x00, 0x00, 0x00, 0x00 };

            tex.LoadRawTextureData(data);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        /// <summary>
        ///     Creates a sprite from a sub-section of the given texture.
        /// </summary>
        /// <param name="data">Sprite texture data</param>
        /// <param name="x">X location of the sprite within the texture.</param>
        /// <param name="y">Y Locaiton of the sprite within the texture.</param>
        /// <param name="width">Width of sprite</param>
        /// <param name="height">Height of sprite</param>
        /// <returns></returns>
        public static Sprite CreateSprite(byte[] data, int x, int y, int width, int height)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            tex.anisoLevel = 0;
            return Sprite.Create(tex, new Rect(x, y, width, height), Vector2.zero);
        }


        /// <summary>
        ///     Creates a base panel for other panels to use.
        /// </summary>
        /// <param name="parent">Parent Game Object under which this panel will be held</param>
        /// <param name="rd">Rectangle data for this panel</param>
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
        ///     Transforms the RectData into a RectTransform for the GameObject.
        /// </summary>
        /// <param name="go">GameObject to which this rectdata should be put into.</param>
        /// <param name="rd">Rectangle Data</param>
        public static void AddRectTransform(GameObject go, RectData rd)
        {
            // Create a rectTransform
            // Set the total size of the content
            // all you need to know is, 
            // --

            // sizeDelta is size of the difference of the anchors multiplied by screen size so
            // the sizeDelta width is actually = ((anchorMax.x-anchorMin.x)*screenWidth) + sizeDelta.x
            // so assuming a streched horizontally rectTransform on a 1920 screen, this would be
            // ((1-0)*1920)+sizeDelta.x
            // 1920 + sizeDelta.x
            // so if you wanted a 100pixel wide box in the center of the screen you'd do -1820, height as 1920+-1820 = 100
            // and if you wanted a fullscreen wide box, its just 0 because 1920+0 = 1920
            // the same applies for height

            // anchorPosition is basically an offset to the center of the anchors multiplies by screen size so
            // a 0.5,0.5 min and 0.5,0.5 max, would put the anchor in the middle of the screen but anchorPosition just offsets that 
            // i.e on a 1920x1080 screen
            // anchorPosition 100,100 would do (1920*0.5)+100,(1080*0.5)+100, so 1060,640

            // ANCHOR MIN / MAX
            // --
            // 0,0 = bottom left
            // 0,1 = top left
            // 1,0 = bottom right
            // 1,1 = top right
            // --


            // The only other rects I'd use are
            // anchorMin = 0.0, yyy anchorMax = 1.0, yyy (strech horizontally) y = 0.0 is bottom, y = 0.5 is center, y = 1.0 is top
            // anchorMin = xxx, 0.0 anchorMax = xxx, 1.0 (strech vertically) x = 0.0 is left, x = 0.5 is center, x = 1.0 is right
            // anchorMin = 0.0, 0.0 anchorMax = 1.0, 1.0 (strech to fill)
            // --
            // technically you can anchor these anywhere on the screen
            // you can even use negative values to float something offscreen

            // as for the pivot, the pivot determines where the "center" of the rect is which is useful if you want to rotate something by its corner, note that this DOES offset the anchor positions
            // i.e. with a 100x100 square, setting the pivot to be 1,1 will put the top right of the square at the anchor position (-50,-50 from its starting position)

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMax = rd.AnchorMax;
            rt.anchorMin = rd.AnchorMin;
            rt.pivot = rd.AnchorPivot;
            rt.sizeDelta = rd.RectSizeDelta;
            rt.anchoredPosition = rd.AnchorPosition;
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
        ///     Creates a Canvas Element that is scaled to the parent's size.
        /// </summary>
        /// <param name="renderMode">Render Mode to Use</param>
        /// <param name="referencePixelsPerUnit"></param>
        /// <returns></returns>
        public static GameObject CreateCanvas(RenderMode renderMode, int referencePixelsPerUnit)
        {
            GameObject c = CreateCanvas(renderMode);
            c.GetComponent<CanvasScaler>().referencePixelsPerUnit = referencePixelsPerUnit;
            return c;
        }


        /// <summary>
        ///     Creates a Canvas Element.
        /// </summary>
        /// <param name="renderMode">RenderMode to Use</param>
        /// <param name="size">Size of the Canvas</param>
        /// <returns></returns>
        public static GameObject CreateCanvas(RenderMode renderMode, Vector2 size)
        {
            GameObject c = CreateCanvas(renderMode);
            c.GetComponent<CanvasScaler>().referenceResolution = size;
            return c;
        }

        private static GameObject CreateCanvas(RenderMode renderMode)
        {
            GameObject c = new GameObject();
            c.AddComponent<Canvas>().renderMode = renderMode;
            CanvasScaler cs = c.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            c.AddComponent<GraphicRaycaster>();
            c.AddComponent<CanvasGroup>();
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
        ///     Creates a Text Object
        /// </summary>
        /// <param name="parent">The GameObject that this text will be put into.</param>
        /// <param name="text">The text that will be shown with this object</param>
        /// <param name="fontSize">The text's font size.</param>
        /// <param name="textAnchor">The location within the rectData where the text anchor should be.</param>
        /// <param name="rectData">Rectangle Data to describe the Text Panel.</param>
        /// <param name="font">The Font to use</param>
        /// <returns></returns>
        public static GameObject CreateTextPanel(GameObject parent, string text, int fontSize, TextAnchor textAnchor,
            RectData rectData, Font font)
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
        ///     Creates a Text Object
        /// </summary>
        /// <param name="parent">The GameObject that this text will be put into.</param>
        /// <param name="text">The text that will be shown with this object</param>
        /// <param name="fontSize">The text's font size.</param>
        /// <param name="textAnchor">The location within the rectData where the text anchor should be.</param>
        /// <param name="rectData">Rectangle Data to describe the Text Panel.</param>
        /// <param name="bold">If True, TrajanBold will be the font used, else TrajanNormal</param>
        /// <returns></returns>
        public static GameObject CreateTextPanel(GameObject parent, string text, int fontSize, TextAnchor textAnchor,
            RectData rectData, bool bold = true)
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
        ///     Creates an Image Panel
        /// </summary>
        /// <param name="parent">The Parent GameObject for this image.</param>
        /// <param name="sprite">The Image/Sprite to use</param>
        /// <param name="rectData">The rectangle description for this sprite to inhabit</param>
        /// <returns></returns>
        public static GameObject CreateImagePanel(GameObject parent, Sprite sprite, RectData rectData)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            Image img = panel.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.useSpriteMesh = true;
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
        ///     Creates a Button
        /// </summary>
        /// <param name="parent">The Parent GameObject for this Button</param>
        /// <param name="action">Action to take when butotn is clicked</param>
        /// <param name="id">Id passed to the action</param>
        /// <param name="spr">Sprite to use for the button</param>
        /// <param name="text">Text for the button</param>
        /// <param name="fontSize">Size of the Text</param>
        /// <param name="textAnchor">Where to Anchor the text within the button</param>
        /// <param name="rectData">The rectangle description for this button</param>
        /// <param name="bold">If Set, uses Trajan-Bold, else Trajan for the font</param>
        /// <param name="extraSprites">
        ///     Size 3 array of other sprite states for the button.  0 = Highlighted Sprite, 1 = Pressed
        ///     Sprited, 2 = Disabled Sprite
        /// </param>
        /// <returns></returns>
        public static GameObject CreateButton(GameObject parent, Action<int> action, int id, Sprite spr, string text,
            int fontSize, TextAnchor textAnchor, RectData rectData, bool bold = true, params Sprite[] extraSprites)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            CreateTextPanel(panel, text, fontSize, textAnchor, rectData, bold);

            Image img = panel.AddComponent<Image>();
            img.sprite = spr;

            Button button = panel.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(delegate { action(id); });

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
        ///     Creates a checkbox
        /// </summary>
        /// <param name="parent">The Parent GameObject for this Checkbox</param>
        /// <param name="action">Action to take when butotn is clicked</param>
        /// <param name="boxBgSprite">Sprite to use for the background of the box</param>
        /// <param name="boxFgSprite">Sprite to use for the foreground of the box</param>
        /// <param name="text">Text for the Checkbox</param>
        /// <param name="fontSize">Size of the Text</param>
        /// <param name="textAnchor">Where to Anchor the text within the checkbox</param>
        /// <param name="rectData">The rectangle description for this checkbox</param>
        /// <param name="bold">If Set, uses Trajan-Bold, else Trajan for the font</param>
        /// <param name="isOn">Determines if the initial state of the checkbox is checked or not</param>
        /// <returns></returns>
        public static GameObject CreateToggle(GameObject parent, Action<bool> action, Sprite boxBgSprite,
            Sprite boxFgSprite, string text, int fontSize, TextAnchor textAnchor, RectData rectData, bool bold = true,
            bool isOn = false)
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

            toggle.onValueChanged.AddListener(delegate(bool b) { action(b); });

            ToggleGroup group = parent.GetComponent<ToggleGroup>();

            if (group != null)
            {
                toggle.group = group;
            }

            return panel;
        }

        /// <summary>
        ///     Allows for a radio button style group of toggles where only 1 can be toggled at once.
        /// </summary>
        /// <returns></returns>
        public static GameObject CreateToggleGroup()
        {
            GameObject panel = new GameObject();

            AddRectTransform(panel,
                new RectData(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f)));

            panel.AddComponent<ToggleGroup>();

            return panel;
        }

        /// <summary>
        ///     Hides everything in this object and children objects that goes outside this objects rect
        /// </summary>
        /// <param name="parent">Parent Object for this Panel</param>
        /// <param name="rectData">Describes the panel's rectangle</param>
        /// <returns></returns>
        public static GameObject CreateRectMask2DPanel(GameObject parent, RectData rectData)
        {
            GameObject panel = CreateBasePanel(parent, rectData);

            panel.AddComponent<RectMask2D>();
            return panel;
        }


        /// <summary>
        ///     Fades the Canvas Group In When it is &lt; 1f
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
        ///     Fades the Canvas Group Out When it is &gt; .05f
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

        /// <summary>
        ///     Rectangle Helper Class
        /// </summary>
        public class RectData
        {
            /// <summary>
            ///     Describes on of the X,Y Positions of the Element
            /// </summary>
            public Vector2 AnchorMax;

            /// <summary>
            ///     Describes on of the X,Y Positions of the Element
            /// </summary>
            public Vector2 AnchorMin;

            /// <summary>
            /// </summary>
            public Vector2 AnchorPivot;

            /// <summary>
            ///     Relative Offset Postion where Element is anchored as compared to Min / Max
            /// </summary>
            public Vector2 AnchorPosition;

            /// <summary>
            ///     Difference in size of the rectangle as compared to it's parent.
            /// </summary>
            public Vector2 RectSizeDelta;

            /// <inheritdoc />
            /// <summary>
            ///     Describes a Rectangle's relative size, shape, and relative position to it's parent.
            /// </summary>
            /// <param name="sizeDelta">
            ///     sizeDelta is size of the difference of the anchors multiplied by screen size so
            ///     the sizeDelta width is actually = ((anchorMax.x-anchorMin.x)*screenWidth) + sizeDelta.x
            ///     so assuming a streched horizontally rectTransform on a 1920 screen, this would be
            ///     ((1-0)*1920)+sizeDelta.x
            ///     1920 + sizeDelta.x
            ///     so if you wanted a 100pixel wide box in the center of the screen you'd do -1820, height as 1920+-1820 = 100
            ///     and if you wanted a fullscreen wide box, its just 0 because 1920+0 = 1920
            ///     the same applies for height
            /// </param>
            /// <param name="anchorPosition">Relative Offset Postion where Element is anchored as compared to Min / Max</param>
            public RectData(Vector2 sizeDelta, Vector2 anchorPosition)
                : this(sizeDelta, anchorPosition, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f))
            {
            }


            /// <inheritdoc />
            /// <summary>
            ///     Describes a Rectangle's relative size, shape, and relative position to it's parent.
            /// </summary>
            /// <param name="sizeDelta">
            ///     sizeDelta is size of the difference of the anchors multiplied by screen size so
            ///     the sizeDelta width is actually = ((anchorMax.x-anchorMin.x)*screenWidth) + sizeDelta.x
            ///     so assuming a streched horizontally rectTransform on a 1920 screen, this would be
            ///     ((1-0)*1920)+sizeDelta.x
            ///     1920 + sizeDelta.x
            ///     so if you wanted a 100pixel wide box in the center of the screen you'd do -1820, height as 1920+-1820 = 100
            ///     and if you wanted a fullscreen wide box, its just 0 because 1920+0 = 1920
            ///     the same applies for height
            /// </param>
            /// <param name="anchorPosition">Relative Offset Postion where Element is anchored as compared to Min / Max</param>
            /// <param name="min">
            ///     Describes 1 corner of the rectangle
            ///     0,0 = bottom left
            ///     0,1 = top left
            ///     1,0 = bottom right
            ///     1,1 = top right
            /// </param>
            /// <param name="max">
            ///     Describes 1 corner of the rectangle
            ///     0,0 = bottom left
            ///     0,1 = top left
            ///     1,0 = bottom right
            ///     1,1 = top right
            /// </param>
            public RectData(Vector2 sizeDelta, Vector2 anchorPosition, Vector2 min, Vector2 max)
                : this(sizeDelta, anchorPosition, min, max, new Vector2(0.5f, 0.5f))
            {
            }

            /// <summary>
            ///     Describes a Rectangle's relative size, shape, and relative position to it's parent.
            /// </summary>
            /// <param name="sizeDelta">
            ///     sizeDelta is size of the difference of the anchors multiplied by screen size so
            ///     the sizeDelta width is actually = ((anchorMax.x-anchorMin.x)*screenWidth) + sizeDelta.x
            ///     so assuming a streched horizontally rectTransform on a 1920 screen, this would be
            ///     ((1-0)*1920)+sizeDelta.x
            ///     1920 + sizeDelta.x
            ///     so if you wanted a 100pixel wide box in the center of the screen you'd do -1820, height as 1920+-1820 = 100
            ///     and if you wanted a fullscreen wide box, its just 0 because 1920+0 = 1920
            ///     the same applies for height
            /// </param>
            /// <param name="anchorPosition">Relative Offset Postion where Element is anchored as compared to Min / Max</param>
            /// <param name="min">
            ///     Describes 1 corner of the rectangle
            ///     0,0 = bottom left
            ///     0,1 = top left
            ///     1,0 = bottom right
            ///     1,1 = top right
            /// </param>
            /// <param name="max">
            ///     Describes 1 corner of the rectangle
            ///     0,0 = bottom left
            ///     0,1 = top left
            ///     1,0 = bottom right
            ///     1,1 = top right
            /// </param>
            /// <param name="pivot">Controls the location to use to rotate the rectangle if necessary.</param>
            public RectData(Vector2 sizeDelta, Vector2 anchorPosition, Vector2 min, Vector2 max, Vector2 pivot)
            {
                RectSizeDelta = sizeDelta;
                AnchorPosition = anchorPosition;
                AnchorMin = min;
                AnchorMax = max;
                AnchorPivot = pivot;
            }
        }
    }
}
