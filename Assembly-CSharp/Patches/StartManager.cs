using System;
using System.Collections;
using System.Threading;
using MonoMod;
using UnityEngine;
using UObject = UnityEngine.Object;

// ReSharper disable All
#pragma warning disable 1591, CS0649
// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::StartManager")]
    public class StartManager : global::StartManager
    {
        private bool startedPreloading = false;

        private extern void orig_Awake();

        private void Awake()
        {
            if (ModLoader.LoadState == ModLoader.ModLoadState.NotStarted)
            {
                Logger.APILogger.Log("Main menu loading");
                startedPreloading = true;
                ModLoader.LoadState = ModLoader.ModLoadState.Started;

                GameObject obj = new GameObject();
                DontDestroyOnLoad(obj);

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                // NonBouncer does absolutely nothing, which makes it a good dummy to run the loader
                obj.AddComponent<NonBouncer>().StartCoroutine(ModLoader.LoadModsInit(obj));
            }
            else
            {
                // Debug log because this is the expected code path
                Logger.APILogger.LogDebug($"StartManager: Already begun mod loading (state {ModLoader.LoadState})");
            }

            orig_Awake();
        }

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

        [MonoModReplace]
        private IEnumerator Start()
        {
            this.controllerImage.sprite = this.GetControllerSpriteForPlatform(this.platform);

            AsyncOperation loadOperation = null;
            if (!startedPreloading)
            {
                loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu_Title");
                loadOperation.allowSceneActivation = false;
            }
            Platform.Current.SetSceneLoadState(true, false);
            bool showLanguageSelect = !this.CheckIsLanguageSet();
            if (showLanguageSelect && Platform.Current.ShowLanguageSelect)
            {
                yield return base.StartCoroutine(this.ShowLanguageSelect());
                while (!this.confirmedLanguage)
                {
                    yield return null;
                }

                yield return base.StartCoroutine(this.LanguageSettingDone());
            }
            TeamCherry.Localization.LanguageCode currentLanguage = Language.Language.CurrentLanguage();
            while (!Platform.Current.IsSharedDataMounted)
            {
                yield return null;
            }
            bool flag = false;
            string text;
            if (TeamCherry.Localization.LocalizationProjectSettings.TryGetSavedLanguageCode(out text))
            {
                TeamCherry.Localization.LanguageCode languageEnum = TeamCherry.Localization.LocalizationSettings.GetLanguageEnum(text);
                if (currentLanguage != languageEnum)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                Language.Language.LoadLanguage();
                ChangeFontByLanguage[] array = UObject.FindObjectsByType<ChangeFontByLanguage>(FindObjectsSortMode.None);
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].SetFont();
                }
                SetTextMeshProGameText[] componentsInChildren = base.GetComponentsInChildren<SetTextMeshProGameText>(true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].UpdateText();
                }
                LogoLanguage[] componentsInChildren2 = base.GetComponentsInChildren<LogoLanguage>(true);
                for (int i = 0; i < componentsInChildren2.Length; i++)
                {
                    componentsInChildren2[i].SetSprite();
                }
            }
            this.startManagerAnimator.SetBool("WillShowControllerNotice", false);
            this.startManagerAnimator.SetBool("WillShowQuote", true);

            StandaloneLoadingSpinner loadSpinner = UnityEngine.Object.Instantiate<StandaloneLoadingSpinner>(this.loadSpinnerPrefab);
            loadSpinner.Setup(null);
            bool didWaitForPlayerPrefs = false;
            while (!Platform.Current.IsPlayerPrefsLoaded)
            {
                if (!didWaitForPlayerPrefs)
                {
                    didWaitForPlayerPrefs = true;
                    Debug.LogFormat("Waiting for PlayerPrefs load...", Array.Empty<object>());
                }
                yield return null;
            }
            if (!didWaitForPlayerPrefs)
            {
                Debug.LogFormat("Didn't need to wait for PlayerPrefs load.", Array.Empty<object>());
            }
            else
            {
                Debug.LogFormat("Finished waiting for PlayerPrefs load.", Array.Empty<object>());
            }
            Platform.Current.SetSceneLoadState(true, true);
            if (!startedPreloading)
            {
                loadOperation.allowSceneActivation = true;
                yield return loadOperation;
            }
            yield break;
        }
    }
}