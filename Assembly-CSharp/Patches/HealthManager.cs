using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;

// ReSharper disable all
#pragma warning disable 1591, CS0108

namespace Modding.Patches
{
    [MonoModPatch( "global::HealthManager" )]
    public class HealthManager : global::HealthManager
    {
        [MonoModIgnore]
        [Attributes.IEnumeratorIlPatch(nameof(IlPatches.HealthManager_CheckPersistence))]
        extern protected IEnumerator CheckPersistence();
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void HealthManager_CheckPersistence(ILContext il, TypeDefinition stateMachineTypeDef)
        {
            // add a `isDead = ModHooks.OnEnableEnemy( gameObject, isDead );` before the `this.isDead` check
            ILCursor cursor = new ILCursor(il);

            // Insert a call to your custom method
            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchLdfld<global::HealthManager>("isDead")
            );
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Callvirt, ReflectionHelper.GetMethodInfo(typeof(global::HealthManager), "get_gameObject", true));
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HealthManager), "isDead", true));
            cursor.EmitDelegate(global::Modding.ModHooks.OnEnableEnemy);
            cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(global::HealthManager), "isDead", true));
        }
    }
}
