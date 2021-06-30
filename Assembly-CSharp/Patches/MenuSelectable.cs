using System;
using MonoMod;
using UnityEngine.EventSystems;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("UnityEngine.UI.MenuSelectable")]
    public class MenuSelectable : UnityEngine.UI.MenuSelectable
    {
        public Action<MenuSelectable> customCancelAction { get; set; }

        public extern void orig_OnCancel(BaseEventData eventData);

        public void OnCancel(BaseEventData eventData)
        {
            if ((CancelAction)this.cancelAction == CancelAction.CustomCancelAction && this.customCancelAction != null)
            {
                this.ForceDeselect();
                customCancelAction(this);
                this.PlayCancelSound();
                return;
            }
            orig_OnCancel(eventData);
        }
    }

    public static class MenuSelectableExt
    {
        public static void SetDynamicMenuCancel(
            this UnityEngine.UI.MenuSelectable ms,
            MenuScreen to
        )
        {
            ms.cancelAction = (GlobalEnums.CancelAction)CancelAction.CustomCancelAction;
            (ms as MenuSelectable).customCancelAction = (self) =>
            {
                var uim = (UIManager)global::UIManager.instance;
                uim.StartMenuAnimationCoroutine(uim.GoToDynamicMenu(to));
            };
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
        CustomCancelAction
    }
}