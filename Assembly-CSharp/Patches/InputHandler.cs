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

        // Reverted cursor behavior
        [MonoModReplace]
        private void OnGUI()
        {
            Cursor.lockState = CursorLockMode.None;
            if (isTitleScreenScene)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }

            if (!isMenuScene)
            {
                ModHooks.OnCursor(gm);
                return;
            }

            if (controllerPressed)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                return;
            }

            Cursor.visible = true;
        }
    }
}