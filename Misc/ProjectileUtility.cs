using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Dungeonator;

namespace Alexandria.Misc
{
   public static class ProjectileUtility
    {
        //Extremely important misc methods

        /// <summary>
        /// Fetches the PlayerController of the player who owns the projectile. If the projectile is not owned by a player, returns null.
        /// </summary>
        /// <param name="bullet">The target projectile</param>
        public static PlayerController ProjectilePlayerOwner(this Projectile bullet)
        {
            if (bullet && bullet.Owner && bullet.Owner is PlayerController) return bullet.Owner as PlayerController;
            else return null;
        }

        //Methods related to spawning and firing Projectiles

        /// <summary>
        /// Instantiates a projectile and fires it in the direction of a given Vector2 position. Returns the instantiated projectile for further modification.
        /// </summary>
        /// <param name="projectile">The projectile prefab to be instantiated.</param>
        /// <param name="startingPosition">The position the projectile should be spawned at.</param>
        /// <param name="targetPosition">The target position that the projectile should be fired towards.</param>
        /// <param name="angleOffset">The amount of degrees that the projectile's trajectory should be offset by. For example, '45' will cause the projectile to be fired 45 degrees to the left of the target. Leave 0 for a direct shot.</param>
        /// <param name="angleVariance">The amount of degrees the projectile's angle can vary from the final angle. Essentially accuracy/spread.</param>
        /// <param name="playerToScaleAccuracyOff">If set to a player controller, that player's accuracy stat will affect the angle variance of the instantiated projectile.</param>
        public static GameObject InstantiateAndFireTowardsPosition(this Projectile projectile, Vector2 startingPosition, Vector2 targetPosition, float angleOffset = 0, float angleVariance = 0, PlayerController playerToScaleAccuracyOff = null)
        {
            Vector2 dirVec = (targetPosition - startingPosition);
            if (angleOffset != 0)
            {
                dirVec = dirVec.Rotate(angleOffset);
            }
            if (angleVariance != 0)
            {
                if (playerToScaleAccuracyOff != null) angleVariance *= playerToScaleAccuracyOff.stats.GetStatValue(PlayerStats.StatType.Accuracy);
                float positiveVariance = angleVariance * 0.5f;
                float negativeVariance = positiveVariance * -1f;
                float finalVariance = UnityEngine.Random.Range(negativeVariance, positiveVariance);
                dirVec = dirVec.Rotate(finalVariance);
            }
            return SpawnManager.SpawnProjectile(projectile.gameObject, startingPosition, Quaternion.Euler(0f, 0f, dirVec.ToAngle()), true);
        }

        /// <summary>
        /// Instantiates a projectile and fires it along a given angle. Returns the instantiated projectile for further modification.
        /// </summary>
        /// <param name="projectile">The projectile prefab to be instantiated.</param>
        /// <param name="startingPosition">The position the projectile should be spawned at.</param>
        /// <param name="angle">The angle the projectile should be fired. 0 corresponds with directly to the right. 180 corresponds with directly to the left.</param>
        /// <param name="angleVariance">The amount of degrees the projectile's angle can vary from the given angle. Essentially accuracy/spread.</param>
        /// <param name="playerToScaleAccuracyOff">If set to a player controller, that player's accuracy stat will affect the angle variance of the instantiated projectile.</param>
        public static GameObject InstantiateAndFireInDirection(this Projectile projectile, Vector2 startingPosition, float angle, float angleVariance = 0, PlayerController playerToScaleAccuracyOff = null)
        {                      
            if (angleVariance != 0)
            {
                if (playerToScaleAccuracyOff != null) angleVariance *= playerToScaleAccuracyOff.stats.GetStatValue(PlayerStats.StatType.Accuracy);
                float positiveVariance = angleVariance * 0.5f;
                float negativeVariance = positiveVariance * -1f;
                float finalVariance = UnityEngine.Random.Range(negativeVariance, positiveVariance);
                angle += finalVariance;
            }
            return SpawnManager.SpawnProjectile(projectile.gameObject, startingPosition, Quaternion.Euler(0f, 0f, angle), true);
        }
      
        /// <summary>
        /// Returns the RoomHandler of the room that the projectile is inside.
        /// </summary>
        /// <param name="bullet">The target projectile.</param>
        public static RoomHandler GetAbsoluteRoom(this Projectile bullet)
        {
            Vector2 bulletPosition = bullet.sprite ? bullet.sprite.WorldCenter : (Vector2)bullet.transform.position;
            IntVector2 bulletPositionIntVector2 = bulletPosition.ToIntVector2();
            return GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(bulletPositionIntVector2);
        }

        /// <summary>
        /// Sends the bullet flying in a random direction.
        /// </summary>
        /// <param name="bullet">The target projectile.</param>
        public static void SendInRandomDirection(this Projectile bullet)
        {
            if (bullet == null) { return; }
            Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
            bullet.SendInDirection(dirVec, false, true);
        }

        /// <summary>
        /// Returns a vector corresponding to the direction of the nearest enemy to the projectile's position. Returns Vector2.zero if the Projectile is null
        /// </summary>
        /// <param name="bullet">The target projectile</param>
        /// <param name="checkIsWorthShooting">If true, the projectile will ignore enemies with IsWorthShootingAt set to false, such as Mountain Cubes.</param>
        /// <param name="type">Determines whether or not the projectile should take into account if an enemy needs to be killed for room clear.</param>
        /// <param name="overrideValidityCheck">A function which allows for the setting of custom parameters for whether or not an enemy is valid.</param>
        /// <param name="excludedActors">Enemies that are in this list will not be taken into account.</param>

        public static Vector2 GetVectorToNearestEnemy(this Projectile bullet, bool checkIsWorthShooting = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, List<AIActor> excludedActors = null, Func<AIActor, bool> overrideValidityCheck = null)
        {
            if (bullet == null) { return Vector2.zero; }
            IntVector2 bulletPositionIntVector2 = bullet.sprite != null ? bullet.sprite.WorldCenter.ToIntVector2() : bullet.specRigidbody.UnitCenter.ToIntVector2();
            AIActor closestToPosition = bulletPositionIntVector2.GetNearestEnemyToPosition(checkIsWorthShooting, type, excludedActors, overrideValidityCheck);
            if (closestToPosition) return closestToPosition.CenterPosition - (bullet.sprite != null ? bullet.sprite.WorldCenter : bullet.specRigidbody.UnitCenter);
            else return Vector2.zero;
        }

        /// <summary>
        /// Changes the trajectory of the targeted projectile and assigns it a new owner, 'reflecting' it as with the effect of Rolling Eye.
        /// </summary>
        /// <param name="p">The target projectile</param>
        /// <param name="retargetReflectedBullet">If true, the reflected bullet will be automatically sent back in the direction of it's previous owner.</param>
        /// <param name="newOwner">The intended new owner of the projectile. Can be a player or an enemy.</param>
        /// <param name="minReflectedBulletSpeed">The minimum speed of the projectile once it is reflected. If it's current speed is less than the minimum, it will be accelerated to the minimum.</param>
        /// <param name="doPostProcessing">If true, and the newOwner is a player, the reflected bullet will scale with the new owner's stats and will be post processed by their items.</param>
        /// <param name="scaleModifier">A scale multiplier which will be applied to the projectile as it is reflected.</param>
        /// <param name="baseDamage">The damage that the reflected projectile should deal.</param>
        /// <param name="spread">How many degrees the projectile's trajectory can vary by if it is retargeted towards it's previous owner.</param>
        /// <param name="sfx">A sound effect which is played when the projectile is reflected.</param>
        public static void ReflectBullet(this Projectile p, bool retargetReflectedBullet, GameActor newOwner, float minReflectedBulletSpeed, bool doPostProcessing = false, float scaleModifier = 1f, float baseDamage = 10f, float spread = 0f, string sfx = null)
        {
            p.RemoveBulletScriptControl();
            if (sfx != null) AkSoundEngine.PostEvent(sfx, GameManager.Instance.gameObject);
            if (retargetReflectedBullet && p.Owner && p.Owner.specRigidbody)
            {
                p.Direction = (p.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) - p.specRigidbody.UnitCenter).normalized;
            }
            if (spread != 0f) p.Direction = p.Direction.Rotate(UnityEngine.Random.Range(-spread, spread));
            if (p.Owner && p.Owner.specRigidbody) p.specRigidbody.DeregisterSpecificCollisionException(p.Owner.specRigidbody);

            p.Owner = newOwner;
            p.SetNewShooter(newOwner.specRigidbody);
            p.allowSelfShooting = false;
            if (newOwner is AIActor)
            {
                p.collidesWithPlayer = true;
                p.collidesWithEnemies = false;
            }
            else if (newOwner is PlayerController)
            {
                p.collidesWithPlayer = false;
                p.collidesWithEnemies = true;
            }

            if (scaleModifier != 1f)
            {
                SpawnManager.PoolManager.Remove(p.transform);
                p.RuntimeUpdateScale(scaleModifier);
            }
            if (p.Speed < minReflectedBulletSpeed) p.Speed = minReflectedBulletSpeed;
            p.baseData.damage = baseDamage;
            if (doPostProcessing)
                if (doPostProcessing && newOwner is PlayerController)
                {
                    PlayerController player = (newOwner as PlayerController);
                    if (player != null)
                    {
                        p.baseData.damage *= player.stats.GetStatValue(PlayerStats.StatType.Damage);
                        p.baseData.speed *= player.stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
                        p.UpdateSpeed();
                        p.baseData.force *= player.stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
                        p.baseData.range *= player.stats.GetStatValue(PlayerStats.StatType.RangeMultiplier);
                        p.BossDamageMultiplier *= player.stats.GetStatValue(PlayerStats.StatType.DamageToBosses);
                        p.RuntimeUpdateScale(player.stats.GetStatValue(PlayerStats.StatType.PlayerBulletScale));
                        player.DoPostProcessProjectile(p);
                    }
                }
            if (newOwner is AIActor)
            {
                p.baseData.damage = 0.5f;
                p.baseData.SetAll((newOwner as AIActor).bulletBank.GetBullet("default").ProjectileData);
                p.specRigidbody.CollideWithTileMap = false;
                p.ResetDistance();
                p.collidesWithEnemies = (newOwner as AIActor).CanTargetEnemies;
                p.collidesWithPlayer = true;
                p.UpdateCollisionMask();
                p.sprite.color = new Color(1f, 0.1f, 0.1f);
                p.MakeLookLikeEnemyBullet(true);
                p.RemovePlayerOnlyModifiers();
                if ((newOwner as AIActor).IsBlackPhantom)
                {
                    p.baseData.damage = 1;
                    p.BecomeBlackBullet();
                }
            }

            p.UpdateCollisionMask();
            p.Reflected();
            p.SendInDirection(p.Direction, true, true);
        }

        //Methods related to setting up projectile prefabs

        /// <summary>
        /// Automatically instantiates and fakeprefabs the first projectile of the default module of the given gun ID and returns the new fake prefab.
        /// </summary>
        /// <param name="id">The gun ID whose first, default projectile you wish to return a clone of</param>
        public static Projectile SetupProjectile(int id)
        {
            Projectile proj = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(id) as Gun).DefaultModule.projectiles[0]);
            proj.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(proj.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(proj);

            return proj;
        }

        /// <summary>
        /// Automatically instantiates and fakeprefabs the given projectile and returns the new fake prefab.
        /// </summary>
        /// <param name="projToCopy">The original projectile which you intend to return a clone of</param>
        public static Projectile InstantiateAndFakeprefab(this Projectile projToCopy)
        {
            GameObject instantiatedTarget = projToCopy.gameObject.InstantiateAndFakeprefab();
            return instantiatedTarget.GetComponent<Projectile>();
        }

        /// <summary>
        /// Clones an existing ShadeProjModifier and applies it to the target projectile. ShadeProjModifiers are visual effects like that of the Tangler.
        /// </summary>
        /// <param name="self">The target projectile</param>
        /// <param name="shaderToClone">The original ShaderProjModifier which you intend to clone onto the target.</param>
        public static ShaderProjModifier ApplyClonedShaderProjModifier(this Projectile self, ShaderProjModifier shaderToClone)
        {
            ShaderProjModifier tanglify = self.gameObject.AddComponent<ShaderProjModifier>();
            tanglify.ProcessProperty = shaderToClone.ProcessProperty;
            tanglify.ShaderProperty = shaderToClone.ShaderProperty;
            tanglify.StartValue = shaderToClone.StartValue;
            tanglify.EndValue = shaderToClone.EndValue;
            tanglify.LerpTime = shaderToClone.LerpTime;
            tanglify.ColorAttribute = shaderToClone.ColorAttribute;
            tanglify.GlobalSparks = shaderToClone.GlobalSparks;
            tanglify.StartColor = shaderToClone.StartColor;
            tanglify.EndColor = shaderToClone.EndColor;
            tanglify.OnDeath = shaderToClone.OnDeath;
            tanglify.PreventCorpse = shaderToClone.PreventCorpse;
            tanglify.DisablesOutlines = shaderToClone.DisablesOutlines;
            tanglify.EnableEmission = shaderToClone.EnableEmission;
            tanglify.GlobalSparksColor = shaderToClone.GlobalSparksColor;
            tanglify.GlobalSparksForce = shaderToClone.GlobalSparksForce;
            tanglify.GlobalSparksOverrideLifespan = shaderToClone.GlobalSparksOverrideLifespan;
            tanglify.AddMaterialPass = shaderToClone.AddMaterialPass;
            tanglify.AddPass = shaderToClone.AddPass;
            tanglify.IsGlitter = shaderToClone.IsGlitter;
            tanglify.ShouldAffectBosses = shaderToClone.ShouldAffectBosses;
            tanglify.AddsEncircler = shaderToClone.AddsEncircler;
            tanglify.AppliesLocalSlowdown = shaderToClone.AppliesLocalSlowdown;
            tanglify.LocalTimescaleMultiplier = shaderToClone.LocalTimescaleMultiplier;
            tanglify.AppliesParticleSystem = shaderToClone.AppliesParticleSystem;
            tanglify.ParticleSystemToSpawn = shaderToClone.ParticleSystemToSpawn;
            tanglify.GlobalSparkType = shaderToClone.GlobalSparkType;
            tanglify.GlobalSparksRepeat = shaderToClone.GlobalSparksRepeat;
            return tanglify;
        }

        //Relatively unimportant Misc methods

        /// <summary>
        /// Prevents the bullet from pooling and being recycled by enemies. Use on enemy bullets that you apply strange effects to.
        /// </summary>
        /// <param name="proj">The target projectile.</param>
        public static void RemoveFromPool(this Projectile proj)
        {
            SpawnManager.PoolManager.Remove(proj.transform);
        }

        /// <summary>
        /// Easily converts a bullet to the Helix Bullets projectile motion, including compensating for the presence of orbital bullets.
        /// </summary>
        /// <param name="bullet">The target projectile.</param>
        /// <param name="isInverted">Whether or not the helix motion should go left or right to start off.</param>
        public static void ConvertToHelixMotion(this Projectile bullet, bool isInverted)
        {
            if (bullet.OverrideMotionModule != null && bullet.OverrideMotionModule is OrbitProjectileMotionModule)
            {
                OrbitProjectileMotionModule orbitProjectileMotionModule = bullet.OverrideMotionModule as OrbitProjectileMotionModule;
                orbitProjectileMotionModule.StackHelix = true;
                orbitProjectileMotionModule.ForceInvert = isInverted;
            }
            else if (!isInverted)
            {
                bullet.OverrideMotionModule = new HelixProjectileMotionModule();
            }
            else
            {
                bullet.OverrideMotionModule = new HelixProjectileMotionModule
                {
                    ForceInvert = true
                };
            }
        }

        /// <summary>
        /// Applies companion modifiers easily to a target projectile. By default includes the multipliers from Battle Standard and the Lute.
        /// </summary>
        /// <param name="bullet">The target projectile</param>
        /// <param name="owner">The player whose companion modifiers should be taken into account.</param>
        public static void ApplyCompanionModifierToBullet(this Projectile bullet, PlayerController owner)
        {
            if (PassiveItem.IsFlagSetForCharacter(owner, typeof(BattleStandardItem)))
            {
                bullet.baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
            }
            if (owner.CurrentGun && owner.CurrentGun.LuteCompanionBuffActive)
            {
                bullet.baseData.damage *= 2f;
                bullet.RuntimeUpdateScale(1f / bullet.AdditionalScaleMultiplier);
                bullet.RuntimeUpdateScale(1.75f);
            }
        }

        /// <summary>
        /// Conglomerates all status effects that a projectile will apply into a single list.
        /// </summary>
        /// <param name="bullet">The target projectile</param>
        /// <param name="ignoresProbability">If false, the method will only return a status effect if a random value is equal or less than it's chance to apply. If true, it will ignore the status effect's chance to apply and return it regardless.</param>
        public static List<GameActorEffect> GetFullListOfStatusEffects(this Projectile bullet, bool ignoresProbability = false)
        {
            List<GameActorEffect> Effects = new List<GameActorEffect>();
            if (bullet.statusEffectsToApply.Count > 0)
            {
                Effects.AddRange(bullet.statusEffectsToApply);
            }
            if (bullet.AppliesBleed && (UnityEngine.Random.value <= bullet.BleedApplyChance || ignoresProbability)) Effects.Add(bullet.bleedEffect);
            if (bullet.AppliesCharm && (UnityEngine.Random.value <= bullet.CharmApplyChance || ignoresProbability)) Effects.Add(bullet.charmEffect);
            if (bullet.AppliesCheese && (UnityEngine.Random.value <= bullet.CheeseApplyChance || ignoresProbability)) Effects.Add(bullet.cheeseEffect);
            if (bullet.AppliesFire && (UnityEngine.Random.value <= bullet.FireApplyChance || ignoresProbability)) Effects.Add(bullet.fireEffect);
            if (bullet.AppliesFreeze && (UnityEngine.Random.value <= bullet.FreezeApplyChance || ignoresProbability)) Effects.Add(bullet.freezeEffect);
            if (bullet.AppliesPoison && (UnityEngine.Random.value <= bullet.PoisonApplyChance || ignoresProbability)) Effects.Add(bullet.healthEffect);
            if (bullet.AppliesSpeedModifier && (UnityEngine.Random.value <= bullet.SpeedApplyChance || ignoresProbability)) Effects.Add(bullet.speedEffect);
            return Effects;
        }

        /// <summary>
        /// Approximates the damage that the projectile would apply to a given target, applying both boss damage multipliers and jammed damage multipliers. General shorthand.
        /// </summary>
        /// <param name="bullet">Target projectile</param>
        /// <param name="target">The enemy whom the potential damage against is being calculated</param>
        public static float ReturnRealDamageWithModifiers(this Projectile bullet, HealthHaver target)
        {
            float dmg = bullet.baseData.damage;
            if (target.IsBoss) dmg *= bullet.BossDamageMultiplier;
            if (target.aiActor && target.aiActor.IsBlackPhantom) dmg *= bullet.BlackPhantomDamageMultiplier;
            return dmg;
        }
    }

}
