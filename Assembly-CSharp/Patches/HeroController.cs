using GlobalEnums;
using MonoMod;
using UnityEngine;
//We disable a bunch of warnings here because they don't mean anything.  They all relate to not finding proper stuff for methods/properties/fields that are stubs to make the new methods work.
//We don't care about XML docs for these as they are being patched into the original code
// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626
namespace Modding.Patches
{


    [MonoModPatch("global::HeroController")]
    public partial class HeroController : global::HeroController
    {
        [MonoModIgnore] private NailSlash slashComponent;

        [MonoModIgnore] private float focusMP_amount;

        private void orig_StartMPDrain(float time)
        {
        }

        public void StartMPDrain(float time)
        {
            orig_StartMPDrain(time);
            focusMP_amount *= ModHooks.Instance.OnFocusCost();
        }
       
        private void orig_Update()
        {
        }

        private void Update()
        {
            ModHooks.Instance.OnHeroUpdate();
            orig_Update();
        }


        #region Dash()

        [MonoModIgnore]
        private float dash_timer;

        [MonoModIgnore]
        private extern void FinishedDashing();

        [MonoModIgnore] private Rigidbody2D rb2d;
        
        // This is the original dash vector calculating code used by the game
        // It is used to set the input dash velocity vector for the DashVectorHook
        private Vector2 OrigDashVector()
        {
            const float BUMP_VELOCITY = 4f;
            const float BUMP_VELOCITY_DASH = 5f;
            Vector2 origVector;

            float velocity;
            if (this.playerData.equippedCharm_16 && this.cState.shadowDashing)
            {
                velocity = this.DASH_SPEED_SHARP;
            }
            else
            {
                velocity = this.DASH_SPEED;
            }

            if (this.dashingDown)
            {
                origVector = new Vector2(0f, -velocity);
            }
            else if (this.cState.facingRight)
            {
                if (this.CheckForBump(CollisionSide.right))
                {
                    origVector = new Vector2(velocity,
                        (!this.cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY);
                }
                else
                {
                    origVector = new Vector2(velocity, 0f);
                }
            }
            else if (this.CheckForBump(CollisionSide.left))
            {
                origVector = new Vector2(-velocity,
                    (!this.cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY);
            }
            else
            {
                origVector = new Vector2(-velocity, 0f);
            }
            return origVector;
        }
        
        private void Dash()
        {
            AffectedByGravity(false);
            ResetHardLandingTimer();
            if (dash_timer > DASH_TIME)
            {
                FinishedDashing();
                return;
            }
            
            Vector2 vector = OrigDashVector();
            vector = ModHooks.Instance.DashVelocityChange(vector);
            
            rb2d.velocity = vector;
            dash_timer += Time.deltaTime;
        }
        
        
        

        #endregion

        #region CharmUpdate()

        private extern void orig_CharmUpdate();

        public void CharmUpdate()
        {
            orig_CharmUpdate();
            ModHooks.Instance.OnCharmUpdate();
            playerData.UpdateBlueHealth();
        }
        #endregion

        [MonoModIgnore]
        private extern void orig_DoAttack();

        public void DoAttack()
        {
            ModHooks.Instance.OnDoAttack();
            orig_DoAttack();
        }
    }
}
