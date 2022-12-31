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
