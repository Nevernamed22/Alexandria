using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace Alexandria.Misc
{
    public static class SupplyDropDoer
    {
        private static GameObject simplerCratePrefab;

        /// <summary>
        /// Spawns a falling crate (like the Supply Drop item) at the specified position. Returns the spawned crate's crate behaviour. Returned behaviour contains an OnCrateLanded action for custom effects.
        /// </summary>
        /// <param name="position">The position the crate should be spawned at.</param>
        /// <param name="lootIDToSpawn">An item or gun ID to be dropped by the crate. Leave as -1 for no item.</param>
        /// <param name="crateDespawnDelay">How long after opening should the crate wait to despawn. If left as -1, the crate will despawn when the spawned loot is collected (if the loot spawn ID was set).</param>
        /// <param name="preventAutoDespawn">If true, prevents the crate from despawning automatically. Call RemoveCrate() to remove manually.</param>
        public static SimplerCrateBehaviour SpawnSupplyDrop(Vector2 position, int lootIDToSpawn = -1, float crateDespawnDelay = -1, bool preventAutoDespawn = false)
        {
            if (simplerCratePrefab == null)
            {
                GameObject cratePrefab = (GameObject)BraveResources.Load("EmergencyCrate", ".prefab");
                GameObject instantiatedCrate = cratePrefab.InstantiateAndFakeprefab();
                EmergencyCrateController oldCrateComponent = instantiatedCrate.GetComponent<EmergencyCrateController>();

                SimplerCrateBehaviour newCrateBehav = instantiatedCrate.AddComponent<SimplerCrateBehaviour>();
                newCrateBehav.driftAnimationName = oldCrateComponent.driftAnimationName;
                newCrateBehav.landedAnimationName = oldCrateComponent.landedAnimationName;
                newCrateBehav.chuteLandedAnimationName = oldCrateComponent.chuteLandedAnimationName;
                newCrateBehav.crateDisappearAnimationName = oldCrateComponent.crateDisappearAnimationName;
                newCrateBehav.chuteAnimator = oldCrateComponent.chuteAnimator;
                newCrateBehav.landingTargetSprite = oldCrateComponent.landingTargetSprite;
                UnityEngine.Object.Destroy(oldCrateComponent);

                simplerCratePrefab = instantiatedCrate;
            }
            GameObject crate = UnityEngine.Object.Instantiate<GameObject>(simplerCratePrefab);
            SimplerCrateBehaviour crateComp = crate.GetComponent<SimplerCrateBehaviour>();
            crateComp.easyLootID = lootIDToSpawn;
            crateComp.despawnDelay = crateDespawnDelay;
            crateComp.preventAutoDespawn = preventAutoDespawn;
            crateComp.Trigger(new Vector3(-5f, -5f, -5f), new Vector3(position.x + 15f, position.y + 15f, 15f), position.GetAbsoluteRoom());
            position.GetAbsoluteRoom().ExtantEmergencyCrate = crate;
            return crateComp;
        }

        public class SimplerCrateBehaviour : BraveBehaviour
        {
            public SimplerCrateBehaviour()
            {
                easyLootID = -1;
                despawnDelay = -1;
                preventAutoDespawn = false;
            }
            public void Trigger(Vector3 startingVelocity, Vector3 startingPosition, RoomHandler room)
            {
                this.m_parentRoom = room;
                this.m_currentPosition = startingPosition;
                this.m_currentVelocity = startingVelocity;
                this.m_hasBeenTriggered = true;
                base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
                float num = startingPosition.z / -startingVelocity.z;
                Vector3 position = startingPosition + num * startingVelocity;
                this.m_landingTarget = SpawnManager.SpawnVFX(this.landingTargetSprite, position, Quaternion.identity);
                this.m_landingTarget.GetComponentInChildren<tk2dSprite>().UpdateZDepth();
            }
            private void Update()
            {
                if (this.m_hasBeenTriggered)
                {
                    this.m_currentPosition += this.m_currentVelocity * BraveTime.DeltaTime;
                    if (this.m_currentPosition.z <= 0f)
                    {
                        this.m_currentPosition.z = 0f;
                        this.OnLanded();
                    }
                    base.transform.position = BraveUtility.QuantizeVector(this.m_currentPosition.WithZ(this.m_currentPosition.y - this.m_currentPosition.z), (float)PhysicsEngine.Instance.PixelsPerUnit);
                    base.sprite.HeightOffGround = this.m_currentPosition.z;
                    base.sprite.UpdateZDepth();
                }
            }
            private void OnLanded()
            {
                this.m_hasBeenTriggered = false;
                base.sprite.gameObject.layer = LayerMask.NameToLayer("FG_Critical");
                base.sprite.renderer.sortingLayerName = "Background";
                base.sprite.IsPerpendicular = false;
                base.sprite.HeightOffGround = -1f;
                this.m_currentPosition.z = -1f;
                base.spriteAnimator.Play(this.landedAnimationName);
                this.chuteAnimator.PlayAndDestroyObject(this.chuteLandedAnimationName, null);
                if (this.m_landingTarget)
                {
                    SpawnManager.Despawn(this.m_landingTarget);
                }
                this.m_landingTarget = null;

                if (OnCrateLanded != null) OnCrateLanded(base.sprite.WorldCenter, this);

                if (easyLootID != -1)
                {
                    DebrisObject spawned = LootEngine.SpawnItem(PickupObjectDatabase.GetById(easyLootID).gameObject, base.sprite.WorldCenter.ToVector3ZUp(0f) + new Vector3(-0.5f, 0.5f, 0f), Vector2.zero, 0f, false, false, false);
                    if (despawnDelay < 0 && !preventAutoDespawn) base.StartCoroutine(this.DestroyCrateWhenPickedUp(spawned));
                }
                if (despawnDelay >= 0 && !preventAutoDespawn) base.StartCoroutine(this.DestroyCrateDelayed());
            }
            private IEnumerator DestroyCrateDelayed()
            {
                yield return new WaitForSeconds(despawnDelay);
                if (this.m_landingTarget)
                {
                    SpawnManager.Despawn(this.m_landingTarget);
                }
                RemoveCrate();
                yield break;
            }
            private IEnumerator DestroyCrateWhenPickedUp(DebrisObject spawned)
            {
                while (spawned)
                {
                    yield return new WaitForSeconds(0.25f);
                }
                RemoveCrate();
                yield break;
            }

            /// <summary>
            /// Causes the crate to play it's despawn animation, and disappear.
            /// </summary>
            public void RemoveCrate()
            {
                if (this.m_landingTarget)
                {
                    SpawnManager.Despawn(this.m_landingTarget);
                }
                this.m_landingTarget = null;
                if (this.m_parentRoom.ExtantEmergencyCrate == base.gameObject)
                {
                    this.m_parentRoom.ExtantEmergencyCrate = null;
                }
                base.spriteAnimator.Play(this.crateDisappearAnimationName);
            }

            /// <summary>
            /// Removes the crate's landing target VFX, if one is present.
            /// </summary>
            public void ClearLandingTarget()
            {
                if (this.m_landingTarget)
                {
                    SpawnManager.Despawn(this.m_landingTarget);
                }
                this.m_landingTarget = null;
            }

            public int easyLootID;
            public float despawnDelay;
            public bool preventAutoDespawn;

            public string driftAnimationName;
            public string landedAnimationName;
            public string chuteLandedAnimationName;
            public string crateDisappearAnimationName;
            public tk2dSpriteAnimator chuteAnimator;
            public GameObject landingTargetSprite;
            private bool m_hasBeenTriggered;
            private Vector3 m_currentPosition;
            private Vector3 m_currentVelocity;
            private RoomHandler m_parentRoom;
            private GameObject m_landingTarget;

            public Action<Vector2, SimplerCrateBehaviour> OnCrateLanded;
        }
    }
}
