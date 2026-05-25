using System.Collections;
using GlobalEnums;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626, 414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroAnimationController")]
    public class HeroAnimationController : global::HeroAnimationController
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroAnimationController_Update))]
        extern private void Update();
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void HeroAnimationController_Update(ILContext il)
        {
            // remove the `this.pd.betaEnd` check at the end of the method
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroAnimationController>("pd"),
                x => x.MatchLdstr(nameof(global::PlayerData.betaEnd)),
                x => x.MatchCallOrCallvirt<global::PlayerData>(nameof(global::PlayerData.GetBool)),
                x => x.MatchBrfalse(out ILLabel retLabel)
            );
            cursor.Emit(OpCodes.Ret);
            while (cursor.Next.OpCode != OpCodes.Ret)
            {
                cursor.Remove();
            }
            cursor.Remove();
        }
    }
}