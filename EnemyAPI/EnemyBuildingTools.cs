using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using Alexandria.ItemAPI;

namespace Alexandria.EnemyAPI
{
    public static class EnemyBuildingTools
    {
        //NEVERNAMED'S SUPER AWESOME ENEMY CREATION TOOLKIT

        /// <summary>
        /// Sets up the basic gameobject for an entity to be built around. This entity will do basically NOTHING on its own unless you use other tool methods to give it behaviours such as health and knockback.
        /// </summary>
        /// <param name="name">Name of the enemy GameObject. Does not actually define the name of the enemy for the purpose of the Ammonomicon.</param>
        /// <param name="defaultSprite">A default sprite for the entity. Needs to be the name of a valid sprite in the provided collection.</param>
        /// <param name="collection">The sprite collection containing the entity's sprites.</param>
        public static GameObject SetupEntityObject(string name, string defaultSprite, tk2dSpriteCollectionData collection)
        {
            var entity = ItemBuilder.SpriteFromBundle(defaultSprite, collection.GetSpriteIdByName(defaultSprite), collection, new GameObject(name));

            tk2dBaseSprite baseSprite = entity.GetComponent<tk2dBaseSprite>();
            baseSprite.tag = "Enemy";
            baseSprite.renderLayer = 0;
            baseSprite.renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            baseSprite.renderer.sortingOrder = 0;
            baseSprite.renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive | MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            baseSprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitCutoutUber");
            baseSprite.renderer.material.shaderKeywords = new string[] { "BRIGHTNESS_CLAMP_ON", "EMISSIVE_OFF", "PALETTE_OFF", "TINTING_ON" };

            return entity;
        }

        /// <summary>
        /// Adds an AIShooter component to the given AIActor. Necessary for creating entities which hold weapons.
        /// </summary>
        /// <param name="actor">The AIActor you're adding the AIShooter to.</param>
        /// <param name="GunAttachPoint">X and Y offsets of the held weapon. These are measured in in-game tiles. You can easily convert a pixel measurement to in-game tile distance by dividing it by 16.</param>
        /// <param name="gunID">The numerical ID of the gun the entity should hold.</param>
        /// <param name="shouldReload">Whether or not the entity should reload their gun.</param>
        /// <param name="customCooldown">How long the entity should wait between shots. If this is not set, the entity will use the gun's innate cooldown. This is overridden by special attack behaviours and bullet scripts.</param>
        /// <param name="aimTimeScale">A multiplier on how fast the entity can turn their gun to aim it at their target. At 1, the entity aims its gun at a normal speed.</param>
        /// <param name="handHasSprite">Whether or not the entity has a hand texture. Note that hand sprites don't show up on two-handed guns regardless.</param>
        /// <param name="customSpriteName">The sprite name of the entity's custom hand sprite in the given sprite collection. If left blank, the entity will re-use the Bullet Kin's hand sprite.</param>
        /// <param name="spriteCollection">The sprite collection containing the entity's hand sprite.</param>
        public static AIShooter AddAIShooter(this AIActor actor, Vector2 GunAttachPoint, int gunID = 38, bool shouldReload = true, float customCooldown = 2.5f, float aimTimeScale = 1f, bool handHasSprite = true, string customSpriteName = null, tk2dSpriteCollectionData spriteCollection = null)
        {
            string entityName = (actor && !string.IsNullOrEmpty(actor.ActorName)) ? actor.ActorName : "ENTITY_NAME_MISSING";

            GameObject gunAttach = (actor.transform.Find("GunAttachPoint") != null) ? actor.transform.Find("GunAttachPoint").gameObject : new GameObject("GunAttachPoint");
            gunAttach.transform.SetParent(actor.transform);
            gunAttach.transform.localPosition = new Vector3(GunAttachPoint.x, GunAttachPoint.y, 0f);

            AIShooter shooter = actor.gameObject.GetOrAddComponent<AIShooter>();
            shooter.AimTimeScale = aimTimeScale;
            shooter.gunAttachPoint = gunAttach.transform;
            shooter.equippedGunId = gunID;
            shooter.shouldUseGunReload = shouldReload;
            shooter.customShootCooldownPeriod = customCooldown;

            if (!string.IsNullOrEmpty(customSpriteName) && handHasSprite && spriteCollection)
            {
                GameObject hand = ItemBuilder.SpriteFromBundle(customSpriteName, spriteCollection.GetSpriteIdByName(customSpriteName), spriteCollection, new GameObject($"{entityName}Hand"));
                hand.MakeFakePrefab();
                tk2dBaseSprite handSprite = hand.GetComponent<tk2dBaseSprite>();
                handSprite.HeightOffGround = 0.15f;
                handSprite.renderLayer = 0;
                handSprite.automaticallyManagesDepth = false;
                handSprite.renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                handSprite.renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive | MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                handSprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitCutoutUber");
                handSprite.renderer.material.shaderKeywords = new string[] { "BRIGHTNESS_CLAMP_ON", "EMISSIVE_OFF", "PALETTE_OFF", "TINTING_ON" };
                PlayerHandController handController = hand.AddComponent<PlayerHandController>();
                handController.handHeightFromGun = 0.05f;
                handController.ForceRenderersOff = false;
                handController.IsPlayerPrimary = false;
                shooter.handObject = handController;
            }
            else
            {
                shooter.handObject = EnemyDatabase.GetOrLoadByGuid(handHasSprite ? "01972dee89fc4404a5c408d50007dad5" : "128db2f0781141bcb505d8f00f9e4d47").aiShooter.handObject;
            }
            return shooter;
        }

        /// <summary>
        /// Sets up the SpeculativeRigidBody component of the entity. Necessary for it to collide with other things in the environment.
        /// </summary>
        /// <param name="entity">The entity GameObject you're adding the SpecRigidBody to.</param>
        /// <param name="collisionLayers">A list of 'collisionLayers' the entity needs to collide on. For basic enemies, they need the EnemyCollider and EnemyHitBox layers.  When set up by this method, each collision layer set here becomes its own collider.</param>
        /// <param name="colliderDimensions">The dimensions of each collider. Should be in the same order as the collisionLayers list. Measured in pixels.</param>
        /// <param name="colliderOffsets">The offsets of each collider relative to the origin point of the entity. Should be in the same order as the collisionLayers list. Measured in pixels.</param>
        /// <param name="collideWithOthers">Whether this entity collides with other objects.</param>
        /// <param name="collideWithTilemap">Whether this entity collides with the tilemap.</param>
        /// <param name="canBeCarried">Whether this entity can be moved by moving platforms.</param>
        /// <param name="canBePushed">Whether this entity can be pushed by other rigidbodies.</param>
        /// <param name="canCarry">Whether this entity can carry other rigidbodies. Should probably be false unless you know what that means.</param>
        /// <param name="canPush">Whether this entity can push other rigidbodies.</param>
        /// <param name="capVelocity">Whether this entity's movement velocity is capped.</param>
        public static SpeculativeRigidbody SetupEntityRigidBody(GameObject entity, List<CollisionLayer> collisionLayers, List<IntVector2> colliderDimensions, List<IntVector2> colliderOffsets,
            bool collideWithOthers = true, bool collideWithTilemap = true, bool canBeCarried = true, bool canBePushed = false, bool canCarry = false, bool canPush = false, bool capVelocity = false)
        {
            SpeculativeRigidbody rigidBody = entity.GetOrAddComponent<SpeculativeRigidbody>();
            rigidBody.PixelColliders = new List<PixelCollider>();

            for (int i = 0; i < collisionLayers.Count(); i++)
            {
                rigidBody.PixelColliders.Add(new PixelCollider
                {
                    ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
                    CollisionLayer = collisionLayers[i],
                    IsTrigger = false,
                    BagleUseFirstFrameOnly = false,
                    SpecifyBagelFrame = string.Empty,
                    BagelColliderNumber = 0,
                    ManualOffsetX = colliderOffsets[i].x,
                    ManualOffsetY = colliderOffsets[i].y,
                    ManualWidth = colliderDimensions[i].x,
                    ManualHeight = colliderDimensions[i].y,
                    ManualDiameter = 0,
                    ManualLeftX = 0,
                    ManualLeftY = 0,
                    ManualRightX = 0,
                    ManualRightY = 0,
                });
            }
            rigidBody.CanBeCarried = canBeCarried;
            rigidBody.CanBePushed = canBePushed;
            rigidBody.CanCarry = canCarry;
            rigidBody.CanPush = canPush;
            rigidBody.CapVelocity = capVelocity;
            rigidBody.CollideWithOthers = collideWithOthers;
            rigidBody.CollideWithTileMap = collideWithTilemap;
            return rigidBody;
        }

        /// <summary>
        /// Easily adds footstep audio to every animation in the AIAnimator.MoveAnimation of your entity.
        /// </summary>
        /// <param name="animator">The entity AIAnimator you're adding footstep sounds to.</param>
        /// <param name="frames">The indexes of the frames when the footstep sound should play. All animations will use the same footstep indexes when set up this way. Remember, this counts from zero!</param>
        /// <param name="eventAudio">The audio event of the footstep sound. For most enemies this should stay as Play_FS_ENM, but you can set it to a custom sound too.</param>
        public static void AddFootstepSounds(this AIAnimator animator, int[] frames, string eventAudio = "Play_FS_ENM")
        {
            string entityName = (animator && animator.gameActor && !string.IsNullOrEmpty(animator.gameActor.ActorName)) ? animator.gameActor.ActorName : "ENTITY_NAME_MISSING";
            foreach (string animName in animator.MoveAnimation.AnimNames)
            {
                foreach (int targetFrame in frames)
                {
                    if (animator.spriteAnimator.GetClipByName(animName) == null)
                    {
                        string err = $"Error in AddFootstepSounds for {entityName}: The string '{animName}' in the given animator's movement animations does not correspond to a real animation.";
                        ETGModConsole.Log($"<color=#ff0000ff>{err}</color>");
                        Debug.LogError(err);
                    }
                    if (animator.spriteAnimator.GetClipByName(animName).frames.Length - 1 >= targetFrame)
                    {
                        tk2dSpriteAnimationFrame footstepFrame = animator.spriteAnimator.GetClipByName(animName).frames[targetFrame];
                        footstepFrame.triggerEvent = true;
                        if (string.IsNullOrEmpty(footstepFrame.eventInfo)) { footstepFrame.eventInfo = "footstep"; }
                        footstepFrame.eventAudio = "Play_FS_ENM";
                    }
                    else
                    {
                        string err = $"Error in AddFootstepSounds for {entityName}: The animation '{animName}' in the given animator's movement animations has a length ({animator.spriteAnimator.GetClipByName(animName).frames.Length - 1}) that is less than the given footstep frame {targetFrame}.";
                        ETGModConsole.Log($"<color=#ff0000ff>{err}</color>");
                        Debug.LogError(err);
                    }
                }
            }
        }

        /// <summary>
        /// Shorthand method for setting up the KnockbackDoer of an entity. 
        /// </summary>
        /// <param name="entity">The entity GameObject you're adding the KnockbackDoer to.</param>
        /// <param name="weight">The 'Weight' value of the entity, controlling how resistant they are to knockback. Red Shotgun Kin have a default weight of 50.</param>
        /// <param name="knockbackMultiplier">How much incoming knockback to this entity should be multiplied. Leave at 1 for default.</param>
        /// <param name="deathMultiplier">How much incoming knockback to this entity should be multiplied by on fatal damage. This is why some enemies seemingly get knocked back harded by a killing blow.</param>
        /// <param name="shouldBounce">Whether the enemy should bounce off walls when being knocked back.</param>
        /// <param name="collisionDecay"></param>
        /// <param name="timeScalar"></param>
        public static KnockbackDoer SetupEntityKnockback(GameObject entity, float weight = 50, float knockbackMultiplier = 1, float deathMultiplier = 5, bool shouldBounce = true, float collisionDecay = 0.5f, float timeScalar = 1f)
        {
            KnockbackDoer knockback = entity.AddComponent<KnockbackDoer>();
            knockback.weight = weight;
            knockback.shouldBounce = shouldBounce;
            knockback.knockbackMultiplier = knockbackMultiplier;
            knockback.deathMultiplier = deathMultiplier;
            knockback.collisionDecay = collisionDecay;
            knockback.timeScalar = timeScalar;
            return knockback;
        }



        /// <summary>
        /// Adds an AIBeamShooter component to an enemy. AIBeamShooters are important for enemies to be able to use beam-related behaviors.
        /// </summary>
        /// <param name="enemy">The target actor.</param>
        /// <param name="transform">The position at which the beam, when created, will fire from.</param>
        /// <param name="name">The name of your AIBeamShooter component.</param>
        /// <param name="beamProjectile">The projectile component of your beam.</param>
        /// <param name="beamModule">The projectile module which houses your beamProjectile.</param>
        /// <param name="angle">The starting angle of your beam when it is fired. Used only by specific things.</param>
        public static AIBeamShooter AddAIBeamShooter(AIActor enemy, Transform transform, string name, Projectile beamProjectile, ProjectileModule beamModule = null, float angle = 0)
        {
            AIBeamShooter bholsterbeam1 = enemy.gameObject.AddComponent<AIBeamShooter>();
            bholsterbeam1.beamTransform = transform;
            bholsterbeam1.beamModule = beamModule;
            bholsterbeam1.beamProjectile = beamProjectile.projectile;
            bholsterbeam1.firingEllipseCenter = transform.position;
            bholsterbeam1.name = name;
            bholsterbeam1.northAngleTolerance = angle;
            return bholsterbeam1;
        }
        /// <summary>
        /// Adds a directional animation to your AIAnimator (Mostly found in enemies). Directional animations control the animations that play depending on direction.
        /// </summary>
        /// <param name="animator">The target AIAnimator.</param>
        /// <param name="Prefix">The prefix of your directional animation. When making your enemy play a specific animation, use this prefix to play the animation so it also accounts for direction.</param>
        /// <param name="animationNames">All of the tk2d sprite animations in your directional animation. These HAVE to be set up in specific configurations. Check out the bottom of this page to see how https://mtgmodders.gitbook.io/etg-modding-guide/misc/making-an-enemy</param>
        /// <param name="flipType">The amount of different animations your directional animation uses. Ex: a directional aniamtion with left-right aniamtions will be 2.</param>
        /// <param name="directionType">The direction type of your animator. This will change how many angles it will take into account for. Refer to https://mtgmodders.gitbook.io/etg-modding-guide/misc/making-an-enemy for more detail.</param>

        public static DirectionalAnimation AddNewDirectionAnimation(AIAnimator animator, string Prefix, string[] animationNames, DirectionalAnimation.FlipType[] flipType, DirectionalAnimation.DirectionType directionType = DirectionalAnimation.DirectionType.Single)
        {
            DirectionalAnimation newDirectionalAnimation = new DirectionalAnimation
            {
                Type = directionType,
                Prefix = Prefix,
                AnimNames = animationNames,
                Flipped = flipType
            };
            AIAnimator.NamedDirectionalAnimation greg = new AIAnimator.NamedDirectionalAnimation
            {
                name = Prefix,
                anim = newDirectionalAnimation
            };

            if (animator.OtherAnimations == null)
            {
                animator.OtherAnimations = new List<AIAnimator.NamedDirectionalAnimation> { greg };
            }
            else { animator.OtherAnimations.Add(greg); }
            return newDirectionalAnimation;
        }

        /// <summary>
        /// Destroys any remnant hand objects on your enemy.
        /// </summary>
        /// <param name="transform">The targets Transform.</param>
        public static void DestroyUnnecessaryHandObjects(Transform transform)
        {
            foreach (Transform obj in transform.transform)
            {
                if (obj.name == "BulletSkeletonHand(Clone)")
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
        }
        /// <summary>
        /// Links a gameobject to your enemy to be its shadow.
        /// </summary>
        /// <param name="actor">The target.</param>
        /// <param name="shadowObject">The gameobject that will act as a shadow.</param>
        /// <param name="attachpoint">The position of the shadow. Must be somewhere on the enemy, so use your enemy position as your start position, as unintuitive as it seems.</param>
        /// <param name="name">The name of your shadow object.</param>

        public static void AddShadowToAIActor(AIActor actor, GameObject shadowObject, Vector2 attachpoint, string name = "shadowPosition")
        {
            actor.HasShadow = true;
            actor.ShadowPrefab = shadowObject;
            GameObject shadowPoint = new GameObject(name);
            shadowPoint.transform.parent = actor.gameObject.transform;
            shadowPoint.transform.position = attachpoint;
            actor.ShadowParent = shadowPoint.transform;
        }

        /// <summary>
        /// Generates, and returns a gameobject that can be used as a shootpoint.
        /// </summary>
        /// <param name="attacher">The target.</param>
        /// <param name="attachpoint">The position of the shoot point. Must be somewhere on the enemy, so use your enemy position as your start position, as unintuitive as it seems.</param>
        /// <param name="name">The name of your shoot point.</param>
        public static GameObject GenerateShootPoint(GameObject attacher, Vector2 attachpoint, string name = "shootPoint")
        {
            GameObject shootpoint = new GameObject(name);
            shootpoint.transform.parent = attacher.transform;
            shootpoint.transform.position = attachpoint;
            return attacher.transform.Find(name).gameObject;
        }


        /// <summary>
        /// Returns a deplicate BulletBank.Entry that you can modify the bullet Object of without altering the original. Useful for making specific projectiles fire without audio or have them have special effects i.e hitscan.
        /// </summary>
        /// <param name="entryToCopy">The enrty you are copying in the first place.</param>
        /// <param name="Name">Your new Bullet Bank entry name. You will use this name when trying to spawn a projectile from this entry.</param>
        /// <param name="AudioEvent">The audio event your projectile will sound when it is fired. Change this to "DNC" if you want to keep the original audio, or set it to null to have none.</param>
        /// <param name="muzzleflashVFX">Your muzzle flash VFX it will play when the bullet is fired.</param>
        /// <param name="ChangeMuzzleFlashToEmpty">If set to true and muzzleflashVFX is NULL, will remove the muzzleflash from your entry.</param>

        public static AIBulletBank.Entry CopyBulletBankEntry(AIBulletBank.Entry entryToCopy, string Name, string AudioEvent = null, VFXPool muzzleflashVFX = null, bool ChangeMuzzleFlashToEmpty = true)
        {
            AIBulletBank.Entry entry = CopyBulletBankFields<AIBulletBank.Entry>(entryToCopy);
            entry.Name = Name;
            Projectile projectile = UnityEngine.Object.Instantiate<GameObject>(entry.BulletObject).GetComponent<Projectile>();
            projectile.gameObject.SetLayerRecursively(18);
            projectile.transform.position = projectile.transform.position.WithZ(210.5125f);
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            entry.BulletObject = projectile.gameObject;
            if (AudioEvent != "DNC") { entry.AudioEvent = AudioEvent; }
            if (ChangeMuzzleFlashToEmpty == true && muzzleflashVFX == null) { entry.MuzzleFlashEffects = new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] }; }
            else { entry.MuzzleFlashEffects = muzzleflashVFX == null ? new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[0] } : muzzleflashVFX; }
            return entry;
        }


        private static AIBulletBank.Entry CopyBulletBankFields<T>(AIBulletBank.Entry sample2) where T : AIBulletBank.Entry
        {
            AIBulletBank.Entry sample = new AIBulletBank.Entry();
            sample.AudioEvent = sample2.AudioEvent;
            sample.AudioLimitOncePerAttack = sample2.AudioLimitOncePerAttack;
            sample.AudioLimitOncePerFrame = sample2.AudioLimitOncePerFrame;
            sample.AudioSwitch = sample2.AudioSwitch;
            sample.PlayAudio = sample2.PlayAudio;
            sample.BulletObject = sample2.BulletObject;
            sample.conditionalMinDegFromNorth = sample2.conditionalMinDegFromNorth;

            sample.DontRotateShell = sample2.DontRotateShell;
            sample.forceCanHitEnemies = sample2.forceCanHitEnemies;
            sample.MuzzleFlashEffects = sample2.MuzzleFlashEffects;
            sample.MuzzleInheritsTransformDirection = sample2.MuzzleInheritsTransformDirection;
            sample.MuzzleLimitOncePerFrame = sample2.MuzzleLimitOncePerFrame;
            sample.Name = sample2.Name;
            sample.OverrideProjectile = sample2.OverrideProjectile;
            sample.preloadCount = sample2.preloadCount;
            sample.ProjectileData = sample2.ProjectileData;
            sample.rampBullets = sample2.rampBullets;

            sample.rampStartHeight = sample2.rampStartHeight;
            sample.rampTime = sample2.rampTime;
            sample.ShellForce = sample2.ShellForce;
            sample.ShellForceVariance = sample2.ShellForceVariance;
            sample.ShellGroundOffset = sample2.ShellGroundOffset;
            sample.ShellPrefab = sample2.ShellPrefab;
            sample.ShellsLimitOncePerFrame = sample2.ShellsLimitOncePerFrame;
            sample.ShellTransform = sample2.ShellTransform;
            sample.SpawnShells = sample2.SpawnShells;
            sample.suppressHitEffectsIfOffscreen = sample2.suppressHitEffectsIfOffscreen;

            return sample;
        }
    }
}
