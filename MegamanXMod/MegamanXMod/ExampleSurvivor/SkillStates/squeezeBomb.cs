using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MegamanXMod.SkillStates
{
    public class squeezeBomb : BaseSkillState
    {
        public float damageCoefficient = 1f;
        public float baseDuration = 0.55f;
        public float recoil = 0.4f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

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

        private void FireSqueezeB()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    base.PlayAnimation("Attack", "ShootBurst", "attackSpeed", this.duration);
                    Util.PlaySound(Sounds.squeezeBomb, base.gameObject);
                    ProjectileManager.instance.FireProjectile(MegamanX.MegamanXMod.squeezeBomb, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireSqueezeB();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
