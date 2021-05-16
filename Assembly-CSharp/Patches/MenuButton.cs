using System;
using MonoMod;
using UnityEngine.EventSystems;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("UnityEngine.UI.MenuButton")]
    public class MenuButton : UnityEngine.UI.MenuButton
    {
        public Action<MenuButton> submitAction { get; set; }

        public extern void orig_OnSubmit(BaseEventData eventData);
        public void OnSubmit(BaseEventData eventData)
        {
            if (this.buttonType == (UnityEngine.UI.MenuButton.MenuButtonType)MenuButtonType.CustomSubmit)
            {
                submitAction?.Invoke(this);
            }
            orig_OnSubmit(eventData);
        }

        public enum MenuButtonType
        {
            Proceed = 0,
            Activate = 1,
            CustomSubmit,
        }
    }
}