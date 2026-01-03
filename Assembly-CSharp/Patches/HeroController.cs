using System.Collections;
using GlobalEnums;
using MonoMod;
using UnityEngine;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626, 414, CS0626

namespace Modding.Patches
{
    [MonoModPatch("global::HeroController")]
    public class HeroController : global::HeroController
    {
        #region Attack()

        [MonoModIgnore]
        private float attackDuration;

        [MonoModIgnore]
        private PlayMakerFSM slashFsm;

        [MonoModIgnore]
        private float altAttackTime;

        [MonoModIgnore]
        private bool wallSlashing;

        [MonoModIgnore]
        private GameObject grubberFlyBeam;

        [MonoModIgnore]
        private float MANTIS_CHARM_SCALE = 1.35f;

        [MonoModIgnore]
        private bool joniBeam;

        [MonoModIgnore]
        public NailSlash wallSlash;

        [MonoModIgnore]
        public NailSlash normalSlash;

        [MonoModIgnore]
        public NailSlash alternateSlash;

        [MonoModIgnore]
        public NailSlash upSlash;

        [MonoModIgnore]
        public NailSlash downSlash;

        [MonoModReplace]
        public void Attack(AttackDirection attackDir)
        {
            ModHooks.OnAttack(attackDir); //MOD API ADDED
            if (Time.timeSinceLevelLoad - this.altAttackTime > this.ALT_ATTACK_RESET)
                this.cState.altAttack = false;
            this.cState.attacking = true;
            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_32)))
                this.attackDuration = this.ATTACK_DURATION_CH;
            else
                this.attackDuration = this.ATTACK_DURATION;
            if (this.cState.wallSliding)
            {
                this.wallSlashing = true;
                this.slashComponent = this.wallSlash;
                this.slashFsm = this.wallSlashFsm;
                if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_35)))
                {
                    if ((this.playerData.GetInt(nameof(PlayerData.health)) == this.playerData.CurrentMaxHealth && !this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))) || (this.joniBeam && this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))))
                    {
                        if (this.transform.localScale.x > 0f)
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabR.Spawn(this.transform.position);
                        else
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabL.Spawn(this.transform.position);
                        if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                            this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                        else
                            this.grubberFlyBeam.transform.SetScaleY(1f);
                    }
                    if (this.playerData.GetInt(nameof(PlayerData.health)) == 1 && this.playerData.GetBool(nameof(PlayerData.equippedCharm_6)) && this.playerData.GetInt(nameof(PlayerData.healthBlue)) < 1)
                    {
                        if (this.transform.localScale.x > 0f)
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabR_fury.Spawn(this.transform.position);
                        else
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabL_fury.Spawn(this.transform.position);
                        if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                            this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                        else
                            this.grubberFlyBeam.transform.SetScaleY(1f);
                    }
                }
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
                    if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_35)))
                    {
                        if ((this.playerData.GetInt(nameof(PlayerData.health)) >= this.playerData.CurrentMaxHealth && !this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))) || (this.joniBeam && this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))))
                        {
                            if (this.transform.localScale.x < 0f)
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabR.Spawn(this.transform.position);
                            else
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabL.Spawn(this.transform.position);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                            else
                                this.grubberFlyBeam.transform.SetScaleY(1f);
                        }
                        if (this.playerData.GetInt(nameof(PlayerData.health)) == 1 && this.playerData.GetBool(nameof(PlayerData.equippedCharm_6)) && this.playerData.GetInt(nameof(PlayerData.healthBlue)) < 1)
                        {
                            if (this.transform.localScale.x < 0f)
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabR_fury.Spawn(this.transform.position);
                            else
                                this.grubberFlyBeam = this.grubberFlyBeamPrefabL_fury.Spawn(this.transform.position);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
                            else
                                this.grubberFlyBeam.transform.SetScaleY(1f);
                        }
                    }
                }
                else if (attackDir == AttackDirection.upward)
                {
                    this.slashComponent = this.upSlash;
                    this.slashFsm = this.upSlashFsm;
                    this.cState.upAttacking = true;
                    if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_35)))
                    {
                        if ((this.playerData.GetInt(nameof(PlayerData.health)) >= this.playerData.CurrentMaxHealth && !this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))) || (this.joniBeam && this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))))
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabU.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                        }
                        if (this.playerData.GetInt(nameof(PlayerData.health)) == 1 && this.playerData.GetBool(nameof(PlayerData.equippedCharm_6)) && this.playerData.GetInt(nameof(PlayerData.healthBlue)) < 1)
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabU_fury.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                        }
                    }
                }
                else if (attackDir == AttackDirection.downward)
                {
                    this.slashComponent = this.downSlash;
                    this.slashFsm = this.downSlashFsm;
                    this.cState.downAttacking = true;
                    if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_35)))
                    {
                        if ((this.playerData.GetInt(nameof(PlayerData.health)) >= this.playerData.CurrentMaxHealth && !this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))) || (this.joniBeam && this.playerData.GetBool(nameof(PlayerData.equippedCharm_27))))
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabD.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                        }
                        if (this.playerData.GetInt(nameof(PlayerData.health)) == 1 && this.playerData.GetBool(nameof(PlayerData.equippedCharm_6)) && this.playerData.GetInt(nameof(PlayerData.healthBlue)) < 1)
                        {
                            this.grubberFlyBeam = this.grubberFlyBeamPrefabD_fury.Spawn(this.transform.position);
                            this.grubberFlyBeam.transform.SetScaleY(this.transform.localScale.x);
                            this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_13)))
                                this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
                        }
                    }
                }
            }
            if (this.cState.wallSliding)
            {
                if (this.cState.facingRight)
                    this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
                else
                    this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
            }
            else if (attackDir == AttackDirection.normal && this.cState.facingRight)
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
            else if (attackDir == AttackDirection.normal && !this.cState.facingRight)
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
            else if (attackDir == AttackDirection.upward)
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 90f;
            else if (attackDir == AttackDirection.downward)
                this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
            this.altAttackTime = Time.timeSinceLevelLoad;
            ModHooks.AfterAttack(attackDir); //MOD API - Added
            if (!this.cState.attacking) return; //MOD API - Added
            this.slashComponent.StartSlash();
            if (this.playerData.GetBool(nameof(PlayerData.equippedCharm_38)))
                this.fsm_orbitShield.SendEvent("SLASH");
        }

        #endregion

        #region SoulGain

        [MonoModIgnore]
        private GameManager gm;

        [MonoModReplace]
        public void SoulGain()
        {
            int mpcharge = this.playerData.GetInt("MPCharge");
            int num;
            if (mpcharge < this.playerData.GetInt("maxMP"))
            {
                num = 11;
                if (this.playerData.GetBool("equippedCharm_20"))
                {
                    num += 3;
                }

                if (this.playerData.GetBool("equippedCharm_21"))
                {
                    num += 8;
                }
            }
            else
            {
                num = 6;
                if (this.playerData.GetBool("equippedCharm_20"))
                {
                    num += 2;
                }

                if (this.playerData.GetBool("equippedCharm_21"))
                {
                    num += 6;
                }
            }

            int mpreserve = this.playerData.GetInt("MPReserve");
            num = Modding.ModHooks.OnSoulGain(num);
            this.playerData.AddMPCharge(num);
            GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
            if (this.playerData.GetInt("MPReserve") != mpreserve)
            {
                this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
            }
        }

        #endregion

        #region LookForQueueInput

        [MonoModIgnore]
        private bool isGameplayScene;

        [MonoModIgnore]
        private InputHandler inputHandler;

        [MonoModIgnore]
        private extern bool CanWallJump();

        [MonoModIgnore]
        private extern bool CanJump();

        [MonoModIgnore]
        private extern bool CanDoubleJump();

        [MonoModIgnore]
        private extern bool CanInfiniteAirJump();

        [MonoModIgnore]
        private extern bool CanDash();

        [MonoModIgnore]
        private extern bool CanAttack();

        [MonoModIgnore]
        private extern void DoWallJump();

        [MonoModIgnore]
        private extern void HeroJump();

        [MonoModIgnore]
        private extern void DoDoubleJump();

        [MonoModIgnore]
        private extern void CancelJump();

        [MonoModIgnore]
        private extern void ResetLook();

        [MonoModIgnore]
        private extern void HeroDash();

        [MonoModIgnore]
        private extern bool CanSwim();

        [MonoModIgnore]
        private extern void SetState(ActorStates newState);

        [MonoModIgnore]
        private HeroAudioController audioCtrl;

        [MonoModIgnore]
        private int jumpQueueSteps;

        [MonoModIgnore]
        private int doubleJumpQueueSteps;

        [MonoModIgnore]
        private bool doubleJumpQueuing;

        [MonoModIgnore]
        private int jumpReleaseQueueSteps;

        [MonoModIgnore]
        private bool jumpReleaseQueuing;

        [MonoModIgnore]
        private bool jumpQueuing;

        [MonoModIgnore]
        private int dashQueueSteps;

        [MonoModIgnore]
        private bool dashQueuing;

        [MonoModIgnore]
        private int attackQueueSteps;

        [MonoModIgnore]
        private bool attackQueuing;

        [MonoModIgnore]
        private int JUMP_QUEUE_STEPS = 2;

        [MonoModIgnore]
        private int DOUBLE_JUMP_QUEUE_STEPS = 10;

        [MonoModIgnore]
        private int ATTACK_QUEUE_STEPS = 5;


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

                if (this.inputHandler.inputActions.dash.WasPressed && !ModHooks.OnDashPressed())
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

                if (this.inputHandler.inputActions.dash.IsPressed
                    && this.dashQueueSteps <= this.DASH_QUEUE_STEPS
                    && this.CanDash()
                    && this.dashQueuing
                    && !ModHooks.OnDashPressed()
                    && this.CanDash())
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

        #region TakeDamage

        [MonoModIgnore]
        private int hitsSinceShielded;

        [MonoModIgnore]
        private extern bool CanTakeDamage();

        [MonoModIgnore]
        private AudioSource audioSource;

        [MonoModIgnore]
        private extern void CancelAttack();

        [MonoModIgnore]
        private extern void CancelBounce();

        [MonoModIgnore]
        private extern void CancelRecoilHorizontal();

        [MonoModIgnore]
        private bool takeNoDamage;

        [MonoModIgnore]
        private float nailChargeTimer;

        [MonoModIgnore]
        private extern IEnumerator Die();

        [MonoModIgnore]
        private extern IEnumerator DieFromHazard(HazardType hazardType, float angle);

        [MonoModIgnore]
        private extern IEnumerator StartRecoil(CollisionSide impactSide, bool spawnDamageEffect, int damageAmount);

        [MonoModIgnore]
        public event HeroController.TakeDamageEvent OnTakenDamage;

        [MonoModReplace]
        public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            damageAmount = ModHooks.OnTakeDamage(ref hazardType, damageAmount);
            bool spawnDamageEffect = true;
            if (damageAmount > 0)
            {
                if (BossSceneController.IsBossScene)
                {
                    int bossLevel = BossSceneController.Instance.BossLevel;
                    if (bossLevel != 1)
                    {
                        if (bossLevel == 2)
                        {
                            damageAmount = 9999;
                        }
                    }
                    else
                    {
                        damageAmount *= 2;
                    }
                }

                if (this.CanTakeDamage())
                {
                    if (this.damageMode == DamageMode.HAZARD_ONLY && hazardType == 1)
                    {
                        return;
                    }

                    if (this.cState.shadowDashing && hazardType == 1)
                    {
                        return;
                    }

                    if (this.parryInvulnTimer > 0f && hazardType == 1)
                    {
                        return;
                    }

                    VibrationMixer mixer = VibrationManager.GetMixer();
                    if (mixer != null)
                    {
                        mixer.StopAllEmissionsWithTag("heroAction");
                    }

                    bool flag = false;
                    if (this.carefreeShieldEquipped && hazardType == 1)
                    {
                        if (this.hitsSinceShielded > 7)
                        {
                            this.hitsSinceShielded = 7;
                        }

                        switch (this.hitsSinceShielded)
                        {
                            case 1:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 10f)
                                {
                                    flag = true;
                                }

                                break;
                            case 2:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 20f)
                                {
                                    flag = true;
                                }

                                break;
                            case 3:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 30f)
                                {
                                    flag = true;
                                }

                                break;
                            case 4:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 50f)
                                {
                                    flag = true;
                                }

                                break;
                            case 5:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 70f)
                                {
                                    flag = true;
                                }

                                break;
                            case 6:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 80f)
                                {
                                    flag = true;
                                }

                                break;
                            case 7:
                                if ((float) UnityEngine.Random.Range(1, 100) <= 90f)
                                {
                                    flag = true;
                                }

                                break;
                            default:
                                flag = false;
                                break;
                        }

                        if (flag)
                        {
                            this.hitsSinceShielded = 0;
                            this.carefreeShield.SetActive(true);
                            damageAmount = 0;
                            spawnDamageEffect = false;
                        }
                        else
                        {
                            this.hitsSinceShielded++;
                        }
                    }

                    if (this.playerData.GetBool("equippedCharm_5") && this.playerData.GetInt("blockerHits") > 0 && hazardType == 1 && this.cState.focusing && !flag)
                    {
                        this.proxyFSM.SendEvent("HeroCtrl-TookBlockerHit");
                        this.audioSource.PlayOneShot(this.blockerImpact, 1f);
                        spawnDamageEffect = false;
                        damageAmount = 0;
                    }
                    else
                    {
                        this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                    }

                    this.CancelAttack();
                    if (this.cState.wallSliding)
                    {
                        this.cState.wallSliding = false;
                        this.wallSlideVibrationPlayer.Stop();
                    }

                    if (this.cState.touchingWall)
                    {
                        this.cState.touchingWall = false;
                    }

                    if (this.cState.recoilingLeft || this.cState.recoilingRight)
                    {
                        this.CancelRecoilHorizontal();
                    }

                    if (this.cState.bouncing)
                    {
                        this.CancelBounce();
                        this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
                    }

                    if (this.cState.shroomBouncing)
                    {
                        this.CancelBounce();
                        this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
                    }

                    if (!flag)
                    {
                        this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                    }

                    damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
                    if (!this.takeNoDamage && !this.playerData.GetBool("invinciTest"))
                    {
                        if (this.playerData.GetBool("overcharmed"))
                        {
                            this.playerData.TakeHealth(damageAmount * 2);
                        }
                        else
                        {
                            this.playerData.TakeHealth(damageAmount);
                        }
                    }

                    if (this.playerData.GetBool("equippedCharm_3") && damageAmount > 0)
                    {
                        if (this.playerData.GetBool("equippedCharm_35"))
                        {
                            this.AddMPCharge(this.GRUB_SOUL_MP_COMBO);
                        }
                        else
                        {
                            this.AddMPCharge(this.GRUB_SOUL_MP);
                        }
                    }

                    if (this.joniBeam && damageAmount > 0)
                    {
                        this.joniBeam = false;
                    }

                    if (this.cState.nailCharging || this.nailChargeTimer != 0f)
                    {
                        this.cState.nailCharging = false;
                        this.nailChargeTimer = 0f;
                    }

                    if (damageAmount > 0 && this.OnTakenDamage != null)
                    {
                        this.OnTakenDamage();
                    }

                    if (this.playerData.GetInt("health") == 0)
                    {
                        base.StartCoroutine(this.Die());
                    }
                    else if (hazardType == 2)
                    {
                        base.StartCoroutine(this.DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                    }
                    else if (hazardType == 3)
                    {
                        base.StartCoroutine(this.DieFromHazard(HazardType.ACID, 0f));
                    }
                    else if (hazardType == 4)
                    {
                        Debug.Log("Lava death");
                    }
                    else if (hazardType == 5)
                    {
                        base.StartCoroutine(this.DieFromHazard(HazardType.PIT, 0f));
                    }
                    else
                    {
                        base.StartCoroutine(this.StartRecoil(damageSide, spawnDamageEffect, damageAmount));
                    }
                }
                else if (this.cState.invulnerable && !this.cState.hazardDeath && !this.playerData.GetBool("isInvincible"))
                {
                    if (hazardType == 2)
                    {
                        if (!this.takeNoDamage)
                        {
                            damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
                            this.playerData.TakeHealth(damageAmount);
                        }

                        this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                        if (this.playerData.GetInt("health") == 0)
                        {
                            base.StartCoroutine(this.Die());
                        }
                        else
                        {
                            this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                            base.StartCoroutine(this.DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                        }
                    }
                    else if (hazardType == 3)
                    {
                        damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
                        this.playerData.TakeHealth(damageAmount);
                        this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                        if (this.playerData.GetInt("health") == 0)
                        {
                            base.StartCoroutine(this.Die());
                        }
                        else
                        {
                            base.StartCoroutine(this.DieFromHazard(HazardType.ACID, 0f));
                        }
                    }
                    else if (hazardType == 4)
                    {
                        Debug.Log("Lava damage");
                    }
                }
            }
        }

        #endregion

        [MonoModIgnore]
        private NailSlash slashComponent;

        [MonoModIgnore]
        private float focusMP_amount;

        private void orig_StartMPDrain(float time) { }

        public void StartMPDrain(float time)
        {
            orig_StartMPDrain(time);
            focusMP_amount *= ModHooks.OnFocusCost();
        }

        private void orig_Update() { }

        private void Update()
        {
            ModHooks.OnHeroUpdate();
            orig_Update();
        }


        #region Dash()

        [MonoModIgnore]
        private float dash_timer;

        [MonoModIgnore]
        private extern void FinishedDashing();

        [MonoModIgnore]
        private Rigidbody2D rb2d;

        // This is the original dash vector calculating code used by the game
        // It is used to set the input dash velocity vector for the DashVectorHook
        private Vector2 OrigDashVector()
        {
            const float BUMP_VELOCITY = 4f;
            const float BUMP_VELOCITY_DASH = 5f;
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
                    origVector = new Vector2
                    (
                        velocity,
                        (!this.cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY
                    );
                }
                else
                {
                    origVector = new Vector2(velocity, 0f);
                }
            }
            else if (this.CheckForBump(CollisionSide.left))
            {
                origVector = new Vector2
                (
                    -velocity,
                    (!this.cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY
                );
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
            vector = ModHooks.DashVelocityChange(vector);

            rb2d.velocity = vector;
            dash_timer += Time.deltaTime;
        }

        #endregion

        #region CharmUpdate()

        private extern void orig_CharmUpdate();

        public void CharmUpdate()
        {
            orig_CharmUpdate();
            ModHooks.OnCharmUpdate(playerData, this);
            playerData.UpdateBlueHealth();
        }

        #endregion

        [MonoModIgnore]
        private extern void orig_DoAttack();

        public void DoAttack()
        {
            ModHooks.OnDoAttack();
            orig_DoAttack();
        }
    }
}
