using Alexandria.ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class ShadowBulletDoer
    {
        /// <summary>
        /// Spawns a chain of Shadow Bullets style 'shadow' projectiles behind the original bullet. 
        /// </summary>
        /// <param name="source">The target projectile.</param>
        /// <param name="numberInChain">How many shadow bullets should be spawned.</param>
        /// <param name="pauseLength">How long the pause should be between each shadow bullet in the chain spawning.</param>
        /// <param name="chainScaleMult">A scale multiplier automatically applied to the spawned clones.</param>
        /// <param name="overrideProj">If set, the shadow bullet chain will be made up of copies of the override projectile instead of the original projectile.</param>
        /// <param name="shadowcolour">If true, applies the shadowy colouration from the Shadow Bullets item to the clones.</param>
        public static void SpawnChainedShadowBullets(this Projectile source, int numberInChain, float pauseLength, float chainScaleMult = 1, Projectile overrideProj = null, bool shadowcolour = false)
        {
            GameManager.Instance.Dungeon.StartCoroutine(ShadowBulletDoer.HandleShadowChainDelay(source, numberInChain, pauseLength, chainScaleMult, overrideProj, shadowcolour));
        }
        private static IEnumerator HandleShadowChainDelay(Projectile proj, int amount, float delay, float scaleMult, Projectile overrideproj, bool shadowcolour = false)
        {

            GameObject prefab = (overrideproj != null) ? FakePrefab.Clone(overrideproj.gameObject) : FakePrefab.Clone(proj.gameObject);            
            Projectile prefabproj = prefab.GetComponent<Projectile>();
            prefabproj.Owner = proj.Owner;
            prefabproj.Shooter = proj.Shooter;
            Vector3 position = proj.transform.position;
            float rotation = proj.Direction.ToAngle();

            bool isInitialProjectile = true;
            yield return null;
            for (int i = 0; i < amount; i++)
            {
                if (delay > 0f)
                {
                    float ela = 0f;
                    if (isInitialProjectile)
                    {
                        float initDelay = delay - 0.03f;
                        while (ela < initDelay)
                        {
                            ela += BraveTime.DeltaTime;
                            yield return null;
                        }
                        isInitialProjectile = false;
                    }
                    else
                    {
                        while (ela < delay)
                        {
                            ela += BraveTime.DeltaTime;
                            yield return null;
                        }
                    }
                }
                ShadowBulletDoer.SpawnShadowBullet(prefabproj, position, rotation, scaleMult, shadowcolour);
            }
            yield break;
        }

        /// <summary>
        /// Creates a duplicate 'shadow' bullet with the same parameters as the original at a set position and with a set rotation. Returns the spawned clone for additional modification.
        /// </summary>
        /// <param name="obj">The target projectile.</param>
        /// <param name="position">The position that the 'shadow' bullet should be spawned at.</param>
        /// <param name="rotation">The rotation of the 'shadow' bullet. Used to determine angle. 0 is equivalent to directly to the right.</param>
        /// <param name="chainScaleMult">A scale multiplier automatically applied to the spawned clone.</param>
        /// <param name="shadowcolour">If true, applies the shadowy colouration from the Shadow Bullets item to the clone.</param>
        public static Projectile SpawnShadowBullet(Projectile obj, Vector3 position, float rotation, float chainScaleMult = 1, bool shadowcolour = false)
        {
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(obj.gameObject, position, Quaternion.Euler(0f, 0f, rotation));
            if (gameObject2.GetComponent<AutoDoShadowChainOnSpawn>()) UnityEngine.Object.Destroy(gameObject2.GetComponent<AutoDoShadowChainOnSpawn>());
            if (gameObject2.GetComponent<ImprovedHelixProjectile>()) gameObject2.GetComponent<ImprovedHelixProjectile>().SpawnShadowBulletsOnSpawn = false;

            gameObject2.transform.position += gameObject2.transform.right * -0.5f;
            Projectile component2 = gameObject2.GetComponent<Projectile>();
            component2.specRigidbody.Reinitialize();
            component2.collidesWithPlayer = false;
            component2.Owner = obj.Owner;
            component2.Shooter = obj.Shooter;
            component2.baseData.damage = obj.baseData.damage;
            component2.baseData.range = obj.baseData.range;
            component2.baseData.speed = obj.baseData.speed;
            component2.baseData.force = obj.baseData.force;
            component2.RuntimeUpdateScale(chainScaleMult);
            component2.UpdateSpeed();
            if (shadowcolour) component2.ChangeColor(0f, new Color(0.35f, 0.25f, 0.65f, 1f));
            return component2;
        }
    }
}
