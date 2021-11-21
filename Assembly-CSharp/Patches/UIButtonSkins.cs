// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using MonoMod;

namespace Modding.Patches
{
    [MonoModPatch("global::UIButtonSkins")]
    public class UIButtonSkins : global::UIButtonSkins
    {
        [MonoModIgnore]
        private extern ButtonSkin GetButtonSkinFor(string buttonName);
        [MonoModIgnore]
        private extern ButtonSkin orig_GetButtonSkinFor(InputControlType inputControlType);
        
        private InputHandler ih;

        public extern void orig_RefreshKeyMappings();
        public extern IEnumerator orig_ShowCurrentKeyboardMappings();
        public extern void orig_RefreshButtonMappings();
        public extern IEnumerator orig_ShowCurrentButtonMappings();
        public extern void orig_SetupRefs();

        private HashSet<MappableKey> customKeys = new HashSet<MappableKey>();
        public void AddMappableKey(MappableKey b) => this.customKeys.Add(b);
        public void RemoveMappableKey(MappableKey b) => this.customKeys.Remove(b);

        private HashSet<MappableControllerButton> customButtons = new HashSet<MappableControllerButton>();
        public void AddMappableControllerButton(MappableControllerButton b) => this.customButtons.Add(b);
        public void RemoveMappableControllerButton(MappableControllerButton b) => this.customButtons.Remove(b);

        [MonoModReplace]
        public ButtonSkin GetKeyboardSkinFor(PlayerAction action) => this.GetButtonSkinFor(
            action.GetKeyOrMouseBinding().ToString()
        );
        [MonoModReplace]
        public ButtonSkin GetControllerButtonSkinFor(PlayerAction action) => this.GetButtonSkinFor(
            action.GetControllerButtonBinding()
        );
        [MonoModReplace]
        public ButtonSkin GetButtonSkinFor(PlayerAction action) => this.ih.lastActiveController switch
        {
            BindingSourceType.None or BindingSourceType.KeyBindingSource or BindingSourceType.MouseBindingSource
                => GetKeyboardSkinFor(action),
            BindingSourceType.DeviceBindingSource => GetControllerButtonSkinFor(action),
            _ => null
        };

        public void RefreshKeyMappings()
        {
            if (this.customKeys != null) foreach (var k in this.customKeys)
                {
                    if (k == null) continue;
                    k.GetBinding();
                    k.ShowCurrentBinding();
                }
            orig_RefreshKeyMappings();
        }
        public IEnumerator ShowCurrentKeyboardMappings()
        {
            if (this.customKeys != null) foreach (var k in this.customKeys)
                {
                    if (k == null) continue;
                    k.GetBinding();
                    k.ShowCurrentBinding();
                    yield return null;
                }
            var enumerator = orig_ShowCurrentKeyboardMappings();
            while (enumerator.MoveNext()) yield return enumerator.Current;
            yield break;
        }
        public void RefreshButtonMappings()
        {
            if (this.customButtons != null) foreach (var k in this.customButtons)
                {
                    if (k == null) continue;
                    k.ShowCurrentBinding();
                }
            orig_RefreshButtonMappings();
        }
        public IEnumerator ShowCurrentButtonMappings()
        {
            if (this.customButtons != null) foreach (var k in this.customButtons)
                {
                    if (k == null) continue;
                    k.ShowCurrentBinding();
                    yield return null;
                }
            var enumerator = orig_ShowCurrentButtonMappings();
            while (enumerator.MoveNext()) yield return enumerator.Current;
            yield break;
        }
        
        private ButtonSkin GetButtonSkinFor(InputControlType inputControlType)
	    {
            if(this.ih == null){
                this.ih = (Modding.Patches.InputHandler)InputHandler.Instance;
            }
            return orig_GetButtonSkinFor(inputControlType);
        }

        private void SetupRefs()
        {
            this.customKeys = new HashSet<MappableKey>();
            this.customButtons = new HashSet<MappableControllerButton>();
            orig_SetupRefs();
        }
    }
}