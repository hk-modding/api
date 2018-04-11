using HutongGames.PlayMaker;
using MonoMod;
using UnityEngine;

// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches
{
    [MonoModPatch( "HutongGames.PlayMaker.Actions.SetParticleEmissionRate" )]
    public class SetParticleEmissionRate : HutongGames.PlayMaker.Actions.SetParticleEmissionRate
    {
        [MonoModIgnore]
        public FsmOwnerDefault gameObject;

        [MonoModIgnore]
        private ParticleSystem emitter;

        [MonoModOriginalName( "OnEnter" )]
        void orig_OnEnter() { }

        [MonoModOriginalName( "DoSetEmitRate" )]
        void orig_DoSetEmitRate() { }

        //Added checks for null and an attempt to fix any missing references
        //as well as a try/catch in case something goes wrong to keep the whole FSM from breaking down...
        public override void OnEnter()
        {
            try
            {
                if( gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null )
                {
                    if( gameObject == null )
                    {
                        gameObject = new HutongGames.PlayMaker.FsmOwnerDefault();
                        gameObject.OwnerOption = HutongGames.PlayMaker.OwnerDefaultOption.UseOwner;
                    }

                    gameObject.GameObject = new HutongGames.PlayMaker.FsmGameObject( Fsm.GameObject );
                }

                if( ( gameObject == null || gameObject.GameObject == null || gameObject.GameObject.Value == null ) )
                {
                    base.Finish();
                    return;
                }

                GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
                ParticleSystem emitterTest = ownerDefaultTarget.GetComponent<ParticleSystem>();
                if( emitterTest == null )
                {
                    base.Finish();
                    return;
                }

                orig_OnEnter();
            }
            catch( System.Exception ex )
            {
                Logger.LogError( "[API] - " + ex );
                base.Finish();
            }
        }

        private void DoSetEmitRate()
        {
            if( emitter == null )
                return;

            orig_DoSetEmitRate();
        }
    }
}