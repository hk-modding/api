using MonoMod;
using UnityEngine;
using InControl;
using System.Collections.Generic;
using System;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("global::MappableControllerButton")]
    public class MappableControllerButton : global::MappableControllerButton
    {
        public PlayerActionSet actionSet;

        [MonoModIgnore]
        private PlayerAction playerAction;
        [MonoModIgnore]
        private InputControlType currentBinding;
        [MonoModIgnore]
        private UIButtonSkins uibs;
        [MonoModIgnore]
        private global::InputHandler ih;
        [MonoModIgnore]
        private global::GameManager gm;
        [MonoModIgnore]
        private global::UIManager ui;
        [MonoModIgnore]
        private GameSettings gs;
        [MonoModIgnore]
        private bool isListening;
        [MonoModIgnore]
        private List<DeviceBindingSource> unmappableButtons;

        private extern void orig_SetupUnmappableButtons();

        private void SetupUnmappableButtons()
        {
            if (this.actionSet != null)
            {
                unmappableButtons = new List<DeviceBindingSource>();
                unmappableButtons.Add(new DeviceBindingSource(InputControlType.Command));
                unmappableButtons.Add(new DeviceBindingSource(InputControlType.Options));
            }
            else
            {
                orig_SetupUnmappableButtons();
            }
        }
        public void InitCustomActions(PlayerActionSet actionSet, PlayerAction playerAction)
        {
            this.actionSet = actionSet;
            this.playerAction = playerAction;
        }

        private new void OnDestroy()
        {
            if (this.uibs != null) this.uibs.RemoveMappableControllerButton(this);
            base.OnDestroy();
        }

        public extern void orig_GetBinding();
        public extern void orig_SetupBindingListenOptions();
        public extern void orig_SetupRefs();

        
        public void GetBindingPublic(){
            GetBinding();
        }
        public void GetBinding()
        {
            this.SetupRefs();
            if (this.actionSet != null)
            {
                this.currentBinding = this.playerAction.GetControllerButtonBinding();
            }
            else
            {
                orig_GetBinding();
            }
        }

        private void SetupBindingListenOptions()
        {
            if (this.actionSet != null)
            {
                this.actionSet.ListenOptions = new BindingListenOptions
                {
                    IncludeControllers = true,
                    IncludeNonStandardControls = false,
                    IncludeMouseButtons = false,
                    IncludeKeys = false,
                    IncludeModifiersAsFirstClassKeys = false,
                    IncludeUnknownControllers = false,
                    MaxAllowedBindingsPerType = 1u,
                    OnBindingFound = this.OnBindingFound,
                    OnBindingAdded = this.OnBindingAdded,
                    OnBindingRejected = this.OnBindingRejected,
                    UnsetDuplicateBindingsOnSet = false
                };
            }
            else
            {
                orig_SetupBindingListenOptions();
            }
        }

        public extern void orig_ShowCurrentBinding();

        public void ShowCurrentBinding()
        {
            orig_ShowCurrentBinding();
            if (this.actionSet != null)
            {
                if(currentBinding != InputControlType.None && (buttonmapSprite.sprite == null || buttonmapSprite.sprite == uibs.blankKey)){
                    buttonmapText.text = Enum.GetName(typeof(InputControlType), currentBinding);
                }
            }
        }


        private void SetupRefs()
        {
            if (this.actionSet != null)
            {
                this.gm = GameManager.instance;
                this.ui = this.gm.ui;
                this.uibs = (UIButtonSkins)this.ui.uiButtonSkins;
                this.ih = this.gm.inputHandler;
                this.gs = this.gm.gameSettings;
                base.HookUpAudioPlayer();
                this.SetupUnmappableButtons();
                this.uibs.AddMappableControllerButton(this);
            }
            else
            {
                orig_SetupRefs();
            }
        }

        // The options here were to make it so aborting a rebind would revert to the original
        // or so it would just unset the bind.
        // I went with unsetting the bind because this is the current vanilla behaviour.
        // In the vanilla game, TC set the sprite back without actually changing the binding.
        [MonoModReplace]
        public void AbortRebind()
        {
            if(this.isListening)
            {
                // show the unbound key message
                this.ShowCurrentBinding();
                base.interactable = true;
                this.isListening = false;
            }
        }
    }
}