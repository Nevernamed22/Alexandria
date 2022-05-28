using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.EnemyAPI
{
   public static class AIActorUtility
    {
        public static void DeleteOwnedBullets(this GameActor enemy, float chancePerProjectile = 1, bool deleteBulletLimbs = false)
        {
            List<Projectile> BulletsOwnedByEnemy = new List<Projectile>();
            if (deleteBulletLimbs && enemy.aiAnimator)
            {
                BulletLimbController[] limbs = enemy.aiAnimator.GetComponentsInChildren<BulletLimbController>();
                if (limbs != null && limbs.Count() > 0)
                {
                    for (int i = (limbs.Count() - 1); i >= 0; i--)
                    {
                        UnityEngine.Object.Destroy(limbs[i]);
                    }
                }
            }
            foreach (Projectile proj in StaticReferenceManager.AllProjectiles)
            {
                if (proj && proj.Owner)
                {
                    bool ownerValid = false;
                    if (proj.Owner && proj.Owner == enemy) ownerValid = true;
                    if (proj.GetComponent<BasicBeamController>() != null)
                    {
                        if (proj.GetComponent<BasicBeamController>().Owner != null && proj.GetComponent<BasicBeamController>().Owner == enemy) ownerValid = true;
                    }

                    if ((UnityEngine.Random.value <= chancePerProjectile) && ownerValid) BulletsOwnedByEnemy.Add(proj);
                }
            }
            for (int i = (BulletsOwnedByEnemy.Count - 1); i > -1; i--)
            {
                if (BulletsOwnedByEnemy[i] != null && BulletsOwnedByEnemy[i].isActiveAndEnabled)
                {
                    BulletsOwnedByEnemy[i].DieInAir(true, false, false, true);
                    if (BulletsOwnedByEnemy[i].GetComponent<BasicBeamController>() != null)
                    {
                        BulletsOwnedByEnemy[i].GetComponent<BasicBeamController>().CeaseAttack();
                    }
                }
            }
        }
        public static void DoCorrectForWalls(this AIActor enemy)
        {
            if (PhysicsEngine.Instance.OverlapCast(enemy.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]))
            {
                Vector2 vector = enemy.transform.position.XY();
                IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
                int num = 0;
                int num2 = 1;
                for (; ; )
                {
                    for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
                    {
                        enemy.transform.position = vector + PhysicsEngine.PixelToUnit(cardinalsAndOrdinals[i] * num2);
                        enemy.specRigidbody.Reinitialize();
                        if (!PhysicsEngine.Instance.OverlapCast(enemy.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]))
                        {
                            return;
                        }
                    }
                    num2++;
                    num++;
                    if (num > 200)
                    {
                        goto Block_4;
                    }
                }
            Block_4:
                Debug.LogError("FREEZE AVERTED!  TELL RUBEL!  (you're welcome) 147");
                return;
            }
        }
        public static bool IsInMinecart(this AIActor target)
        {
            if (target && target.behaviorSpeculator)
            {
                foreach (MovementBehaviorBase behavbase in target.behaviorSpeculator.MovementBehaviors)
                {
                    if (behavbase is RideInCartsBehavior)
                    {
                        RideInCartsBehavior cartRiding = behavbase as RideInCartsBehavior;
                        bool isRidingCart = ReflectionUtility.ReflectGetField<bool>(typeof(RideInCartsBehavior), "m_ridingCart", cartRiding);
                        return isRidingCart;
                    }
                }
                return false;
            }
            else return false;
        }
        public static Vector2 ClosestPointOnEnemy(this AIActor target, Vector2 pointComparison)
        {
            Vector2 closestPointOnTarget = Vector2.zero;
            if (target.specRigidbody != null && target.specRigidbody.HitboxPixelCollider != null)
            {
                closestPointOnTarget = BraveMathCollege.ClosestPointOnRectangle(pointComparison, target.specRigidbody.HitboxPixelCollider.UnitBottomLeft, target.specRigidbody.HitboxPixelCollider.UnitDimensions);
            }
            return closestPointOnTarget;
        }
        public static Vector2 ClosestPointOnRigidBody(this SpeculativeRigidbody target, Vector2 pointComparison)
        {
            Vector2 closestPointOnTarget = Vector2.zero;
            if (target != null && target.HitboxPixelCollider != null)
            {
                closestPointOnTarget = BraveMathCollege.ClosestPointOnRectangle(pointComparison, target.HitboxPixelCollider.UnitBottomLeft, target.HitboxPixelCollider.UnitDimensions);
            }
            return closestPointOnTarget;
        }
        public static bool IsSecretlyTheMineFlayer(this AIActor target)
        {
            if (target)
            {
                foreach (AIActor maybeFlayer in StaticReferenceManager.AllEnemies)
                {
                    if (maybeFlayer && maybeFlayer.EnemyGuid == "8b0dd96e2fe74ec7bebc1bc689c0008a" && maybeFlayer.behaviorSpeculator)
                    {
                        List<MineFlayerShellGameBehavior> activeShellGames = maybeFlayer.behaviorSpeculator.FindAttackBehaviors<MineFlayerShellGameBehavior>();
                        if (activeShellGames.Count > 0)
                        {
                            foreach (MineFlayerShellGameBehavior behav in activeShellGames)
                            {
                                AIActor myBell = ReflectionUtility.ReflectGetField<AIActor>(typeof(MineFlayerShellGameBehavior), "m_myBell", behav);
                                if (myBell != null)
                                {
                                    if (myBell == target) return true;
                                }
                            }

                        }
                    }
                }
            }
            return false;
        }
        public static void ApplyGlitter(this AIActor target)
        {
            //Material material2;
            int cachedSpriteBodyCount = target.healthHaver.bodySprites.Count;
            List<tk2dBaseSprite> sprites = target.healthHaver.bodySprites;
            for (int i = 0; i < cachedSpriteBodyCount; i++)
            {
                sprites[i].usesOverrideMaterial = true;
                MeshRenderer component4 = target.healthHaver.bodySprites[i].GetComponent<MeshRenderer>();
                Material[] sharedMaterials = component4.sharedMaterials;
                Array.Resize<Material>(ref sharedMaterials, sharedMaterials.Length + 1);
                Material material = UnityEngine.Object.Instantiate<Material>(target.renderer.material);
                material.SetTexture("_MainTex", sharedMaterials[0].GetTexture("_MainTex"));
                sharedMaterials[sharedMaterials.Length - 1] = material;
                component4.sharedMaterials = sharedMaterials;
                sharedMaterials[sharedMaterials.Length - 1].shader = ShaderCache.Acquire("Brave/Internal/GlitterPassAdditive");
            }
        }
    }
}
