using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alexandria.Misc;
using Dungeonator;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public static class MasteryOverrideHandler
    {
        /// <summary>
        /// Initialises the hooks which allow the mastery override system to function.
        /// </summary>
        internal static void InitInternal()
        {
            OublietteRegistered = false;
            AbbeyRegistered = false;
            RNGRegistered = false;
            new Hook(
                typeof(DungeonDatabase).GetMethod("GetOrLoadByName", BindingFlags.Static | BindingFlags.Public),
                typeof(MasteryOverrideHandler).GetMethod("GetOrLoadByNameHookInternal", BindingFlags.Static | BindingFlags.NonPublic));
        }

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
        private static bool OublietteRegistered;
        private static bool AbbeyRegistered;
        private static bool RNGRegistered;

        /// <summary>
        /// The hook method which allows the mastery override system to function.
        /// </summary>
        /// <param name="orig">The original method.</param>
        /// <param name="name">The dungeon name being loaded..</param>
        internal static Dungeon GetOrLoadByNameHookInternal(Func<string, Dungeon> orig, string name)
        {
            Dungeon dungeon = null;
            if (name.ToLower() == "base_cathedral" && AbbeyRegistered) { dungeon = SetMasteryTokenDungeon(GetOrLoadByName_Orig(name)); }
            else if (name.ToLower() == "base_sewer" && OublietteRegistered) { dungeon = SetMasteryTokenDungeon(GetOrLoadByName_Orig(name)); }
            else if (name.ToLower() == "base_nakatomi" && RNGRegistered) { dungeon = SetMasteryTokenDungeon(GetOrLoadByName_Orig(name)); }

            if (dungeon)
            {
                DebugTime.RecordStartTime();
                DebugTime.Log("AssetBundle.LoadAsset<Dungeon>({0})", new object[] { name });
                return dungeon;
            }
            else { return orig(name); }
        }
        private static Dungeon SetMasteryTokenDungeon(Dungeon dungeon) { dungeon.BossMasteryTokenItemId = 469; return dungeon; }

        //NOTE, the Rat Lair and Bullet Hell are HARDCODED to not spawn Pedestals
        public enum ViableRegisterFloors
        {
            OUBLIETTE,
            ABBEY,
            RNG
        }

        /// <summary>
        /// Returns true if the target pedestal contains the default Master Round item for the current level definition.
        /// </summary>
        /// <param name="pedestal">The target pedestal.</param>
        public static bool ContainsMasteryTokenForCurrentLevel(this RewardPedestal pedestal)
        {
            if (pedestal && pedestal.contents && GameManager.Instance && GameManager.Instance.Dungeon && pedestal.contents.PickupObjectId == GameManager.Instance.Dungeon.BossMasteryTokenItemId) return true;
            return false;
        }
        /// <summary>
        /// Loads a specified Dungeon prefab based on the string name. DO NOT USE IF YOU DONT KNOW WHAT YOU'RE DOING.
        /// </summary>
        /// <param name="name">The name of the Dungeon.</param>
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
