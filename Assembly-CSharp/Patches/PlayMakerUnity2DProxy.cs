using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0108

namespace Modding.Patches
{
    [MonoModPatch("global::PlayMakerUnity2DProxy")]
    public class PlayMakerUnity2DProxy : global::PlayMakerUnity2DProxy
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.PlayMakerUnity2DProxy_Start))]
        extern public void Start();
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void PlayMakerUnity2DProxy_Start(ILContext il)
        {
            // add a `SetInt(nameof(healthBlue), GetInt(nameof(healthBlue)) + ModHooks.OnBlueHealth());` at the end
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<global::PlayMakerUnity2DProxy>(nameof(global::PlayMakerUnity2DProxy.RefreshImplementation))
            );
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(global::PlayMakerUnity2DProxy), "get_gameObject", true));
            cursor.EmitDelegate(global::Modding.ModHooks.OnColliderCreate);
        }
    }
}