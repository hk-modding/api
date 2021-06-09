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
            if (Time.timeSinceLevelLoad - altAttackTime > ALT_ATTACK_RESET)
            {
                cState.altAttack = false;
            }

            cState.attacking = true;
            if (playerData.equippedCharm_32)
            {
                attackDuration = ATTACK_DURATION_CH;
            }
            else
            {
                attackDuration = ATTACK_DURATION;
            }

            if (cState.wallSliding)
            {
                wallSlashing = true;
                slashComponent = wallSlash;
                slashFsm = wallSlashFsm;
            }
            else
            {
                wallSlashing = false;
                if (attackDir == AttackDirection.normal)
                {
                    if (!cState.altAttack)
                    {
                        slashComponent = normalSlash;
                        slashFsm = normalSlashFsm;
                        cState.altAttack = true;
                    }
                    else
                    {
                        slashComponent = alternateSlash;
                        slashFsm = alternateSlashFsm;
                        cState.altAttack = false;
                    }

                    if (playerData.equippedCharm_35)
                    {
                        if ((playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27))
                        {
                            if (transform.localScale.x < 0f)
                            {
                                grubberFlyBeam = grubberFlyBeamPrefabR.Spawn(transform.position);
                            }
                            else
                            {
                                grubberFlyBeam = grubberFlyBeamPrefabL.Spawn(transform.position);
                            }

                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, MANTIS_CHARM_SCALE);
                            }
                            else
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, 1f);
                            }
                        }

                        if (playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1)
                        {
                            if (transform.localScale.x < 0f)
                            {
                                grubberFlyBeam = grubberFlyBeamPrefabR_fury.Spawn(transform.position);
                            }
                            else
                            {
                                grubberFlyBeam = grubberFlyBeamPrefabL_fury.Spawn(transform.position);
                            }

                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, MANTIS_CHARM_SCALE);
                            }
                            else
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, 1f);
                            }
                        }
                    }
                }
                else if (attackDir == AttackDirection.upward)
                {
                    slashComponent = upSlash;
                    slashFsm = upSlashFsm;
                    cState.upAttacking = true;
                    if (playerData.equippedCharm_35)
                    {
                        if ((playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27))
                        {
                            grubberFlyBeam = grubberFlyBeamPrefabU.Spawn(transform.position);
                            Extensions.SetScaleY(grubberFlyBeam.transform, transform.localScale.x);
                            grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
                            }
                        }

                        if (playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1)
                        {
                            grubberFlyBeam = grubberFlyBeamPrefabU_fury.Spawn(transform.position);
                            Extensions.SetScaleY(grubberFlyBeam.transform, transform.localScale.x);
                            grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
                            }
                        }
                    }
                }
                else if (attackDir == AttackDirection.downward)
                {
                    slashComponent = downSlash;
                    slashFsm = downSlashFsm;
                    cState.downAttacking = true;
                    if (playerData.equippedCharm_35)
                    {
                        if ((playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27))
                        {
                            grubberFlyBeam = grubberFlyBeamPrefabD.Spawn(transform.position);
                            Extensions.SetScaleY(grubberFlyBeam.transform, transform.localScale.x);
                            grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
                            }
                        }

                        if (playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1)
                        {
                            grubberFlyBeam = grubberFlyBeamPrefabD_fury.Spawn(transform.position);
                            Extensions.SetScaleY(grubberFlyBeam.transform, transform.localScale.x);
                            grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                            if (playerData.equippedCharm_13)
                            {
                                Extensions.SetScaleY(grubberFlyBeam.transform, grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
                            }
                        }
                    }
                }
            }

            if (cState.wallSliding)
            {
                if (cState.facingRight)
                {
                    slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
                }
                else
                {
                    slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
                }
            }
            else if (attackDir == AttackDirection.normal && cState.facingRight)
            {
                slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
            }
            else if (attackDir == AttackDirection.normal && !cState.facingRight)
            {
                slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
            }
            else if (attackDir == AttackDirection.upward)
            {
                slashFsm.FsmVariables.GetFsmFloat("direction").Value = 90f;
            }
            else if (attackDir == AttackDirection.downward)
            {
                slashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
            }

            altAttackTime = Time.timeSinceLevelLoad;
            ModHooks.AfterAttack(attackDir); //MOD API - Added
            if (!cState.attacking) return;       //MOD API - Added
            slashComponent.StartSlash();
            if (playerData.equippedCharm_38)
            {
                fsm_orbitShield.SendEvent("SLASH");
            }
        }

        #endregion

        #region SoulGain

        [MonoModIgnore]
        private GameManager gm;

        [MonoModReplace]
        public void SoulGain()
        {
            int mpcharge = playerData.GetInt("MPCharge");
            int num;
            if (mpcharge < playerData.GetInt("maxMP"))
            {
                num = 11;
                if (playerData.GetBool("equippedCharm_20"))
                {
                    num += 3;
                }

                if (playerData.GetBool("equippedCharm_21"))
                {
                    num += 8;
                }
            }
            else
            {
                num = 6;
                if (playerData.GetBool("equippedCharm_20"))
                {
                    num += 2;
                }

                if (playerData.GetBool("equippedCharm_21"))
                {
                    num += 6;
                }
            }

            int mpreserve = playerData.GetInt("MPReserve");
            num = Modding.ModHooks.OnSoulGain(num);
            playerData.AddMPCharge(num);
            GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
            if (playerData.GetInt("MPReserve") != mpreserve)
            {
                gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
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
            if (acceptingInput && !gm.isPaused && isGameplayScene)
            {
                if (inputHandler.inputActions.jump.WasPressed)
                {
                    if (CanWallJump())
                    {
                        DoWallJump();
                    }
                    else if (CanJump())
                    {
                        HeroJump();
                    }
                    else if (CanDoubleJump())
                    {
                        DoDoubleJump();
                    }
                    else if (CanInfiniteAirJump())
                    {
                        CancelJump();
                        audioCtrl.PlaySound(HeroSounds.JUMP);
                        ResetLook();
                        cState.jumping = true;
                    }
                    else
                    {
                        jumpQueueSteps = 0;
                        jumpQueuing = true;
                        doubleJumpQueueSteps = 0;
                        doubleJumpQueuing = true;
                    }
                }

                if (inputHandler.inputActions.dash.WasPressed && !ModHooks.OnDashPressed())
                {
                    if (CanDash())
                    {
                        HeroDash();
                    }
                    else
                    {
                        dashQueueSteps = 0;
                        dashQueuing = true;
                    }
                }

                if (inputHandler.inputActions.attack.WasPressed)
                {
                    if (CanAttack())
                    {
                        DoAttack();
                    }
                    else
                    {
                        attackQueueSteps = 0;
                        attackQueuing = true;
                    }
                }

                if (inputHandler.inputActions.jump.IsPressed)
                {
                    if (jumpQueueSteps <= JUMP_QUEUE_STEPS && CanJump() && jumpQueuing)
                    {
                        HeroJump();
                    }
                    else if (doubleJumpQueueSteps <= DOUBLE_JUMP_QUEUE_STEPS && CanDoubleJump() && doubleJumpQueuing)
                    {
                        if (cState.onGround)
                        {
                            HeroJump();
                        }
                        else
                        {
                            DoDoubleJump();
                        }
                    }

                    if (CanSwim())
                    {
                        if (hero_state != ActorStates.airborne)
                        {
                            SetState(ActorStates.airborne);
                        }

                        cState.swimming = true;
                    }
                }

                if (inputHandler.inputActions.dash.IsPressed
                    && dashQueueSteps <= DASH_QUEUE_STEPS
                    && CanDash()
                    && dashQueuing
                    && !ModHooks.OnDashPressed()
                    && CanDash())
                {
                    HeroDash();
                }

                if (inputHandler.inputActions.attack.IsPressed && attackQueueSteps <= ATTACK_QUEUE_STEPS && CanAttack() && attackQueuing)
                {
                    DoAttack();
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

                if (CanTakeDamage())
                {
                    if (damageMode == DamageMode.HAZARD_ONLY && hazardType == 1)
                    {
                        return;
                    }

                    if (cState.shadowDashing && hazardType == 1)
                    {
                        return;
                    }

                    if (parryInvulnTimer > 0f && hazardType == 1)
                    {
                        return;
                    }

                    VibrationMixer mixer = VibrationManager.GetMixer();
                    if (mixer != null)
                    {
                        mixer.StopAllEmissionsWithTag("heroAction");
                    }

                    bool flag = false;
                    if (carefreeShieldEquipped && hazardType == 1)
                    {
                        if (hitsSinceShielded > 7)
                        {
                            hitsSinceShielded = 7;
                        }

                        switch (hitsSinceShielded)
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
                            hitsSinceShielded = 0;
                            carefreeShield.SetActive(true);
                            damageAmount = 0;
                            spawnDamageEffect = false;
                        }
                        else
                        {
                            hitsSinceShielded++;
                        }
                    }

                    damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
                    if (playerData.GetBool("equippedCharm_5") && playerData.GetInt("blockerHits") > 0 && hazardType == 1 && cState.focusing && !flag)
                    {
                        proxyFSM.SendEvent("HeroCtrl-TookBlockerHit");
                        audioSource.PlayOneShot(blockerImpact, 1f);
                        spawnDamageEffect = false;
                        damageAmount = 0;
                    }
                    else
                    {
                        proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                    }

                    CancelAttack();
                    if (cState.wallSliding)
                    {
                        cState.wallSliding = false;
                        wallSlideVibrationPlayer.Stop();
                    }

                    if (cState.touchingWall)
                    {
                        cState.touchingWall = false;
                    }

                    if (cState.recoilingLeft || cState.recoilingRight)
                    {
                        CancelRecoilHorizontal();
                    }

                    if (cState.bouncing)
                    {
                        CancelBounce();
                        rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                    }

                    if (cState.shroomBouncing)
                    {
                        CancelBounce();
                        rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                    }

                    if (!flag)
                    {
                        audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                    }

                    if (!takeNoDamage && !playerData.GetBool("invinciTest"))
                    {
                        if (playerData.GetBool("overcharmed"))
                        {
                            playerData.TakeHealth(damageAmount * 2);
                        }
                        else
                        {
                            playerData.TakeHealth(damageAmount);
                        }
                    }

                    if (playerData.GetBool("equippedCharm_3") && damageAmount > 0)
                    {
                        if (playerData.GetBool("equippedCharm_35"))
                        {
                            AddMPCharge(GRUB_SOUL_MP_COMBO);
                        }
                        else
                        {
                            AddMPCharge(GRUB_SOUL_MP);
                        }
                    }

                    if (joniBeam && damageAmount > 0)
                    {
                        joniBeam = false;
                    }

                    if (cState.nailCharging || nailChargeTimer != 0f)
                    {
                        cState.nailCharging = false;
                        nailChargeTimer = 0f;
                    }

                    if (damageAmount > 0 && OnTakenDamage != null)
                    {
                        OnTakenDamage();
                    }

                    if (playerData.GetInt("health") == 0)
                    {
                        base.StartCoroutine(Die());
                    }
                    else if (hazardType == 2)
                    {
                        base.StartCoroutine(DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                    }
                    else if (hazardType == 3)
                    {
                        base.StartCoroutine(DieFromHazard(HazardType.ACID, 0f));
                    }
                    else if (hazardType == 4)
                    {
                        Debug.Log("Lava death");
                    }
                    else if (hazardType == 5)
                    {
                        base.StartCoroutine(DieFromHazard(HazardType.PIT, 0f));
                    }
                    else
                    {
                        base.StartCoroutine(StartRecoil(damageSide, spawnDamageEffect, damageAmount));
                    }
                }
                else if (cState.invulnerable && !cState.hazardDeath && !playerData.GetBool("isInvincible"))
                {
                    if (hazardType == 2)
                    {
                        if (!takeNoDamage)
                        {
                            playerData.TakeHealth(damageAmount);
                        }

                        proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                        if (playerData.GetInt("health") == 0)
                        {
                            base.StartCoroutine(Die());
                        }
                        else
                        {
                            audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                            base.StartCoroutine(DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                        }
                    }
                    else if (hazardType == 3)
                    {
                        playerData.TakeHealth(damageAmount);
                        proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                        if (playerData.GetInt("health") == 0)
                        {
                            base.StartCoroutine(Die());
                        }
                        else
                        {
                            base.StartCoroutine(DieFromHazard(HazardType.ACID, 0f));
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
            if (playerData.equippedCharm_16 && cState.shadowDashing)
            {
                velocity = DASH_SPEED_SHARP;
            }
            else
            {
                velocity = DASH_SPEED;
            }

            if (dashingDown)
            {
                origVector = new Vector2(0f, -velocity);
            }
            else if (cState.facingRight)
            {
                if (CheckForBump(CollisionSide.right))
                {
                    origVector = new Vector2
                    (
                        velocity,
                        (!cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY
                    );
                }
                else
                {
                    origVector = new Vector2(velocity, 0f);
                }
            }
            else if (CheckForBump(CollisionSide.left))
            {
                origVector = new Vector2
                (
                    -velocity,
                    (!cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY
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
            ModHooks.OnCharmUpdate();
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