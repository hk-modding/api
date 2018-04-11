using HutongGames.PlayMaker;
using MonoMod;

// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches
{
    [MonoModPatch( "HutongGames.PlayMaker.Actions.GetAngleToTarget2D" )]
    public class GetAngleToTarget2D : HutongGames.PlayMaker.Actions.GetAngleToTarget2D
    {
        [MonoModIgnore]
        public FsmOwnerDefault gameObject;

        [MonoModIgnore]
        public FsmGameObject target;

        [MonoModOriginalName( "DoGetAngle" )]
        private void orig_DoGetAngle() { }

        //Added checks for null and an attempt to fix any missing references
        //as well as a try/catch in case something goes wrong to keep the whole FSM from breaking down...
        private void DoGetAngle()
        {
            try
            {
                if( target == null || target.Value == null )
                {
                    target = new HutongGames.PlayMaker.FsmGameObject( HeroController.instance?.proxyFSM.Fsm.GameObject );
                }

                if( gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null )
                {
                    if( gameObject == null )
                    {
                        gameObject = new HutongGames.PlayMaker.FsmOwnerDefault();
                        gameObject.OwnerOption = HutongGames.PlayMaker.OwnerDefaultOption.UseOwner;
                    }

                    gameObject.GameObject = new HutongGames.PlayMaker.FsmGameObject( Fsm.GameObject );
                }

                if( ( gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null ) || ( target == null || target.Value == null ) )
                {
                    base.Finish();
                    return;
                }

                orig_DoGetAngle();
            }
            catch( System.Exception ex )
            {
                Logger.LogError( "[API] - " + ex );
                base.Finish();
            }
        }
    }
}