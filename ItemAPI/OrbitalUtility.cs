using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
   public static class OrbitalUtility
    {
        public static GameObject MakeAnimatedOrbital(string name, float orbitRadius, float orbitalDegreesPerSecond, int orbitalTier, PlayerOrbital.OrbitalMotionStyle motionStyle, float perfectOrbitalFactor, List<string> idleAnimPaths, int fps, Vector2 colliderDimensions, Vector2 colliderOffsets, tk2dBaseSprite.Anchor anchorMode, tk2dSpriteAnimationClip.WrapMode wrapMode)
        {
            GameObject prefab = SpriteBuilder.SpriteFromResource(idleAnimPaths[0]);
            prefab.name = name;
            var body = prefab.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(colliderOffsets.ToIntVector2(), colliderDimensions.ToIntVector2());

            body.CollideWithTileMap = false;
            body.CollideWithOthers = true;
            body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;

            tk2dSpriteCollectionData spriteCollection = SpriteBuilder.ConstructCollection(prefab, $"{name}_Collection");
            prefab.AddAnimationToObject(spriteCollection, "start", idleAnimPaths, fps, colliderDimensions, colliderOffsets, anchorMode, wrapMode, true);

            PlayerOrbital orbitalPrefab = prefab.AddComponent<PlayerOrbital>();
            orbitalPrefab.motionStyle = motionStyle;
            orbitalPrefab.shouldRotate = false;
            orbitalPrefab.orbitRadius = orbitRadius;
            orbitalPrefab.perfectOrbitalFactor = perfectOrbitalFactor;
            orbitalPrefab.orbitDegreesPerSecond = orbitalDegreesPerSecond;
            orbitalPrefab.SetOrbitalTier(orbitalTier);

            prefab.MakeFakePrefab();
            return prefab;
        }
        public static GameObject MakeOrbital(string name, float orbitRadius, float orbitalDegreesPerSecond, int orbitalTier, PlayerOrbital.OrbitalMotionStyle motionStyle, float perfectOrbitalFactor, string spritePath, Vector2 colliderDimensions, Vector2 colliderOffsets)
        {
            GameObject prefab = SpriteBuilder.SpriteFromResource(spritePath);
            prefab.name = name;
            var body = prefab.GetComponent<tk2dSprite>().SetUpSpeculativeRigidbody(colliderOffsets.ToIntVector2(), colliderDimensions.ToIntVector2());

            body.CollideWithTileMap = false;
            body.CollideWithOthers = true;
            body.PrimaryPixelCollider.CollisionLayer = CollisionLayer.EnemyBulletBlocker;

            PlayerOrbital orbitalPrefab = prefab.AddComponent<PlayerOrbital>();
            orbitalPrefab.motionStyle = motionStyle;
            orbitalPrefab.shouldRotate = false;
            orbitalPrefab.orbitRadius = orbitRadius;
            orbitalPrefab.perfectOrbitalFactor = perfectOrbitalFactor;
            orbitalPrefab.orbitDegreesPerSecond = orbitalDegreesPerSecond;
            orbitalPrefab.SetOrbitalTier(orbitalTier);

            prefab.MakeFakePrefab();
            return prefab;
        }
        public static void RecalculateOrbitals(this PlayerController player)
        {
            Dictionary<int, int> tiersAndCounts = new Dictionary<int, int>();
            foreach (var o in player.orbitals)
            {
                var orbital = (PlayerOrbital)o;
                int targetTier = PlayerOrbital.CalculateTargetTier(player, o);
                orbital.SetOrbitalTier(targetTier);
                if (tiersAndCounts.ContainsKey(targetTier)) //Count starts at 0
                {
                    int existingCount = tiersAndCounts[targetTier];
                    tiersAndCounts[targetTier] = existingCount + 1;
                }
                else tiersAndCounts.Add(targetTier, 0);
            }
            foreach (var o in player.orbitals)
            {
                var orbital = (PlayerOrbital)o;
                int currentTier = orbital.GetOrbitalTier();
                if (tiersAndCounts.ContainsKey(currentTier))
                {
                    int currentAmtInTier = tiersAndCounts[currentTier];
                    orbital.SetOrbitalTierIndex(tiersAndCounts[currentTier]);
                    tiersAndCounts[currentTier] = currentAmtInTier - 1;

                }
                else
                {
                    orbital.SetOrbitalTierIndex(0);
                }
            }
        }
    }
}
