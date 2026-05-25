using System.Collections;
using GlobalEnums;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626, 414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroController")]
    public class HeroController : global::HeroController
    {
        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_Attack))]
        extern private void Attack(AttackDirection attackDir);

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_SoulGain))]
        extern public void SoulGain();

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_LookForQueueInput))]
        extern private void LookForQueueInput();

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_TakeDamage))]
        extern public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType);

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_StartMPDrain))]
        extern public void StartMPDrain(float time);

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_Update))]
        extern private void Update();

        [MonoModIgnore]
        [Attributes.RawIlPatch(nameof(IlPatches.HeroController_DoAttack))]
        extern private void DoAttack();

        // il patch just dies trying to resolve types for no reason?
        public extern void orig_CharmUpdate();
        public void CharmUpdate()
        {
            orig_CharmUpdate();
            ModHooks.OnCharmUpdate(playerData, this);
            playerData.UpdateBlueHealth();
        }

        #region Dash()

        // This is the original dash vector calculating code used by the game
        // It is used to set the input dash velocity vector for the DashVectorHook
        [MonoModIgnore]
        private float BUMP_VELOCITY;

        [MonoModIgnore]
        private float BUMP_VELOCITY_DASH;

        [MonoModAdded]
        private Vector2 OrigDashVector()
        {
            Vector2 origVector;
            float velocity;
            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_16)) && this.cState.shadowDashing)
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
                    origVector = new Vector2(velocity, this.cState.onGround ? BUMP_VELOCITY : BUMP_VELOCITY_DASH);
                }
                else
                {
                    origVector = new Vector2(velocity, 0f);
                }
            }
            else if (this.CheckForBump(CollisionSide.left))
            {
                origVector = new Vector2(-velocity, this.cState.onGround ? BUMP_VELOCITY : BUMP_VELOCITY_DASH);
            }
            else
            {
                origVector = new Vector2(-velocity, 0f);
            }
            return origVector;
        }

        [MonoModIgnore]
        private float dash_timer;

        [MonoModIgnore]
        private extern void FinishedDashing();

        [MonoModIgnore]
        private Rigidbody2D rb2d;

        [MonoModReplace]
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
            vector = ModHooks.DashVelocityChange(vector);

            rb2d.linearVelocity = vector;
            dash_timer += Time.deltaTime;
        }

        #endregion
    }

    public static partial class IlPatches
    {
        [MonoModIgnore]
        public static void HeroController_Attack(ILContext il)
        {
            // remove the `this.pd.betaEnd` check at the end of the method
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(global::Modding.ModHooks.OnAttack);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>("slashComponent"),
                x => x.MatchCallOrCallvirt<global::NailSlash>(nameof(global::NailSlash.StartSlash))
            );
            var labelToJumpOverRet = cursor.DefineLabel();
            labelToJumpOverRet.Target = cursor.Next;
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate(global::Modding.ModHooks.AfterAttack);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroController), nameof(global::HeroController.cState)));
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroControllerStates), nameof(global::HeroControllerStates.attacking)));
            cursor.Emit(OpCodes.Brtrue_S, labelToJumpOverRet);
            cursor.Emit(OpCodes.Ret);
        }

        [MonoModIgnore]
        public static void HeroController_SoulGain(ILContext il)
        {
            // add a `num = Modding.ModHooks.OnSoulGain(num);` near the end of the method
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>(nameof(global::HeroController.playerData)),
                x => x.MatchLdloc(0),
                x => x.MatchCallOrCallvirt<global::PlayerData>(nameof(global::PlayerData.AddMPCharge))
            );
            cursor.Emit(OpCodes.Ldloc_0);
            cursor.EmitDelegate(global::Modding.ModHooks.OnSoulGain);
            cursor.Emit(OpCodes.Stloc_0);
        }

        [MonoModIgnore]
        public static void HeroController_LookForQueueInput(ILContext il)
        {
            // add a `&& !Modding.ModHooks.OnDashPressed()` to two if (...)
            ILCursor cursor = new ILCursor(il);

            ILLabel labelForFirstIf = cursor.DefineLabel();
            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>("inputHandler"),
                x => x.MatchLdfld<global::InputHandler>(nameof(global::InputHandler.inputActions)),
                x => x.MatchLdfld<global::HeroActions>(nameof(global::HeroActions.dash)),
                x => x.MatchCallOrCallvirt<global::InControl.OneAxisInputControl>("get_WasPressed"),
                x => x.MatchBrfalse(out labelForFirstIf)
            );
            cursor.EmitDelegate(global::Modding.ModHooks.OnDashPressed);
            cursor.Emit(OpCodes.Brtrue_S, labelForFirstIf);

            ILLabel labelForSecondIf = cursor.DefineLabel();
            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>("inputHandler"),
                x => x.MatchLdfld<global::InputHandler>(nameof(global::InputHandler.inputActions)),
                x => x.MatchLdfld<global::HeroActions>(nameof(global::HeroActions.dash)),
                x => x.MatchCallOrCallvirt<global::InControl.OneAxisInputControl>("get_IsPressed"),
                x => x.MatchBrfalse(out labelForSecondIf)
            );
            cursor.EmitDelegate(global::Modding.ModHooks.OnDashPressed);
            cursor.Emit(OpCodes.Brtrue_S, labelForSecondIf);
        }

        [MonoModIgnore]
        public static void HeroController_TakeDamage(ILContext il)
        {
            // add a `damageAmount = Modding.ModHooks.OnTakeDamage(ref hazardType, damageAmount);` at the start and a `damageAmount = Modding.ModHooks.AfterTakeDamage(hazardType, damageAmount);` somewhere in the middle
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.Emit(OpCodes.Ldarga_S, il.Method.Parameters[3]); // ref hazardType
            cursor.Emit(OpCodes.Ldarg_3); // damageAmount
            cursor.EmitDelegate(global::Modding.ModHooks.OnTakeDamage);
            cursor.Emit(OpCodes.Starg_S, il.Method.Parameters[2]); // damageAmount

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>(nameof(global::HeroController.takeNoDamage)),
                x => x.MatchBrtrue(out var _)
            );
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[3]); // hazardType
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[2]); // damageAmount
            cursor.EmitDelegate(global::Modding.ModHooks.AfterTakeDamage);
            cursor.Emit(OpCodes.Starg_S, il.Method.Parameters[2]); // damageAmount

            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdcI4(3),
                x => x.MatchBneUn(out var _)
            );  // to skip over some parts
            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<global::HeroController>(nameof(global::HeroController.takeNoDamage)),
                x => x.MatchBrtrue(out var _)
            );
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[3]); // hazardType
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[2]); // damageAmount
            cursor.EmitDelegate(global::Modding.ModHooks.AfterTakeDamage);
            cursor.Emit(OpCodes.Starg_S, il.Method.Parameters[2]); // damageAmount

            cursor.GotoNext
            (
                MoveType.After,
                x => x.MatchLdcI4(3),
                x => x.MatchBneUn(out var _)
            );
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[3]); // hazardType
            cursor.Emit(OpCodes.Ldarg_S, il.Method.Parameters[2]); // damageAmount
            cursor.EmitDelegate(global::Modding.ModHooks.AfterTakeDamage);
            cursor.Emit(OpCodes.Starg_S, il.Method.Parameters[2]); // damageAmount
        }

        [MonoModIgnore]
        public static void HeroController_StartMPDrain(ILContext il)
        {
            // add a `this.focusMP_amount *= Modding.ModHooks.OnFocusCost();` at the end
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext
            (
                MoveType.AfterLabel,
                x => x.MatchRet()
            );
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroController), "focusMP_amount"));
            cursor.EmitDelegate(global::Modding.ModHooks.OnFocusCost);
            cursor.Emit(OpCodes.Mul);
            cursor.Emit(OpCodes.Stfld, ReflectionHelper.GetFieldInfo(typeof(global::HeroController), "focusMP_amount"));
        }

        [MonoModIgnore]
        public static void HeroController_Update(ILContext il)
        {
            // add a `ModHooks.OnHeroUpdate();` at the start
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.EmitDelegate(global::Modding.ModHooks.OnHeroUpdate);
        }

        [MonoModIgnore]
        public static void HeroController_DoAttack(ILContext il)
        {
            // add a `ModHooks.OnDoAttack();` at the start
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.EmitDelegate(global::Modding.ModHooks.OnDoAttack);
        }
    }
}
