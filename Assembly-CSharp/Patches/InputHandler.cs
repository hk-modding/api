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

        [MonoModIgnore]
        private extern void SetCursorVisible(bool value);

        // Reverted cursor behavior
        [MonoModReplace]
        private void OnGUI()
        {
            Cursor.lockState = CursorLockMode.None;
            if (this.isTitleScreenScene)
            {
                Cursor.visible = false;
                return;
            }
            if (this.isMenuScene)
            {
                Cursor.visible = !this.controllerPressed;
                return;
            }
            if (!this.gm.isPaused)
            {
                Cursor.visible = false;
                return;
            }
            Cursor.visible = !this.controllerPressed;
        }
    }
}