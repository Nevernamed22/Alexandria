using Alexandria.NPCAPI;
using Dungeonator;
using HarmonyLib;
using InControl.NativeProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Alexandria.DungeonAPI.SpecialComponents;
using static Alexandria.DungeonAPI.SpecialComponents.ShopItemPosition;

namespace Alexandria.DungeonAPI
{
    public class DungeonPatches
    {


        [HarmonyPatch(typeof(DungeonFlowNode))]
        [HarmonyPatch("UsesGlobalBossData", MethodType.Getter)]
        public class Postfix_DungeonFlowNode 
        {
            [HarmonyPrefix]
            public static bool Postfix(ref bool __result, DungeonFlowNode __instance)
            {
                bool isGlitchedTable = __instance.overrideRoomTable != null ? __instance.overrideRoomTable.name == "alexandriaGlitchTable" : false;
                return GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SUPERBOSSRUSH && __instance.overrideExactRoom == null && __instance.roomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && isGlitchedTable == false;
            }
        }

        [HarmonyPatch(typeof(RoomHandler))]
        [HarmonyPatch("HandleBossClearReward", MethodType.Normal)]
        public class Postfix_RoomHandler
        {
            [HarmonyPrefix]
            public static void Postfix(RoomHandler __instance)
            {
                var name = __instance.GetRoomName();
                if (name != null && StaticReferences.GlitchBossNames.Contains(name))
                {
                    for (int j = 0; j < 8; j++)
                    {
                        GameObject gameObject = GameManager.Instance.Dungeon.sharedSettingsPrefab.ChestsForBosses.SelectByWeight();
                        RewardPedestal component = gameObject.GetComponent<RewardPedestal>();
                        DungeonData data = GameManager.Instance.Dungeon.data;
                        IntVector2 centeredVisibleClearSpot = __instance.GetCenteredVisibleClearSpot(2, 2);
                        RewardPedestal rewardPedestal2 = RewardPedestal.Spawn(component, centeredVisibleClearSpot, __instance);
                        rewardPedestal2.IsBossRewardPedestal = true;
                        rewardPedestal2.lootTable.lootTable = __instance.OverrideBossRewardTable;
                        data[centeredVisibleClearSpot].isOccupied = true;
                        data[centeredVisibleClearSpot + IntVector2.Right].isOccupied = true;
                        data[centeredVisibleClearSpot + IntVector2.Up].isOccupied = true;
                        data[centeredVisibleClearSpot + IntVector2.One].isOccupied = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(RoomHandler))]
        [HarmonyPatch("AssignRoomVisualType", MethodType.Normal)]
        public class Postfix_RoomHandler_2
        {
            [HarmonyPostfix]
            public static void Postfix(RoomHandler __instance)
            {
                if (__instance.area.PrototypeRoomName == null) { return; }
                if (RoomFactory.roomNames.Contains(__instance.area.PrototypeRoomName))
                {
                    var fuckYouDodgeRoll = __instance.area.prototypeRoom;
                    if (fuckYouDodgeRoll.overrideRoomVisualTypeForSecretRooms == true && fuckYouDodgeRoll.overrideRoomVisualType > -1)
                    {
                        __instance.RoomVisualSubtype = fuckYouDodgeRoll.overrideRoomVisualType;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AttackMoveBehavior))]
        [HarmonyPatch("Start", MethodType.Normal)]
        public class Postfix_AttackMoveBehavior
        {
            [HarmonyPostfix]
            public static void Postfix(AttackMoveBehavior __instance)
            {
                if (__instance.m_aiActor.GetAbsoluteParentRoom() == null) { return; }
                var leapPoints = __instance.m_aiActor.GetAbsoluteParentRoom().GetComponentsInRoom<AttackLeapPoint>();
                if (leapPoints != null && leapPoints.Count > 0)
                {
                    var room = __instance.m_aiActor.GetAbsoluteParentRoom().GetCenterCell().ToCenterVector2();

                    __instance.Positions = new Vector2[]
                    {

                    };

                    List<Vector2> p = new List<Vector2>();
                    foreach (var entry in leapPoints)
                    {
                        var pos = entry.gameObject.transform.position;
                        p.Add(new Vector2(pos.x, pos.y)-room );
                    }
                    __instance.Positions = p.ToArray();
                }
            }
        }
        [HarmonyPatch(typeof(BaseShopController))]
        [HarmonyPatch("HandleEnter", MethodType.Normal)]
        public class Prefix_BaseShopController
        {
            [HarmonyPostfix]
            public static void Prefix(BaseShopController __instance)
            {
                if (__instance == null) { return; }
                if (__instance.GetAbsoluteParentRoom() == null) { return; }
                var room = __instance.GetAbsoluteParentRoom();

                var leapPoints = room.GetComponentsInRoom<ShopItemPosition>();
                var shops = room.GetComponentsInRoom<BaseShopController>();


                if (leapPoints != null && leapPoints.Count > 0)
                {
                    foreach (var entry in leapPoints)
                    {



                        if (shops.Count() == 1)
                        {
                            if (entry.Used == false)
                            {
                                if (entry.SeenByAny != true && entry.Type == __instance.baseShopType)
                                {
                                    entry.Used = true;
                                    entry.DoItemPlace(__instance);
                                }
                                else if (entry.SeenByAny == true)
                                {
                                    entry.Used = true;
                                    entry.DoItemPlace(__instance);
                                }
                            }
                        }
                        else if (shops.Count() > 1)
                        {
                            if (GetClosestShop(entry.transform, shops) == __instance)
                            {
                                if (entry.Used == false)
                                {
                                    if (entry.SeenByAny != true && entry.Type == __instance.baseShopType)
                                    {
                                        entry.Used = true;
                                        entry.DoItemPlace(__instance);
                                    }
                                    else if (entry.SeenByAny == true)
                                    {
                                        entry.Used = true;
                                        entry.DoItemPlace(__instance);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            static BaseShopController GetClosestShop(Transform s, List<BaseShopController> enemies)
            {
                BaseShopController bestTarget = null;
                float closestDistanceSqr = Mathf.Infinity;
                Vector3 currentPosition = s.position;
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
        [HarmonyPatch(typeof(ShopItemController))]
        [HarmonyPatch("ModifiedPrice", MethodType.Getter)]
        public class Postfix_ShopItemController
        {
            [HarmonyPostfix]
            public static int Postfix(int __result, ShopItemController __instance)
            {
                if (__instance.GetComponent<ShopDiscountController>() == null) { return __result; }
                int Cost = (int)(__result * __instance.GetComponent<ShopDiscountController>().DoPriceReduction());
                if (__instance.GetComponent<ShopDiscountController>().DoPriceOverride() > -1) { Cost = __instance.GetComponent<ShopDiscountController>().DoPriceOverride(); }
                return Cost;
            }
        }
    }
}
