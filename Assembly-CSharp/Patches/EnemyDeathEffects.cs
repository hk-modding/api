using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414
#pragma warning disable CS0649, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::EnemyDeathEffects")]
    public class EnemyDeathEffects : global::EnemyDeathEffects
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.RecieveDeathEvent))]
        extern public void RecieveDeathEvent(float? attackDirection, bool resetDeathEvent = false, bool spellBurn = false, bool isWatery = false);

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.RecordKillForJournal))]
        extern public static void RecordKillForJournal(string playerDataName);
    }

    [MonoModIgnore]
    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void RecieveDeathEvent(ILContext il)
        {
            // add a `ModHooks.OnRecieveDeathEvent(this, didFire, ref attackDirection, ref resetDeathEvent, ref spellBurn, ref isWatery);` at the start of the method
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdarg(0));

            // Insert a call to your custom method
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::EnemyDeathEffects), "didFire", true));
            cursor.Emit(OpCodes.Ldarga_S, il.Method.Parameters[0]); // attackDirection
            cursor.Emit(OpCodes.Ldarga_S, il.Method.Parameters[1]); // resetDeathEvent
            cursor.Emit(OpCodes.Ldarga_S, il.Method.Parameters[2]); // spellBurn
            cursor.Emit(OpCodes.Ldarga_S, il.Method.Parameters[3]); // isWatery
            cursor.EmitDelegate(global::Modding.ModHooks.OnRecieveDeathEvent);
        }

        [MonoModIgnore]
        public static void RecordKillForJournal(ILContext il)
        {
            // add a `ModHooks.OnRecordKillForJournal(this, this.playerDataName, $"killed{this.playerDataName}", $"kills{this.playerDataName}", $"newData{this.playerDataName}");` at the start of the method
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.AfterLabel, x => x.MatchLdcI4(0));

            // Insert a call to your custom method
            cursor.Emit(OpCodes.Ldarg_0);                                                                                         // this
            cursor.Emit(OpCodes.Ldarg_0);                                                                                         // this
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::EnemyDeathEffects), "playerDataName", true)); // .playerDataName
            cursor.Emit(OpCodes.Ldloc_1);                                                                                         // killed text
            cursor.Emit(OpCodes.Ldloc_2);                                                                                         // kills text
            cursor.Emit(OpCodes.Ldloc_3);                                                                                         // newData text
            cursor.EmitDelegate(global::Modding.ModHooks.OnRecordKillForJournal);
        }
    }
}