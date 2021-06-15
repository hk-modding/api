using System;
using GlobalEnums;
using UnityEngine;
using UnityEngine.UI;
using Modding.Menu.Config;
using Patch = Modding.Patches;
using InControl;

namespace Modding.Menu
{
    /// <summary>
    /// A helper class for creating keybind mapping buttons.
    /// </summary>
    public static class KeybindContent
    {
        /// <summary>
        /// Creates a keybind menu item.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the keybind item in.</param>
        /// <param name="name">The name of the keybind game object.</param>
        /// <param name="action">The <c>PlayerAction</c> to associate with this keybind.</param>
        /// <param name="config">The configuration options for the keybind item.</param>
        /// <returns></returns>
        public static ContentArea AddKeybind(
            this ContentArea content,
            string name,
            PlayerAction action,
            KeybindConfig config
        ) => content.AddKeybind(name, action, config, out _);

        /// <summary>
        /// Creates a keybind menu item.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the keybind item in.</param>
        /// <param name="name">The name of the keybind game object.</param>
        /// <param name="action">The <c>PlayerAction</c> to associate with this keybind.</param>
        /// <param name="config">The configuration options for the keybind item.</param>
        /// <param name="mappableKey">The <c>MappablKey</c> component on the created keybind item.</param>
        /// <returns></returns>
        public static ContentArea AddKeybind(
            this ContentArea content,
            string name,
            PlayerAction action,
            KeybindConfig config,
            out MappableKey mappableKey
        )
        {
            var style = config.Style ?? KeybindStyle.VanillaStyle;
            // Keybind object
            var keybind = new GameObject($"{name}");
            GameObject.DontDestroyOnLoad(keybind);
            keybind.transform.SetParent(content.ContentObject.transform, false);
            // CanvasRenderer
            keybind.AddComponent<CanvasRenderer>();
            // RectTransform
            var keybindRt = keybind.AddComponent<RectTransform>();
            new RelVector2(new Vector2(650f, 100f)).GetBaseTransformData().Apply(keybindRt);
            content.Layout.ModifyNext(keybindRt);
            // MappableKey
            var mapKey = (Patch.MappableKey)keybind.AddComponent<MappableKey>();
            mapKey.InitCustomActions(action.Owner, action);
            var mkbutton = (Patch.MenuSelectable)(MenuSelectable)mapKey;
            mkbutton.cancelAction = (CancelAction)Patch.CancelAction.CustomCancelAction;
            mkbutton.customCancelAction = _ =>
            {
                mapKey.AbortRebind();
                config.CancelAction?.Invoke(mapKey);
            };
            content.NavGraph.AddNavigationNode(mapKey);

            // Text object
            var text = new GameObject("Text");
            GameObject.DontDestroyOnLoad(text);
            text.transform.SetParent(keybind.transform, false);
            // CanvasRenderer
            text.AddComponent<CanvasRenderer>();
            // RectTransform
            var textRt = text.AddComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(0f, 0f);
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.anchoredPosition = new Vector2(0f, 0f);
            textRt.pivot = new Vector2(0.5f, 0.5f);
            // Text
            var labelText = text.AddComponent<Text>();
            labelText.font = MenuResources.TrajanBold;
            labelText.fontSize = style.LabelTextSize;
            labelText.resizeTextMaxSize = style.LabelTextSize;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.text = config.Label;
            labelText.supportRichText = true;
            // FixVerticalAlign
            text.AddComponent<FixVerticalAlign>();

            // LeftCursor object
            var cursorL = new GameObject("CursorLeft");
            GameObject.DontDestroyOnLoad(cursorL);
            cursorL.transform.SetParent(keybind.transform, false);
            // CanvasRenderer
            cursorL.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorLRt = cursorL.AddComponent<RectTransform>();
            cursorLRt.sizeDelta = new Vector2(154f, 112f);
            cursorLRt.pivot = new Vector2(0.5f, 0.5f);
            cursorLRt.anchorMin = new Vector2(0f, 0.5f);
            cursorLRt.anchorMax = new Vector2(0f, 0.5f);
            cursorLRt.anchoredPosition = new Vector2(-52f, 0f);
            cursorLRt.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // Animator
            var cursorLAnimator = cursorL.AddComponent<Animator>();
            cursorLAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorLAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorLAnimator.applyRootMotion = false;
            // Image
            cursorL.AddComponent<Image>();
            // Post Component Config
            mapKey.leftCursor = cursorLAnimator;

            // RightCursor object
            var cursorR = new GameObject("CursorRight");
            GameObject.DontDestroyOnLoad(cursorR);
            cursorR.transform.SetParent(keybind.transform, false);
            // CanvasRenderer
            cursorR.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorRRt = cursorR.AddComponent<RectTransform>();
            cursorRRt.sizeDelta = new Vector2(154f, 112f);
            cursorRRt.pivot = new Vector2(0.5f, 0.5f);
            cursorRRt.anchorMin = new Vector2(1f, 0.5f);
            cursorRRt.anchorMax = new Vector2(1f, 0.5f);
            cursorRRt.anchoredPosition = new Vector2(52f, 0f);
            cursorRRt.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
            // Animator
            var cursorRAnimator = cursorR.AddComponent<Animator>();
            cursorRAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorRAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorRAnimator.applyRootMotion = false;
            // Image
            cursorR.AddComponent<Image>();
            // Post Component Config
            mapKey.rightCursor = cursorRAnimator;

            // Keymap object
            var keymap = new GameObject("Keymap");
            GameObject.DontDestroyOnLoad(keymap);
            keymap.transform.SetParent(keybind.transform, false);
            // CanvasRenderer
            keymap.AddComponent<CanvasRenderer>();
            // RectTransform
            var keymapRt = keymap.AddComponent<RectTransform>();
            keymapRt.sizeDelta = new Vector2(145.8f, 82.4f);
            keymapRt.anchorMin = new Vector2(1f, 0.5f);
            keymapRt.anchorMax = new Vector2(1f, 0.5f);
            keymapRt.anchoredPosition = new Vector2(0f, 0f);
            keymapRt.pivot = new Vector2(1f, 0.5f);
            // Image
            var keymapImg = keymap.AddComponent<Image>();
            keymapImg.preserveAspect = true;
            mapKey.keymapSprite = keymapImg;

            // Keymap Text object
            var keymapText = new GameObject("Text");
            GameObject.DontDestroyOnLoad(keymapText);
            keymapText.transform.SetParent(keymap.transform, false);
            // CanvasRenderer
            keymapText.AddComponent<CanvasRenderer>();
            // RectTransform
            var keymapTextRt = keymapText.AddComponent<RectTransform>();
            keymapTextRt.sizeDelta = new Vector2(65f, 60f);
            keymapTextRt.anchorMin = new Vector2(0.5f, 0.5f);
            keymapTextRt.anchorMin = new Vector2(0.5f, 0.5f);
            keymapTextRt.anchoredPosition = new Vector2(32f, 0f);
            keymapTextRt.pivot = new Vector2(0.5f, 0.5f);
            // Text
            var keymapTextText = keymapText.AddComponent<Text>();
            keymapTextText.font = MenuResources.Perpetua;
            mapKey.keymapText = keymapTextText;
            // FixVerticalAlign
            keymapText.AddComponent<FixVerticalAlign>().labelFixType = FixVerticalAlign.LabelFixType.KeyMap;

            mapKey.GetBinding();
            mapKey.ShowCurrentBinding();
            mappableKey = mapKey;
            return content;
        }
    }

    namespace Config
    {
        /// <summary>
        /// Configuration options for creating a menu keybind option.
        /// </summary>
        public struct KeybindConfig
        {
            /// <summary>
            /// The displayed text for the name of the keybind.
            /// </summary>
            public string Label;
            /// <summary>
            /// The style of the keybind.
            /// </summary>
            public KeybindStyle? Style;
            /// <summary>
            /// The action to run when pressing the menu cancel key while selecting this item.
            /// </summary>
            public Action<MappableKey> CancelAction;
        }

        /// <summary>
        /// The styling options of a keybind menu item.
        /// </summary>
        public struct KeybindStyle
        {
            /// <summary>
            /// The style preset of a keybind in the vanilla game.
            /// </summary>
            public static readonly KeybindStyle VanillaStyle = new KeybindStyle
            {
                LabelTextSize = 37
            };
            /// <summary>
            /// The text size of the label text.
            /// </summary>
            public int LabelTextSize;
        }
    }
}