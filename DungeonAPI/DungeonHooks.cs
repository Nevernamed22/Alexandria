using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Dungeonator;
using MonoMod.RuntimeDetour;

using Alexandria.Misc;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Alexandria.DungeonAPI
{
    [HarmonyPatch]
    public static class DungeonHooks
    {
        public static event Action<LoopDungeonGenerator, Dungeon, DungeonFlow, int> OnPreDungeonGeneration;
        public static event Action OnPostDungeonGeneration;
        public static event Action OnFoyerAwake;
        private static GameManager targetInstance;

        [HarmonyPatch(typeof(MainMenuFoyerController), nameof(MainMenuFoyerController.Awake))]
        [HarmonyPostfix]
        private static void MainMenuFoyerControllerAwakePatch(MainMenuFoyerController __instance)
        {
            OnFoyerAwake?.Invoke();
        }

        [HarmonyPatch(typeof(LoopDungeonGenerator), MethodType.Constructor, new[]{typeof(Dungeon), typeof(int)})]
        [HarmonyPostfix]
        private static void LoopDungeonGeneratorMethodToPatchPatch(LoopDungeonGenerator __instance, Dungeon dungeon, int dungeonSeed)
        {
            if (GameManager.Instance != null && GameManager.Instance != targetInstance)
            {
                targetInstance = GameManager.Instance;
                targetInstance.OnNewLevelFullyLoaded += OnLevelLoadInternal;
            }

            OnPreDungeonGeneration?.Invoke(__instance, dungeon, __instance.m_assignedFlow, dungeonSeed);
            dungeon = null;
        }

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void OnLevelLoad() { }

        private static void OnLevelLoadInternal()
        {
            OnPostDungeonGeneration?.Invoke();
        }
    }
}
