using HutongGames.PlayMaker;
using MonoMod;

// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches
{
    [MonoModPatch( "HutongGames.PlayMaker.Actions.FaceObject" )]
    public class FaceObject : HutongGames.PlayMaker.Actions.FaceObject
    {
        [MonoModIgnore]
        public FsmGameObject objectA;

        [MonoModIgnore]
        public FsmGameObject objectB;

        [MonoModOriginalName( "DoFace" )]
        private void orig_DoFace() { }

        //Added checks for null and an attempt to fix any missing references
        //as well as a try/catch in case something goes wrong to keep the whole FSM from breaking down...
        private void DoFace()
        {
            try
            {
                if( objectA == null || objectA.Value == null )
                {
                    objectA = new HutongGames.PlayMaker.FsmGameObject( Fsm.GameObject );
                }

                if( (objectB == null || objectB.Value == null) && HeroController.instance != null )
                {
                    objectB = new HutongGames.PlayMaker.FsmGameObject( HeroController.instance?.proxyFSM.Fsm.GameObject );
                }

                if( ( objectA == null || objectA.Value == null ) || ( objectB == null || objectA.Value == null ) )
                {
                    base.Finish();
                    return;
                }

                orig_DoFace();
            }
            catch( System.Exception ex )
            {
                Logger.LogError( "[API] - " + ex );
                base.Finish();
            }
        }
    }
}