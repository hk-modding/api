using System.Collections;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0649
// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::StartManager")]
    public class StartManager : global::StartManager
    {
        [MonoModIgnore]
        private bool confirmedLanguage;

        [MonoModIgnore]
        private RuntimePlatform platform;

        [MonoModIgnore]
        private StandaloneLoadingSpinner loadSpinnerPrefab;

        [MonoModIgnore]
        private extern Sprite GetControllerSpriteForPlatform(RuntimePlatform runtimePlatform);

        [MonoModIgnore]
        private extern IEnumerator ShowLanguageSelect();

        [MonoModIgnore]
        private extern IEnumerator LanguageSettingDone();

        private IEnumerator Start()
        {
            controllerImage.sprite = GetControllerSpriteForPlatform(platform);
            AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu_Title");
            loadOperation.allowSceneActivation = false;
            bool showLanguageSelect = !CheckIsLanguageSet();
            if (showLanguageSelect && Platform.Current.ShowLanguageSelect)
            {
                yield return base.StartCoroutine(ShowLanguageSelect());
                while (!confirmedLanguage)
                {
                    yield return null;
                }

                yield return base.StartCoroutine(LanguageSettingDone());
            }

            startManagerAnimator.SetBool("WillShowControllerNotice", false);
            startManagerAnimator.SetBool("WillShowQuote", true);

            StandaloneLoadingSpinner loadSpinner = UnityEngine.Object.Instantiate<StandaloneLoadingSpinner>(loadSpinnerPrefab);
            loadSpinner.Setup(null);
            loadOperation.allowSceneActivation = true;
            yield return loadOperation;
            yield break;
        }
    }
}