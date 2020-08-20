using Modding.Menu;
using UnityEngine;
using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::UIManager")]
    public class UIManager : global::UIManager
    {
        [MonoModIgnore]
        private static UIManager _instance;

        public static UIManager get_instance()
        {
            if (UIManager._instance == null)
            {
                UIManager._instance = UnityEngine.Object.FindObjectOfType<UIManager>();

                if (UIManager._instance == null)
                {
                    return null;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(UIManager._instance.gameObject);
                }
            }

            return UIManager._instance;
        }

        public extern void orig_UIClosePauseMenu();

        public void UIClosePauseMenu()
        {
            if (FauxUIManager.Instance != null && ModManager.ModMenuScreen != null && ModManager.ModMenuScreen.isActiveAndEnabled)
            {
                FauxUIManager.Instance.UIquitModMenu(false);
            }

            orig_UIClosePauseMenu();
        }
    }
}