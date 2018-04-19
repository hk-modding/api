using MonoMod;
using UnityEngine;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0414
#pragma warning disable CS0649
namespace Modding.Patches
{
    [MonoModPatch( "global::EnemyDeathEffects" )]
    public class EnemyDeathEffects : global::EnemyDeathEffects
    {
        [MonoModIgnore]
        private bool didFire;

        public void orig_RecieveDeathEvent( float? attackDirection, bool resetDeathEvent = false, bool spellBurn = false, bool isWatery = false ) { }

        //Use this to hook into when an enemy dies. Check EnemyDeathEffects.didFire to prevent doing any actions on redundant invokes.
        public void RecieveDeathEvent( float? attackDirection, bool resetDeathEvent = false, bool spellBurn = false, bool isWatery = false )
        {
            ModHooks.Instance.OnRecieveDeathEvent( this, didFire, ref attackDirection, ref resetDeathEvent, ref spellBurn, ref isWatery );
            orig_RecieveDeathEvent( attackDirection, resetDeathEvent, spellBurn, isWatery );
        }

        [MonoModIgnore]
        private string playerDataName;

        private void orig_RecordKillForJournal() { }

        //Use this hook to get the journal name of the enemy that was just killed.
        private void RecordKillForJournal()
        {
            string boolName = "killed" + this.playerDataName;
            string intName = "kills" + this.playerDataName;
            string boolName2 = "newData" + this.playerDataName;
            ModHooks.Instance.OnRecordKillForJournal( this, playerDataName, boolName, intName, boolName2 );
            orig_RecordKillForJournal();
        }
    }
}
