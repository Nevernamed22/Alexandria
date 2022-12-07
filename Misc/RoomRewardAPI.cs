using Alexandria.Misc;
using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria
{
    public static class RoomRewardAPI
    {
        //Thank you to June for submitting this!
        public static void Init()
        {
            Hook roomClearRewardHook = new Hook(typeof(RoomHandler).GetMethod("HandleRoomClearReward", BindingFlags.Public | BindingFlags.Instance), typeof(RoomRewardAPI).GetMethod("RoomClearRewardHook"));
        }
        public static bool GuaranteeRoomClearRewardDrop = false;
        public static void RoomClearRewardHook(Action<RoomHandler> orig, RoomHandler self)
        {
            try
            {

                Type selfType = typeof(RoomHandler);
                if (GameManager.Instance.IsFoyer || GameManager.Instance.InTutorial)
                {
                    return;
                }
                if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
                {
                    return;
                }
                if (self.m_hasGivenReward)
                {
                    return;
                }
                if (self.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD)
                {
                    return;
                }
                self.m_hasGivenReward = true;
                if (self.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && self.area.PrototypeRoomBossSubcategory == PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS)
                {
                    self.HandleBossClearReward();
                    return;
                }
                if (self.PreventStandardRoomReward)
                {
                    return;
                }
                FloorRewardData currentRewardData = GameManager.Instance.RewardManager.CurrentRewardData;
                LootEngine.AmmoDropType ammoDropType = LootEngine.AmmoDropType.DEFAULT_AMMO;
                bool flag = LootEngine.DoAmmoClipCheck(currentRewardData, out ammoDropType);
                string path = (ammoDropType != LootEngine.AmmoDropType.SPREAD_AMMO) ? "Ammo_Pickup" : "Ammo_Pickup_Spread";
                float value = UnityEngine.Random.value;
                float num = currentRewardData.ChestSystem_ChestChanceLowerBound;
                float num2 = GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
                float num3 = -(GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f);
                if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                {
                    num2 += GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
                    num3 -= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f;
                }
                if (PassiveItem.IsFlagSetAtAll(typeof(ChamberOfEvilItem)))
                {
                    num3 *= -2f;
                }
                num = Mathf.Clamp(num + GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance, currentRewardData.ChestSystem_ChestChanceLowerBound, currentRewardData.ChestSystem_ChestChanceUpperBound) + num2 + num3;
                bool flag2 = currentRewardData.SingleItemRewardTable != null;
                bool flag3 = false;
                float num4 = 0.1f;
                if (!RoomHandler.HasGivenRoomChestRewardThisRun && MetaInjectionData.ForceEarlyChest)
                {
                    flag3 = true;
                }
                if (flag3)
                {
                    if (!RoomHandler.HasGivenRoomChestRewardThisRun && (GameManager.Instance.CurrentFloor == 1 || GameManager.Instance.CurrentFloor == -1))
                    {
                        flag2 = false;
                        num += num4;
                        if (GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.NumRoomsCleared > 4)
                        {
                            num = 1f;
                        }
                    }
                    if (!RoomHandler.HasGivenRoomChestRewardThisRun && self.distanceFromEntrance < RoomHandler.NumberOfRoomsToPreventChestSpawning)
                    {
                        GameManager.Instance.Dungeon.InformRoomCleared(false, false);
                        return;
                    }
                }
                BraveUtility.Log("Current chest spawn chance: " + num, Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);

                //Now here's the kicker
                ValidRoomRewardContents contentsSet = new ValidRoomRewardContents();
                contentsSet.overrideItemPool = new List<Tuple<float, int>>();
                contentsSet.overrideFunctionPool = new List<Action<Vector3, RoomHandler>>();

                if (OnRoomRewardDetermineContents != null) OnRoomRewardDetermineContents(self, contentsSet, num);

                if (contentsSet.overrideFunctionPool.Count > 0)
                {
                    Vector3 pos = self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true).ToVector3() + new Vector3(0.25f, 0f, 0f);
                    foreach (var item in contentsSet.overrideFunctionPool)
                    {
                        item?.Invoke(pos, self);
                    }

                    return;
                }
                if (value > num && !GuaranteeRoomClearRewardDrop)
                {
                    if (flag)
                    {
                        IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
                        LootEngine.SpawnItem((GameObject)BraveResources.Load(path, ".prefab"), bestRewardLocation.ToVector3(), Vector2.up, 1f, true, true, false);
                    }
                    GameManager.Instance.Dungeon.InformRoomCleared(false, false);
                    return;
                }
                if (flag2)
                {
                    float num5 = currentRewardData.PercentOfRoomClearRewardsThatAreChests;
                    if (PassiveItem.IsFlagSetAtAll(typeof(AmazingChestAheadItem)))
                    {
                        num5 *= 2f;
                        num5 = Mathf.Max(0.5f, num5);
                    }
                    flag2 = (UnityEngine.Random.value > num5);
                }
                if (flag2)
                {
                    float num6 = (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? GameManager.Instance.RewardManager.SinglePlayerPickupIncrementModifier : GameManager.Instance.RewardManager.CoopPickupIncrementModifier;
                    GameObject gameObject;
                    if (contentsSet.overrideItemPool.Count > 0)
                    {
                        GenericLootTable onTheFlyLootTable = LootUtility.CreateLootTable();
                        foreach (Tuple<float, int> entry in contentsSet.overrideItemPool) { onTheFlyLootTable.AddItemToPool(entry.Second, entry.First); }
                        gameObject = onTheFlyLootTable.defaultItemDrops.SelectByWeight();
                    }
                    else if (UnityEngine.Random.value < 1f / num6)
                    {
                        gameObject = currentRewardData.SingleItemRewardTable.SelectByWeight();
                    }
                    else
                    {
                        gameObject = ((UnityEngine.Random.value >= 0.9f) ? GameManager.Instance.RewardManager.FullHeartPrefab.gameObject : GameManager.Instance.RewardManager.HalfHeartPrefab.gameObject);
                    }
                    UnityEngine.Debug.Log(gameObject.name + "SPAWNED");
                    DebrisObject debrisObject = LootEngine.SpawnItem(gameObject, self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true).ToVector3() + new Vector3(0.25f, 0f, 0f), Vector2.up, 1f, true, true, false);
                    OnRoomClearItemDrop?.Invoke(debrisObject, self);
                    Exploder.DoRadialPush(debrisObject.sprite.WorldCenter.ToVector3ZUp(debrisObject.sprite.WorldCenter.y), 8f, 3f);
                    AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", debrisObject.gameObject);
                    GameManager.Instance.Dungeon.InformRoomCleared(true, false);
                }
                else
                {
                    IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(2, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
                    bool isRainbowRun = GameStatsManager.Instance.IsRainbowRun;
                    if (isRainbowRun)
                    {
                        LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteChest, bestRewardLocation.ToCenterVector2(), self, true);
                        RoomHandler.HasGivenRoomChestRewardThisRun = true;
                    }
                    else
                    {
                        Chest exists = self.SpawnRoomRewardChest(null, bestRewardLocation);
                        if (exists)
                        {
                            RoomHandler.HasGivenRoomChestRewardThisRun = true;
                        }
                    }
                    GameManager.Instance.Dungeon.InformRoomCleared(true, true);
                }
                if (flag)
                {
                    IntVector2 bestRewardLocation = self.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true);
                    Vector3 spawnpos = bestRewardLocation.ToVector3() + new Vector3(0.25f, 0f, 0f);
                    LootEngine.DelayedSpawnItem(1f, (GameObject)BraveResources.Load(path, ".prefab"), spawnpos, Vector2.up, 1f, true, true);
                }
            }
            catch (Exception err)
            {
                ETGModConsole.Log(err);
            }
        }


        public static Action<DebrisObject, RoomHandler> OnRoomClearItemDrop;
        public static Action<RoomHandler, ValidRoomRewardContents, float> OnRoomRewardDetermineContents;

        public class ValidRoomRewardContents : EventArgs
        {
            public List<Action<Vector3, RoomHandler>> overrideFunctionPool;
            public List<Tuple<float, int>> overrideItemPool;
        }
    }
}