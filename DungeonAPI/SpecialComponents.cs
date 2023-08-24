using Alexandria.ItemAPI;
using Dungeonator;
using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static PickupObject;

namespace Alexandria.DungeonAPI
{
    public class SpecialComponents
    {
        public class TurretCartReboot : MonoBehaviour
        {
            public IEnumerator Start()
            {
                yield return null;
                var turret = this.GetComponentInChildren<CartTurretController>();
                if (turret)
                {
                    if (GameManager.Instance.PlayerIsInRoom(this.transform.position.GetAbsoluteRoom()))
                    {
                        turret.Activate(GameManager.Instance.GetActivePlayerClosestToPoint(this.transform.position));
                    }
                }
                yield break;
            }
        }

        public class MineCart_Proximity_Grabber : MonoBehaviour
        {
            public bool DestroyAfterCopy = true;
            public MineCartController fallBackCart;

            public void Start()
            {
                this.StartCoroutine(Delay());
            }

            public IEnumerator Delay()
            {
                var MineCartFactory = this.GetComponent<MineCartFactory>();
                if (MineCartFactory == null)
                {
                    yield break; 
                }
                yield return null;
                var room = this.transform.position.GetAbsoluteRoom();
                if (GameManager.Instance.Dungeon == null) { yield break; }
                if (room != null)
                {
                    var list = room.GetComponentsAbsoluteInRoom<MineCartController>();
                    var selectedCart = GetClosestCart(list ?? new List<MineCartController>() { });
                    if (selectedCart != null)
                    {
                        MineCartFactory.MineCartPrefab = selectedCart.gameObject.InstantiateAndFakeprefab().GetComponent<MineCartController>();
                        MineCartFactory.MineCartPrefab.gameObject.AddComponent<TurretCartReboot>();
                        if (DestroyAfterCopy == true)
                        {
                            if (room.IsRegistered(selectedCart)) { room.DeregisterInteractable(selectedCart); }
                            if (selectedCart.m_rider != null && selectedCart.m_rider is AIActor)
                            {
                                var enemy = (selectedCart.m_rider as AIActor);
                                enemy.MovementSpeed = enemy.BaseMovementSpeed;
                                enemy.specRigidbody.Reinitialize();
                            }
                            Destroy(selectedCart.gameObject);
                        }
                        var copiedCartComponent = MineCartFactory.MineCartPrefab.GetComponent<ForceNearestToRide>();
                        if (copiedCartComponent != null)
                        {
                            Destroy(copiedCartComponent);
                        }
                    }
                    else
                    {
                        MineCartFactory.MineCartPrefab = fallBackCart;
                    }
                }
                yield break;
            }

            MineCartController GetClosestCart(List<MineCartController> enemies)
            {
                MineCartController bestTarget = null;
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

        public class Glitchinator : MonoBehaviour
        {
            public IEnumerator Start()
            {
                yield return null;
                var chest = this.GetComponent<Chest>();
                if (chest != null)
                {
                    chest.BecomeGlitchChest();
                }         
                Destroy(this);
                yield break;
            }
            
        }        

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
            public enum LootType
            {
                N_A,
                RANDOM_GUN,
                RANDOM_ITEM,
                RANDOM,
                CREST,
                SET
            }

            public LootType myLootType = LootType.N_A;

            public void Start()
            {
                if (c == null) {c = this.GetComponent<RewardPedestal>(); }

                c.m_room = this.transform.position.GetAbsoluteRoom();

                int ID = returnItemID();

                if (ID != -1)
                {
                    if (c.m_itemDisplaySprite == null)
                    {
                        c.contents = PickupObjectDatabase.GetById(ID);
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

            public int returnItemID()
            {
                switch (myLootType)
                {
                    case LootType.RANDOM_GUN:
                        return LootEngine.GetItemOfTypeAndQuality<PickupObject>(ReturnRandomQuality(), GameManager.Instance.RewardManager.GunsLootTable, false).PickupObjectId;
                    case LootType.RANDOM_ITEM:
                        return LootEngine.GetItemOfTypeAndQuality<PickupObject>(ReturnRandomQuality(), GameManager.Instance.RewardManager.ItemsLootTable, false).PickupObjectId;
                    case LootType.RANDOM:
                        return LootEngine.GetItemOfTypeAndQuality<PickupObject>(ReturnRandomQuality(), UnityEngine.Random.value > 0.5f ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable, false).PickupObjectId;//BraveUtility.RandomBool() ? GameManager.Instance.RewardManager.ItemsLootTable.SelectByWeight().GetComponent<PickupObject>().PickupObjectId : GameManager.Instance.RewardManager.GunsLootTable.SelectByWeight().GetComponent<PickupObject>().PickupObjectId;

                    case LootType.CREST:
                        return 305;
                    case LootType.SET:
                        if (Help != -1) { return Help; }
                        return -1;
                    case LootType.N_A:
                        return -1;
                    default:
                        return -1;
                }

            }
            private ItemQuality ReturnRandomQuality()
            {
                int i = UnityEngine.Random.Range(1, 6);
                switch (i)
                {
                    case 1:
                        return PickupObject.ItemQuality.D;
                    case 2:
                        return PickupObject.ItemQuality.C;
                    case 3:
                        return PickupObject.ItemQuality.B;
                    case 4:
                        return PickupObject.ItemQuality.A;
                    case 5:
                        return PickupObject.ItemQuality.S;
                    default:
                        return PickupObject.ItemQuality.D;
                }
            }
        }
    }
}
