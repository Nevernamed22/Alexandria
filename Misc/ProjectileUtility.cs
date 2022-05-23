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
        public static RoomHandler GetAbsoluteRoom(this Projectile bullet)
        {
            Vector2 bulletPosition = bullet.sprite.WorldCenter;
            IntVector2 bulletPositionIntVector2 = bulletPosition.ToIntVector2();
            return GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(bulletPositionIntVector2);
        }
        public static PlayerController ProjectilePlayerOwner(this Projectile bullet)
        {
            if (bullet && bullet.Owner && bullet.Owner is PlayerController) return bullet.Owner as PlayerController;
            else return null;
        }
        public static void RemoveFromPool(this Projectile proj)
        {
            SpawnManager.PoolManager.Remove(proj.transform);
        }
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
        public static Projectile SetupProjectile(int id)
        {
            Projectile proj = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(id) as Gun).DefaultModule.projectiles[0]);
            proj.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(proj.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(proj);

            return proj;
        }
        public static Projectile SetupProjectile(Projectile projToCopy)
        {
            Projectile proj = UnityEngine.Object.Instantiate<Projectile>(projToCopy);
            proj.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(proj.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(proj);

            return proj;
        }
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
        public static void SendInRandomDirection(this Projectile bullet)
        {
            Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
            bullet.SendInDirection(dirVec, false, true);
        }
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
        public static Vector2 GetVectorToNearestEnemy(this Projectile bullet, bool checkIsWorthShooting = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, Func<AIActor, bool> overrideValidityCheck = null)
        {    
            IntVector2 bulletPositionIntVector2 = bullet.sprite != null ? bullet.sprite.WorldCenter.ToIntVector2() : bullet.specRigidbody.UnitCenter.ToIntVector2();
            AIActor closestToPosition = bulletPositionIntVector2.GetNearestEnemyToPosition(checkIsWorthShooting, type, overrideValidityCheck);
            if (closestToPosition) return closestToPosition.CenterPosition - (bullet.sprite != null ? bullet.sprite.WorldCenter : bullet.specRigidbody.UnitCenter);
            else return Vector2.zero;
        }
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
            p.collidesWithPlayer = false;
            p.collidesWithEnemies = true;
            if (scaleModifier != 1f)
            {
                SpawnManager.PoolManager.Remove(p.transform);
                p.RuntimeUpdateScale(scaleModifier);
            }
            if (p.Speed < minReflectedBulletSpeed) p.Speed = minReflectedBulletSpeed;
            p.baseData.damage = baseDamage;
            if (doPostProcessing)
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
            p.UpdateCollisionMask();
            p.Reflected();
            p.SendInDirection(p.Direction, true, true);
        }
    }
}
