using System;
using GlobalEnums;
using UnityEngine;
using UnityEngine.UI;
using Modding.Menu.Config;
using Patch = Modding.Patches;

namespace Modding.Menu
{
    /// <summary>
    /// A helper class for creating horizontal menu options.
    /// </summary>
    public static class HorizontalOptionContent
    {
        /// <summary>
        /// Creates a horizontal option.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the option in.</param>
        /// <param name="name">The name of the option game object.</param>
        /// <param name="config">The configuration options for the horizontal option.</param>
        /// <returns></returns>
        public static ContentArea AddHorizontalOption(
            this ContentArea content,
            string name,
            HorizontalOptionConfig config
        ) => content.AddHorizontalOption(name, config, out _);

        /// <summary>
        /// Creates a horizontal option.
        /// </summary>
        /// <param name="content">The <c>ContentArea</c> to put the option in.</param>
        /// <param name="name">The name of the option game object.</param>
        /// <param name="config">The configuration options for the horizontal option.</param>
        /// <param name="horizontalOption">The <c>MenuOptionHorizontal</c> component on the created horizontal option.</param>
        /// <returns></returns>
        public static ContentArea AddHorizontalOption(
            this ContentArea content,
            string name,
            HorizontalOptionConfig config,
            out MenuOptionHorizontal horizontalOption
        )
        {
            var style = config.style ?? HorizontalOptionStyle.vanillaStyle;

            // Option object
            var option = new GameObject($"{name}");
            GameObject.DontDestroyOnLoad(option);
            option.transform.SetParent(content.contentObject.transform, false);
            // CanvasRenderer
            option.AddComponent<CanvasRenderer>();
            // RectTransform
            var optionRt = option.AddComponent<RectTransform>();
            style.size.GetBaseTransformData().Apply(optionRt);
            content.layout.ModifyNext(optionRt);
            // MenuOptionHorizontal
            var menuOptionHorizontal = option.AddComponent<MenuOptionHorizontal>();
            menuOptionHorizontal.optionList = config.options;
            menuOptionHorizontal.applySettingOn = MenuOptionHorizontal.ApplyOnType.Scroll;
            menuOptionHorizontal.cancelAction = (CancelAction)Patch.CancelAction.CustomCancelAction;
            ((Patch.MenuSelectable)(MenuSelectable)menuOptionHorizontal).customCancelAction = config.cancelAction;
            content.RegisterMenuItem(menuOptionHorizontal);
            // MenuSetting
            var menuSetting = (Patch.MenuSetting)option.AddComponent<MenuSetting>();
            menuSetting.settingType = (MenuSetting.MenuSettingType)Patch.MenuSetting.MenuSettingType.CustomSetting;
            menuSetting.customApplySetting = config.applySetting;
            menuSetting.customRefreshSetting = config.refreshSetting;
            menuSetting.optionList = menuOptionHorizontal;
            // Post Component Config
            menuOptionHorizontal.menuSetting = menuSetting;

            // Label object
            var label = new GameObject("Label");
            GameObject.DontDestroyOnLoad(label);
            label.transform.SetParent(option.transform, false);
            // CanvasRenderer
            label.AddComponent<CanvasRenderer>();
            // RectTransform
            var labelRt = label.AddComponent<RectTransform>();
            // the RectTransform that TC uses is utter garbage and imo this make far more sense
            labelRt.sizeDelta = new Vector2(0f, 0f); // this makes sense if you think about it
            labelRt.pivot = new Vector2(0.5f, 0.5f);
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.anchoredPosition = new Vector2(0f, 0f);
            // Text
            var labelText = label.AddComponent<Text>();
            labelText.font = MenuResources.TrajanBold;
            labelText.fontSize = style.labelTextSize;
            labelText.resizeTextMaxSize = style.labelTextSize;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.text = config.label;
            labelText.supportRichText = true;
            // FixVerticalAlign
            label.AddComponent<FixVerticalAlign>();

            // Text object
            var optionText = new GameObject("Text");
            GameObject.DontDestroyOnLoad(optionText);
            optionText.transform.SetParent(option.transform, false);
            // CanvasRenderer
            optionText.AddComponent<CanvasRenderer>();
            // RectTransform
            var optionTextRt = optionText.AddComponent<RectTransform>();
            optionTextRt.sizeDelta = new Vector2(0f, 0f); // this makes sense if you think about it
            optionTextRt.pivot = new Vector2(0.5f, 0.5f);
            optionTextRt.anchorMin = new Vector2(0f, 0f);
            optionTextRt.anchorMax = new Vector2(1f, 1f);
            optionTextRt.anchoredPosition = new Vector2(0f, 0f);
            // Text
            var optionTextText = optionText.AddComponent<Text>();
            optionTextText.font = MenuResources.TrajanRegular;
            optionTextText.fontSize = style.valueTextSize;
            optionTextText.resizeTextMaxSize = style.valueTextSize;
            optionTextText.alignment = TextAnchor.MiddleRight;
            optionTextText.text = config.label;
            optionTextText.supportRichText = true;
            // FixVerticalAlign
            optionText.AddComponent<FixVerticalAlign>();
            // Post Component Config
            menuOptionHorizontal.optionText = optionTextText;

            // LeftCursor object
            var cursorL = new GameObject("CursorLeft");
            GameObject.DontDestroyOnLoad(cursorL);
            cursorL.transform.SetParent(option.transform, false);
            // CanvasRenderer
            cursorL.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorLRt = cursorL.AddComponent<RectTransform>();
            cursorLRt.sizeDelta = new Vector2(164f, 119f);
            cursorLRt.pivot = new Vector2(0.5f, 0.5f);
            cursorLRt.anchorMin = new Vector2(0f, 0.5f);
            cursorLRt.anchorMax = new Vector2(0f, 0.5f);
            cursorLRt.anchoredPosition = new Vector2(-70f, 0f);
            cursorLRt.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // Animator
            var cursorLAnimator = cursorL.AddComponent<Animator>();
            cursorLAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorLAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorLAnimator.applyRootMotion = false;
            // Image
            cursorL.AddComponent<Image>();
            // Post Component Config
            menuOptionHorizontal.leftCursor = cursorLAnimator;

            // RightCursor object
            var cursorR = new GameObject("CursorRight");
            GameObject.DontDestroyOnLoad(cursorR);
            cursorR.transform.SetParent(option.transform, false);
            // CanvasRenderer
            cursorR.AddComponent<CanvasRenderer>();
            // RectTransform
            var cursorRRt = cursorR.AddComponent<RectTransform>();
            cursorRRt.sizeDelta = new Vector2(164f, 119f);
            cursorRRt.pivot = new Vector2(0.5f, 0.5f);
            cursorRRt.anchorMin = new Vector2(1f, 0.5f);
            cursorRRt.anchorMax = new Vector2(1f, 0.5f);
            cursorRRt.anchoredPosition = new Vector2(70f, 0f);
            cursorRRt.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
            // Animator
            var cursorRAnimator = cursorR.AddComponent<Animator>();
            cursorRAnimator.runtimeAnimatorController = MenuResources.MenuCursorAnimator;
            cursorRAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            cursorRAnimator.applyRootMotion = false;
            // Image
            cursorR.AddComponent<Image>();
            // Post Component Config
            menuOptionHorizontal.rightCursor = cursorRAnimator;

            // Description
            if (config.description is DescriptionInfo descInfo)
            {
                var descStyle = descInfo.style ?? DescriptionStyle.singleLineVanillaStyle;

                var description = new GameObject("Description");
                GameObject.DontDestroyOnLoad(description);
                description.transform.SetParent(option.transform, false);
                // CanvasRenderer
                cursorR.AddComponent<CanvasRenderer>();
                // RectTransform
                var rt = description.AddComponent<RectTransform>();
                RectTransformData.FromSizeAndPos(
                    new RelVector2(new RelLength(0, 1), descStyle.height),
                    new AnchoredPosition(new Vector2(0, 0), new Vector2(0, 1), new Vector2(60, 0))
                ).Apply(rt);
                // Animator
                var anim = description.AddComponent<Animator>();
                anim.runtimeAnimatorController = MenuResources.TextHideShowAnimator;
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                // Text
                var descText = description.AddComponent<Text>();
                descText.font = MenuResources.Perpetua;
                descText.fontSize = descStyle.textSize;
                descText.resizeTextMaxSize = descStyle.textSize;
                descText.alignment = TextAnchor.UpperLeft;
                descText.text = descInfo.text;
                descText.supportRichText = true;
                // Post Component Config
                menuOptionHorizontal.descriptionText = anim;
            }

            horizontalOption = menuOptionHorizontal;
            return content;
        }
    }

    namespace Config
    {
        /// <summary>
        /// Configuration options for creating a horizontal option.
        /// </summary>
        public struct HorizontalOptionConfig
        {
            /// <summary>
            /// The list of options to display.
            /// </summary>
            public string[] options;
            /// <summary>
            /// The displayed name of the option.
            /// </summary>
            public string label;
            /// <summary>
            /// The action to run when the menu setting is changed.
            /// </summary>
            public Patch.MenuSetting.ApplySetting applySetting;
            /// <summary>
            /// The action to run when loading the saved setting.
            /// </summary>
            public Patch.MenuSetting.RefreshSetting refreshSetting;
            /// <summary>
            /// The action to run when pressing the menu cancel key while selecting this item.
            /// </summary>
            public Action<MenuSelectable> cancelAction;
            /// <summary>
            /// The styling of the menu option.
            /// </summary>
            public HorizontalOptionStyle? style;
            /// <summary>
            /// The description of the option that gets displayed underneath.
            /// </summary>
            public DescriptionInfo? description;
        }

        /// <summary>
        /// The styling options for a horizontal option.
        /// </summary>
        public struct HorizontalOptionStyle
        {
            /// <summary>
            /// The style preset of a horizontal option in the vanilla game.
            /// </summary>
            public static readonly HorizontalOptionStyle vanillaStyle = new HorizontalOptionStyle
            {
                size = new RelVector2
                {
                    relative = new Vector2(),
                    delta = new Vector2(1000f, 60f)
                },
                labelTextSize = 46,
                valueTextSize = 46
            };

            /// <summary>
            /// The size of the main option.
            /// </summary>
            public RelVector2 size;
            /// <summary>
            /// The size of the text on the option label.
            /// </summary>
            public int labelTextSize;
            /// <summary>
            /// The size of the text on the option value.
            /// </summary>
            public int valueTextSize;
        }

        /// <summary>
        /// Configuration options for a horizontal option's description text.
        /// </summary>
        public struct DescriptionInfo
        {
            /// <summary>
            /// The text of the description.
            /// </summary>
            public string text;
            /// <summary>
            /// The styling of the description text.
            /// </summary>
            public DescriptionStyle? style;
        }

        /// <summary>
        /// The styling options of a horizontal option's description text
        /// </summary>
        public struct DescriptionStyle
        {
            /// <summary>
            /// The style preset of a single line description in the vanilla game.
            /// </summary>
            public static readonly DescriptionStyle singleLineVanillaStyle = new DescriptionStyle
            {
                textSize = 38,
                height = new RelLength(40),
            };
            /// <summary>
            /// The size of the text on the description.
            /// </summary>
            public int textSize;
            /// <summary>
            /// The height of the description text.
            /// </summary>
            public RelLength height;
        }
    }
}