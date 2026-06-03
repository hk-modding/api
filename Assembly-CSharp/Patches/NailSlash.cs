using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::NailSlash")]
    public class NailSlash : global::NailSlash
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.NailSlash_OnTriggerEnter2D))]
        extern private void OnTriggerEnter2D(Collider2D otherCollider);
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void NailSlash_OnTriggerEnter2D(ILContext il)
        {
            // add a `ModHooks.OnSlashHit(otherCollider, gameObject);` at the start
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchRet()
            );
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(global::NailSlash), "get_gameObject", true));
            cursor.EmitDelegate(global::Modding.ModHooks.OnSlashHit);
        }
    }
}