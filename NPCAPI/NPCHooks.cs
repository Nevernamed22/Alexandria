using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace Alexandria.NPCAPI
{
    [HarmonyPatch]
    class NPCHooks
    {
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void Init() { }

        [HarmonyPatch(typeof(BaseShopController), nameof(BaseShopController.LockItems))]
        [HarmonyPrefix]
        private static bool BaseShopControllerLockItemsPatch(BaseShopController __instance)
        {
            if (__instance is not CustomShopController csc)
                return true;
            csc.LockItems();
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.Locked), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool ShopItemControllerLockedPatch(ShopItemController __instance, ref bool __result)
        {
            if (__instance is not CustomShopItemController csic)
                return true;
            __result = csic.Locked;
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.ModifiedPrice), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool ShopItemControllerModifiedPricePatch(ShopItemController __instance, ref int __result)
        {
            if (__instance is not CustomShopItemController csic)
                return true;
            __result = csic.ModifiedPrice;
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.OnEnteredRange))]
        [HarmonyPrefix]
        private static bool ShopItemControllerOnEnteredRangePatch(ShopItemController __instance, PlayerController interactor)
        {
            if (!__instance || __instance is not CustomShopItemController csic)
                return true;
            csic.OnEnteredRange(interactor);
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.Interact))]
        [HarmonyPrefix]
        private static bool ShopItemControllerInteractPatch(ShopItemController __instance, PlayerController player)
        {
            if (__instance is not CustomShopItemController csic)
                return true;
            csic.Interact(player);
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.ForceSteal))]
        [HarmonyPrefix]
        private static bool ShopItemControllerForceStealPatch(ShopItemController __instance, PlayerController player)
        {
            if (__instance is not CustomShopItemController csic)
                return true;
            csic.ForceSteal(player);
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.OnExitRange))]
        [HarmonyPrefix]
        private static bool ShopItemControllerOnExitRangePatch(ShopItemController __instance, PlayerController interactor)
        {
            if (__instance is not CustomShopItemController csic)
                return true;
            csic.OnExitRange(interactor);
            return false;
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.InitializeInternal))]
        [HarmonyPrefix]
        private static bool ShopItemControllerInitializeInternalPatch(ShopItemController __instance, PickupObject i)
        {
            if (!__instance || __instance is not CustomShopItemController csic)
                return true;
            csic.InitializeInternal(i);
            return false;
        }
    }
}
