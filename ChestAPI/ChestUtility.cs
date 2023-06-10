using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.ChestAPI
{
    //This is NOT a method to aid in the setup of custom chest types. This is a toolbox of useful utility methods and extensions for modifying and spawning chests at runtime.
    public static class ChestUtility
    {
        /// <summary>
        /// Attaches a fuse to the chest, and ignites it. Does nothing if the chest is already fused.
        /// </summary>
        /// <param name="chest">The chest to be fused.</param>
        public static void AddFuse(this Chest chest)
        {
            if (chest.GetFuse() == null)
            {
                chest.TriggerCountdownTimer();
                AkSoundEngine.PostEvent("Play_OBJ_fuse_loop_01", chest.gameObject);
            }
        }

        /// <summary>
        /// Returns the fuse attached to a chest, if one is present. NOTE: If you're using the stripped DLL, you can simply use 'chest.extantFuse'
        /// </summary>
        /// <param name="chest">The chest to be checked.</param>
        public static ChestFuseController GetFuse(this Chest chest) { return chest.extantFuse; }

        /// <summary>
        /// A simple, advanced method for spawning reward chests. 
        /// </summary>
        /// <param name="location">The position at which the chest should be spawned.</param>
        /// <param name="tier">The tier of chest that should be spawned. Includes edge cases such as Glitched, Rainbow, Secret Rainbow, and Rat, but cannot spawn Truth Chests.</param>
        /// <param name="locked">Whether or not the chest should be locked.</param>
        /// <param name="type">Whether the chest should spawn an item, gun, or be random.</param>
        /// <param name="mimic">Whether the chest should be a mimic. Leave UNSPECIFIED for regular chances.</param>
        /// <param name="fused">Whether the chest should be fused.</param>
        public static Chest SpawnChestEasy(IntVector2 location, ChestTier tier, bool locked, Chest.GeneralChestType type = Chest.GeneralChestType.UNSPECIFIED, ThreeStateValue mimic = ThreeStateValue.UNSPECIFIED, ThreeStateValue fused = ThreeStateValue.UNSPECIFIED)
        {
            GameObject chestPrefab = null;
            switch (tier)
            {
                case ChestTier.BLACK:
                    chestPrefab = GameManager.Instance.RewardManager.S_Chest.gameObject;
                    break;
                case ChestTier.BLUE:
                    chestPrefab = GameManager.Instance.RewardManager.C_Chest.gameObject;
                    break;
                case ChestTier.BROWN:
                    chestPrefab = GameManager.Instance.RewardManager.D_Chest.gameObject;
                    break;
                case ChestTier.GREEN:
                    chestPrefab = GameManager.Instance.RewardManager.B_Chest.gameObject;
                    break;
                case ChestTier.RED:
                    chestPrefab = GameManager.Instance.RewardManager.A_Chest.gameObject;
                    break;
                case ChestTier.SYNERGY:
                    chestPrefab = GameManager.Instance.RewardManager.Synergy_Chest.gameObject;
                    break;
                case ChestTier.RAINBOW:
                    chestPrefab = GameManager.Instance.RewardManager.Rainbow_Chest.gameObject;
                    break;
                case ChestTier.SECRETRAINBOW:
                    chestPrefab = GameManager.Instance.RewardManager.D_Chest.gameObject;
                    break;
                case ChestTier.GLITCHED:
                    chestPrefab = GameManager.Instance.RewardManager.B_Chest.gameObject;
                    break;
                case ChestTier.RAT:
                    chestPrefab = LoadHelper.LoadAssetFromAnywhere<GameObject>("chest_rat");
                    break;
                case ChestTier.TRUTH:
                    Debug.LogError("Alexandria (ChestUtility): Cannot spawn Truth Chest.");
                    break;
                case ChestTier.OTHER:
                    Debug.LogError("Alexandria (ChestUtility): Cannot spawn 'Other' Chest.");
                    break;
            }
            if (chestPrefab != null)
            {
                Chest spawnedChest = Chest.Spawn(chestPrefab.GetComponent<Chest>(), location);
                if (locked) spawnedChest.IsLocked = true;
                else spawnedChest.IsLocked = false;
                if (tier == ChestTier.GLITCHED)
                {
                    spawnedChest.BecomeGlitchChest();
                }
                if (tier == ChestTier.SECRETRAINBOW)
                {
                    spawnedChest.IsRainbowChest = true;
                    spawnedChest.ChestIdentifier = Chest.SpecialChestIdentifier.SECRET_RAINBOW;
                }

                //Loot Type
                if (type == Chest.GeneralChestType.ITEM) { spawnedChest.lootTable.lootTable = GameManager.Instance.RewardManager.ItemsLootTable; }
                else if (type == Chest.GeneralChestType.WEAPON) { spawnedChest.lootTable.lootTable = GameManager.Instance.RewardManager.GunsLootTable; }
                else if (type == Chest.GeneralChestType.UNSPECIFIED)
                {
                    bool IsAGun = UnityEngine.Random.value <= 0.5f;
                    spawnedChest.lootTable.lootTable = (IsAGun ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
                }

                //Mimic State
                if (mimic == ThreeStateValue.FORCEYES) spawnedChest.overrideMimicChance = 100;
                if (mimic == ThreeStateValue.FORCENO) spawnedChest.overrideMimicChance = 0;
                spawnedChest.MaybeBecomeMimic();

                //Fuse State
                if (fused == ThreeStateValue.FORCEYES) spawnedChest.AddFuse();
                if (fused == ThreeStateValue.FORCENO) spawnedChest.PreventFuse = true;

                spawnedChest.RegisterChestOnMinimap(spawnedChest.GetAbsoluteParentRoom());

                return spawnedChest;
            }
            else return null;
        }

        /// <summary>
        /// Returns the quality of a chest in the form of the custom ChestTier enum. Chests which do not meet any of the other requirements will return type 'OTHER'.
        /// </summary>
        /// <param name="chest">The chest to be checked.</param>
        public static ChestTier GetChestTier(this Chest chest)
        {
            if (chest.IsGlitched) return ChestTier.GLITCHED;
            if (chest.breakAnimName.Contains("wood_"))
            {
                if (chest.IsRainbowChest) return ChestTier.SECRETRAINBOW;
                else return ChestTier.BROWN;
            }
            if (chest.breakAnimName.Contains("silver_")) return ChestTier.BLUE;
            else if (chest.breakAnimName.Contains("green_")) return ChestTier.GREEN;
            else if (chest.breakAnimName.Contains("redgold_"))
            {
                if (chest.IsRainbowChest) return ChestTier.RAINBOW;
                else return ChestTier.RED;
            }
            else if (chest.breakAnimName.Contains("blackbone_")) return ChestTier.BLACK;
            else if (chest.breakAnimName.Contains("synergy_")) return ChestTier.SYNERGY;
            else if (chest.breakAnimName.Contains("truth_")) return ChestTier.TRUTH;
            else if (chest.breakAnimName.Contains("rat_")) return ChestTier.RAT;
            return ChestTier.OTHER;
        }
        public enum ChestTier
        {
            BROWN,
            BLUE,
            GREEN,
            RED,
            BLACK,
            RAINBOW,
            SECRETRAINBOW,
            RAT,
            SYNERGY,
            TRUTH,
            GLITCHED,
            OTHER
        }
    }
}
