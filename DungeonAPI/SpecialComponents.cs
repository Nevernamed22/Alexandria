﻿using Alexandria.ItemAPI;
using Alexandria.NPCAPI;
using Dungeonator;
using System;   
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using static PickupObject;

namespace Alexandria.DungeonAPI
{
    public class SpecialComponents
    {
        public class NoPickup : MonoBehaviour
        {
            public List<Vector3> offsets  = new List<Vector3>();
            public void Start()
            {
                if (GameManager.Instance.Dungeon == null)
                {
                    return; }

                foreach (var entry in offsets)
                {
                    IntVector2 intVector = ((base.transform.position + entry).XY() + new Vector2(0.5f, 0.5f)).ToIntVector2(VectorConversions.Floor);
                    if (GameManager.Instance.Dungeon.data[intVector.x, intVector.y] != null)
                    {
                        GameManager.Instance.Dungeon.data[intVector.x, intVector.y].PreventRewardSpawn = true;
                    }
                }
            }
        }
        public class ShopItemPosition : MonoBehaviour 
        {

            public bool Used = false;
            public TableType thisType = TableType.PRIMARY;
            public BaseShopController.AdditionalShopType Type = BaseShopController.AdditionalShopType.NONE;
            public bool SeenByAny = true;
            public bool OmniDirectional = true;
            public DungeonData.Direction direction = DungeonData.Direction.NORTH;
            public int itemID = -1;
            public int OverridePrice = -1;
            public float PriceMultiplier = 1f;

            public Vector3 Offset = new Vector3(0, 0, 0);

            public enum TableType
            {
                PRIMARY,
                SECONDARY
            };

            public float Chance = 1f;

            public void DoItemPlace(BaseShopController Controller)
            {
                if (Chance < UnityEngine.Random.value) { return; }
                Func<GameObject, float, float> weightModifier = null;
                if (SecretHandshakeItem.NumActive > 0)
                {
                    weightModifier = delegate (GameObject prefabObject, float sourceWeight)
                    {
                        PickupObject component10 = prefabObject.GetComponent<PickupObject>();
                        float num7 = sourceWeight;
                        if (component10 != null)
                        {
                            int quality = (int)component10.quality;
                            num7 *= 1f + (float)quality / 10f;
                        }
                        return num7;
                    };
                }

                GameObject gunObj = itemID != -1 ? PickupObjectDatabase.GetById(itemID).gameObject : thisType == TableType.PRIMARY ? Controller.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(Controller.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded) : Controller.shopItemsGroup2 != null ? Controller.shopItemsGroup2.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(Controller.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded) : Controller.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(Controller.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);

                GameObject gameObject8 = new GameObject("Additional Shop Item(" + gunObj.name + ")");
                Transform transform4 = gameObject8.transform;

                GameObject transObj = new GameObject();
                transObj.transform.position = this.transform.position + Offset; 
                transform4.position = transObj.transform.position;


                transform4.parent = transObj.transform;


                EncounterTrackable component9 = gunObj.GetComponent<EncounterTrackable>();
                if (component9 != null)
                {
                    GameManager.Instance.ExtantShopTrackableGuids.Add(component9.EncounterGuid);
                }

                ShopItemController shopItemController2 = gameObject8.AddComponent<ShopItemController>();
                Controller.AssignItemFacing(transObj.transform, shopItemController2);
                shopItemController2.UseOmnidirectionalItemFacing = this.OmniDirectional;
                shopItemController2.itemFacing = this.direction;


                shopItemController2.Initialize(gunObj.GetComponent<PickupObject>(), Controller);
                shopItemController2.CurrencyType = Controller.m_itemControllers[0].CurrencyType;
                Controller.m_itemControllers.Add(shopItemController2);
                Controller.m_shopItems.Add(gunObj);
                Controller.m_room.RegisterInteractable(shopItemController2);

                var discounter = shopItemController2.gameObject.GetOrAddComponent<ShopDiscountController>();
                discounter.localDiscounts.Add(new ShopDiscount() 
                {
                    PriceMultiplier = PriceMultiplier,
                    isCompleteOverrideCost = OverridePrice != -1 ? true : false,
                    CustomCost = OverridePrice
                });
                discounter.discounts = CustomDiscountManager.DiscountsToAdd ?? new List<ShopDiscount>() { };
                trans = (shopItemController2.gameObject);
            }
            private GameObject trans;
            public void Update()
            {
                if (trans == null) { return; }
                trans.transform.position = trans.transform.position.WithZ(20);
            }
        }

        public class AttackLeapPoint : MonoBehaviour { }
        public class ProjectileJammer : MonoBehaviour
        {
            public void Start()
            {
                var proj = this.GetComponent<Projectile>();
                if (proj != null)
                {
                    if (proj.BulletScriptSettings != null)
                    {
                        proj.BulletScriptSettings.preventPooling = true;
                    }
                    proj.BecomeBlackBullet();
                }
            }
        }

        public class ProjectileWallUnfuckinator : MonoBehaviour
        {
            public void Start()
            {
                var proj = this.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.IgnoreTileCollisionsFor(1 / proj.baseData.speed);
                    proj.UpdateCollisionMask();
                }
            }
        }

        public class Glitched_Boss_Modifier : MonoBehaviour
        {
            public float TimeScale = 1;
            public float DamageMultiplier = 1;
            public float MovementSpeed = 1;
            public bool ForceSlight = false;


            public IEnumerator Start()
            {
                yield return null;
                var room = this.transform.position.GetAbsoluteRoom();
                if (room != null)
                {
                    GameManager.Instance.Dungeon.IsGlitchDungeon = true;
                    
                    var enemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                    if (enemies == null) { yield break; }
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        var enemy = enemies[i];
                        if (enemy != null)
                        {
                            enemy.sprite.usesOverrideMaterial = true;
                            enemy.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/Glitch");

                            enemy.healthHaver.AllDamageMultiplier *= DamageMultiplier;

                            enemy.LocalTimeScale *= TimeScale;
                            enemy.MovementSpeed *= MovementSpeed;
                            if (ForceSlight == true)
                            {
                                enemy.SetIsFlying(true, "Glitch_", true, true);
                            }
                        }
                    }              
                }
                yield break;
            }
        }


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
                            Destroy(selectedCart.gameObject, 0.1f);
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
                var body2 = this.GetComponentInChildren<SpeculativeRigidbody>();
                if (body2 != null)
                {
                    body2.Reinitialize();
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
                for (int i = c.spawnTransform.childCount-1; i > -1; i--)
                {
                    if (c.spawnTransform.GetChild(i).gameObject.name.Contains("Display Sprite"))
                    {
                        Destroy(c.spawnTransform.GetChild(i).gameObject);
                    }
                }
                c.m_itemDisplaySprite = null;
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
