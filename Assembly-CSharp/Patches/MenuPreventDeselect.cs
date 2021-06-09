using System;
using MonoMod;
using UnityEngine.EventSystems;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{

    [MonoModPatch("UnityEngine.UI.MenuPreventDeselect")]
    public class MenuPreventDeselect : UnityEngine.UI.MenuPreventDeselect
    {
        public Action<MenuPreventDeselect> customCancelAction { get; set; }

        [MonoModIgnore]
        private MenuAudioController uiAudioPlayer;

        public extern void orig_OnCancel(BaseEventData eventData);
        public void OnCancel(BaseEventData eventData)
        {
            if ((CancelAction)cancelAction == CancelAction.CustomCancelAction && customCancelAction != null)
            {
                ForceDeselect();
                customCancelAction(this);
                uiAudioPlayer.PlayCancel();
                return;
            }
            orig_OnCancel(eventData);
        }
    }
}