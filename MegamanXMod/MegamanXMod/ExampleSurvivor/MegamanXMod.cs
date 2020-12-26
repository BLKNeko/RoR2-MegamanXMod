using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using EntityStates.ExampleSurvivorStates;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using System.IO;
using static R2API.SoundAPI;
using R2API.AssetPlus;
using AK.Wwise;
using System.Collections;
using System.Text;
using System.Runtime.CompilerServices;



namespace ExampleSurvivor
{

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "MegamanXMod", "1.1.4")] // put your own name and version here
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(ItemAPI), nameof(DifficultyAPI), nameof(BuffAPI))] // need these dependencies for the mod to work properly


    public class MegamanXMod : BaseUnityPlugin
    {
        public const string MODUID = "com.BLKNeko.MegamanXMod"; // put your own names here

        public static GameObject characterPrefab; // the survivor body prefab
        public GameObject characterDisplay; // the prefab used for character select
        public GameObject doppelganger; // umbra shit

        public static GameObject arrowProjectile; // prefab for our survivor's primary attack projectile
        public static GameObject iceBombProjectile; // prefab for our survivor's FMJ secondary attack projectile
        public static GameObject chargeProjectile; // prefab for chargeshot
        public static GameObject greenNProjectile; // prefab for chargeshot
        public static GameObject meltCreeper; //prefab para o meltCreeper normal
        public static GameObject meltCreeperC; //prefab para o melt creeper Carregado
        public static GameObject squeezeBomb; // prefab for Squeeze Bomb
        public static GameObject shotgunIceCharge; //prefab for shotgunIce charge
        public static GameObject redShot; //prefab for falcon Buster shot
        public static GameObject shotFMJ; //prefab for falcon buster CHARGE shot

        public static GameObject testProjectile; // testes de projéteis

        public SkillLocator skillLocator;

        private static readonly Color characterColor = new Color(0.0f, 0.24f, 0.48f); // color used for the survivor


        private void Awake()
        {
            Assets.PopulateAssets(); // first we load the assets from our assetbundle
            CreatePrefab(); // then we create our character's body prefab
            RegisterStates(); // register our skill entitystates for networking
            RegisterCharacter(); // and finally put our new survivor in the game
            CreateDoppelganger(); // not really mandatory, but it's simple and not having an umbra is just kinda lame

        }

        public static GameObject CreateModel(GameObject main)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            // make sure it's set up right in the unity project
            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("mdlExampleSurvivorLX");

            return model;
        }

        internal static void CreatePrefab()
        {
            // first clone the commando prefab so we can turn that into our own survivor
            characterPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody"), "ExampleSurvivorBody", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "CreatePrefab", 151);

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
            bodyComponent.baseNameToken = "EXAMPLESURVIVOR_NAME"; // name token
            bodyComponent.subtitleNameToken = "EXAMPLESURVIVOR_SUBTITLE"; // subtitle token- used for umbras
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 150;
            bodyComponent.levelMaxHealth = 25;
            bodyComponent.baseRegen = 0.85f;
            bodyComponent.levelRegen = 0.55f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0.25f;
            bodyComponent.baseMoveSpeed = 8;
            bodyComponent.levelMoveSpeed = 0.25f;
            bodyComponent.baseAcceleration = 85;
            bodyComponent.baseJumpPower = 18;
            bodyComponent.levelJumpPower = 0.35f;
            bodyComponent.baseDamage = 25;
            bodyComponent.levelDamage = 5f;
            bodyComponent.baseAttackSpeed = 1.8f;
            bodyComponent.levelAttackSpeed = 0.2f;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0.35f;
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
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

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
        }

        private void RegisterCharacter()
        {
            // now that the body prefab's set up, clone it here to make the display prefab
            characterDisplay = PrefabAPI.InstantiateClone(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "ExampleSurvivorDisplay", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 153);
            characterDisplay.AddComponent<NetworkIdentity>();

            // clone rex's syringe projectile prefab here to use as our own projectile
            arrowProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageIceBolt"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            arrowProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            arrowProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            arrowProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (arrowProjectile) PrefabAPI.RegisterNetworkPrefab(arrowProjectile);

            // clone FMJ's syringe projectile prefab here to use as our own projectile
            iceBombProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageIceBombProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            iceBombProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            iceBombProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            iceBombProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Freeze2s;

            // register it for networking
            if (iceBombProjectile) PrefabAPI.RegisterNetworkPrefab(iceBombProjectile);

            // clone FMJ's syringe projectile prefab here to use as our own projectile
            chargeProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirebolt"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            chargeProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            chargeProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            chargeProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (chargeProjectile) PrefabAPI.RegisterNetworkPrefab(chargeProjectile);

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            greenNProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MissileProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);

            // just setting the numbers to 1 as the entitystate will take care of those
            greenNProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            greenNProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            greenNProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            // register it for networking
            if (greenNProjectile) PrefabAPI.RegisterNetworkPrefab(greenNProjectile);
            //------------------------------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------------------------------
            // clone Lunar's syringe projectile prefab here to use as our own projectile
            meltCreeper = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirewallWalkerProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            meltCreeper.GetComponent<ProjectileDamage>().damage = 1f;
            meltCreeper.GetComponent<ProjectileController>().procCoefficient = 1f;
            meltCreeper.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;

            // register it for networking
            if (meltCreeper) PrefabAPI.RegisterNetworkPrefab(meltCreeper);
            //---------------------------------------------------------------------------------------------------------------

            //--------------------------------------------------------------------------------------------------------------
            // clone Lunar's syringe projectile prefab here to use as our own projectile
            meltCreeperC = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/MageFirewallSeedProjectile"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            meltCreeperC.GetComponent<ProjectileDamage>().damage = 1f;
            meltCreeperC.GetComponent<ProjectileController>().procCoefficient = 1f;
            meltCreeperC.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;
            

            // register it for networking
            if (meltCreeperC) PrefabAPI.RegisterNetworkPrefab(meltCreeperC);
            //---------------------------------------------------------------------------------------------------------------

            // clone Lunar's syringe projectile prefab here to use as our own projectile
            squeezeBomb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/GravSphere"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);


            // just setting the numbers to 1 as the entitystate will take care of those
            squeezeBomb.GetComponent<ProjectileDamage>().damage = 1f;
            squeezeBomb.GetComponent<ProjectileController>().procCoefficient = 1f;
            squeezeBomb.GetComponent<ProjectileDamage>().damageType = DamageType.WeakOnHit;

            // register it for networking
            if (squeezeBomb) PrefabAPI.RegisterNetworkPrefab(squeezeBomb);
            //---------------------------------------------------------------------------------------------------------------



            //--------------------------------------------------------------------------------------------------------------
            // clone Lunar's syringe projectile prefab here to use as our own projectile
            testProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/GravSphere"), "Prefabs/Projectiles/ExampleArrowProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);


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
                list.Add(arrowProjectile);
                list.Add(chargeProjectile);
                list.Add(iceBombProjectile);
                list.Add(greenNProjectile);
                list.Add(testProjectile);
                list.Add(meltCreeper);
                list.Add(meltCreeperC);
                list.Add(squeezeBomb);
            };


            // write a clean survivor description here!
            string desc = "Megaman X<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X can transform either of his arms into a powerful buster to shoot bullets of compressed solar energy, and has an energy amplifier that allows it to be charged up and release a more powerful shot." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X fires a shard of ice that can freeze and/or damage a target." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Emergency Acceleration System(DASH) is a move that temporarily speeds up the character." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > X shoots a bamboo-rocket which pursues enemies.</color>" + Environment.NewLine + Environment.NewLine;

            // add the language tokens
            LanguageAPI.Add("EXAMPLESURVIVOR_NAME", "X");
            LanguageAPI.Add("EXAMPLESURVIVOR_DESCRIPTION", desc);
            LanguageAPI.Add("EXAMPLESURVIVOR_SUBTITLE", "Megaman X, B class Hunter");

            // add our new survivor to the game~
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "EXAMPLESURVIVOR_NAME",
                unlockableName = "",
                descriptionToken = "EXAMPLESURVIVOR_DESCRIPTION",
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
            LoadoutAPI.AddSkill(typeof(ShotgunIce));
            LoadoutAPI.AddSkill(typeof(DodgeState));
            LoadoutAPI.AddSkill(typeof(greenSpinner));
        }

        void PassiveSetup()
        {
            // set up the passive skill here if you want
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PASSIVE_NAME", "Limitless Potential");
            LanguageAPI.Add("EXAMPLESURVIVOR_PASSIVE_DESCRIPTION", "<style=cIsUtility>X's true potential still unachieved, but his adaptation and improvement grows super fast</style> <style=cIsHealing>AttackPower, Life Regen, armor and shields grows on Level Up increase</style>.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "EXAMPLESURVIVOR_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "EXAMPLESURVIVOR_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }

        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME", "X-Buster");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW_DESCRIPTION", "Shoot with X-Buster, dealing <style=cIsDamage>170% damage</style>.");

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
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW_NAME";

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


            // add this code after defining a new skilldef if you're adding an alternate skill

            /*Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = newSkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(newSkillDef.skillNameToken, false, null)
            };*/
        }

        void SecondarySetup() 
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW2_NAME", "ShotgunIce");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW2_DESCRIPTION", "Shoot an IceMissle that pierce enemies, dealing <style=cIsDamage>200% damage</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(ShotgunIce));
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
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2_NAME";

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

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW2V_NAME", "Squeeze Bomb");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW2V_DESCRIPTION", "A gravity-based weapon. Creates localized black holes that hold up enemies.");

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
            mySkillDef.icon = Assets.icon5;
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2V_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2V_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW2V_NAME";

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

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME", "Dash");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_DESCRIPTION", "<style=cIsDamage>Perform a Dash</style>.");

            // set up your primary skill def here!

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(DodgeState));
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
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW3_NAME";

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

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW4_NAME", "GreenNeedle");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW4_DESCRIPTION", "Shoot a small missle tha follow some targets, dealing <style=cIsDamage>145% base damage</style>.");

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
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4_NAME";

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

            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW4V_NAME", "Melt Creeper");
            LanguageAPI.Add("EXAMPLESURVIVOR_PRIMARY_CROSSBOW4V_DESCRIPTION", " It sends a small wave of flames on the ground, dealing <style=cIsDamage>200% base damage</style>, When charged up, it sends flames both sides, dealing even more damage");

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
            mySkillDef.skillDescriptionToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4V_DESCRIPTION";
            mySkillDef.skillName = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4V_NAME";
            mySkillDef.skillNameToken = "EXAMPLESURVIVOR_PRIMARY_CROSSBOW4V_NAME";

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

            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), "ExampleSurvivorMonsterMaster", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "CreateDoppelganger", 159);

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

        public static Sprite iconP;
        public static Sprite icon1;
        public static Sprite icon2;
        public static Sprite icon3;
        public static Sprite icon4;
        public static Sprite icon5;

        public static void PopulateAssets()
        {
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

}





// the entitystates namespace is used to make the skills, i'm not gonna go into detail here but it's easy to learn through trial and error
namespace EntityStates.ExampleSurvivorStates
{
    public class chargeShot : BaseSkillState
    {
        public float damageCoefficient = 1.7f;
        public float baseDuration = 0.55f;
        public float recoil = 0.7f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");



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
            this.muzzleString = "Muzzle";
            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
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
                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.xBullet, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.arrowProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        private void FireArrowC()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.xChargeShot, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.chargeProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * 4f * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
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
                    chargingSFX = true;
                }

                if (chargeTime >= 1.8f && chargeFullSFX == false)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
                    chargeFullSFX = true;
                    LastChargeTime = chargeTime;
                }

                if ((chargeTime - LastChargeTime) >= 0.68f && chargeFullSFX == true)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
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

namespace EntityStates.ExampleSurvivorStates
{
    public class ShotgunIce : BaseSkillState
    {
        public float damageCoefficient = 2f;
        public float baseDuration = 0.75f;
        public float recoil = 0.5f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");
       

        private float duration;
        // Token: 0x06003E4C RID: 15948 RVA: 0x001043C4 File Offset: 0x001025C4
        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetSpreadBloom(0f, false);
            this.stopwatch = 0f;
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.delayBeforeFiringProjectile = this.baseDelayBeforeFiringProjectile / this.attackSpeedStat;
            Util.PlaySound(Sounds.shotgunIce, base.gameObject);
            //AkSoundEngine.PostEvent(772649499, base.gameObject);

            base.PlayAnimation("Gesture, Override", "ShotgunIceAnim", "FireArrow.playbackRate", this.duration);
        }

        // Token: 0x06003E4D RID: 15949 RVA: 0x00032FA7 File Offset: 0x000311A7
        public override void OnExit()
        {
            base.OnExit();
        }


        // Token: 0x06003E4F RID: 15951 RVA: 0x00104490 File Offset: 0x00102690
        protected virtual void Fire()
        {
            string muzzleName = "MuzzleCenter";
            base.AddRecoil(-2f * this.recoilAmplitude, -3f * this.recoilAmplitude, -1f * this.recoilAmplitude, 1f * this.recoilAmplitude);
            if (this.effectPrefab)
            {
                AkSoundEngine.PostEvent(772649499, base.gameObject);
                EffectManager.SimpleMuzzleFlash(this.effectPrefab, base.gameObject, muzzleName, false);
            }
            this.firedProjectile = true;
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay.direction = Util.ApplySpread(aimRay.direction, this.minSpread, this.maxSpread, 1f, 1f, 0f, this.projectilePitchBonus);
                ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.iceBombProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * this.damageCoefficient, this.force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }

        // Token: 0x06003E50 RID: 15952 RVA: 0x001045A0 File Offset: 0x001027A0
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch >= this.delayBeforeFiringProjectile && !this.firedProjectile)
            {
                this.Fire();
            }
            if (this.stopwatch >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        // Token: 0x06003E51 RID: 15953 RVA: 0x0000D472 File Offset: 0x0000B672
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        // Token: 0x04003987 RID: 14727
        [SerializeField]
        public GameObject effectPrefab;

        // Token: 0x04003988 RID: 14728
        [SerializeField]
        public GameObject projectilePrefab;


        // Token: 0x0400398A RID: 14730
        [SerializeField]
        public float force;

        // Token: 0x0400398B RID: 14731
        [SerializeField]
        public float minSpread;

        // Token: 0x0400398C RID: 14732
        [SerializeField]
        public float maxSpread;


        // Token: 0x0400398E RID: 14734
        [SerializeField]
        public float recoilAmplitude = 1f;

        // Token: 0x04003990 RID: 14736
        [SerializeField]
        public float projectilePitchBonus;

        // Token: 0x04003991 RID: 14737
        [SerializeField]
        public float baseDelayBeforeFiringProjectile;

        // Token: 0x04003992 RID: 14738
        private float stopwatch;

        // Token: 0x04003994 RID: 14740
        private float delayBeforeFiringProjectile;

        // Token: 0x04003995 RID: 14741
        private bool firedProjectile;

    }
   
}

namespace EntityStates.ExampleSurvivorStates
{
    public class DodgeState : BaseState
    {
        public float baseDuration = 0.75f;
        private float duration = 0.9f;
        private Animator animator;


        // Token: 0x06003E1F RID: 15903 RVA: 0x00102CA0 File Offset: 0x00100EA0
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(Sounds.xDash, base.gameObject);
            this.animator = base.GetModelAnimator();
            ChildLocator component = this.animator.GetComponent<ChildLocator>();
            if (base.isAuthority && base.inputBank && base.characterDirection)
            {
                this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            }
            Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.forwardDirection;
            Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
            float num = Vector3.Dot(this.forwardDirection, rhs);
            float num2 = Vector3.Dot(this.forwardDirection, rhs2);
            this.animator.SetFloat("forwardSpeed", num, 0.1f, Time.fixedDeltaTime);
            this.animator.SetFloat("rightSpeed", num2, 0.1f, Time.fixedDeltaTime);
            if (Mathf.Abs(num) > Mathf.Abs(num2))
            {
                base.PlayAnimation("Body", (num > 0f) ? "Dash" : "Dash", "Dodge.playbackRate", this.duration);
            }
            else
            {
                base.PlayAnimation("Body", (num2 > 0f) ? "Dash" : "Dash", "Dodge.playbackRate", this.duration);
            }
            if (DodgeState.jetEffect)
            {
                Transform transform = component.FindChild("LeftJet");
                Transform transform2 = component.FindChild("RightJet");
                if (transform)
                {
                    UnityEngine.Object.Instantiate<GameObject>(DodgeState.jetEffect, transform);
                }
                if (transform2)
                {
                    UnityEngine.Object.Instantiate<GameObject>(DodgeState.jetEffect, transform2);
                }
                
            }
            this.RecalculateRollSpeed();
            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity = this.forwardDirection * this.rollSpeed;
            }
            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;
        }

        // Token: 0x06003E20 RID: 15904 RVA: 0x00102EFD File Offset: 0x001010FD
        private void RecalculateRollSpeed()
        {
            this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(this.initialSpeedCoefficient, this.finalSpeedCoefficient, base.fixedAge / this.duration);
        }

        // Token: 0x06003E21 RID: 15905 RVA: 0x00102F2C File Offset: 0x0010112C
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateRollSpeed();
            if (base.cameraTargetParams)
            {
                //base.cameraTargetParams.fovOverride = Mathf.Lerp(DodgeState.dodgeFOV, 60f, base.fixedAge / this.duration);
                base.cameraTargetParams.fovOverride = -2.5f;
            }
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.rollSpeed;
                float y = vector.y;
                vector.y = 0f;
                float d = Mathf.Max(Vector3.Dot(vector, this.forwardDirection), 0f);
                vector = this.forwardDirection * d;
                vector.y += Mathf.Max(y, 0f);
                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        // Token: 0x06003E22 RID: 15906 RVA: 0x0010305D File Offset: 0x0010125D
        public override void OnExit()
        {
            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }
            base.OnExit();
        }

        // Token: 0x06003E23 RID: 15907 RVA: 0x00103082 File Offset: 0x00101282
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.forwardDirection);
        }

        // Token: 0x06003E24 RID: 15908 RVA: 0x00103097 File Offset: 0x00101297
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.forwardDirection = reader.ReadVector3();
        }

        // Token: 0x0400392E RID: 14638

        public float initialSpeedCoefficient = 5.8f;

        // Token: 0x0400392F RID: 14639
 
        public float finalSpeedCoefficient = 4.8f;

        // Token: 0x04003930 RID: 14640
        public static string dodgeSoundString;

        // Token: 0x04003931 RID: 14641
        public static GameObject jetEffect;

        // Token: 0x04003932 RID: 14642
        public static float dodgeFOV;

        // Token: 0x04003933 RID: 14643
        private float rollSpeed = 5.5f;

        // Token: 0x04003934 RID: 14644
        private Vector3 forwardDirection;

        // Token: 0x04003936 RID: 14646
        private Vector3 previousPosition;
    }
}


namespace EntityStates.ExampleSurvivorStates
{
    public class greenSpinner : BaseSkillState
    {
        public float damageCoefficient = 1.45f;
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
            this.muzzleString = "Muzzle";


            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireShadowR()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.greenSpinner, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.greenNProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireShadowR();
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

namespace EntityStates.ExampleSurvivorStates
{
    public class meltCreeper : BaseSkillState
    {
        public float damageCoefficient = 2f;
        public float baseDuration = 0.55f;
        public float recoil = 0.7f;
        public static GameObject tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerToolbotRebar");



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
            this.muzzleString = "Muzzle";
            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void meltCreeperFire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.meltCreeper, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.meltCreeper, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        private void meltCreeperCFire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);
                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.meltCreeper, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.meltCreeperC, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * 1.4f * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (base.inputBank.skill4.down)
            {
                chargeTime += Time.deltaTime;

                if (chargeTime > 0.5f && chargeTime <= 1.8f && chargingSFX == false)
                {
                    Util.PlaySound(Sounds.charging, base.gameObject);
                    chargingSFX = true;
                }

                if (chargeTime >= 1.8f && chargeFullSFX == false)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
                    chargeFullSFX = true;
                    LastChargeTime = chargeTime;
                }

                if ((chargeTime - LastChargeTime) >= 0.68f && chargeFullSFX == true)
                {
                    Util.PlaySound(Sounds.fullCharge, base.gameObject);
                    LastChargeTime = chargeTime;
                }
            }

            if (!base.inputBank.skill4.down)
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
                meltCreeperCFire();
            }

            if ((base.fixedAge >= this.fireDuration || !base.inputBank || !base.inputBank.skill1.down) && hasCharged == false && hasTime == true)
            {
                meltCreeperFire();
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



namespace EntityStates.ExampleSurvivorStates
{
    public class testSkill : BaseSkillState
    {
        public float damageCoefficient = 1.45f;
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
            this.muzzleString = "Muzzle";


            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireShadowR()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    //Util.PlaySound(Sounds.greenSpinner, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.testProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                FireShadowR();
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

namespace EntityStates.ExampleSurvivorStates
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
            this.muzzleString = "Muzzle";


            base.PlayAnimation("Gesture, Override", "FireArrow", "FireArrow.playbackRate", this.duration);
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
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol.effectPrefab, base.gameObject, this.muzzleString, false);

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.squeezeBomb, base.gameObject);
                    ProjectileManager.instance.FireProjectile(ExampleSurvivor.MegamanXMod.squeezeBomb, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
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
