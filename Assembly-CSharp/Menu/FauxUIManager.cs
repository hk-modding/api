using System.Collections;
using UnityEngine;

// ReSharper disable file UnusedMember.Global

namespace Modding.Menu
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides a menu UI manager
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class FauxUIManager : MonoBehaviour
    {
        private static GameManager _gm;

        public static FauxUIManager Instance;

        private readonly SimpleLogger _logger = new SimpleLogger("FauxUIManager");

        // ReSharper disable once InconsistentNaming
        private static GameManager gameManager => _gm != null ? _gm : _gm = GameManager.instance;

        public void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private IEnumerator ShowMenu(MenuScreen menu)
        {
            gameManager.inputHandler.StopUIInput();
            if (menu.screenCanvasGroup != null)
            {
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(menu.screenCanvasGroup));
            }

            if (menu.title != null)
            {
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(menu.title));
            }

            if (menu.topFleur != null)
            {
                yield return StartCoroutine(gameManager.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
                menu.topFleur.ResetTrigger("hide");
                menu.topFleur.SetTrigger("show");
            }

            yield return StartCoroutine(gameManager.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
            if (menu.content != null)
            {
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(menu.content));
            }

            if (menu.controls != null)
            {
                StartCoroutine(CanvasUtil.FadeInCanvasGroup(menu.controls));
            }

            if (menu.bottomFleur != null)
            {
                menu.bottomFleur.ResetTrigger("hide");
                menu.bottomFleur.SetTrigger("show");
            }

            yield return StartCoroutine(gameManager.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
            gameManager.inputHandler.StartUIInput();
            yield return null;
            menu.HighlightDefault();
        }

        private IEnumerator HideMenu(MenuScreen menu)
        {
            gameManager.inputHandler.StopUIInput();
            if (menu.title != null)
            {
                StartCoroutine(CanvasUtil.FadeOutCanvasGroup(menu.title));
                yield return StartCoroutine(gameManager.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
            }

            if (menu.topFleur != null)
            {
                menu.topFleur.ResetTrigger("show");
                menu.topFleur.SetTrigger("hide");
                yield return StartCoroutine(gameManager.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
            }

            if (menu.content != null)
            {
                StartCoroutine(CanvasUtil.FadeOutCanvasGroup(menu.content));
            }

            if (menu.controls != null)
            {
                StartCoroutine(CanvasUtil.FadeOutCanvasGroup(menu.controls));
            }

            if (menu.bottomFleur != null)
            {
                menu.bottomFleur.ResetTrigger("show");
                menu.bottomFleur.SetTrigger("hide");
                yield return StartCoroutine(_gm.timeTool.TimeScaleIndependentWaitForSeconds(0.1f));
            }

            if (menu.screenCanvasGroup != null)
            {
                yield return StartCoroutine(CanvasUtil.FadeOutCanvasGroup(menu.screenCanvasGroup));
            }

            gameManager.inputHandler.StartUIInput();
        }

        public void UIloadModMenu()
        {
            StartCoroutine(LoadModMenu());
        }

        public void UIquitModMenu(bool showOptions = true)
        {
            ModHooks.Instance.SaveGlobalSettings();
            StartCoroutine(QuitModMenu(showOptions));
        }

        private IEnumerator LoadModMenu()
        {
            _logger.Log("Loading Mod Menu");
            yield return StartCoroutine(HideMenu(UIManager.instance.optionsMenuScreen));
            yield return StartCoroutine(ShowMenu(ModManager.ModMenuScreen));
            gameManager.inputHandler.StartUIInput();
        }

        private IEnumerator QuitModMenu(bool showOptions)
        {
            _logger.Log("Quitting Mod Menu");
            yield return StartCoroutine(HideMenu(ModManager.ModMenuScreen));

            if (showOptions)
            {
                yield return StartCoroutine(ShowMenu(UIManager.instance.optionsMenuScreen));
                gameManager.inputHandler.StartUIInput();
            }
        }
    }
}