using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alexandria.Misc;
using Dungeonator;
using UnityEngine;

using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Alexandria.DungeonAPI
{
    [HarmonyPatch]
    public static class MasteryOverrideHandler
    {
        private static bool OublietteRegistered = false;
        private static bool AbbeyRegistered = false;
        private static bool RNGRegistered = false;

        //NOTE, the Rat Lair and Bullet Hell are HARDCODED to not spawn Pedestals
        public enum ViableRegisterFloors { OUBLIETTE, ABBEY, RNG }

        [HarmonyPatch(typeof(DungeonDatabase), nameof(DungeonDatabase.GetOrLoadByName))]
        [HarmonyPostfix]
        private static void DungeonDatabaseGetOrLoadByNamePatch(string name, ref Dungeon __result)
        {
            if (name.ToLower() == "base_cathedral" && AbbeyRegistered) { SetMasteryTokenDungeon(__result); }
            else if (name.ToLower() == "base_sewer" && OublietteRegistered) { SetMasteryTokenDungeon(__result); }
            else if (name.ToLower() == "base_nakatomi" && RNGRegistered) { SetMasteryTokenDungeon(__result); }
        }

        private static void SetMasteryTokenDungeon(Dungeon dungeon) { dungeon.BossMasteryTokenItemId = 469; }

        /// <summary>
        /// 'Registers' the specified floor to spawn a master round. 
        /// By default, will spawn the Keep Master Round. Use 'OnRewardPedestalDetermineContents' in CustomActions to change this.
        /// </summary>
        /// <param name="floorToRegister">The floor you want to register. Only three of the five floors without masteries are supported, due to Dodge Roll hardcoding boss rewards.</param>
        public static void RegisterFloorForMasterySpawn(ViableRegisterFloors floorToRegister)
        {
            switch (floorToRegister)
            {
                case ViableRegisterFloors.ABBEY:
                    AbbeyRegistered = true;
                    break;
                case ViableRegisterFloors.OUBLIETTE:
                    OublietteRegistered = true;
                    break;
                case ViableRegisterFloors.RNG:
                    RNGRegistered = true;
                    break;
            }
        }

        /// <summary>
        /// Returns true if the target pedestal contains the default Master Round item for the current level definition.
        /// </summary>
        /// <param name="pedestal">The target pedestal.</param>
        public static bool ContainsMasteryTokenForCurrentLevel(this RewardPedestal pedestal)
        {
            return (pedestal && pedestal.contents && GameManager.Instance && GameManager.Instance.Dungeon && pedestal.contents.PickupObjectId == GameManager.Instance.Dungeon.BossMasteryTokenItemId);
        }

        /// <summary>
        /// Loads a specified Dungeon prefab based on the string name. DO NOT USE IF YOU DONT KNOW WHAT YOU'RE DOING.
        /// </summary>
        /// <param name="name">The name of the Dungeon.</param>
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static Dungeon GetOrLoadByName_Orig(string name)
        {
            AssetBundle assetBundle = ResourceManager.LoadAssetBundle("dungeons/" + name.ToLower());
            DebugTime.RecordStartTime();
            Dungeon component = assetBundle.LoadAsset<GameObject>(name).GetComponent<Dungeon>();
            DebugTime.Log("AssetBundle.LoadAsset<Dungeon>({0})", new object[] { name });
            return component;
        }

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void Init() {}

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static Dungeon GetOrLoadByNameHook(Func<string, Dungeon> orig, string name) => null;
    }
}
