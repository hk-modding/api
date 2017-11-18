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

        /*DRMDONE: LookForQueueInput Changes:
         * change 
                if (this.inputHandler.inputActions.dash.WasPressed)
            to
      			if (this.inputHandler.inputActions.dash.WasPressed && !Modding.ModHooks.Instance.OnDashPressed())

            change 
                if (this.inputHandler.inputActions.dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && this.CanDash())
            to
            	if (this.inputHandler.inputActions.dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && !Modding.ModHooks.Instance.OnDashPressed() && this.CanDash())
        */

        //DRMDONE: SoulGain: Add num = Modding.ModHooks.Instance.OnSoulGain(num); before this.playerData.AddMPCharge(num); 

            /*
        [UpdateHeroControllerAttack]
        public void orig_Attack(AttackDirection attackDir) { }
        
        public void Attack(AttackDirection attackDir)
        {
            ModHooks.Instance.OnAttack(attackDir);
            orig_Attack(attackDir);
            //DRMDONE: Make sure to add before this.SlashComponent.StartSlash();
            //    Modding.ModHooks.Instance.AfterAttack(attackDir);
		    //    if (!this.cState.attacking) return; 
            //
        }*/

        private void orig_StartMPDrain(float time)
        {
        }

        public void StartMPDrain(float time)
        {
            orig_StartMPDrain(time);
            focusMP_amount *= ModHooks.Instance.OnFocusCost();
        }
       


        private void orig_TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
        }

        public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            damageAmount = ModHooks.Instance.OnTakeDamage(ref hazardType, damageAmount);
            orig_TakeDamage(go, damageSide, damageAmount, hazardType);
            //TODO: Add damageAmount = Modding.ModHooks.Instance.AfterTakeDamage(hazardType, damageAmount); before if (this.playerData.equippedCharm_5 && this.playerData.blockerHits > 0 && hazardType == 1 && this.cState.focusing && !flag)
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

        private extern void orig_Dash();

        private void Dash()
        {
            AffectedByGravity(false);
            ResetHardLandingTimer();
            if (dash_timer > DASH_TIME)
            {
                FinishedDashing();
                return;
            }

            // Check if we run our own dash code.
            Vector2? vector = ModHooks.Instance.DashVelocityChange();
            if (vector == null)
            {
                // Run the original dash code.
                orig_Dash();
                return;
            }
            rb2d.velocity = vector.Value;
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

        //Note, moved the OnDoAttack call a little to make it easier to merge.
        //Done: DoAttack: Add Modding.ModHooks.Instance.OnDoAttack(); after this.cState.recoiling=false; 
        //[MonoModIgnore]
        //[UpdateHeroControllerDoAttack]
        //public void DoAttack() { }
    }

    //[MonoModCustomAttribute("UpdateHeroController_CharmUpdate")]
    //public class UpdateHeroControllerCharmUpdate : Attribute { }

    //[MonoModCustomAttribute("UpdateHeroController_Attack")]
    //public class UpdateHeroControllerAttack : Attribute { }

    //[MonoModCustomAttribute("UpdateHeroController_DoAttack")]
    //public class UpdateHeroControllerDoAttack : Attribute { }
}

namespace MonoMod { 
    
    public static partial class MonoModRules
    {

        /*
        /// <summary>
        /// This method rewrites the CharmUpdate function.  It alters it to injects Modding.ModHooks.Instance.OnCharmUpdate() right before PlayerData.Instance.UpdateBlueHealth();
        /// </summary>
        /// <param name="method"></param>
        /// <param name="attrib"></param>
        
        public static void UpdateHeroController_CharmUpdate(MethodDefinition method, CustomAttribute attrib)
        {
            try
            {
                Console.WriteLine("Modifying IL code for " + method.DeclaringType.Name + "." + method.Name);

                // The method must have a body, otherwise there's nothing to replace!
                if (!method.HasBody)
                return;
            
                ILProcessor ilProcessor = method.Body.GetILProcessor();
                FieldDefinition careFreeShield = null;

                foreach (FieldDefinition field in method.DeclaringType.Fields)
                {
                    if (field.Name == "carefreeShieldEquipped")
                        careFreeShield = field;
                }

                if (careFreeShield == null)
                {
                    Console.WriteLine("WARNING: Couldn't Patch HeroController.CharmUpdate because carefreeShieldEquipped couldn't be found on HeroController");
                }

                MethodDefinition def = ModHooksInstance(method);

                Instruction arg = ilProcessor.Create(OpCodes.Ldarg_0);
                List<Instruction> instructionsToAdd = new List<Instruction>
                {
                    arg,
                    ilProcessor.Create(OpCodes.Ldc_I4_0),
                    ilProcessor.Create(OpCodes.Stfld, careFreeShield),
                    ilProcessor.Create(OpCodes.Call, def),
                    ilProcessor.Create(OpCodes.Callvirt, ModHooksHook(method, "OnCharmUpdate")),
                    ilProcessor.Create(OpCodes.Ldarg_0),
                    ilProcessor.Create(OpCodes.Ldfld, GetClassField(method, "playerData")),
                    ilProcessor.Create(OpCodes.Callvirt, GetMethodDefinition(method, "PlayerData", "UpdateBlueHealth")),
                    ilProcessor.Create(OpCodes.Ret)
                };

                instructionsToAdd.Insert(0, ilProcessor.Create(OpCodes.Br_S, instructionsToAdd[3]));


                List<Instruction> instructionsToRemove = new List<Instruction>();

                bool deleteRest = false;
//                Console.WriteLine(careFreeShieldEquipped.Operand.ToString());
                // Iterate through the method body.
                foreach (Instruction instr in method.Body.Instructions)
                {
                    //Debug
                     // if (instr.Operand != null)
                     //   Console.WriteLine(instr.OpCode.ToString() + "=> " + instr.Operand.ToString());

                    if (deleteRest)
                    {
                        instructionsToRemove.Add(instr);
                    }

                    // Check if the instruction is a ldstr instruction (load constant string literal).
                    if (instr.OpCode == OpCodes.Stfld && instr.Operand?.ToString() == "System.Boolean HeroController::carefreeShieldEquipped")
                    {
                        deleteRest = true;
                    }
                }

                //Console.WriteLine("number of instructions to remove:" +instructionsToRemove.Count);
                foreach (Instruction instr in instructionsToRemove)
                    method.Body.Instructions.Remove(instr);

                foreach (Instruction instr in instructionsToAdd)
                    method.Body.Instructions.Add(instr);


                //Note that when we removed the instructions before adding new ones, we broke the internal reference on where the if command should go after it's done.  this restores that reference (otherwise you end up with a do/while loop.)
                bool fixNext = false;
                foreach (Instruction instr in method.Body.Instructions)
                {
                    if (instr.Operand != null &&
                        instr.Operand?.ToString() == "System.Boolean PlayerData::equippedCharm_40")
                        fixNext = true;

                    if (fixNext && (instr.OpCode == OpCodes.Brfalse_S || instr.OpCode == OpCodes.Bne_Un_S))
                    {
                        instr.Operand = arg;
                    }
                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        */

        /*
    /// <summary>
    /// This method rewrites the Attack function.  It alters it to injects Modding.ModHooks.Instance.AfterAttack(AttackDirection) and a check to return if attacking is false right before this.slashComponent.StartSlash();
    /// </summary>
    /// <param name="method"></param>
    /// <param name="attrib"></param>
    public static void UpdateHeroController_Attack(MethodDefinition method, CustomAttribute attrib)
    {
        try
        {
            // The method must have a body, otherwise there's nothing to replace!
            if (!method.HasBody)
                return;

            Console.WriteLine("Modifying IL code for " + method.DeclaringType.Name + "." + method.Name);

            ILProcessor ilProcessor = method.Body.GetILProcessor();

            MethodDefinition def = ModHooksInstance(method);

            // Iterate through the method body. Need to figure out where to start the injection
            Instruction temp = null;
            foreach (Instruction instr in method.Body.Instructions)
            {
                // Check if the instruction is a ldstr instruction (load constant string literal).
                if (instr.OpCode == OpCodes.Stfld && instr.Operand?.ToString() == "System.float32 HeroController::altAttackTime")
                {
                    temp = instr;
                    break;
                }
            }

            if (temp != null)
            {
                Instruction ldarg_0 = ilProcessor.Create(OpCodes.Ldarg_0);

                List<Instruction> instructionsToAdd = new List<Instruction>()
                {
                    ilProcessor.Create(OpCodes.Call, def),
                    ilProcessor.Create(OpCodes.Ldarg_1),
                    ilProcessor.Create(OpCodes.Callvirt, ModHooksHook(method, "AfterAttack")),
                    ilProcessor.Create(OpCodes.Ldarg_0),
                    ilProcessor.Create(OpCodes.Ldfld, GetClassField(method, "cstate")),
                    ilProcessor.Create(OpCodes.Ldfld, GetClassField(method, "HeroControllerStates", "attacking")),
                    ilProcessor.Create(OpCodes.Ldfld, GetClassField(method, "cstate")),
                    ilProcessor.Create(OpCodes.Brtrue_S, ldarg_0),
                    ilProcessor.Create(OpCodes.Ret),
                    ldarg_0
                };

                Instruction last = temp;
                while (instructionsToAdd.Any())
                {
                    Instruction next = instructionsToAdd[0];
                    instructionsToAdd.RemoveAt(0);
                    ilProcessor.InsertAfter(last, next);
                    last = next;
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    */

        /*
    /// <summary>
    /// This method rewrites the DoAttack function.  It alters it to injects Modding.ModHooks.Instance.DoAttack() after this.cstate.recoiling = false;
    /// </summary>
    /// <param name="method"></param>
    /// <param name="attrib"></param>
    public static void UpdateHeroController_DoAttack(MethodDefinition method, CustomAttribute attrib)
    {
        try
        {
            // The method must have a body, otherwise there's nothing to replace!
            if (!method.HasBody)
                return;

            Console.WriteLine("Modifying IL code for " + method.DeclaringType.Name + "." + method.Name);

            ILProcessor ilProcessor = method.Body.GetILProcessor();

            MethodDefinition def = ModHooksInstance(method);

            // Iterate through the method body. Need to figure out where to start the injection
            Instruction temp = null;
            foreach (Instruction instr in method.Body.Instructions)
            {
                // Check if the instruction is a ldstr instruction (load constant string literal).
                if (instr.OpCode == OpCodes.Stfld && instr.Operand?.ToString() == "System.Boolean HeroControllerStates::recoiling")
                {
                    temp = instr;
                    break;
                }
            }

            if (temp != null)
            {
                Instruction ldarg_0 = ilProcessor.Create(OpCodes.Ldarg_0);

                List<Instruction> instructionsToAdd = new List<Instruction>()
                {
                    ilProcessor.Create(OpCodes.Call, def),
                    ilProcessor.Create(OpCodes.Callvirt, ModHooksHook(method, "OnDoAttack")),
                    ilProcessor.Create(OpCodes.Ldarg_0)
                };

                Instruction last = temp;
                while (instructionsToAdd.Any())
                {
                    Instruction next = instructionsToAdd[0];
                    instructionsToAdd.RemoveAt(0);
                    ilProcessor.InsertAfter(last, next);
                    last = next;
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    */
    }
}
