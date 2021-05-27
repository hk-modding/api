using UnityEngine;


namespace Modding.Menu
{
    /// <summary>
    /// Cached resources for the menu api to use
    /// </summary>
    public class MenuResources
    {
        // ReSharper disable CS1591
#pragma warning disable 1591

        public static Font TrajanRegular { get; private set; }
        public static Font TrajanBold { get; private set; }
        public static Font Perpetua { get; private set; }

        public static RuntimeAnimatorController MenuTopFleurAnimator { get; private set; }
        public static RuntimeAnimatorController MenuCursorAnimator { get; private set; }
        public static RuntimeAnimatorController MenuButtonFlashAnimator { get; private set; }
        public static AnimatorOverrideController TextHideShowAnimator { get; private set; }

        public static Sprite ScrollbarHandleSprite { get; private set; }
        public static Sprite ScrollbarBackgroundSprite { get; private set; }

        // ReSharper restore CS1591
#pragma warning restore 1591

        static MenuResources()
        {
            ReloadResources();
        }

        /// <summary>
        /// Reloads all resources, searching to find each one again.
        /// </summary>
        public static void ReloadResources()
        {
            foreach (var animator in Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>())
            {
                if (animator != null) switch (animator.name)
                    {
                        case "Menu Animate In Out":
                            MenuTopFleurAnimator = animator;
                            break;
                        case "Menu Fleur":
                            MenuCursorAnimator = animator;
                            break;
                        case "Menu Flash Effect":
                            MenuButtonFlashAnimator = animator;
                            break;
                    }
            }
            foreach (var animator in Resources.FindObjectsOfTypeAll<AnimatorOverrideController>())
            {
                if (animator != null) switch (animator.name)
                    {
                        case "TextHideShow":
                            TextHideShowAnimator = animator;
                            break;
                    }
            }
            foreach (var font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font != null) switch (font.name)
                    {
                        case "TrajanPro-Regular":
                            TrajanRegular = font;
                            break;
                        case "TrajanPro-Bold":
                            TrajanBold = font;
                            break;
                        case "Perpetua":
                            Perpetua = font;
                            break;
                    }
            }
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite != null) switch (sprite.name)
                    {
                        case "scrollbar_fleur_new":
                            ScrollbarHandleSprite = sprite;
                            break;
                        case "scrollbar_single":
                            ScrollbarBackgroundSprite = sprite;
                            break;
                    }
            }
        }
    }
}