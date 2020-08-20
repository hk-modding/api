using Modding.Menu;
using MonoMod;
using UnityEngine.EventSystems;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("UnityEngine.UI.MenuSelectable")]
    public class MenuSelectable : UnityEngine.UI.MenuSelectable
    {
        [MonoModIgnore]
        internal CancelAction cancelAction;

        public extern void orig_OnCancel(BaseEventData eventData);

        public void OnCancel(BaseEventData eventData)
        {
            if (cancelAction == CancelAction.QuitModMenu)
            {
                ForceDeselect();
                FauxUIManager.Instance.UIquitModMenu();
                PlayCancelSound();
                return;
            }

            orig_OnCancel(eventData);
        }
    }

    [MonoModPatch("GlobalEnums.CancelAction")]
    public enum CancelAction
    {
        DoNothing,
        GoToMainMenu,
        GoToOptionsMenu,
        GoToVideoMenu,
        GoToPauseMenu,
        LeaveOptionsMenu,
        GoToExitPrompt,
        GoToProfileMenu,
        GoToControllerMenu,
        ApplyRemapGamepadSettings,
        ApplyAudioSettings,
        ApplyVideoSettings,
        ApplyGameSettings,
        ApplyKeyboardSettings,
        QuitModMenu
    }
}