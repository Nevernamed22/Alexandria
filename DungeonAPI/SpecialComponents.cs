using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public class SpecialComponents
    {

        public class Repositioner : MonoBehaviour
        {
            public Vector3 reposition = Vector3.zero;
            public void Start()
            {
                this.transform.position += reposition;
                var body = this.GetComponent<SpeculativeRigidbody>();
                if (body != null)
                {
                    body.Reinitialize();
                }
            }
        }
        public class ForceNearestToRide : MonoBehaviour
        {
            private MineCartController self;
            public void Start()
            {
                self = this.GetComponent<MineCartController>();
                var room = this.transform.position.GetAbsoluteRoom();
                if (self != null && room != null)
                {
                    var c = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                    if (c != null && c.Count() > 0)
                    {
                        var enemy = GetClosestEnemy(c);
                        if (enemy)
                        {
                            self.BecomeOccupied(enemy);
                            enemy.MovementSpeed = 0;
                        }
                    }
                }
            }
            AIActor GetClosestEnemy(List<AIActor> enemies)
            {
                AIActor bestTarget = null;
                float closestDistanceSqr = Mathf.Infinity;
                Vector3 currentPosition = transform.position;
                foreach (var potentialTarget in enemies)
                {
                    Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = potentialTarget;
                    }
                }

                return bestTarget;
            }
        }


        public class WinchesterCameraHelper : MonoBehaviour
        {
            private ArtfulDodgerRoomController m_dodgerRoom;
            private SpeculativeRigidbody body;

            protected RoomHandler m_room;
            private void Start()
            {
                this.StartCoroutine(waitFrame());
            }

            private IEnumerator waitFrame()
            {
                yield return null;
                this.body = this.GetComponent<SpeculativeRigidbody>();
                this.m_room = this.transform.position.GetAbsoluteRoom();
                if (m_room != null && body != null)
                {
                    this.m_dodgerRoom = this.m_room.GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>()[0];
                    body.OnEnterTrigger += OnEnterTrigger;
                    body.OnExitTrigger += OnExitTrigger;
                    body.OnTriggerCollision += OnEnterTrigger;
                }
                yield break;
            }

            private void OnEnterTrigger(SpeculativeRigidbody mySpecRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
            {
                foreach (var cameras in m_dodgerRoom.m_cameraZones)
                {
                    if (m_dodgerRoom.m_rewardHandled == false)
                    {
                        cameras.Trigger(1);
                    }
                }
            }
            private void OnExitTrigger(SpeculativeRigidbody obj, SpeculativeRigidbody source)
            {
                foreach (var cameras in m_dodgerRoom.m_cameraZones)
                {
                    cameras.m_triggeredFrame = false;
                }
            }
        }
        public class WinchesterAlterer : MonoBehaviour { public Vector2 movement = new Vector2(0,0); public float goneTime = 1; }

        public class PedestalSetter : MonoBehaviour
        {
            public int Help =-1;
            public RewardPedestal c;
            public void Start()
            {
                if (c == null) {c = this.GetComponent<RewardPedestal>(); }

                c.m_room = this.transform.position.GetAbsoluteRoom();

                if (Help != -1)
                {
                    if (c.m_itemDisplaySprite == null)
                    {
                        c.contents = PickupObjectDatabase.GetById(Help);
                        c.m_itemDisplaySprite = tk2dSprite.AddComponent(new GameObject("Display Sprite")
                        {
                            transform =
                            {
                                parent = c.spawnTransform
                            }
                        }, c.contents.sprite.Collection, c.contents.sprite.spriteId);
                        SpriteOutlineManager.AddOutlineToSprite(c.m_itemDisplaySprite, Color.black, 0.1f, 0.05f, SpriteOutlineManager.OutlineType.NORMAL);
                        c.sprite.AttachRenderer(c.m_itemDisplaySprite);
                        c.m_itemDisplaySprite.HeightOffGround = 0.25f;
                        c.m_itemDisplaySprite.depthUsesTrimmedBounds = true;
                        c.m_itemDisplaySprite.PlaceAtPositionByAnchor(c.spawnTransform.position, tk2dBaseSprite.Anchor.LowerCenter);
                        c.m_itemDisplaySprite.transform.position = c.m_itemDisplaySprite.transform.position.Quantize(0.0625f);
                        GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
                        tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
                        component.PlaceAtPositionByAnchor(c.m_itemDisplaySprite.WorldCenter.ToVector3ZUp(0f), tk2dBaseSprite.Anchor.MiddleCenter);
                        component.HeightOffGround = 5f;
                        component.UpdateZDepth();
                        c.sprite.UpdateZDepth();
                    }
                }
                else
                {
                    if (c.m_room != null)
                    {
                        if (c.m_room.IsRegistered(c))
                        {
                            c.m_room.DeregisterInteractable(c);
                        }
                    }
                }
            }
        }
    }
}
