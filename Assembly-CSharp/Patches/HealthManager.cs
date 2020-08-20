using System.Collections;
using MonoMod;

// ReSharper disable all
#pragma warning disable 1591, CS0108

namespace Modding.Patches
{
    [MonoModPatch( "global::HealthManager" )]
    public class HealthManager : global::HealthManager
    {
        [MonoModIgnore]
        public bool isDead;
        
        ///This may be used by mods to find new enemies. Check this isDead flag to see if they're already dead
        [MonoModReplace]
        protected IEnumerator CheckPersistence()
        {
            yield return null;
            //We insert the hook here because I think some enemys' FSMs need 1 frame to mark the "isDead" bool for things that it thinks should be dead.
            isDead = ModHooks.Instance.OnEnableEnemy( gameObject, isDead );
            if( this.isDead )
            {
                base.gameObject.SetActive( false );
            }
            yield break;
        }
    }
}
