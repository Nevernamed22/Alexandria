using System;
using UnityEngine;
using HarmonyLib;

namespace Alexandria.ItemAPI
{
    [HarmonyPatch]
    public static class FakePrefabHooks
    {
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void Init() { }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.AcquirePassiveItemPrefabDirectly))]
        [HarmonyPrefix]
        private static void PlayerControllerAcquirePassiveItemPrefabDirectlyPrefixPatch(PlayerController __instance, PassiveItem item, ref bool __state)
        {
            __state = FakePrefab.IsFakePrefab(item.gameObject);
            if (__state)
                item.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.AcquirePassiveItemPrefabDirectly))]
        [HarmonyPostfix]
        private static void PlayerControllerAcquirePassiveItemPrefabDirectlyPostfixPatch(PlayerController __instance, PassiveItem item, bool __state)
        {
            if (__state)
                item.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(PlayerItem), nameof(PlayerItem.Pickup))]
        [HarmonyPrefix]
        private static void PlayerItemPickupPrefixPatch(PlayerItem __instance, PlayerController player, ref bool __state)
        {
            __state = FakePrefab.IsFakePrefab(__instance.gameObject);
            if (__state)
                __instance.gameObject.SetActive(true);
        }

        [HarmonyPatch(typeof(PlayerItem), nameof(PlayerItem.Pickup))]
        [HarmonyPostfix]
        private static void PlayerItemPickupPostfixPatch(PlayerItem __instance, PlayerController player, bool __state)
        {
            if (__state)
                __instance.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object))]
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Transform))]
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Transform), typeof(bool))]
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion))]
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform))]
        [HarmonyPostfix]
        private static void UnityEngineObjectInstantiatePatch(UnityEngine.Object original, ref UnityEngine.Object __result)
        {
            __result = FakePrefab.Instantiate(original, __result);
        }
    }
}
