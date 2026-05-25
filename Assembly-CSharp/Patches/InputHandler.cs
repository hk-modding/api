using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using UnityEngine;

#pragma warning disable 1591
#pragma warning disable CS0649

namespace Modding.Patches
{
    [MonoModPatch("global::InputHandler")]
    public class InputHandler : global::InputHandler
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.InputHandler_OnGUI))]
        extern private void OnGUI();
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void InputHandler_OnGUI(ILContext il)
        {
            // add a `Cursor.lockState = CursorLockMode.None;` before every (4) `ret`
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            while (cursor.TryGotoNext(MoveType.AfterLabel, x => x.MatchRet()))
            {
                cursor.Emit(OpCodes.Ldc_I4, CursorLockMode.None);
                cursor.Emit(OpCodes.Call, ReflectionHelper.GetMethodInfo(typeof(global::UnityEngine.Cursor), "set_lockState", false));
                cursor.GotoNext();
            }
        }
    }
}