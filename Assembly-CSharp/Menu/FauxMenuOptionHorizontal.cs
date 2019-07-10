using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MenuSelectable = Modding.Patches.MenuSelectable;

// ReSharper disable file UnusedMember.Local
// ReSharper disable file UnusedMember.Global

namespace Modding.Menu
{
    // by @KDT
    /// <summary>
    ///     Provides a simple toggle menu
    /// </summary>
    [PublicAPI]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class FauxMenuOptionHorizontal : MenuSelectable, IPointerClickHandler, IMoveHandler, ISubmitHandler
    {
        /// <summary>
        ///     Delegate for OnUpdate for when the settings for the menu option change.
        /// </summary>
        /// <param name="selectedOption"></param>
        public delegate void OnUpdateHandler(int selectedOption);

        private readonly SimpleLogger _log = new SimpleLogger("FauxMenuOptionHorizontal");

        /// <summary>
        /// </summary>
        private GameManager _gm;

        /// <summary>
        ///     Determines when the setting should change.
        /// </summary>
        [Header("Interaction")] public MenuOptionHorizontal.ApplyOnType ApplySettingOn;

        /// <summary>
        ///     Should text be localized
        /// </summary>
        [Header("Localization")] public bool LocalizeText;

        /// <summary>
        ///     Menu Options
        /// </summary>
        public string[] OptionList;

        /// <summary>
        ///     Option List Settings
        /// </summary>
        [Header("Option List Settings")] public Text OptionText;

        /// <summary>
        ///     Currently Selected Option Index
        /// </summary>
        public int SelectedOptionIndex;

        /// <summary>
        ///     Sheet Title
        /// </summary>
        public string SheetTitle;

        /// <summary>
        ///     Called when movement is detected (controller?)
        /// </summary>
        /// <param name="move"></param>
        public new void OnMove(AxisEventData move)
        {
            switch (move.moveDir)
            {
                case MoveDirection.Left:
                    DecrementOption();
                    uiAudioPlayer.PlaySlider();
                    break;
                case MoveDirection.Right:
                    IncrementOption();
                    uiAudioPlayer.PlaySlider();
                    break;
                default:
                    base.OnMove(move);
                    break;
            }
        }

        /// <summary>
        ///     Called when the mouse is clicked
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                IncrementOption();
                uiAudioPlayer.PlaySlider();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                DecrementOption();
                uiAudioPlayer.PlaySlider();
            }
        }

        /// <summary>
        ///     Called when the menu option has changed.  Passes in the newly selected option index
        /// </summary>
        public event OnUpdateHandler OnUpdate;

        private void UpdateSetting()
        {
            OnUpdate?.Invoke(SelectedOptionIndex);
        }

        private new void Awake()
        {
            _gm = GameManager.instance;
        }

        private new void OnEnable()
        {
            _gm.RefreshLanguageText += UpdateText;
            UpdateText();
        }

        private new void OnDisable()
        {
            _gm.RefreshLanguageText -= UpdateText;
        }

        /// <summary>
        ///     Sets the options
        /// </summary>
        /// <param name="optionList"></param>
        public void SetOptionList(string[] optionList)
        {
            OptionList = optionList;
        }

        /// <summary>
        ///     Get's the currently selected option's text, localized if applicable
        /// </summary>
        /// <returns></returns>
        public string GetSelectedOptionText()
        {
            return LocalizeText
                ? Language.Language.Get(OptionList[SelectedOptionIndex], SheetTitle)
                : OptionList[SelectedOptionIndex];
        }

        /// <summary>
        ///     Get's the currently selected option's text, never localized
        /// </summary>
        /// <returns></returns>
        public string GetSelectedOptionTextRaw()
        {
            return OptionList[SelectedOptionIndex];
        }

        /// <summary>
        ///     Set's the option
        /// </summary>
        /// <param name="optionNumber"></param>
        public virtual void SetOptionTo(int optionNumber)
        {
            if (optionNumber >= 0 && optionNumber < OptionList.Length)
            {
                SelectedOptionIndex = optionNumber;
                UpdateText();
            }
            else
            {
                _log.LogError(
                    $"{name} - Trying to select an option outside the list size (index: {optionNumber} listsize: {OptionList.Length})");
            }
        }

        /// <summary>
        ///     Updates the menu's option text.
        /// </summary>
        protected virtual void UpdateText()
        {
            if (OptionList == null || OptionText == null)
            {
                return;
            }

            try
            {
                OptionText.text = LocalizeText
                    ? Language.Language.Get(OptionList[SelectedOptionIndex], SheetTitle)
                    : OptionList[SelectedOptionIndex];
            }
            catch (Exception ex)
            {
                _log.LogError($"{OptionText.text} : {OptionList} : {SelectedOptionIndex} {ex}");
            }

            OptionText.GetComponent<FixVerticalAlign>().AlignText();
        }

        /// <summary>
        ///     Reduces the selected option's index by 1 (circling around at 0)
        /// </summary>
        protected void DecrementOption()
        {
            if (SelectedOptionIndex > 0)
            {
                SelectedOptionIndex--;
                if (ApplySettingOn == MenuOptionHorizontal.ApplyOnType.Scroll)
                {
                    UpdateSetting();
                }

                UpdateText();
            }
            else if (SelectedOptionIndex == 0)
            {
                SelectedOptionIndex = OptionList.Length - 1;
                if (ApplySettingOn == MenuOptionHorizontal.ApplyOnType.Scroll)
                {
                    UpdateSetting();
                }

                UpdateText();
            }
        }

        /// <summary>
        ///     Increases the selection option's index by 1 (circling around to 0 at the max option list index)
        /// </summary>
        protected void IncrementOption()
        {
            if (SelectedOptionIndex >= 0 && SelectedOptionIndex < OptionList.Length - 1)
            {
                SelectedOptionIndex++;
                if (ApplySettingOn == MenuOptionHorizontal.ApplyOnType.Scroll)
                {
                    UpdateSetting();
                }

                UpdateText();
            }
            else if (SelectedOptionIndex == OptionList.Length - 1)
            {
                SelectedOptionIndex = 0;
                if (ApplySettingOn == MenuOptionHorizontal.ApplyOnType.Scroll)
                {
                    UpdateSetting();
                }

                UpdateText();
            }
        }

        /// <summary>
        ///     Called when Enter/A is pressed
        /// </summary>
        /// <param name="eventData">The event data for the submit</param>
        public void OnSubmit(BaseEventData eventData)
        {
            IncrementOption();
            uiAudioPlayer.PlaySlider();
        }
    }
}