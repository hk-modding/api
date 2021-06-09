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
        private extern ButtonSkin GetButtonSkinFor(InputControlType inputControlType);
        [MonoModIgnore]
        private InputHandler ih;

        public extern void orig_RefreshKeyMappings();
        public extern IEnumerator orig_ShowCurrentKeyboardMappings();
        public extern void orig_RefreshButtonMappings();
        public extern IEnumerator orig_ShowCurrentButtonMappings();
        public extern void orig_SetupRefs();

        private HashSet<MappableKey> customKeys = new();
        public void AddMappableKey(MappableKey b) => customKeys.Add(b);
        public void RemoveMappableKey(MappableKey b) => customKeys.Remove(b);

        private HashSet<MappableControllerButton> customButtons = new();
        public void AddMappableControllerButton(MappableControllerButton b) => customButtons.Add(b);
        public void RemoveMappableControllerButton(MappableControllerButton b) => customButtons.Remove(b);

        [MonoModReplace]
        public ButtonSkin GetKeyboardSkinFor(PlayerAction action) => GetButtonSkinFor(
            action.GetKeyOrMouseBinding().ToString()
        );
        [MonoModReplace]
        public ButtonSkin GetControllerButtonSkinFor(PlayerAction action) => GetButtonSkinFor(
            action.GetControllerButtonBinding()
        );
        [MonoModReplace]
        public ButtonSkin GetButtonSkinFor(PlayerAction action) => ih.lastActiveController switch
        {
            BindingSourceType.None | BindingSourceType.KeyBindingSource | BindingSourceType.MouseBindingSource
                => GetKeyboardSkinFor(action),
            BindingSourceType.DeviceBindingSource => GetControllerButtonSkinFor(action),
            _ => null
        };

        public void RefreshKeyMappings()
        {
            if (customKeys != null) foreach (var k in customKeys)
                {
                    if (k == null) continue;
                    k.GetBinding();
                    k.ShowCurrentBinding();
                }
            orig_RefreshKeyMappings();
        }
        public IEnumerator ShowCurrentKeyboardMappings()
        {
            if (customKeys != null) foreach (var k in customKeys)
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
            if (customButtons != null) foreach (var k in customButtons)
                {
                    if (k == null) continue;
                    k.ShowCurrentBinding();
                }
            orig_RefreshButtonMappings();
        }
        public IEnumerator ShowCurrentButtonMappings()
        {
            if (customButtons != null) foreach (var k in customButtons)
                {
                    if (k == null) continue;
                    k.ShowCurrentBinding();
                    yield return null;
                }
            var enumerator = orig_ShowCurrentButtonMappings();
            while (enumerator.MoveNext()) yield return enumerator.Current;
            yield break;
        }

        private void SetupRefs()
        {
            customKeys = new HashSet<MappableKey>();
            customButtons = new HashSet<MappableControllerButton>();
            orig_SetupRefs();
        }
    }
}