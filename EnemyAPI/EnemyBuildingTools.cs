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
        /// Adds a directional animation to your AIAnimator (Mostly found in enemies). Directional animations control, the animations that play depending on direction.
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
    }
}
