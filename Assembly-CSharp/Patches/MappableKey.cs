using MonoMod;
using UnityEngine;
using InControl;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("global::MappableKey")]
    public class MappableKey : global::MappableKey
    {
        public PlayerActionSet actionSet;

        [MonoModIgnore]
        private PlayerAction playerAction;
        [MonoModIgnore]
        private InputHandler.KeyOrMouseBinding currentBinding;
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
        private extern void SetupUnmappableKeys();

        public void InitCustomActions(PlayerActionSet actionSet, PlayerAction playerAction)
        {
            this.actionSet = actionSet;
            this.playerAction = playerAction;
        }

        private new void OnDestroy()
        {
            if (this.uibs != null) this.uibs.RemoveMappableKey(this);
            base.OnDestroy();
        }

        public extern void orig_GetBinding();
        public extern void orig_SetupBindingListenOptions();
        public extern void orig_SetupRefs();

        public void GetBinding()
        {
            this.SetupRefs();
            if (this.actionSet != null)
            {
                this.currentBinding = this.playerAction.GetKeyOrMouseBinding();
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
                    IncludeControllers = false,
                    IncludeNonStandardControls = false,
                    IncludeMouseButtons = true,
                    IncludeKeys = true,
                    IncludeModifiersAsFirstClassKeys = true,
                    IncludeUnknownControllers = false,
                    MaxAllowedBindingsPerType = 1,
                    OnBindingFound = this.OnBindingFound,
                    OnBindingAdded = this.OnBindingAdded,
                    OnBindingRejected = this.OnBindingRejected,
                    UnsetDuplicateBindingsOnSet = true
                };
            }
            else
            {
                orig_SetupBindingListenOptions();
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
                this.SetupUnmappableKeys();
                this.uibs.AddMappableKey(this);
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