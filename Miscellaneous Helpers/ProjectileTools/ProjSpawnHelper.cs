using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace Alexandria.Misc
{
   public static class ProjSpawnHelper
    {
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
    }
}
