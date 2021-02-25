using MonoMod;
using UnityEngine;
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
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

        //Reverted cursor behavior
        [MonoModReplace]
        private void OnGUI()
        {
            /*
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
            */


            if (isTitleScreenScene)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
                //Cursor.set_lockState(1);
                //Cursor.set_visible(true);
                return;


            }
            if (this.isMenuScene)
            {
                if (this.controllerPressed)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    //Cursor.set_lockState(1);
                    //Cursor.set_visible(false);
                    return;
                }
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                //Cursor.set_lockState(0);
                //Cursor.set_visible(true);
                return;
            }
            if (!GameManager.instance.isPaused)
            //if (!this.gm.isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                //Cursor.set_lockState(1);
                //Cursor.set_visible(false);
                return;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //Cursor.set_lockState(0);
            //Cursor.set_visible(true);

        }
    }
}
