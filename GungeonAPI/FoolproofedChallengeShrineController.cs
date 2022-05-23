using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Dungeonator;
using UnityEngine;
using System.Reflection;


namespace Alexandria.DungeonAPI
{
	public class FoolproofedChallengeShrineController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
	{
		public FoolproofedChallengeShrineController()
		{
			this.playerVFXOffset = Vector3.zero;
		}

		public void ConfigureOnPlacement(RoomHandler room)
		{
			this.m_parentRoom = room;
			this.m_parentRoom.PreventStandardRoomReward = true;
			this.RegisterMinimapIcon();
		}
		private void Update()
		{

			if (this.m_parentRoom.IsSealed && GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.CurrentRoom != null)
			{
				if (GameManager.Instance.PrimaryPlayer.CurrentRoom != this.m_parentRoom)
				{
					this.m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
					this.m_parentRoom.UnsealRoom();
				}
				else if (!this.m_parentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
				{
					this.m_noEnemySealTime += BraveTime.DeltaTime;
					if (this.m_noEnemySealTime > 3f)
					{
						this.m_parentRoom.TriggerNextReinforcementLayer();
					}
					if (this.m_noEnemySealTime > 5f)
					{
						this.m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
						this.m_parentRoom.UnsealRoom();
					}
				}
				else
				{
					this.m_noEnemySealTime = 0f;
				}
			}
		}

		public void RegisterMinimapIcon()
		{
			this.m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(this.m_parentRoom, (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon", ".prefab"), false);
		}

		public void GetRidOfMinimapIcon()
		{
			if (this.m_instanceMinimapIcon != null)
			{
				Minimap.Instance.DeregisterRoomIcon(this.m_parentRoom, this.m_instanceMinimapIcon);
				this.m_instanceMinimapIcon = null;
			}
		}
		private void DoShrineEffect(PlayerController player)
		{
			this.m_parentRoom.TriggerNextReinforcementLayer();
			this.m_parentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, false);
			this.m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealAll;
			this.m_parentRoom.SealRoom();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameManager.Instance.GetOtherPlayer(player).ReuniteWithOtherPlayer(player, false);
			}
			RoomHandler parentRoom = this.m_parentRoom;
			parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(this.HandleEnemiesClearedA));
			
			
			if (this.onPlayerVFX != null)
			{
				player.PlayEffectOnActor(this.onPlayerVFX, this.playerVFXOffset, true, false, false);
			}
			this.GetRidOfMinimapIcon();
		}

		private void HandleEnemiesClearedA()
		{
			if (!this.m_parentRoom.TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, false))
			{
				this.HandleFinalEnemiesCleared();
			}
		}

		

		private void HandleFinalEnemiesCleared()
		{
			this.m_parentRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
			RoomHandler parentRoom = this.m_parentRoom;
			parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(this.HandleFinalEnemiesCleared));
			Chest chest = GameManager.Instance.RewardManager.SpawnRewardChestAt(this.m_parentRoom.GetBestRewardLocation(new IntVector2(3, 2), RoomHandler.RewardLocationStyle.CameraCenter, true), -1f, PickupObject.ItemQuality.EXCLUDED);
			if (chest)
			{
				chest.ForceUnlock();
				chest.RegisterChestOnMinimap(this.m_parentRoom);
			}
		}

		public float GetDistanceToPoint(Vector2 point)
		{
			tk2dSprite sprite = null;
			foreach (Component item in base.GetComponentsInChildren(typeof(Component)))
			{
				if (item is tk2dSprite && item.name == "ShrineBase")
                {
					sprite = item as tk2dSprite;
				}
			}
			if (sprite == null)
            {
				return 100f;
            }
			Vector3 v = BraveMathCollege.ClosestPointOnRectangle(point, base.GetComponentInChildren<SpeculativeRigidbody>().UnitBottomLeft, base.GetComponentInChildren<SpeculativeRigidbody>().UnitDimensions);
			return Vector2.Distance(point, v) / 1.5f;
		}

		public float GetOverrideMaxDistance()
		{
			return -1f;
		}

		public void OnEnteredRange(PlayerController interactor)
		{
			SpriteOutlineManager.AddOutlineToSprite(this.AlternativeOutlineTarget ?? base.sprite, Color.white);
		}

		public void OnExitRange(PlayerController interactor)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(this.AlternativeOutlineTarget ?? base.sprite, false);
		}

		private IEnumerator HandleShrineConversation(PlayerController interactor)
		{
			TextBoxManager.ShowStoneTablet(this.talkPoint.position, this.talkPoint, -1f, StringTableManager.GetString(this.displayTextKey), true, false);
			int selectedResponse = -1;
			interactor.SetInputOverride("shrineConversation");
			yield return null;
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(this.acceptOptionKey), StringTableManager.GetString(this.declineOptionKey));
			while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
			{
				yield return null;
			}
			interactor.ClearInputOverride("shrineConversation");
			TextBoxManager.ClearTextBox(this.talkPoint);
			if (selectedResponse == 0)
			{
				this.DoShrineEffect(interactor);
			}
			else
			{
				this.m_useCount--;
				this.m_parentRoom.RegisterInteractable(this);
			}
			yield break;
		}

		public void Interact(PlayerController interactor)
		{
			if (this.m_useCount > 0)
			{
				return;
			}
			this.m_useCount++;
			this.m_parentRoom.DeregisterInteractable(this);
			base.StartCoroutine(this.HandleShrineConversation(interactor));
		}

		public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
		{
			shouldBeFlipped = false;
			return string.Empty;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public string displayTextKey;

		public string acceptOptionKey;

		public string declineOptionKey;

		public Transform talkPoint;

		public GameObject onPlayerVFX;

		public Vector3 playerVFXOffset;

		public bool usesCustomChestTable;

		public WeightedGameObjectCollection CustomChestTable;

		public tk2dBaseSprite AlternativeOutlineTarget;

		private int m_useCount;

		private RoomHandler m_parentRoom;

		private GameObject m_instanceMinimapIcon;

		private float m_noEnemySealTime;
	}

}
