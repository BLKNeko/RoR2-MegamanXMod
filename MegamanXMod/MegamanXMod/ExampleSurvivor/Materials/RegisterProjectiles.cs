using BepInEx;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace MegamanXMod.Materials
{
    public class RegisterProjectiles : BaseUnityPlugin
    {
        public static GameObject CerberusPhantonFMJProjectile;



        public static void Register()
        {

            //-------------------------------------START --------------------------------------------

            //CommandoGrenadeProjectile (boa, quica uma vez e explode  depois de um tempo)
            //CryoCanisterBombletsProjectile (boa, ele apenas solta a granada no chão)
            CerberusPhantonFMJProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/FMJ"), "Prefabs/Projectiles/BombProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanXVile\\MegamanXVile\\MegamanXVile\\MegamanXVile.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            CerberusPhantonFMJProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            CerberusPhantonFMJProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            CerberusPhantonFMJProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (CerberusPhantonFMJProjectile) PrefabAPI.RegisterNetworkPrefab(CerberusPhantonFMJProjectile);

            //--------------------------------------END --------------------------------------------

            // add it to the projectile catalog or it won't work in multiplayer
            ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(CerberusPhantonFMJProjectile);
            };
        }

    }
}
