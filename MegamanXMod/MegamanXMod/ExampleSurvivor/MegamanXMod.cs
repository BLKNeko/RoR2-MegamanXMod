using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.ExampleSurvivorStates;
using KinematicCharacterController;
using MegamanXMod.SkillStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace MegamanX
{

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "MegamanXMod", "2.1.1")] // put your own name and version here
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(ItemAPI), nameof(DifficultyAPI), nameof(BuffAPI))] // need these dependencies for the mod to work properly


    public class MegamanXMod : BaseUnityPlugin
    {
        public const string MODUID = "com.BLKNeko.MegamanXMod"; // put your own names here

        public static GameObject characterPrefab; // the survivor body prefab
        public GameObject characterDisplay; // the prefab used for character select
        public GameObject doppelganger; // umbra shit

        public static GameObject XShot; // prefab for our survivor's primary attack projectile
        public static GameObject iceBombProjectile; // prefab for our survivor's FMJ secondary attack projectile
        public static GameObject chargeProjectile; // prefab for chargeshot
        public static GameObject greenNProjectile; // prefab for chargeshot
        public static GameObject meltCreeper; //prefab para o meltCreeper normal
        public static GameObject meltCreeperC; //prefab para o melt creeper Carregado
        public static GameObject squeezeBomb; // prefab for Squeeze Bomb
        public static GameObject shotgunIceCharge; //prefab for shotgunIce charge
        public static GameObject redShot; //prefab for falcon Buster shot
        public static GameObject shotFMJ; //prefab for falcon buster CHARGE shot
        public static GameObject aBurst; //prefab for AcidBurst

        // clone this material to make our own with proper shader/properties
        public static Material commandoMat;

        public static GameObject testProjectile; // testes de projéteis

        public SkillLocator skillLocator;

        private static readonly Color characterColor = new Color(0.0f, 0.24f, 0.48f); // color used for the survivor

        public static ConfigEntry<int> skinConfig { get; set; }



        private void Awake()
        {
            //------------------------START CONFIG--------------------------
            skinConfig = Config.Bind<int>(
            "SKIN_SELECTOR",
            "SkinIndex",
            0,
            "X Default skin = 0 // X Command Mission Skin = 1"
            );


            //------------------------END CONFIG----------------------------
            Assets.PopulateAssets(); // first we load the assets from our assetbundle
            CreatePrefab(); // then we create our character's body prefab
            RegisterStates(); // register our skill entitystates for networking
            RegisterCharacter(); // and finally put our new survivor in the game
            CreateDoppelganger(); // not really mandatory, but it's simple and not having an umbra is just kinda lame
            MegamanX.Skins.RegisterSkins();

        }


        public class CustomRendererInfo
        {
            public string childName;
            public Material material;
            public bool ignoreOverlays;
        }



        public static GameObject CreateModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            int skinIndex = skinConfig.Value;

            GameObject model;

            switch (skinIndex)
            {
                case 0:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlMegamanX");
                    break;
                case 1:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlMegamanXCM");
                    break;
                default:
                    model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlMegamanX");
                    break;
            }
            // make sure it's set up right in the unity project
            //GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlMegamanXT");

            return model;
        }


        public static void CreatePrefab()
        {


            // first clone the commando prefab so we can turn that into our own survivor
            characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "XBody", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "CreatePrefab", 151);

            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            // create the model here, we're gonna replace commando's model with our own
            GameObject model = CreateModel(characterPrefab);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = characterPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            // set up the character body here
            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.bodyIndex = -1;
            bodyComponent.baseNameToken = "X_NAME"; // name token
            bodyComponent.subtitleNameToken = "X_SUBTITLE"; // subtitle token- used for umbras
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 100;
            bodyComponent.levelMaxHealth = 21f;
            bodyComponent.baseRegen = 0.34f;
            bodyComponent.levelRegen = 0.25f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0.25f;
            bodyComponent.baseMoveSpeed = 7.5f;
            bodyComponent.levelMoveSpeed = 0.25f;
            bodyComponent.baseAcceleration = 85;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0.35f;
            bodyComponent.baseDamage = 22;
            bodyComponent.levelDamage = 2.8f;
            bodyComponent.baseAttackSpeed = 1.2f;
            bodyComponent.levelAttackSpeed = 0.075f;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0.25f;
            bodyComponent.baseArmor = 0.5f;
            bodyComponent.levelArmor = 0.45f;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.charPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;

            // the charactermotor controls the survivor's movement and stuff
            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.4f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;
            //characterMotor.useGravity = true;
            //characterMotor.isFlying = false;

            InputBankTest inputBankTest = characterPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.idealLocalCameraPos = Vector3.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            // this component is used to locate the character model(duh), important to set this up here
            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false; // set true if you want your character to rotate on terrain like acrid does
            modelLocator.preserveModel = false;

            // childlocator is something that must be set up in the unity project, it's used to find any child objects for things like footsteps or muzzle flashes
            // also important to set up if you want quality

            

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            // this component is used to handle all overlays and whatever on your character, without setting this up you won't get any cool effects like burning or freeze on the character
            // it goes on the model object of course
            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                // set up multiple rendererinfos if needed, but for this example there's only the one
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                   // defaultMaterial = model.GetComponentInChildren<MeshRenderer>().material,
                   // renderer = model.GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<MeshRenderer>().sharedMaterial,
                    renderer = model.GetComponentInChildren<MeshRenderer>(),
                   // defaultMaterial = model.GetComponentInChildren<MeshRenderer>().material,
                   // renderer = model.GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = true
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                   // defaultMaterial = model.GetComponentInChildren<MeshRenderer>().material,
                   // renderer = model.GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();


            //---------------------------------------------------------------------------------------





            //characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            characterModel.mainSkinnedMeshRenderer = characterModel.baseRendererInfos[0].renderer.GetComponent<SkinnedMeshRenderer>();
            

            TeamComponent teamComponent = null;
            if (characterPrefab.GetComponent<TeamComponent>() != null) teamComponent = characterPrefab.GetComponent<TeamComponent>();
            else teamComponent = characterPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 100f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            // this disables ragdoll since the character's not set up for it, and instead plays a death animation
            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            // edit the sfxlocator if you want different sounds
            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            //sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.deathSound = Sounds.die;
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = characterPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = characterPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.center = new Vector3(0, 0, 0);
            capsuleCollider.material = null;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            // this sets up the character's hurtbox, kinda confusing, but should be fine as long as it's set up in unity right
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = model.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            // this is for handling footsteps, not needed but polish is always good
            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            //footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.baseFootstepString = Sounds.xFootstep;
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");

            // ragdoll controller is a pain to set up so we won't be doing that here..
            RagdollController ragdollController = model.AddComponent<RagdollController>();
            ragdollController.bones = null;
            ragdollController.componentsToDisableOnRagdoll = null;

            // this handles the pitch and yaw animations, but honestly they are nasty and a huge pain to set up so i didn't bother
            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;

            

            //trying to add a passive
            LoadoutAPI.AddSkill(typeof(Unlimited));
            EntityStateMachine stateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(Unlimited));

        }



        private void RegisterCharacter()
        {
            // now that the body prefab's set up, clone it here to make the display prefab
            characterDisplay = PrefabAPI.InstantiateClone(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "XDisplay", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 153);
            characterDisplay.AddComponent<NetworkIdentity>();

            // clone rex's syringe projectile prefab here to use as our own projectile
            XShot = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageIceBolt"), "Prefabs/Projectiles/XshotProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            XShot.GetComponent<ProjectileController>().procCoefficient = 1f;
            XShot.GetComponent<ProjectileDamage>().damage = 1f;
            XShot.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (XShot) PrefabAPI.RegisterNetworkPrefab(XShot);

            // clone FMJ's syringe projectile prefab here to use as our own projectile
            iceBombProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageIceBombProjectile"), "Prefabs/Projectiles/ShotgIceProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            iceBombProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            iceBombProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            iceBombProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Freeze2s;

            // register it for networking
            if (iceBombProjectile) PrefabAPI.RegisterNetworkPrefab(iceBombProjectile);

            // clone FMJ's syringe projectile prefab here to use as our own projectile
            chargeProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirebolt"), "Prefabs/Projectiles/XShotCProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            chargeProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            chargeProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            chargeProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (chargeProjectile) PrefabAPI.RegisterNetworkPrefab(chargeProjectile);

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            greenNProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MissileProjectile"), "Prefabs/Projectiles/GreenNProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            greenNProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            greenNProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            greenNProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (greenNProjectile) PrefabAPI.RegisterNetworkPrefab(greenNProjectile);
            //------------------------------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------------------------------
            // clone Lunar's syringe projectile prefab here to use as our own projectile
            meltCreeper = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirewallWalkerProjectile"), "Prefabs/Projectiles/MeltCProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            meltCreeper.GetComponent<ProjectileDamage>().damage = 1f;
            meltCreeper.GetComponent<ProjectileController>().procCoefficient = 1f;
            meltCreeper.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;

            // register it for networking
            if (meltCreeper) PrefabAPI.RegisterNetworkPrefab(meltCreeper);
            //---------------------------------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------------------------------
            // clone Lunar's syringe projectile prefab here to use as our own projectile
            meltCreeperC = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirewallSeedProjectile"), "Prefabs/Projectiles/MeltCCProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            meltCreeperC.GetComponent<ProjectileDamage>().damage = 1f;
            meltCreeperC.GetComponent<ProjectileController>().procCoefficient = 1f;
            meltCreeperC.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;


            // register it for networking
            if (meltCreeperC) PrefabAPI.RegisterNetworkPrefab(meltCreeperC);
            //---------------------------------------------------------------------------------------------------------------

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            squeezeBomb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/GravSphere"), "Prefabs/Projectiles/SqueezeBProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            squeezeBomb.GetComponent<ProjectileDamage>().damage = 1f;
            squeezeBomb.GetComponent<ProjectileController>().procCoefficient = 1f;
            squeezeBomb.GetComponent<ProjectileDamage>().damageType = DamageType.WeakOnHit;

            // register it for networking
            if (squeezeBomb) PrefabAPI.RegisterNetworkPrefab(squeezeBomb);
            //---------------------------------------------------------------------------------------------------------------

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            shotFMJ = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/FMJ"), "Prefabs/Projectiles/shotFMJProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            shotFMJ.GetComponent<ProjectileDamage>().damage = 1f;
            shotFMJ.GetComponent<ProjectileController>().procCoefficient = 1f;
            shotFMJ.GetComponent<ProjectileDamage>().damageType = DamageType.BypassArmor;

            // register it for networking
            if (shotFMJ) PrefabAPI.RegisterNetworkPrefab(shotFMJ);
            //


            //--------------------------------------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------------------------------------

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            aBurst = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/PoisonOrbProjectile"), "Prefabs/Projectiles/RShieldProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            aBurst.GetComponent<ProjectileDamage>().damage = 1f;
            aBurst.GetComponent<ProjectileController>().procCoefficient = 1f;
            aBurst.GetComponent<ProjectileDamage>().damageType = DamageType.PoisonOnHit;

            // register it for networking
            if (aBurst) PrefabAPI.RegisterNetworkPrefab(aBurst);
            //


            //--------------------------------------------------------------------------------------------------------------


            // clone Lunar's syringe projectile prefab here to use as our own projectile
            testProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/GravSphere"), "Prefabs/Projectiles/TestExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            testProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            testProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            testProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (testProjectile) PrefabAPI.RegisterNetworkPrefab(testProjectile);
            //---------------------------------------------------------------------------------------------------------------

            // add it to the projectile catalog or it won't work in multiplayer
            ProjectileCatalog.getAdditionalEntries += list =>
            {
                list.Add(XShot);
                list.Add(chargeProjectile);
                list.Add(iceBombProjectile);
                list.Add(greenNProjectile);
                list.Add(testProjectile);
                list.Add(meltCreeper);
                list.Add(meltCreeperC);
                list.Add(squeezeBomb);
                list.Add(shotFMJ);
                list.Add(aBurst);
            };


            // write a clean survivor description here!
            string desc = "Megaman X<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X can transform either of his arms into a powerful buster to shoot bullets of compressed solar energy, and has an energy amplifier that allows it to be charged up and release a more powerful shot." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X-Buster is powerful but slow and his charged shot have a limited range, FK-Buster is weaker but faster and have no range limit</color>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X fires a shard of ice that can freeze and damage a target." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Emergency Acceleration System(DASH) is a move that temporarily speeds up the character." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X shoots a bamboo-rocket which pursues enemies.</color>" + Environment.NewLine + Environment.NewLine;

            // add the language tokens
            LanguageAPI.Add("X_NAME", "X");
            LanguageAPI.Add("X_DESCRIPTION", desc);
            LanguageAPI.Add("X_SUBTITLE", "Megaman X, B class Hunter");

            // add our new survivor to the game~
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "X_NAME",
                unlockableName = "",
                descriptionToken = "X_DESCRIPTION",
                primaryColor = characterColor,
                bodyPrefab = characterPrefab,
                displayPrefab = characterDisplay
            };


            SurvivorAPI.AddSurvivor(survivorDef);

            // set up the survivor's skills here
            SkillSetup();

            // gotta add it to the body catalog too
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(characterPrefab);
            };
        }

        void SkillSetup()
        {
            // get rid of the original skills first, otherwise we'll have commando's loadout and we don't want that
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            skillLocator = characterPrefab.GetComponent<SkillLocator>();

            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        void RegisterStates()
        {
            // register the entitystates for networking reasons
            LoadoutAPI.AddSkill(typeof(chargeShot));
            LoadoutAPI.AddSkill(typeof(shotgunIce));
            LoadoutAPI.AddSkill(typeof(dodgeState));
            LoadoutAPI.AddSkill(typeof(greenSpinner));
            LoadoutAPI.AddSkill(typeof(meltCreeper));
            LoadoutAPI.AddSkill(typeof(FKBuster));
            LoadoutAPI.AddSkill(typeof(squeezeBomb));
            LoadoutAPI.AddSkill(typeof(FireW));
            LoadoutAPI.AddSkill(typeof(FireW2));
            LoadoutAPI.AddSkill(typeof(FireW3));
            LoadoutAPI.AddSkill(typeof(Unlimited));
            LoadoutAPI.AddSkill(typeof(AcidBurst));
            LoadoutAPI.AddSkill(typeof(AcidBurst2));
            LoadoutAPI.AddSkill(typeof(ChameleonSting));
        }


        void PassiveSetup()
        {
            // set up the passive skill here if you want
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("X_PASSIVE_NAME", "Limitless Potential");
            LanguageAPI.Add("X_PASSIVE_DESCRIPTION", "<style=cIsUtility>X's true potential still unachieved, but his adaptation and improvement grow's super fast.</style> <style=cIsHealing>When X HP gets Low, he uses his true powers, getting temporary stronger and generating a shield</style>, <style=cIsDamage> but after this he need to recharge before use this again.</style>");

            
            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "X_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "X_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        

        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("X_PRIMARY_CROSSBOW_NAME", "X-Buster");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW_DESCRIPTION", "Shoot with X-Buster, dealing <style=cIsDamage>170% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(chargeShot));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.primary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill secondary Squeeze Bomb

            LanguageAPI.Add("X_PRIMARY_V_NAME", "FK-Buster");
            LanguageAPI.Add("X_PRIMARY_V_DESCRIPTION", "Shoot with FK-Buster, dealing <style=cIsDamage>125% damage</style>. his charged attack bypass some enemies armor");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FKBuster));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon7;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_V_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_V_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("X_PRIMARY_CROSSBOW2_NAME", "ShotgunIce");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW2_DESCRIPTION", "Shoot an IceMissle that pierce enemies, dealing <style=cIsDamage>200% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(shotgunIce));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 3;
            mySkillDef.baseRechargeInterval = 8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = true;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 1f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon2;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW2_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW2_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW2_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // alternate skill secondary Squeeze Bomb

            LanguageAPI.Add("X_PRIMARY_CROSSBOW2V_NAME", "Squeeze Bomb");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW2V_DESCRIPTION", "A gravity-based weapon. Creates localized black holes that hold up enemies.");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(squeezeBomb));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 10f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.5f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon6;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW2V_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW2V_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW2V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // alternate skill secondary FireWave

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            LanguageAPI.Add("X_PRIMARY_CROSSBOW2V2_NAME", "Fire Wave");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW2V2_DESCRIPTION", "X releases a constant stream of flames from his buster");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(FireW));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon8;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW2V2_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW2V2_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW2V2_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        void UtilitySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("X_PRIMARY_CROSSBOW3_NAME", "Dash");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW3_DESCRIPTION", "<style=cIsDamage>Perform a Dash</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(dodgeState));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 3f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW3_DESCRIPTION";
            mySkillDef.skillName = "EX_PRIMARY_CROSSBOW3_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW3_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
        }

        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("X_PRIMARY_CROSSBOW4_NAME", "GreenNeedle");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW4_DESCRIPTION", "Shoot a small missle tha follow some targets, dealing <style=cIsDamage>145% base damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(greenSpinner));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 5;
            mySkillDef.baseRechargeInterval = 3f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.3f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon4;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW4_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW4_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW4_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = characterPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill special testprojectile

            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V_NAME", "Melt Creeper");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V_DESCRIPTION", " It sends a small wave of flames on the ground, dealing <style=cIsDamage>200% base damage</style>, When charged up, it sends flames both sides, dealing even more damage");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(meltCreeper));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.5f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon5;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW4V_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW4V_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW4V_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill special AcidBurst

            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V2_NAME", "Acid Burst");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V2_DESCRIPTION", " When fired, it creates a glob of acid which, upon contact with any surface, will create acid crystals, dealing <style=cIsDamage>125% base damage</style> and poisoning enemies,  When charged, X will fire two balls of acid dealing a little more damage");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AcidBurst));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 2;
            mySkillDef.baseRechargeInterval = 4.8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.5f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon9;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW4V2_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW4V2_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW4V2_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };

            // alternate skill special ChameleonSting

            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V3_NAME", "Chameleon Sting");
            LanguageAPI.Add("X_PRIMARY_CROSSBOW4V3_DESCRIPTION", " When fired, it creates a glob of acid which, upon contact with any surface, will create acid crystals, dealing <style=cIsDamage>125% base damage</style> and poisoning enemies,  When charged, X will fire two balls of acid dealing a little more damage");

            // set up your primary skill def here!

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ChameleonSting));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 20;
            mySkillDef.baseRechargeInterval = 4.8f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0.5f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon7;
            mySkillDef.skillDescriptionToken = "X_PRIMARY_CROSSBOW4V3_DESCRIPTION";
            mySkillDef.skillName = "X_PRIMARY_CROSSBOW4V3_NAME";
            mySkillDef.skillNameToken = "X_PRIMARY_CROSSBOW4V3_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            // add this code after defining a new skilldef if you're adding an alternate skill

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateDoppelganger()
        {
            // set up the doppelganger for artifact of vengeance here
            // quite simple, gets a bit more complex if you're adding your own ai, but commando ai will do

            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), "XMonsterMaster", true, "C:\\Users\\test\\Documents\\ror2mods\\MegamanX\\MegamanX\\MegamanX\\MegamanX.cs", "CreateDoppelganger", 159);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
    }



    // get the assets from your assetbundle here
    // if it's returning null, check and make sure you have the build action set to "Embedded Resource" and the file names are right because it's not gonna work otherwise
    public static class Assets
    {
        public static AssetBundle MainAssetBundle = null;
        public static AssetBundleResourcesProvider Provider;

        public static Texture charPortrait;

        public static GameObject chargeeffect1C;
        public static GameObject chargeeffect1W;
        public static GameObject chargeeffect2C;
        public static GameObject CrystalEffect;

        public static Material matTest;
        public static Material matTest2;

        public static Material commandoMat;

        public static Sprite iconP;
        public static Sprite icon1;
        public static Sprite icon2;
        public static Sprite icon3;
        public static Sprite icon4;
        public static Sprite icon5;
        public static Sprite icon6;
        public static Sprite icon7;
        public static Sprite icon8;
        public static Sprite icon9;

        public static Sprite SkinIcon1;

        public static Mesh BodyMesh;
        public static Mesh Buster_Mesh;

        public static void PopulateAssets()
        {
            commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            if (MainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MegamanXMod.MegamanXBundle"))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                    Provider = new AssetBundleResourcesProvider("@ExampleSurvivor", MainAssetBundle);
                }
            }

            // include this if you're using a custom soundbank
            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("MegamanXMod.MegamanXNewSB.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }

            // and now we gather the assets
            charPortrait = MainAssetBundle.LoadAsset<Sprite>("Megaman_X_Icon").texture;

            iconP = MainAssetBundle.LoadAsset<Sprite>("PassiveIcon");
            icon1 = MainAssetBundle.LoadAsset<Sprite>("Skill1Icon");
            icon2 = MainAssetBundle.LoadAsset<Sprite>("Skill2Icon");
            icon3 = MainAssetBundle.LoadAsset<Sprite>("Skill3Icon");
            icon4 = MainAssetBundle.LoadAsset<Sprite>("Skill4Icon");
            icon5 = MainAssetBundle.LoadAsset<Sprite>("Skill5Icon");
            icon6 = MainAssetBundle.LoadAsset<Sprite>("Skill6Icon");
            icon7 = MainAssetBundle.LoadAsset<Sprite>("Skill7Icon");
            icon8 = MainAssetBundle.LoadAsset<Sprite>("Skill8Icon");
            icon9 = MainAssetBundle.LoadAsset<Sprite>("Skill9Icon");

            SkinIcon1 = MainAssetBundle.LoadAsset<Sprite>("SkinIcon1");

            matTest = MainAssetBundle.LoadAsset<Material>("matT");
            matTest2 = MainAssetBundle.LoadAsset<Material>("NewMat");

            BodyMesh = MainAssetBundle.LoadAsset<Mesh>("BodyMesh");
            Buster_Mesh = MainAssetBundle.LoadAsset<Mesh>("Buster_Mesh");
            

            chargeeffect1C = Assets.LoadEffect("ChargeLight1C", "");
            chargeeffect1W = Assets.LoadEffect("ChargeLight1W", "");
            chargeeffect2C = Assets.LoadEffect("ChargeLight2C", "");
            CrystalEffect = Assets.LoadEffect("CristalShine", "");


        }


        private static GameObject LoadEffect(string resourceName, string soundName)
        {
            GameObject newEffect = MainAssetBundle.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = 12;
            newEffect.AddComponent<NetworkIdentity>();
            newEffect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = true;
            effect.positionAtReferencedTransform = true;
            effect.soundName = soundName;

            EffectAPI.AddEffect(newEffect);

            return newEffect;
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material tempMat = Assets.MainAssetBundle.LoadAsset<Material>(materialName);
            if (!tempMat)
            {
                return commandoMat;
            }

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            mat.name = materialName;

            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }


    }
}

public static class Sounds
{
    public static readonly string xBullet = "CallXBullet";
    public static readonly string shotgunIce = "CallShotgunIce";
    public static readonly string greenSpinner = "CallGreenSpinner";
    public static readonly string xFootstep = "CallXFootstep";
    public static readonly string charging = "CallCharging";
    public static readonly string fullCharge = "CallFullCharged";
    public static readonly string die = "CallDie";
    public static readonly string xDash = "CallXDash";
    public static readonly string xChargeShot = "CallXChargeShot";
    public static readonly string xFullPower = "CallXFullPower";
    public static readonly string xHurt = "CallXHurt";
    public static readonly string meltCreeper = "CallMeltCreeper";
    public static readonly string squeezeBomb = "CallSqueezeBomb";
    public static readonly string FireWave = "CallFireWave";
    public static readonly string FireWaveSFX = "CallFireWaveSFX";
    public static readonly string XAttack = "CallXHaa";
    public static readonly string XPassive = "CallXPassive";

}


// the entitystates namespace is used to make the skills, i'm not gonna go into detail here but it's easy to learn through trial and error

namespace EntityStates.ExampleSurvivorStates
{
    public class Unlimited : GenericCharacterMain
    {
        public float MaxHP;
        public float GetHP;
        public double MinHP;
        public float Timer = 0;
        public float baseDuration = 1f;
        private float duration;
        private Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();

        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Timer -= Time.fixedDeltaTime;
            MinHP = 0.3 + (base.characterBody.level / 200);
            if (base.characterBody.healthComponent.combinedHealthFraction < MinHP && Timer < 5f)
            {
                Util.PlaySound(Sounds.XPassive, base.gameObject);
                EffectManager.SimpleMuzzleFlash(MegamanX.Assets.CrystalEffect, base.gameObject, "Crystal", true);
                base.healthComponent.AddBarrierAuthority(base.characterBody.healthComponent.fullHealth/2f);
                if (NetworkServer.active)
                {
                    base.characterBody.AddTimedBuff(BuffIndex.LifeSteal, 5.8f);
                    base.characterBody.AddTimedBuff(BuffIndex.FullCrit, 9.5f);
                }
                Timer = 100f;
                
            }

            return;

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

namespace MegamanX
{
    public static class Skins
    {
        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, string unlockName)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = new LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[0],
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableName = unlockName
            };

            SkinDef skin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            return skin;
        }

        public static SkinDef CreateSkinDef(string skinName, Sprite skinIcon, CharacterModel.RendererInfo[] rendererInfos, SkinnedMeshRenderer mainRenderer, GameObject root, string unlockName, Mesh skinMesh)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = new LoadoutAPI.SkinDefInfo
            {
                BaseSkins = Array.Empty<SkinDef>(),
                GameObjectActivations = new SkinDef.GameObjectActivation[0],
                Icon = skinIcon,
                MeshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        renderer = mainRenderer,
                        mesh = skinMesh
                    }
                },
                MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0],
                Name = skinName,
                NameToken = skinName,
                ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0],
                RendererInfos = rendererInfos,
                RootObject = root,
                UnlockableName = unlockName
            };

            SkinDef skin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            return skin;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!MegamanXMod.commandoMat) MegamanXMod.commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(MegamanXMod.commandoMat);
            Material tempMat = Assets.MainAssetBundle.LoadAsset<Material>(materialName);
            if (!tempMat)
            {
                return MegamanXMod.commandoMat;
            }

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static void RegisterSkins()
        {
            GameObject bodyPrefab = MegamanXMod.characterPrefab;

            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;
            //SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            List<SkinDef> skinDefs = new List<SkinDef>();

            #region DefaultSkin
            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;
            SkinDef defaultSkin = CreateSkinDef("X_DEFAULT_SKIN", Assets.SkinIcon1, defaultRenderers, mainRenderer, model, "");
            defaultSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.BodyMesh,
                    renderer = defaultRenderers[0].renderer
                },
            };

            skinDefs.Add(defaultSkin);
            #endregion



            #region MinecraftSkin
            CharacterModel.RendererInfo[] minecraftRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(minecraftRendererInfos, 0);

            minecraftRendererInfos[1].defaultMaterial = Assets.matTest2;
            minecraftRendererInfos[2].defaultMaterial = Assets.matTest2;
            // minecraftRendererInfos[1].defaultMaterial = CreateMaterial("matMinecraftPaladin", 3, Color.white);

            SkinDef minecraftSkin = CreateSkinDef("PALADINBODY_MINECRAFT_SKIN_NAME", Assets.icon2, minecraftRendererInfos, mainRenderer, model, "");
            minecraftSkin.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = Assets.Buster_Mesh,
                    renderer = defaultRenderers[1].renderer
                },
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = defaultRenderers[2].renderer
                }
            };

            //skinDefs.Add(minecraftSkin);
            #endregion


            skinController.skins = skinDefs.ToArray();
        }
    }
}

