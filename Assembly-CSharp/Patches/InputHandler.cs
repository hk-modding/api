using MonoMod;
using UnityEngine;

#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::InputHandler")]
    public class InputHandler : global::InputHandler
    {
        [MonoModIgnore]
        private bool isTitleScreenScene;

        [MonoModIgnore]
        private bool isMenuScene;

        [MonoModIgnore]
        private bool controllerPressed;

        [MonoModIgnore]
        private GameManager gm;

        [MonoModReplace]
        private void OnGUI()
        {
            if (this.isTitleScreenScene)
            {
                SetCursorVisible(false);
                return;
            }
            if (this.isMenuScene)
            {
                SetCursorVisible(!this.controllerPressed);
                return;
            }
            if (!this.gm.isPaused)
            {
                SetCursorVisible(false);
                return;
            }
            SetCursorVisible(!this.controllerPressed);
        }

        [MonoModIgnore]
        private extern void SetCursorVisible(bool value);
    }
}
