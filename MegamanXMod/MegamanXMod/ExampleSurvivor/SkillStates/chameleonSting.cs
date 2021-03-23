using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MegamanXMod.SkillStates
{
    public class ChameleonSting : BaseSkillState
    {
        public float damageCoefficient = 0.4f;
        public float baseDuration = 0.15f;
        public float recoil = 1f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;
            this.fireDuration = 0.25f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Weapon";


            //Util.PlaySound(Sounds.vileAttack, base.gameObject);
            base.PlayAnimation("Attack", "TestShot", "attackSpeed", this.duration);
        }

        public override void OnExit()
        {

            base.OnExit();
        }

        private void FireCS()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.SetSpreadBloom(0.4f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FireShotgun.effectPrefab, base.gameObject, this.muzzleString, false);
                float spread_value = ((aimRay.direction.x) - (aimRay.direction.x * 3f));
                //EffectManager.SimpleMuzzleFlash(EntityStates.Mage.Weapon.FireLaserbolt.impactEffectPrefab, base.gameObject, this.muzzleString, false);
                Vector3 raygun1 = new Vector3(aimRay.direction.x + 0.2f, aimRay.direction.y, aimRay.direction.z);
                Vector3 raygun2 = new Vector3(aimRay.direction.x - 0.2f, aimRay.direction.y, aimRay.direction.z);
                //Vector3 raygun3 = Vector3.Cross(Vector3.left, aimRay.origin).normalized;

                if (base.isAuthority)
                {
                    //Util.PlaySound(Sounds.vileCerberusPhantom, base.gameObject);
                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.CerberusPhantonFMJProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(raygun1.normalized), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.CerberusPhantonFMJProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(raygun2.normalized), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                    ProjectileManager.instance.FireProjectile(Materials.RegisterProjectiles.CerberusPhantonFMJProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);


                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireCS();
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
