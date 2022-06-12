using System;
using System.Collections.Generic;
using Alexandria.Misc;
using Dungeonator;
using HutongGames.PlayMaker;
using UnityEngine;


namespace Alexandria.NPCAPI
{
	public class CustomShopController : BaseShopController
	{

		public CustomShopController()
		{
			poolType = ShopItemPoolType.DEFAULT;
		}

		public ShopItemPoolType poolType;
		public enum ShopItemPoolType
		{
			DEFAULT,
			DUPES,
			DUPES_AND_NOEXCLUSION
		};

		public Func<CustomShopController, PlayerController, int, bool> customCanBuy;
		public Func<CustomShopController, PlayerController, int, int> removeCurrency;
		public Func<CustomShopController, CustomShopItemController, PickupObject, int> customPrice;
		public Func<PlayerController, PickupObject, int, bool> OnPurchase;
		public Func<PlayerController, PickupObject, int, bool> OnSteal;

		public string customPriceSprite;

		public CustomShopItemController.ShopCurrencyType currencyType;
		public bool giveStatsOnPurchase;
		public bool canBeRobbed;
		public StatModifier[] statsToGive;
		public DungeonPrerequisite[] prerequisites = new DungeonPrerequisite[0];
		public bool AllowedToSpawnOnRainbowMode;


        public override void ConfigureOnPlacement(RoomHandler room)
        {			
			base.ConfigureOnPlacement(room);
			room.IsShop = true;
			this.m_room = room;
		}

        public int CustomPriceMethod(CustomShopController shop, CustomShopItemController shopItem, PickupObject item)
		{
			if (customPrice != null)
				return customPrice(shop, shopItem, item);
			return 0;
		}
		public int RemoveCurrencyMethod(CustomShopController shop, PlayerController player, int cost)
		{
			if (removeCurrency != null)
				return removeCurrency(shop, player, cost);
			return 0;
		}
		public bool CustomCanBuyMethod(CustomShopController shop, PlayerController player, int cost)
		{
			if (customCanBuy != null)
				return customCanBuy(shop, player, cost);
			return true;
		}

		public bool OnStealMethod(PlayerController player, PickupObject item, int cost)
		{
			if (OnSteal != null)
			{
				return OnSteal(player, item, cost);
			}
			return false;
		}

		public bool OnPurchaseMethod(PlayerController player, PickupObject item, int cost)
		{
			TryPlayAnimation(this.shopkeepFSM.gameObject, "purchase");
			if (OnPurchase != null)
			{
				return OnPurchase(player, item, cost);
			}
			return false;
		}

		public void LockItems()
		{
			for (int i = 0; i < this.m_itemControllers.Count; i++)
			{
				if (this.m_itemControllers[i])
				{
					this.m_itemControllers[i].Locked = true;
				}
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if (playerController && playerController.healthHaver.IsAlive)
				{
					playerController.ForceRefreshInteractable = true;
				}
			}
		}

		public static void TryPlayAnimation(GameObject shopKeeperObject, string animation)
		{
			bool hasSpecAnim1 = false;
			bool hasSpecAnim2 = false;

			List<AIAnimator.NamedDirectionalAnimation> lists = shopKeeperObject.GetComponent<AIAnimator>().OtherAnimations;
			for (int k = 0; k < lists.Count; k++)
			{
				if (lists[k].anim.Prefix == animation)
				{
					hasSpecAnim1 = true;
				}
			}
			List<AIAnimator.NamedDirectionalAnimation> lists2 = shopKeeperObject.GetComponentInChildren<AIAnimator>().OtherAnimations;
			for (int k = 0; k < lists2.Count; k++)
			{
				if (lists2[k].anim.Prefix == animation)
				{
					hasSpecAnim2 = true;
				}
			}
			if (shopKeeperObject.GetComponent<AIAnimator>() != null && hasSpecAnim1 == true)
			{
				shopKeeperObject.GetComponent<AIAnimator>().PlayUntilFinished(animation);
			}
			if (shopKeeperObject.GetComponentInChildren<AIAnimator>() != null && hasSpecAnim2 == true)
			{
				shopKeeperObject.GetComponentInChildren<AIAnimator>().PlayUntilFinished(animation);
			}
		}


		public new void NotifyStealFailed()
		{
			TryPlayAnimation(this.shopkeepFSM.gameObject, "stolen");
			this.shopkeepFSM.SendEvent("caughtStealing");
			this.m_wasCaughtStealing = true;
		}

		public override void NotifyFailedPurchase(ShopItemController itemController)
		{
			TryPlayAnimation(this.shopkeepFSM.gameObject, "denied");
			if (this.shopkeepFSM != null)
			{
				FsmObject fsmObject = this.shopkeepFSM.FsmVariables.FindFsmObject("referencedItem");
				if (fsmObject != null)
				{
					fsmObject.Value = itemController;
				}
				this.shopkeepFSM.SendEvent("failedPurchase");
			}
		}



		public override void PurchaseItem(ShopItemController itemBad, bool actualPurchase = true, bool allowSign = true)
		{
			var item = itemBad as CustomShopItemController;
			float heightOffGround = -1f;
			if (item && item.sprite)
			{
				heightOffGround = item.sprite.HeightOffGround;
			}
			if (actualPurchase)
			{
				if (giveStatsOnPurchase)
				{
					foreach (var stat in statsToGive)
					{
						item.LastInteractingPlayer.ownerlessStatModifiers.Add(stat);
					}
					item.LastInteractingPlayer.stats.RecalculateStats(item.LastInteractingPlayer, false, false);
				}
				if (this.shopkeepFSM != null)
				{
					FsmObject fsmObject = this.shopkeepFSM.FsmVariables.FindFsmObject("referencedItem");
					if (fsmObject != null)
					{
						fsmObject.Value = item;
					}
					this.shopkeepFSM.SendEvent("succeedPurchase");
				}
			}


			if (!item.item.PersistsOnPurchase)
			{
				if (allowSign)
				{
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/Sign_SoldOut", ".prefab"));
					tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
					component.PlaceAtPositionByAnchor(item.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
					gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
					component.HeightOffGround = heightOffGround;
					component.UpdateZDepth();
				}
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
				tk2dBaseSprite component2 = gameObject2.GetComponent<tk2dBaseSprite>();
				component2.PlaceAtPositionByAnchor(item.sprite.WorldCenter.ToVector3ZUp(0f), tk2dBaseSprite.Anchor.MiddleCenter);
				component2.transform.position = component2.transform.position.Quantize(0.0625f);
				component2.HeightOffGround = 5f;
				component2.UpdateZDepth();

				if (currencyType != CustomShopItemController.ShopCurrencyType.META_CURRENCY)
				{
					this.m_room.DeregisterInteractable(item);
				}
				else
                {
					if (!RoomHandler.unassignedInteractableObjects.Contains(item)) RoomHandler.unassignedInteractableObjects.Remove(item);
				}

				
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}

		public void ActionAndFuncSetUp(Func<CustomShopController, PlayerController, int, bool> CustomCanBuySetUp, Func<CustomShopController, PlayerController, int, int> RemoveCurrencySetUp, Func<CustomShopController, CustomShopItemController, PickupObject, int> CustomPriceSetUp,
			Func<PlayerController, PickupObject, int, bool> OnPurchase, Func<PlayerController, PickupObject, int, bool> OnSteal)
		{
			this.customCanBuy = CustomCanBuySetUp;
			this.removeCurrency = RemoveCurrencySetUp;
			this.customPrice = CustomPriceSetUp;
			this.OnPurchase = OnPurchase;
			this.OnSteal = OnSteal;

		}


		protected override void DoSetup()
		{
			base.m_shopItems = new List<GameObject>();

			List<int> list = new List<int>();
			Func<GameObject, float, float> weightModifier = null;
			if (base.m_room == null) { GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(new IntVector2((int)base.gameObject.transform.position.x, (int)base.gameObject.transform.position.y)); ETGModConsole.Log("null room"); }


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
			bool flag = GameStatsManager.Instance.IsRainbowRun && AllowedToSpawnOnRainbowMode == false;
			for (int i = 0; i < base.spawnPositions.Length; i++)
			{
				if (flag == true)
				{
					base.m_shopItems.Add(null);
				}
				else if (this.currencyType == CustomShopItemController.ShopCurrencyType.META_CURRENCY && this.ExampleBlueprintPrefab != null)
				{
					if (this.FoyerMetaShopForcedTiers)
					{
						List<WeightedGameObject> compiledRawItems = this.shopItems.GetCompiledRawItems();
						int num = 0;
						bool flag2 = true;
						while (flag2)
						{
							for (int j = num; j < num + this.spawnPositions.Length; j++)
							{
								if (j >= compiledRawItems.Count)
								{
									flag2 = false;
									break;
								}
								GameObject gameObject = compiledRawItems[j].gameObject;
								PickupObject component = gameObject.GetComponent<PickupObject>();
								if (!component.encounterTrackable.PrerequisitesMet())
								{
									flag2 = false;
									break;
								}
							}
							if (flag2)
							{
								num += this.spawnPositions.Length;
							}
						}
						for (int k = num; k < num + this.spawnPositions.Length; k++)
						{
							if (k >= compiledRawItems.Count)
							{
								this.m_shopItems.Add(null);
								list.Add(1);
							}
							else
							{
								GameObject gameObject2 = compiledRawItems[k].gameObject;
								PickupObject component2 = gameObject2.GetComponent<PickupObject>();
								if (this.m_shopItems.Contains(gameObject2) || component2.encounterTrackable.PrerequisitesMet())
								{
									this.m_shopItems.Add(null);
									list.Add(1);
								}
								else
								{
									this.m_shopItems.Add(gameObject2);
									list.Add(Mathf.RoundToInt(compiledRawItems[k].weight));
								}
							}
						}
					}
					else
					{
						ETGModConsole.Log("a");
						List<WeightedGameObject> compiledRawItems2 = this.shopItems.GetCompiledRawItems();
						GameObject gameObject3 = null;
						ETGModConsole.Log("b");
						for (int l = 0; l < compiledRawItems2.Count; l++)
						{
							ETGModConsole.Log("c");
							GameObject gameObject4 = compiledRawItems2[l].gameObject;
							PickupObject component3 = gameObject4.GetComponent<PickupObject>();
							if (component3 == null) { continue; }
							if (!this.m_shopItems.Contains(gameObject4))
							{
								if (!component3.encounterTrackable.PrerequisitesMet())
								{
									gameObject3 = gameObject4;
									list.Add(Mathf.RoundToInt(compiledRawItems2[l].weight));
									break;
								}
							}
							ETGModConsole.Log("d");
						}
						ETGModConsole.Log("e");
						this.m_shopItems.Add(gameObject3);
						if (gameObject3 == null)
						{
							list.Add(1);
						}
						ETGModConsole.Log("f");
					}
				}
				else
				{
					GameObject gameObject5 = new GameObject();
					switch (this.poolType)
					{
						case ShopItemPoolType.DEFAULT:
							gameObject5 = this.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(this.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);
							this.m_shopItems.Add(gameObject5);
							break;
						case ShopItemPoolType.DUPES:
							gameObject5 = this.shopItems.SelectByWeight(GameManager.Instance.IsSeeded);
							this.m_shopItems.Add(gameObject5);
							break;
						case ShopItemPoolType.DUPES_AND_NOEXCLUSION:
							gameObject5 = this.shopItems.SelectByWeightNoExclusions(GameManager.Instance.IsSeeded);
							this.m_shopItems.Add(gameObject5);
							break;
						default:
							gameObject5 = this.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(this.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);
							this.m_shopItems.Add(gameObject5);
							break;
					}
				}
			}
			

			m_itemControllers = new List<ShopItemController>();
			for (int m = 0; m < base.spawnPositions.Length; m++)
			{
				Transform transform = base.spawnPositions[m];
				if (!flag && !(base.m_shopItems[m] == null))
				{
					PickupObject component4 = base.m_shopItems[m].GetComponent<PickupObject>();
					if (!(component4 == null))
					{
						GameObject gameObject6 = new GameObject("Shop item " + m.ToString());
						Transform transform2 = gameObject6.transform;
						transform2.parent = transform;
						transform2.localPosition = Vector3.zero;
						EncounterTrackable component5 = component4.GetComponent<EncounterTrackable>();
						if (component5 != null)
						{
							GameManager.Instance.ExtantShopTrackableGuids.Add(component5.EncounterGuid);
						}
						CustomShopItemController shopItemController = gameObject6.AddComponent<CustomShopItemController>();

						this.AssignItemFacing(transform, shopItemController);
						if (base.m_room != null)
						{
							if (!base.m_room.IsRegistered(shopItemController))
							{
								base.m_room.RegisterInteractable(shopItemController);
							}
						}
						else
                        {
							if (!RoomHandler.unassignedInteractableObjects.Contains(shopItemController))
							{
								RoomHandler.unassignedInteractableObjects.Add(shopItemController);
							}
						}

						
						if (this.baseShopType == BaseShopController.AdditionalShopType.FOYER_META && this.ExampleBlueprintPrefab != null)
						{
							GameObject gameObject7 = UnityEngine.Object.Instantiate<GameObject>(this.ExampleBlueprintPrefab, new Vector3(150f, -50f, -100f), Quaternion.identity);
							ItemBlueprintItem component6 = gameObject7.GetComponent<ItemBlueprintItem>();
							EncounterTrackable component7 = gameObject7.GetComponent<EncounterTrackable>();
							component7.journalData.PrimaryDisplayName = component4.encounterTrackable.journalData.PrimaryDisplayName;
							component7.journalData.NotificationPanelDescription = component4.encounterTrackable.journalData.NotificationPanelDescription;
							component7.journalData.AmmonomiconFullEntry = component4.encounterTrackable.journalData.AmmonomiconFullEntry;
							component7.journalData.AmmonomiconSprite = component4.encounterTrackable.journalData.AmmonomiconSprite;
							component7.DoNotificationOnEncounter = false;
							component6.UsesCustomCost = true;
							component6.CustomCost = list[m];
							GungeonFlags saveFlagToSetOnAcquisition = GungeonFlags.NONE;
							for (int n = 0; n < component4.encounterTrackable.prerequisites.Length; n++)
							{
								if (component4.encounterTrackable.prerequisites[n].prerequisiteType == DungeonPrerequisite.PrerequisiteType.FLAG)
								{
									saveFlagToSetOnAcquisition = component4.encounterTrackable.prerequisites[n].saveFlagToCheck;
								}
							}
							component6.SaveFlagToSetOnAcquisition = saveFlagToSetOnAcquisition;
							component6.HologramIconSpriteName = component7.journalData.AmmonomiconSprite;
							shopItemController.CurrencyType = currencyType;
							shopItemController.Initialize(component6, this);
							gameObject7.SetActive(false);
						}
						else
						{

							shopItemController.CurrencyType = currencyType;

							shopItemController.customCanBuy += CustomCanBuyMethod;
							shopItemController.customPrice += CustomPriceMethod;
							shopItemController.removeCurrency += RemoveCurrencyMethod;

							shopItemController.OnPurchase += OnPurchaseMethod;
							shopItemController.OnSteal += OnStealMethod;

							shopItemController.customPriceSprite = this.customPriceSprite;

							shopItemController.Initialize(component4, this);
						}

						m_itemControllers.Add(shopItemController);
					}
				}
			}
			bool flag3 = false;
			if (base.shopItemsGroup2 != null && base.spawnPositionsGroup2.Length > 0)
			{

				int count = base.m_shopItems.Count;
				for (int num2 = 0; num2 < base.spawnPositionsGroup2.Length; num2++)
				{
					if (flag)
					{
						base.m_shopItems.Add(null);
					}
					else
					{
						float num3 = base.spawnGroupTwoItem1Chance;
						if (num2 == 1)
						{
							num3 = base.spawnGroupTwoItem2Chance;
						}
						else if (num2 == 2)
						{
							num3 = base.spawnGroupTwoItem3Chance;
						}
						bool isSeeded = GameManager.Instance.IsSeeded;
						if (((!isSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) < num3)
						{
							float replaceFirstRewardWithPickup = GameManager.Instance.RewardManager.CurrentRewardData.ReplaceFirstRewardWithPickup;
							if (!flag3 && ((!isSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) < replaceFirstRewardWithPickup)
							{
								flag3 = true;
								GameObject gameObject5 = new GameObject();
								switch (this.poolType)
								{
									case ShopItemPoolType.DEFAULT:
										gameObject5 = this.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(this.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);
										this.m_shopItems.Add(gameObject5);
										break;
									case ShopItemPoolType.DUPES:
										gameObject5 = this.shopItems.SelectByWeight(GameManager.Instance.IsSeeded);
										this.m_shopItems.Add(gameObject5);
										break;
									case ShopItemPoolType.DUPES_AND_NOEXCLUSION:
										gameObject5 = this.shopItems.SelectByWeightNoExclusions(GameManager.Instance.IsSeeded);
										this.m_shopItems.Add(gameObject5);
										break;
									default:
										gameObject5 = this.shopItems.SubshopSelectByWeightWithoutDuplicatesFullPrereqs(this.m_shopItems, weightModifier, 1, GameManager.Instance.IsSeeded);
										this.m_shopItems.Add(gameObject5);
										break;
								}
							}
							else if (!GameStatsManager.Instance.IsRainbowRun || AllowedToSpawnOnRainbowMode == true)
							{
								GameObject rewardObjectShopStyle2 = GameManager.Instance.RewardManager.GetRewardObjectShopStyle(GameManager.Instance.PrimaryPlayer, false, false, base.m_shopItems);
								base.m_shopItems.Add(rewardObjectShopStyle2);
							}
							else
							{
								base.m_shopItems.Add(null);
							}
						}
						else
						{
							base.m_shopItems.Add(null);
						}
					}
				}
				for (int num4 = 0; num4 < base.spawnPositionsGroup2.Length; num4++)
				{
					Transform transform3 = base.spawnPositionsGroup2[num4];
					if (!flag && !(base.m_shopItems[count + num4] == null))
					{
						PickupObject component8 = base.m_shopItems[count + num4].GetComponent<PickupObject>();
						if (!(component8 == null))
						{
							GameObject gameObject8 = new GameObject("Shop 2 item " + num4.ToString());
							Transform transform4 = gameObject8.transform;
							transform4.parent = transform3;
							transform4.localPosition = Vector3.zero;
							EncounterTrackable component9 = component8.GetComponent<EncounterTrackable>();
							if (component9 != null)
							{
								GameManager.Instance.ExtantShopTrackableGuids.Add(component9.EncounterGuid);
							}
							CustomShopItemController shopItemController2 = gameObject8.AddComponent<CustomShopItemController>();
							this.AssignItemFacing(transform3, shopItemController2);
							if (!base.m_room.IsRegistered(shopItemController2))
							{
								base.m_room.RegisterInteractable(shopItemController2);
							}

							shopItemController2.Initialize(component8, this);

							m_itemControllers.Add(shopItemController2);
						}
					}
				}
			}
			/*for (int num6 = 0; num6 < m_customShopItemControllers.Count; num6++)
			{
				
				m_customShopItemControllers[num6].CurrencyType = currencyType;

				m_customShopItemControllers[num6].CustomCanBuy += Balls.CustomCanBuy;
				m_customShopItemControllers[num6].CustomPrice += Balls.CustomPrice;
				m_customShopItemControllers[num6].RemoveCurrency += Balls.RemoveCurrency;
				m_customShopItemControllers[num6].customPriceSprite = this.customPriceSprite;
			}*/
		}

		private void AssignItemFacing(Transform spawnTransform, CustomShopItemController itemController)
		{
			if (this.currencyType == CustomShopItemController.ShopCurrencyType.META_CURRENCY)
			{
				itemController.UseOmnidirectionalItemFacing = true;
			}
			else if (spawnTransform.name.Contains("SIDE") || spawnTransform.name.Contains("EAST"))
			{
				itemController.itemFacing = DungeonData.Direction.EAST;
			}
			else if (spawnTransform.name.Contains("WEST"))
			{
				itemController.itemFacing = DungeonData.Direction.WEST;
			}
			else if (spawnTransform.name.Contains("NORTH"))
			{
				itemController.itemFacing = DungeonData.Direction.NORTH;
			}
		}
	}
}