using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using MonoMod;
using UnityEngine;

namespace Modding.Patches
{
    [MonoModPatch("global::HeroController")]
    public class HeroController : global::HeroController
    {
        [MonoModIgnore]
        private NailSlash slashComponent;

        [MonoModIgnore]
        private float focusMP_amount;

        //TODO: CharmUpdate : Add Modding.ModHooks.Instance.OnCharmUpdate(); before this.playerData.UpdateBlueHealth()
        /*TODO: Dash() before float num; then end the else block right before this.dash_timer += Time.deltaTime;
        		Vector2 vector = Modding.ModHooks.Instance.DashVelocityChange();
		        if (vector.x != 0f || vector.y != 0f)
		        {
			        this.rb2d.velocity = vector;
		        }
		        else
		        {
        */
        //TODO: DoAttack: Add Modding.ModHooks.Instance.OnDoAttack(); after this.cState.recoiling=false;
        /*TODO: LookForQueueInput Changes:
         * change 
                if (this.inputHandler.inputActions.dash.WasPressed)
            to
      			if (this.inputHandler.inputActions.dash.WasPressed && !Modding.ModHooks.Instance.OnDashPressed())

            change 
                if (this.inputHandler.inputActions.dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && this.CanDash())
            to
            	if (this.inputHandler.inputActions.dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && !Modding.ModHooks.Instance.OnDashPressed() && this.CanDash())
        */

        //TODO: SoulGain: Add num = Modding.ModHooks.Instance.OnSoulGain(num); before this.playerData.AddMPCharge(num); 

        public void orig_Attack(AttackDirection attackDir) { }

        public void Attack(AttackDirection attackDir)
        {
            ModHooks.Instance.OnAttack(attackDir);
            orig_Attack(attackDir);
            /*TODO: Make sure to add before this.SlashComponent.StartSlash();
                Modding.ModHooks.Instance.AfterAttack(attackDir);
		        if (!this.cState.attacking) return; 
            */
        }

        private void orig_StartMPDrain(float time) { }

        public void StartMPDrain(float time)
        {
            orig_StartMPDrain(time);
            focusMP_amount *= ModHooks.Instance.OnFocusCost();
        }

        private void orig_TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType) { }

        public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            damageAmount = ModHooks.Instance.OnTakeDamage(ref hazardType, damageAmount);
            orig_TakeDamage(go, damageSide, damageAmount, hazardType);
            //TODO: Add damageAmount = Modding.ModHooks.Instance.AfterTakeDamage(hazardType, damageAmount); before if (this.playerData.equippedCharm_5 && this.playerData.blockerHits > 0 && hazardType == 1 && this.cState.focusing && !flag)
        }

        private void orig_Update() { }

        private void Update()
        {
            ModHooks.Instance.OnHeroUpdate();
            orig_Update();
        }
    }
}
