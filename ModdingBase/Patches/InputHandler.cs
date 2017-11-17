using MonoMod;
using UnityEngine;

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

        //Reverted cursor behavior
        private void OnGUI()
        {
            Cursor.lockState = CursorLockMode.None;
            if (isTitleScreenScene)
            {
                Cursor.visible = false;
                return;
            }
            if (!isMenuScene)
            {
                ModHooks.Instance.OnCursor();
                return;
            }
            if (controllerPressed)
            {
                Cursor.visible = false;
                return;
            }
            Cursor.visible = true;
        }
    }
}
