using GlobalEnums;
using MonoMod;
using UnityEngine;
// ReSharper disable All
//Sticking this here because right now, we're not sold on the source thing.  But i want to do this to make my life easier.
#pragma warning disable 1591, 0108, 0169, 0649, 0414
namespace Modding.Patches
{
    //These are flat out copied from the game's decompiled source.  We tried doing IL edits, but it was so complicated as to make it not worth it.  If there ever is an easy way to decompile a method, get it as c#, edit, and recompile in monomod, we can remove this.
    public partial class HeroController
    {

        #region Attack()
        [MonoModIgnore] private float attackDuration;
        [MonoModIgnore] private PlayMakerFSM slashFsm;
        [MonoModIgnore] private float altAttackTime;
        [MonoModIgnore] private bool wallSlashing;
        [MonoModIgnore] private GameObject grubberFlyBeam;
        [MonoModIgnore] private float MANTIS_CHARM_SCALE = 1.35f;
        [MonoModIgnore] private bool joniBeam;
        [MonoModIgnore] public NailSlash wallSlash;
        [MonoModIgnore] public NailSlash normalSlash;
        [MonoModIgnore] public NailSlash alternateSlash;
        [MonoModIgnore] public NailSlash upSlash;
        [MonoModIgnore] public NailSlash downSlash;

        [MonoModReplace]
        public void Attack(AttackDirection attackDir)
        {
            ModHooks.Instance.OnAttack(attackDir);//MOD API ADDED
            if (Time.timeSinceLevelLoad - this.altAttackTime > this.ALT_ATTACK_RESET)
            {
                this.cState.altAttack = false;
            }
            this.cState.attacking = true;
            if (this.playerData.equippedCharm_32)
            {
                this.attackDuration = this.ATTACK_DURATION_CH;
            }
            else
            {
                this.attackDuration = this.ATTACK_DURATION;
            }
            if (this.cState.wallSliding)
            {
                this.wallSlashing = true;
                this.slashComponent = this.wallSlash;
                this.slashFsm = this.wallSlashFsm;
            }
            else
            {
                this.wallSlashing = false;
                if (attackDir == AttackDirection.normal)
                {
                    if (!this.cState.altAttack)
                    {
                        this.slashComponent = this.normalSlash;
                        this.slashFsm = this.normalSlashFsm;
                        this.cState.altAttack = true;
                    }
                    else
                    {
                        this.slashComponent = this.alternateSlash;
                        this.slashFsm = this.alternateSlashFsm;
                        this.cState.altAttack = false;
                    }
                    if (this.playerData.equippedCharm_35)
                    {
                        if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.equippedCharm_27) || (this.joniBeam && this.playerData.equippedCharm_27))
                        {
                            if (this.transform.localScale.x < 0f)
                            {
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabR.Spawn(this.transform.position);
                            }
                            else
                            {
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabL.Spawn(this.transform.position);
                            }
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                            }
                            else
                            {
                                this.grubberFlyBeam.transform.SetScaleY(1f);
                            }
                        }
                        if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
                        {
                            if (this.transform.localScale.x < 0f)
                            {
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabR_fury.Spawn(this.transform.position);
                            }
                            else
                            {
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabL_fury.Spawn(this.transform.position);
                            }
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                            }
                            else
                            {
                                this.grubberFlyBeam.transform.SetScaleY(1f);
                            }
                        }
                    }
                }
                else if (attackDir == AttackDirection.upward)
                {
                    this.slashComponent = this.upSlash;
                    this.slashFsm = this.upSlashFsm;
                    this.cState.upAttacking = true;
                    if (this.playerData.equippedCharm_35)
                    {
                        if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.equippedCharm_27) || (this.joniBeam && this.playerData.equippedCharm_27))
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabU.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                            }
                        }
                        if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabU_fury.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                            }
                        }
                    }
                }
                else if (attackDir == AttackDirection.downward)
                {
                    this.slashComponent = this.downSlash;
                    this.slashFsm = this.downSlashFsm;
                    this.cState.downAttacking = true;
                    if (this.playerData.equippedCharm_35)
                    {
                        if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.equippedCharm_27) || (this.joniBeam && this.playerData.equippedCharm_27))
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabD.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                            }
                        }
                        if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabD_fury.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (this.playerData.equippedCharm_13)
                            {
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                            }
                        }
                    }
                }
            }
            if (this.cState.wallSliding)
            {
                if (this.cState.facingRight)
                {
                    this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
                }
                else
                {
                    this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
                }
            }
            else if (attackDir == AttackDirection.normal && this.cState.facingRight)
            {
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
            }
            else if (attackDir == AttackDirection.normal && !this.cState.facingRight)
            {
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
            }
            else if (attackDir == AttackDirection.upward)
            {
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 90f;
            }
            else if (attackDir == AttackDirection.downward)
            {
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
            }
            this.altAttackTime = Time.timeSinceLevelLoad;
            ModHooks.Instance.AfterAttack(attackDir); //MOD API - Added
            if (!this.cState.attacking) return;  //MOD API - Added
            this.slashComponent.StartSlash();
            if (this.playerData.equippedCharm_38)
            {
                this.fsm_orbitShield.SendEvent("SLASH");
            }
        }
        #endregion

        #region SoulGain

        [MonoModIgnore] private GameManager gm;

        [MonoModReplace]
        public void SoulGain()
        {
            int mpcharge = this.playerData.MPCharge;
            int num;
            if (mpcharge < this.playerData.maxMP)
            {
                num = 11;
                if (this.playerData.equippedCharm_20)
                {
                    num += 3;
                }
                if (this.playerData.equippedCharm_21)
                {
                    num += 8;
                }
            }
            else
            {
                num = 6;
                if (this.playerData.equippedCharm_20)
                {
                    num += 2;
                }
                if (this.playerData.equippedCharm_21)
                {
                    num += 6;
                }
            }
            int mpreserve = this.playerData.MPReserve;
            num = Modding.ModHooks.Instance.OnSoulGain(num);
            this.playerData.AddMPCharge(num);
            GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
            if (this.playerData.MPReserve != mpreserve)
            {
                this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
            }
        }

        #endregion

        #region LookForQueueInput

        [MonoModIgnore] private bool isGameplayScene;
        [MonoModIgnore] private InputHandler inputHandler;
        [MonoModIgnore] private extern bool CanWallJump();
        [MonoModIgnore] private extern bool CanJump();
        [MonoModIgnore] private extern bool CanDoubleJump();
        [MonoModIgnore] private extern bool CanInfiniteAirJump();
        [MonoModIgnore] private extern bool CanDash();
        [MonoModIgnore] private extern bool CanAttack();
        [MonoModIgnore] private extern void DoWallJump();
        [MonoModIgnore] private extern void HeroJump();
        [MonoModIgnore] private extern void DoDoubleJump();
        [MonoModIgnore] private extern void CancelJump();
        [MonoModIgnore] private extern void ResetLook();
        [MonoModIgnore] private extern void HeroDash();
        [MonoModIgnore] private extern bool CanSwim();
        [MonoModIgnore] private extern void SetState(ActorStates newState);
        [MonoModIgnore] private HeroAudioController audioCtrl;
        [MonoModIgnore] private int jumpQueueSteps;
        [MonoModIgnore] private int doubleJumpQueueSteps;
        [MonoModIgnore] private bool doubleJumpQueuing;
        [MonoModIgnore] private int jumpReleaseQueueSteps;
        [MonoModIgnore] private bool jumpReleaseQueuing;
        [MonoModIgnore] private bool jumpQueuing;
        [MonoModIgnore] private int dashQueueSteps;
        [MonoModIgnore] private bool dashQueuing;
        [MonoModIgnore] private int attackQueueSteps;
        [MonoModIgnore] private bool attackQueuing;
        [MonoModIgnore] private int JUMP_QUEUE_STEPS = 2;
        [MonoModIgnore] private int DOUBLE_JUMP_QUEUE_STEPS = 10;
        [MonoModIgnore] private int ATTACK_QUEUE_STEPS = 5;


        [MonoModReplace]
        private void LookForQueueInput()
        {
            if (this.acceptingInput && !this.gm.isPaused && this.isGameplayScene)
            {
                if (this.inputHandler.inputActions.jump.WasPressed)
                {
                    if (this.CanWallJump())
                    {
                        this.DoWallJump();
                    }
                    else if (this.CanJump())
                    {
                        this.HeroJump();
                    }
                    else if (this.CanDoubleJump())
                    {
                        this.DoDoubleJump();
                    }
                    else if (this.CanInfiniteAirJump())
                    {
                        this.CancelJump();
                        this.audioCtrl.PlaySound(HeroSounds.JUMP);
                        this.ResetLook();
                        this.cState.jumping = true;
                    }
                    else
                    {
                        this.jumpQueueSteps = 0;
                        this.jumpQueuing = true;
                        this.doubleJumpQueueSteps = 0;
                        this.doubleJumpQueuing = true;
                    }
                }
                if (this.inputHandler.inputActions.dash.WasPressed && !ModHooks.Instance.OnDashPressed())
                {
                    if (this.CanDash())
                    {
                        this.HeroDash();
                    }
                    else
                    {
                        this.dashQueueSteps = 0;
                        this.dashQueuing = true;
                    }
                }
                if (this.inputHandler.inputActions.attack.WasPressed)
                {
                    if (this.CanAttack())
                    {
                        this.DoAttack();
                    }
                    else
                    {
                        this.attackQueueSteps = 0;
                        this.attackQueuing = true;
                    }
                }
                if (this.inputHandler.inputActions.jump.IsPressed)
                {
                    if (this.jumpQueueSteps <= this.JUMP_QUEUE_STEPS && this.CanJump() && this.jumpQueuing)
                    {
                        this.HeroJump();
                    }
                    else if (this.doubleJumpQueueSteps <= this.DOUBLE_JUMP_QUEUE_STEPS && this.CanDoubleJump() && this.doubleJumpQueuing)
                    {
                        if (this.cState.onGround)
                        {
                            this.HeroJump();
                        }
                        else
                        {
                            this.DoDoubleJump();
                        }
                    }
                    if (this.CanSwim())
                    {
                        if (this.hero_state != ActorStates.airborne)
                        {
                            this.SetState(ActorStates.airborne);
                        }
                        this.cState.swimming = true;
                    }
                }
                if (this.inputHandler.inputActions.dash.IsPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && !ModHooks.Instance.OnDashPressed() && this.CanDash())
                {
                    this.HeroDash();
                }
                if (this.inputHandler.inputActions.attack.IsPressed && this.attackQueueSteps <= this.ATTACK_QUEUE_STEPS && this.CanAttack() && this.attackQueuing)
                {
                    this.DoAttack();
                }
            }
        }
        #endregion

    }
}
