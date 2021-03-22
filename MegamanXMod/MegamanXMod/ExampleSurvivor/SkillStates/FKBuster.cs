﻿using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MegamanXMod.SkillStates
{
    public class FKBuster : BaseSkillState
    {
        public float damageCoefficient = 1.25f;
        public float baseDuration = 0.44f;
        public float recoil = 0.5f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/TracerBanditPistol");
        public static GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/BulletImpactSoft");



        public float chargeTime = 0f;
        public float LastChargeTime = 0f;
        public bool chargeFullSFX = false;
        public bool hasTime = false;
        public bool hasCharged = false;
        public bool chargingSFX = false;



        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.fireDuration = 0.25f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Weapon";
            base.PlayAnimation("Attack", "ShootPose", "attackSpeed", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireArrow()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                base.characterBody.AddSpreadBloom(0.15f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    base.PlayAnimation("Attack", "ShootBurst", "attackSpeed", this.duration);
                    Util.PlaySound(Sounds.xBullet, base.gameObject);
                    //ProjectileManager.instance.FireProjectile(XSurvivor.MegamanXMod.XShot, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    new BulletAttack
                    {
                        owner = base.gameObject,
                        weapon = base.gameObject,
                        origin = aimRay.origin,
                        aimVector = aimRay.direction,
                        minSpread = 0.1f,
                        maxSpread = 0.4f,
                        damage = damageCoefficient * this.damageStat,
                        force = 1f,
                        tracerEffectPrefab = FKBuster.tracerEffectPrefab,
                        muzzleName = muzzleString,
                        hitEffectPrefab = FKBuster.hitEffectPrefab,
                        isCrit = Util.CheckRoll(this.critStat, base.characterBody.master)
                    }.Fire();
                }
            }
        }

        private void FireArrowC()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                base.characterBody.AddSpreadBloom(0.45f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    base.PlayAnimation("Attack", "ShootBurst", "attackSpeed", this.duration);
                    Util.PlaySound(Sounds.xChargeShot, base.gameObject);
                    ProjectileManager.instance.FireProjectile(MegamanX.MegamanXMod.shotFMJ, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * 3f * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (base.inputBank.skill1.down)
            {
                chargeTime += Time.deltaTime;

                if (chargeTime > 0.5f && chargeTime <= 1.8f && chargingSFX == false)
                {
                    Util.PlaySound(Sounds.charging, base.gameObject);
                    EffectManager.SimpleMuzzleFlash(MegamanX.Assets.chargeeffect1C, base.gameObject, "Center", true);
                    EffectManager.SimpleMuzzleFlash(MegamanX.Assets.chargeeffect1W, base.gameObject, "Center", true);
                    chargingSFX = true;
                }

                if (chargeTime >= 1.8f && chargeFullSFX == false)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
                    EffectManager.SimpleMuzzleFlash(MegamanX.Assets.chargeeffect2C, base.gameObject, "Center", true);
                    chargeFullSFX = true;
                    LastChargeTime = chargeTime;
                }

                if ((chargeTime - LastChargeTime) >= 0.68f && chargeFullSFX == true)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
                    EffectManager.SimpleMuzzleFlash(MegamanX.Assets.chargeeffect2C, base.gameObject, "Center", true);
                    LastChargeTime = chargeTime;
                }
            }

            if (!base.inputBank.skill1.down)
            {
                if (chargeTime >= 1.8f)
                    hasCharged = true;
                chargingSFX = false;
                hasTime = true;
            }

        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((base.fixedAge >= this.fireDuration || !base.inputBank || !base.inputBank.skill1.down) && hasCharged == true && hasTime == true)
            {
                FireArrowC();
            }

            if ((base.fixedAge >= this.fireDuration || !base.inputBank || !base.inputBank.skill1.down) && hasCharged == false && hasTime == true)
            {
                FireArrow();
            }

            if (base.fixedAge >= this.duration && base.isAuthority && hasTime == true)
            {
                hasTime = false;
                chargeTime = 0f;
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}
