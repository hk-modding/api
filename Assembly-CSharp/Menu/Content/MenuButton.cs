using System;
using GlobalEnums;
using UnityEngine;
using UnityEngine.UI;
using Modding.Menu.Config;
using Patch = Modding.Patches;

namespace Modding.Menu
{
    /// <summary>
    /// A helper class for creating menu buttons.
    /// </summary>
    public static class MenuButtonContent
    {
        /// <summary>
        /// Creates a menu button.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the button in.</param>
        /// <param name="name">The name of the button game object.</param>
        /// <param name="config">The configuration options for the menu button.</param>
        /// <returns></returns>
        public static ContentArea AddMenuButton(
            this ContentArea content,
            string name,
            MenuButtonConfig config
        ) => content.AddMenuButton(name, config, out _);

        /// <summary>
        /// Creates a menu button.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the button in.</param>
        /// <param name="name">The name of the button game object.</param>
        /// <param name="config">The configuration options for the menu button.</param>
        /// <param name="button">The <c>MenuButton</c> component on the created menu button.</param>
        /// <returns></returns>
        public static ContentArea AddMenuButton(
            this ContentArea content,
            string name,
            MenuButtonConfig config,
            out MenuButton button
        )
        {
            var style = config.Style ?? MenuButtonStyle.VanillaStyle;

            // Option object
            var option = new GameObject($"{name}");
            GameObject.DontDestroyOnLoad(option);
            option.transform.SetParent(content.contentObject.transform, false);
            // CanvasRenderer
            option.AddComponent<CanvasRenderer>();
            // RectTransform
            var optionRt = option.AddComponent<RectTransform>();
            new RelVector2(new RelLength(0f, 1f), style.Height)
                .GetBaseTransformData()
                .Apply(optionRt);
            content.layout.ModifyNext(optionRt);
            // MenuButton
            var menuButton = (Patch.MenuButton)option.AddComponent<MenuButton>();
            menuButton.buttonType = (MenuButton.MenuButtonType)Patch.MenuButton.MenuButtonType.CustomSubmit;
            menuButton.submitAction = config.SubmitAction;
            menuButton.cancelAction = (CancelAction)Patch.CancelAction.CustomCancelAction;
            menuButton.proceed = config.Proceed;
            ((Patch.MenuSelectable)(MenuSelectable)menuButton).customCancelAction = config.CancelAction;
            content.RegisterMenuItem(menuButton);

            // Label object
            var label = new GameObject("Label");
            GameObject.DontDestroyOnLoad(label);
            label.transform.SetParent(option.transform, false);
            // CanvasRenderer
            label.AddComponent<CanvasRenderer>();
            // RectTransform
            var labelRt = label.AddComponent<RectTransform>();
            labelRt.sizeDelta = new Vector2(0f, 0f);
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.anchoredPosition = new Vector2(0f, 0f);
            // Text
            var labelText = label.AddComponent<Text>();
            labelText.font = MenuResources.TrajanBold;
            labelText.fontSize = style.TextSize;
            labelText.resizeTextMaxSize = style.TextSize;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = config.Label;
            labelText.supportRichText = true;
            // FixVerticalAlign
            label.AddComponent<FixVerticalAlign>();
            // ContentSizeFitter
            var labelCsf = label.AddComponent<ContentSizeFitter>();
            labelCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // LeftCursor object
            var cursorL = new GameObject("CursorLeft");
            GameObject.DontDestroyOnLoad(cursorL);
            cursorL.transform.SetParent(label.transform, false);
            // CanvasRenderer
            cursorL.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorLRt = cursorL.AddComponent<RectTransform>();
            cursorLRt.sizeDelta = new Vector2(164f, 119f);
            cursorLRt.pivot = new Vector2(0.5f, 0.5f);
            cursorLRt.anchorMin = new Vector2(0f, 0.5f);
            cursorLRt.anchorMax = new Vector2(0f, 0.5f);
            cursorLRt.anchoredPosition = new Vector2(-65f, 0f);
            cursorLRt.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // Animator
            var cursorLAnimator = cursorL.AddComponent<Animator>();
            cursorLAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorLAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorLAnimator.applyRootMotion = false;
            // Image
            cursorL.AddComponent<Image>();
            // Post Component Config
            menuButton.leftCursor = cursorLAnimator;

            // RightCursor object
            var cursorR = new GameObject("CursorRight");
            GameObject.DontDestroyOnLoad(cursorR);
            cursorR.transform.SetParent(label.transform, false);
            // CanvasRenderer
            cursorR.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorRRt = cursorR.AddComponent<RectTransform>();
            cursorRRt.sizeDelta = new Vector2(164f, 119f);
            cursorRRt.pivot = new Vector2(0.5f, 0.5f);
            cursorRRt.anchorMin = new Vector2(1f, 0.5f);
            cursorRRt.anchorMax = new Vector2(1f, 0.5f);
            cursorRRt.anchoredPosition = new Vector2(65f, 0f);
            cursorRRt.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
            // Animator
            var cursorRAnimator = cursorR.AddComponent<Animator>();
            cursorRAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorRAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorRAnimator.applyRootMotion = false;
            // Image
            cursorR.AddComponent<Image>();
            // Post Component Config
            menuButton.rightCursor = cursorRAnimator;

            // FlashEffect object
            var flash = new GameObject("FlashEffect");
            GameObject.DontDestroyOnLoad(flash);
            flash.transform.SetParent(label.transform, false);
            // CanvasRenderer
            flash.AddComponent<CanvasRenderer>();
            // RectTransform
            var flashRt = flash.AddComponent<RectTransform>();
            flashRt.sizeDelta = new Vector2(0f, 0f);
            flashRt.anchorMin = new Vector2(0f, 0f);
            flashRt.anchorMax = new Vector2(1f, 1f);
            flashRt.anchoredPosition = new Vector2(0f, 0f);
            flashRt.pivot = new Vector2(0.5f, 0.5f);
            // Image
            flash.AddComponent<Image>();
            // Animator
            var flashAnimator = flash.AddComponent<Animator>();
            flashAnimator.runtimeAnimatorController = MenuResources.MenuButtonFlashAnimator;
            flashAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            flashAnimator.applyRootMotion = false;
            // Post Component Config
            menuButton.flashEffect = flashAnimator;

            button = menuButton;
            return content;
        }
    }

    namespace Config
    {
        /// <summary>
        /// Configuration options for creating a menu button.
        /// </summary>
        public struct MenuButtonConfig
        {
            /// <summary>
            /// The text to render on the button.
            /// </summary>
            public string Label;
            /// <summary>
            /// The action to run when the button is pressed.
            /// </summary>
            public Action<Patch.MenuButton> SubmitAction;
            /// <summary>
            /// Whether the button when activated proceeds to a new menu.
            /// </summary>
            public bool Proceed;
            /// <summary>
            /// The action to run when pressing the menu cancel key while selecting this item.
            /// </summary>
            public Action<MenuSelectable> CancelAction;
            /// <summary>
            /// The styling of the menu button.
            /// </summary>
            public MenuButtonStyle? Style;
        }

        /// <summary>
        /// The styling options for a menu button.
        /// </summary>
        public struct MenuButtonStyle
        {
            /// <summary>
            /// The style preset of a menu button in the vanilla game.
            /// </summary>
            public static readonly MenuButtonStyle VanillaStyle = new MenuButtonStyle
            {
                Height = new RelLength(60f),
                TextSize = 45
            };

            /// <summary>
            /// The size of the menu button.
            /// </summary>
            public RelLength Height;
            /// <summary>
            /// The size of the text on the button.
            /// </summary>
            public int TextSize;
        }
    }
}