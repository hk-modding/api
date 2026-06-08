using System;
using System.Collections;
using System.Threading;
using MonoMod;
using UnityEngine;
using UObject = UnityEngine.Object;
using Lang = TeamCherry.Localization.Language;

// ReSharper disable All
#pragma warning disable 1591, CS0649
// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::StartManager")]
    public class StartManager : global::StartManager
    {
        private MonoBehaviour modLoaderObj = null;

        private extern void orig_Awake();
        private void Awake()
        {
            // i love working with self-contained libraries where one has to work around cyclic dependencies
            ReflectionHelper.SetField(typeof(TeamCherry.Localization.Language), "LanguageGet", ModHooks.LanguageGet);

            orig_Awake();

            if (ModLoader.LoadState == ModLoader.ModLoadState.NotStarted)
            {
                Logger.APILogger.Log("Main menu loading");
                ModLoader.LoadState = ModLoader.ModLoadState.Started;

                GameObject obj = new GameObject();
                DontDestroyOnLoad(obj);

                // Preload reflection
                new Thread(ReflectionHelper.PreloadCommonTypes).Start();

                // NonBouncer does absolutely nothing, which makes it a good dummy to run the loader
                modLoaderObj = obj.AddComponent<NonBouncer>();
            }
            else
            {
                // Debug log because this is the expected code path
                Logger.APILogger.LogDebug($"StartManager: Already begun mod loading (state {ModLoader.LoadState})");
            }
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

        // todo: make IL hook: seems trivial enough?
        [MonoModReplace]
        private IEnumerator Start()
        {
            this.controllerImage.sprite = this.GetControllerSpriteForPlatform(this.platform);
            // AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu_Title");
            // AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Quit_To_Menu");
            // loadOperation.allowSceneActivation = false;
            Platform.Current.SetSceneLoadState(true, false);
            if (!this.CheckIsLanguageSet() && Platform.Current.ShowLanguageSelect)
            {
                yield return base.StartCoroutine(this.ShowLanguageSelect());
                while (!this.confirmedLanguage)
                {
                    yield return null;
                }
                yield return base.StartCoroutine(this.LanguageSettingDone());
            }
            while (!Platform.Current.IsSharedDataMounted)
            {
                yield return null;
            }
            bool savedLanguageDifferentFromDefault = false;
            string savedSelectedLanguageCode;
            if (TeamCherry.Localization.LocalizationProjectSettings.TryGetSavedLanguageCode(out savedSelectedLanguageCode))
            {
                TeamCherry.Localization.LanguageCode languageEnum = TeamCherry.Localization.LocalizationSettings.GetLanguageEnum(savedSelectedLanguageCode);
                if (((TeamCherry.Localization.LanguageCode) Lang.CurrentLanguage()) != languageEnum)
                {
                    savedLanguageDifferentFromDefault = true;
                }
            }
            if (savedLanguageDifferentFromDefault)
            {
                Lang.LoadLanguage();
                ChangeFontByLanguage[] changeFontByLanguages = UObject.FindObjectsByType<ChangeFontByLanguage>(FindObjectsSortMode.None);
                for (int i = 0; i < changeFontByLanguages.Length; i++)
                {
                    changeFontByLanguages[i].SetFont();
                }
                SetTextMeshProGameText[] setTextMeshProGameTexts = base.GetComponentsInChildren<SetTextMeshProGameText>(true);
                for (int i = 0; i < setTextMeshProGameTexts.Length; i++)
                {
                    setTextMeshProGameTexts[i].UpdateText();
                }
                LogoLanguage[] logoLanguages = base.GetComponentsInChildren<LogoLanguage>(true);
                for (int i = 0; i < logoLanguages.Length; i++)
                {
                    logoLanguages[i].SetSprite();
                }
            }
            this.startManagerAnimator.SetBool("WillShowControllerNotice", false);
            this.startManagerAnimator.SetBool("WillShowQuote", true);
            /* ################################################################################################################################## */
            // this.startManagerAnimator.SetTrigger("Start");
            // int loadingIconNameHash = Animator.StringToHash("LoadingIcon");
            // while (this.startManagerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != loadingIconNameHash)
            // {
            //     yield return null;
            // }
            /* ################################################################################################################################## */
            UnityEngine.Object.Instantiate<StandaloneLoadingSpinner>(this.loadSpinnerPrefab).Setup(null);
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

            modLoaderObj.StartCoroutine(ModLoader.LoadModsInit(modLoaderObj.gameObject));
            //yield return ModLoader.LoadModsInit(modLoaderObj.gameObject);
            yield break;
        }
    }
}