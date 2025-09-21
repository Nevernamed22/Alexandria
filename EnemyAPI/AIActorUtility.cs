using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using Alexandria.ItemAPI;

namespace Alexandria.EnemyAPI
{
    public static class AIActorUtility
    {
        /// <summary>
        /// Deletes all bullets belonging to the GameActor.
        /// </summary>
        /// <param name="enemy">The target actor.</param>
        /// <param name="chancePerProjectile">The individual chance per bullet for it to be deleted. 100% by default.</param>
        /// <param name="deleteBulletLimbs">If true 'bullet limbs' belonging to the actor will also be deleted. Bullet limbs exist on Misfire Beasts, Revolvenants, and others.</param>
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

        /// <summary>
        /// Updates the position of the AIActor to hopefully prevent it from becoming stuck in a wall. NOT FOOLPROOF.
        /// </summary>
        /// <param name="enemy">The target AIActor.</param>       
        public static void DoCorrectForWalls(this AIActor enemy)
        {
            if (!PhysicsEngine.Instance.OverlapCast(enemy.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]))
                return;

            Vector2 vector = enemy.transform.position.XY();
            IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
            for (int num2 = 1; num2 <= 201; ++num2)
            {
                for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
                {
                    enemy.transform.position = vector + PhysicsEngine.PixelToUnit(cardinalsAndOrdinals[i] * num2);
                    enemy.specRigidbody.Reinitialize();
                    if (!PhysicsEngine.Instance.OverlapCast(enemy.specRigidbody, null, true, false, null, null, false, null, null, new SpeculativeRigidbody[0]))
                        return;
                }
            }
            Debug.LogError("FREEZE AVERTED!  TELL RUBEL!  (you're welcome) 147");
            return;
        }

        /// <summary>
        /// Returns true if the AIActor is riding in a minecart.
        /// </summary>
        /// <param name="target">The target AIActor.</param>
        public static bool IsInMinecart(this AIActor target)
        {
            if (!target || !target.behaviorSpeculator)
                return false;
            foreach (MovementBehaviorBase behavbase in target.behaviorSpeculator.MovementBehaviors)
                if (behavbase is RideInCartsBehavior)
                    return (behavbase as RideInCartsBehavior).m_ridingCart;
            return false;
        }

        /// <summary>
        /// Returns The closest Vector2 position on the enemy's hitbox to the given position.
        /// </summary>
        /// <param name="target">The target AIActor.</param>
        /// <param name="pointComparison">The Vector2 position to which the closest point on the enemy's hitbox should be calculated.</param>
        public static Vector2 ClosestPointOnEnemy(this AIActor target, Vector2 pointComparison)
        {
            Vector2 closestPointOnTarget = Vector2.zero;
            if (target.specRigidbody != null && target.specRigidbody.HitboxPixelCollider != null)
            {
                closestPointOnTarget = BraveMathCollege.ClosestPointOnRectangle(pointComparison, target.specRigidbody.HitboxPixelCollider.UnitBottomLeft, target.specRigidbody.HitboxPixelCollider.UnitDimensions);
            }
            return closestPointOnTarget;
        }

        /// <summary>
        /// Returns The closest Vector2 position on the SpeculativeRigidbody's hitbox to the given position.
        /// </summary>
        /// <param name="target">The target SpeculativeRigidbody.</param>
        /// <param name="pointComparison">The Vector2 position to which the closest point on the SpeculativeRigidbody's hitbox should be calculated.</param>
        public static Vector2 ClosestPointOnRigidBody(this SpeculativeRigidbody target, Vector2 pointComparison)
        {
            Vector2 closestPointOnTarget = Vector2.zero;
            if (target != null && target.HitboxPixelCollider != null)
            {
                closestPointOnTarget = BraveMathCollege.ClosestPointOnRectangle(pointComparison, target.HitboxPixelCollider.UnitBottomLeft, target.HitboxPixelCollider.UnitDimensions);
            }
            return closestPointOnTarget;
        }

        /// <summary>
        /// Returns true AIActor is 'secretly' the Mine Flayer as part of the Mine Flayer's bell shell-game attack.
        /// </summary>
        /// <param name="target">The AIActor to be checked.</param>
        public static bool IsSecretlyTheMineFlayer(this AIActor target)
        {
            if (!target)
                return false;

            foreach (AIActor maybeFlayer in StaticReferenceManager.AllEnemies)
            {
                if (!maybeFlayer || maybeFlayer.EnemyGuid != EnemyGUIDs.Mine_Flayer_GUID || !maybeFlayer.behaviorSpeculator)
                    continue;
                List<MineFlayerShellGameBehavior> activeShellGames = maybeFlayer.behaviorSpeculator.FindAttackBehaviors<MineFlayerShellGameBehavior>();
                if (activeShellGames.Count == 0)
                    continue;
                foreach (MineFlayerShellGameBehavior behav in activeShellGames)
                    if (behav.m_myBell == target)
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Applies the 'Glitter' visual effect from the Mailbox to the AIActor.
        /// </summary>
        /// <param name="target">The AIActor to be glittered.</param>
        public static void ApplyGlitter(this AIActor target)
        {
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
            target.HasBeenGlittered = true;
        }

        /// <summary>
        /// Causes a Genie to punch the AIActor, like the effect of the Magic Lamp.
        /// </summary>
        /// <param name="enemy">The AIActor to be punched.</param>
        /// <param name="owner">The owner of the Genie.</param>
        public static void DoGeniePunch(this AIActor enemy, PlayerController owner)
        {
            if (geniePunchFacilitater == null)
            {
                geniePunchFacilitater = ((Gun)ETGMod.Databases.Items[0]).DefaultModule.projectiles[0].InstantiateAndFakeprefab();
                geniePunchFacilitater.baseData.damage = 0;
                geniePunchFacilitater.baseData.force = 0;
                geniePunchFacilitater.baseData.speed = 0;
                geniePunchFacilitater.baseData.range = 0;
                geniePunchFacilitater.pierceMinorBreakables = true;
                geniePunchFacilitater.collidesWithPlayer = false;
                geniePunchFacilitater.sprite.renderer.enabled = false;
            }
            if (enemy && enemy.behaviorSpeculator)
            {
                enemy.behaviorSpeculator.Stun(1f, false);
                for (int i = 0; i < 3; i++)
                {
                    GameObject gameObject = SpawnManager.SpawnProjectile(geniePunchFacilitater.gameObject, enemy.sprite.WorldCenter, Quaternion.Euler(0f, 0f, 0f), true);
                    Projectile component = gameObject.GetComponent<Projectile>();
                    component.IgnoreTileCollisionsFor(10);
                    component.Owner = owner;
                }
            }
        }
        private static Projectile geniePunchFacilitater;

        /// <summary>
        /// Returns a directional animation on the AIAnimator corresponding to the given animation name.
        /// </summary>
        /// <param name="self">The AIAnimator to be checked.</param>
        /// <param name="animName">The name of the animation being checked for.</param>
        public static DirectionalAnimation GetDirectionalAnimation(this AIAnimator self, string animName)
        {
            if (string.IsNullOrEmpty(animName))
            {
                return null;
            }
            if (animName.Equals("idle", StringComparison.OrdinalIgnoreCase))
            {
                return self.IdleAnimation;
            }
            if (animName.Equals("move", StringComparison.OrdinalIgnoreCase))
            {
                return self.MoveAnimation;
            }
            if (animName.Equals("talk", StringComparison.OrdinalIgnoreCase))
            {
                return self.TalkAnimation;
            }
            if (animName.Equals("hit", StringComparison.OrdinalIgnoreCase))
            {
                return self.HitAnimation;
            }
            if (animName.Equals("flight", StringComparison.OrdinalIgnoreCase))
            {
                return self.FlightAnimation;
            }
            DirectionalAnimation result = null;
            int num = 0;
            for (int i = 0; i < self.OtherAnimations.Count; i++)
            {
                if (animName.Equals(self.OtherAnimations[i].name, StringComparison.OrdinalIgnoreCase))
                {
                    num++;
                    result = self.OtherAnimations[i].anim;
                }
            }
            if (num == 0)
            {
                return null;
            }
            if (num == 1)
            {
                return result;
            }
            int num2 = UnityEngine.Random.Range(0, num);
            num = 0;
            for (int j = 0; j < self.OtherAnimations.Count; j++)
            {
                if (animName.Equals(self.OtherAnimations[j].name, StringComparison.OrdinalIgnoreCase))
                {
                    if (num == num2)
                    {
                        return self.OtherAnimations[j].anim;
                    }
                    num++;
                }
            }
            return null;
        }

        /// <summary>
        /// A more advanced method of transmogrifying an enemy into another enemy. Returns the resulting enemy. Returns null if the original enemy could not be transmogrified.
        /// </summary>
        /// <param name="startEnemy">The enemy who will be transmogrified.</param>
        /// <param name="EnemyPrefab">The prefab of the enemy that the original will be transmogrified into.</param>
        /// <param name="EffectVFX">The VFX to be played at the position of the transmogrification.</param>
        /// <param name="audioEvent">The audio to be played when the transmogrification occurs.</param>
        /// <param name="ignoreAlreadyTransmogged">If true, will be unable to transmogrify enemies who have already been transmogrified.</param>
        /// <param name="guidsToIgnore">If the target's guid is present in this list, it cannot be transmogrified.</param>
        /// <param name="tagsToIgnore">If the target has any of the tags present in this list, it cannot be transmogrified.</param>
        /// <param name="defuseExplosives">If true, enemies set to explode upon death will not explode when transmogrified.</param>
        /// <param name="carryOverRewards">If true, the loot drops of the old enemy will carry over to the new enemy.</param>
        /// <param name="maintainHealthPercent">If true, the new enemy's percentage of remaining HP will be the same as the old enemy's percentage of remaining HP.</param>
        /// <param name="maintainsJammedState">If true, the jamedness of the new enemy depends on the jamedness of the old enemy.</param>
        /// <param name="giveIsTransmogrifiedBool">If true, sets the new actor's 'IsTransmogrified' bool to true. If false, there will be no way to tell that the actor has been transmogrified.</param>
        /// <param name="logEverything">If true, non-essential information about why enemies were unable to be transmogged will be added to the log. Do not leave true in release builds.</param>
        public static AIActor AdvancedTransmogrify(this AIActor startEnemy, AIActor EnemyPrefab, GameObject EffectVFX, string audioEvent = "Play_ENM_wizardred_appear_01", bool ignoreAlreadyTransmogged = false, List<string> guidsToIgnore = null, List<string> tagsToIgnore = null, bool defuseExplosives = true, bool carryOverRewards = false, bool maintainHealthPercent = false, bool maintainsJammedState = false, bool giveIsTransmogrifiedBool = true, bool logEverything = false)
        {
            if (startEnemy == null) { Debug.LogError("Tried to transmog a null enemy!"); return null; }
            if (EnemyPrefab == null) { Debug.LogError("Tried to transmog to a null prefab!"); return null; }
            if (startEnemy.EnemyGuid == EnemyPrefab.EnemyGuid)
            {
                if (logEverything) Debug.Log($"Tried to transmog an enemy into an actor with the same guid!");
                return null;
            }
            if (ignoreAlreadyTransmogged && startEnemy.IsTransmogrified)
            {
                if (logEverything) Debug.Log("Tried to transmog an enemy that has already been transmogged.");
                return null;
            }
            if (!startEnemy.healthHaver || startEnemy.healthHaver.IsBoss || startEnemy.ParentRoom == null)
            {
                if (logEverything) Debug.Log("Either the healthhaver or parent room on the target was null, or they were a boss!");
                return null;
            }
            if (tagsToIgnore != null && startEnemy.HasTags(tagsToIgnore))
            {
                if (logEverything) Debug.Log("Tried to transmog an enemy with a forbidden tag.");
                return null;
            }
            if (guidsToIgnore != null && guidsToIgnore.Contains(startEnemy.EnemyGuid))
            {
                if (logEverything) Debug.Log("Tried to transmog an enemy with a forbidden guid.");
                return null;
            }

            Vector2 centerPosition = startEnemy.CenterPosition;
            if (EffectVFX != null)
            {
                SpawnManager.SpawnVFX(EffectVFX, centerPosition, Quaternion.identity);
            }

            AIActor aiactor = AIActor.Spawn(EnemyPrefab, centerPosition.ToIntVector2(VectorConversions.Floor), startEnemy.ParentRoom, true, AIActor.AwakenAnimationType.Default, true);
            if (aiactor)
            {
                if (giveIsTransmogrifiedBool) aiactor.IsTransmogrified = true;
                if (maintainHealthPercent)
                {
                    float healthPercent = startEnemy.healthHaver.GetCurrentHealthPercentage();
                    float aiactorHP = aiactor.healthHaver.GetMaxHealth();
                    float resultHP = aiactorHP * healthPercent;
                    aiactor.healthHaver.ForceSetCurrentHealth(resultHP);
                }
            }

            if (!string.IsNullOrEmpty(audioEvent)) AkSoundEngine.PostEvent(audioEvent, startEnemy.gameObject);

            if (maintainsJammedState)
            {
                if (startEnemy.IsBlackPhantom && !aiactor.IsBlackPhantom) aiactor.BecomeBlackPhantom();
                if (!startEnemy.IsBlackPhantom && aiactor.IsBlackPhantom) aiactor.UnbecomeBlackPhantom();
            }

            if (defuseExplosives && startEnemy.GetComponent<ExplodeOnDeath>() != null)
            {
                UnityEngine.Object.Destroy(startEnemy.GetComponent<ExplodeOnDeath>());
            }

            if (carryOverRewards && aiactor)
            {
                aiactor.CanDropCurrency = startEnemy.CanDropCurrency;
                aiactor.AssignedCurrencyToDrop = startEnemy.AssignedCurrencyToDrop;
                aiactor.AdditionalSafeItemDrops = startEnemy.AdditionalSafeItemDrops;
                aiactor.AdditionalSimpleItemDrops = startEnemy.AdditionalSimpleItemDrops;
                aiactor.AdditionalSingleCoinDropChance = startEnemy.AdditionalSingleCoinDropChance;
                aiactor.CanDropDuplicateItems = startEnemy.CanDropDuplicateItems;
                aiactor.CanDropItems = startEnemy.CanDropItems;
                aiactor.ChanceToDropCustomChest = startEnemy.ChanceToDropCustomChest;
                aiactor.CustomLootTableMaxDrops = startEnemy.CustomLootTableMaxDrops;
                aiactor.CustomLootTableMinDrops = startEnemy.CustomLootTableMinDrops;
                aiactor.CustomLootTable = startEnemy.CustomLootTable;
                aiactor.SpawnLootAtRewardChestPos = startEnemy.SpawnLootAtRewardChestPos;
                if (startEnemy.GetComponent<KeyBulletManController>())
                {
                    KeyBulletManController controller = startEnemy.GetComponent<KeyBulletManController>();
                    int numberOfIterations = 1;
                    if (startEnemy.IsBlackPhantom && controller.doubleForBlackPhantom) numberOfIterations++;
                    for (int i = 0; i < numberOfIterations; i++)
                    {
                        GameObject objToAdd = null;
                        if (controller.lootTable) objToAdd = controller.lootTable.SelectByWeight(false);
                        else if (controller.lookPickupId > -1) objToAdd = PickupObjectDatabase.GetById(controller.lookPickupId).gameObject;
                        if (objToAdd != null)
                        {
                            aiactor.AdditionalSafeItemDrops.Add(objToAdd.GetComponent<PickupObject>());
                        }
                    }
                }
                startEnemy.EraseFromExistence(false);
            }
            else
            {
                startEnemy.EraseFromExistenceWithRewards(false);
            }
            return aiactor;
        }
    }
}
