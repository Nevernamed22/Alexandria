using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.IO;
using Dungeonator;
using System.Reflection;
using System.Collections;
using Alexandria.Misc;

namespace Alexandria.SaveAPI
{
    /// <summary>
    /// The core class in SaveAPI
    /// </summary>
    public static class SaveAPIManager
    {


        public static List<string> saveFiles = new List<string>();

        public static Dictionary<string, SaveManager.SaveType> AdvancedGameSaves = new Dictionary<string, SaveManager.SaveType>();
        public static Dictionary<string, SaveManager.SaveType> AdvancedMidGameSaves = new Dictionary<string, SaveManager.SaveType>();

        /// <summary>
        /// Call this method in your <see cref="ETGModule.Init"/> method. Adds SaveAPI <see cref="Hook"/>s, loads <see cref="AdvancedGameStatsManager"/> and setups the custom <see cref="SaveManager.SaveType"/>s
        /// </summary>
        /// <param name="prefix">Mod prefix for SaveTypes</param>
        public static void Setup(string prefix)
        {
            
            AdvancedGameSaves.Add(prefix, new SaveManager.SaveType
            {
                filePattern = "Slot{0}." + prefix + "Save",
                encrypted = false,
                backupCount = 3,
                backupPattern = "Slot{0}." + prefix + "Backup.{1}",
                backupMinTimeMin = 45,
                legacyFilePattern = prefix + "GameStatsSlot{0}.txt"
            });

            AdvancedMidGameSaves.Add(prefix, new SaveManager.SaveType
            {
                filePattern = "Active{0}." + prefix + "Game",
                legacyFilePattern = prefix + "ActiveSlot{0}.txt",
                encrypted = false,
                backupCount = 0,
                backupPattern = "Active{0}." + prefix + "Backup.{1}",
                backupMinTimeMin = 60
            });

            
            for (int i = 0; i < 3; i++)
            {
                SaveManager.SaveSlot saveSlot = (SaveManager.SaveSlot)i;
                SaveTools.SafeMove(Path.Combine(SaveManager.OldSavePath, string.Format(AdvancedGameSaves[prefix].legacyFilePattern, saveSlot)), Path.Combine(SaveManager.OldSavePath,
                    string.Format(AdvancedGameSaves[prefix].filePattern, saveSlot)), false);
                SaveTools.SafeMove(Path.Combine(SaveManager.OldSavePath, string.Format(AdvancedGameSaves[prefix].filePattern, saveSlot)), Path.Combine(SaveManager.OldSavePath,
                    string.Format(AdvancedGameSaves[prefix].filePattern, saveSlot)), false);
                SaveTools.SafeMove(SaveTools.PathCombine(SaveManager.SavePath, "01", string.Format(AdvancedGameSaves[prefix].filePattern, saveSlot)), Path.Combine(SaveManager.SavePath,
                    string.Format(AdvancedGameSaves[prefix].filePattern, saveSlot)), true);
            }
            saveFiles.Add(prefix);
        }

        public static void Init()
        {
            CustomHuntQuests.DoSetup();
            saveHook = new Hook(
                typeof(GameStatsManager).GetMethod("Save", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("SaveHook")
            );
            loadHook = new Hook(
                typeof(GameStatsManager).GetMethod("Load", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("LoadHook")
            );
            resetHook = new Hook(
                typeof(GameStatsManager).GetMethod("DANGEROUS_ResetAllStats", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("ResetHook")
            );
            beginSessionHook = new Hook(
                typeof(GameStatsManager).GetMethod("BeginNewSession", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("BeginSessionHook")
            );
            endSessionHook = new Hook(
                typeof(GameStatsManager).GetMethod("EndSession", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("EndSessionHook")
            );
            clearAllStatsHook = new Hook(
                typeof(GameStatsManager).GetMethod("ClearAllStatsGlobal", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("ClearAllStatsHook")
            );
            deleteMidGameSaveHook = new Hook(
                typeof(SaveManager).GetMethod("DeleteCurrentSlotMidGameSave", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("DeleteMidGameSaveHook")
            );
            midgameSaveHook = new Hook(
                typeof(GameManager).GetMethod("DoMidgameSave", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("MidgameSaveHook")
            );
            invalidateSaveHook = new Hook(
                typeof(GameManager).GetMethod("InvalidateMidgameSave", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("InvalidateSaveHook")
            );
            revalidateSaveHook = new Hook(
                typeof(GameManager).GetMethod("RevalidateMidgameSave", BindingFlags.Public | BindingFlags.Static),
                typeof(SaveAPIManager).GetMethod("RevalidateSaveHook")
            );
            frameDelayedInitizlizationHook = new Hook(
                typeof(Dungeon).GetMethod("FrameDelayedMidgameInitialization", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("FrameDelayedInitizlizationHook")
            );
            moveSessionStatsHook = new Hook(
                typeof(GameStatsManager).GetMethod("MoveSessionStatsToSavedSessionStats", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("MoveSessionStatsHook")
            );
            prerequisiteHook = new Hook(
                typeof(DungeonPrerequisite).GetMethod("CheckConditionsFulfilled", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("PrerequisiteHook")
            );
            clearActiveGameDataHook = new Hook(
                typeof(GameManager).GetMethod("ClearActiveGameData", BindingFlags.Public | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("ClearActiveGameDataHook")
            );
            aiactorRewardsHook = new Hook(
                typeof(AIActor).GetMethod("HandleRewards", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("AIActorRewardsHook")
            );
            aiactorEngagedHook = new Hook(
                typeof(AIActor).GetMethod("OnEngaged", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(SaveAPIManager).GetMethod("AIActorEngagedHook")
            );

            LoadGameStatsFirstLoad();
            BreachShopTool.DoSetup();

            m_loaded = true;
        }

        public static bool IsFirstLoad
        {
            get
            {
                return FirstLoad;
            }
        }

        /// <summary>
        /// Unloads SaveAPI and then setups SaveAPI again
        /// </summary>
        /// <param name="prefix">Mod prefix for SaveTypes</param>
        public static void Reload(string prefix)
        {
            Unload();
            Setup(prefix);
        }

        private static void LoadGameStatsFirstLoad()
        {
            bool cachedvalue = FirstLoad;
            FirstLoad = true;
            GameStatsManager.Load();
            FirstLoad = cachedvalue;
        }

        /// <summary>
        /// Disposes SaveAPI <see cref="Hook"/>s, unloads <see cref="AdvancedGameStatsManager"/> and nulls custom <see cref="SaveManager.SaveType"/>s
        /// </summary>
        public static void Unload()
        {
            if (!m_loaded)
            {
                return;
            }
            //AdvancedGameSave = null;
            //AdvancedMidGameSave = null;
            saveHook?.Dispose();
            loadHook?.Dispose();
            resetHook?.Dispose();
            beginSessionHook?.Dispose();
            endSessionHook?.Dispose();
            clearAllStatsHook?.Dispose();
            deleteMidGameSaveHook?.Dispose();
            midgameSaveHook?.Dispose();
            invalidateSaveHook?.Dispose();
            revalidateSaveHook?.Dispose();
            frameDelayedInitizlizationHook?.Dispose();
            moveSessionStatsHook?.Dispose();
            prerequisiteHook?.Dispose();
            clearActiveGameDataHook?.Dispose();
            aiactorRewardsHook?.Dispose();
            aiactorEngagedHook?.Dispose();
            CustomHuntQuests.Unload();
            AdvancedGameStatsManager.Save();
            AdvancedGameStatsManager.Unload();
            BreachShopTool.Unload();
            m_loaded = false;
        }


        public static CustomDungeonFlags CreateNewDungeonFlag(string guid, string name)
        {
            return EnumUtility.GetEnumValue<CustomDungeonFlags>(guid, name);
        }

        public static CustomCharacterSpecificGungeonFlags CreateNewCharacterSpecificGungeonFlags(string guid, string name)
        {
            return EnumUtility.GetEnumValue<CustomCharacterSpecificGungeonFlags>(guid, name);
        }

        public static CustomTrackedMaximums CreateNewTrackedMaximums(string guid, string name)
        {
            return EnumUtility.GetEnumValue<CustomTrackedMaximums>(guid, name);
        }

        public static CustomTrackedStats CreateNewTrackedStats(string guid, string name)
        {
            
            return EnumUtility.GetEnumValue<CustomTrackedStats>(guid, name);
        }

        /// <summary>
        /// Gets <paramref name="flag"/>'s value
        /// </summary>
        /// <param name="flag">The flag to check</param>
        /// <returns>The value of <paramref name="flag"/> or <see langword="false"/> if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static bool GetFlag(string guid, CustomDungeonFlags flag)
        {
            if (!AdvancedGameStatsManager.HasInstance(guid))
            {
                return false;
            }
            return AdvancedGameStatsManager.GetInstance(guid).GetFlag(flag);
        }



        /// <summary>
        /// Gets the total value of <paramref name="stat"/>
        /// </summary>
        /// <param name="stat">Target stat.</param>
        /// <returns>The value of <paramref name="stat"/> or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static float GetPlayerStatValue(string guid, CustomTrackedStats stat)
        {
            if (!AdvancedGameStatsManager.HasInstance(guid))
            {
                return 0f;
            }
            return AdvancedGameStatsManager.GetInstance(guid).GetPlayerStatValue(stat);
        }

        /// <summary>
        /// Gets the session value of <paramref name="stat"/>
        /// </summary>
        /// <param name="stat">Target stat</param>
        /// <returns>The value of <paramref name="stat"/> in the current session or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance or the player isn't in a session</returns>
        public static float GetSessionStatValue(string guid, CustomTrackedStats stat)
        {
            if (AdvancedGameStatsManager.HasInstance(guid) && AdvancedGameStatsManager.GetInstance(guid).IsInSession)
            {
                return AdvancedGameStatsManager.GetInstance(guid).GetSessionStatValue(stat);
            }
            return 0f;
        }

        /// <summary>
        /// Gets <paramref name="character"/>'s <paramref name="stat"/> value.
        /// </summary>
        /// <param name="stat">Target stat</param>
        /// <param name="character">The character</param>
        /// <returns><paramref name="character"/>'s <paramref name="stat"/> value or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static float GetCharacterStatValue(string guid, PlayableCharacters character, CustomTrackedStats stat)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                return AdvancedGameStatsManager.GetInstance(guid).GetCharacterStatValue(character, stat);
            }
            return 0f;
        }

        /// <summary>
        /// Gets the primary player's or the Pilot's (if primary player doesn't exist) <paramref name="stat"/> value.
        /// </summary>
        /// <param name="stat">Target stat</param>
        /// <returns>Primary player's or the Pilot's (if primary player doesn't exist) <paramref name="stat"/> value or 0 if <see cref="AdvancedGameStatsManager"/> doesn't haven an instance</returns>
        public static float GetCharacterStatValue(string guid, CustomTrackedStats stat)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                if(GameManager.HasInstance && GameManager.Instance.PrimaryPlayer != null)
                {
                    return AdvancedGameStatsManager.GetInstance(guid).GetCharacterStatValue(stat);
                }
                return AdvancedGameStatsManager.GetInstance(guid).GetCharacterStatValue(PlayableCharacters.Pilot, stat);
            }
            return 0f;
        }

        /// <summary>
        /// Gets the session character's or the Pilot's (if the player isn't in session) <paramref name="flag"/> value
        /// </summary>
        /// <param name="flag">The character-specific flag to check</param>
        /// <returns>The session character's or the Pilot's (if the player isn't in session) <paramref name="flag"/> value or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static bool GetCharacterSpecificFlag(string guid, CustomCharacterSpecificGungeonFlags flag)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                if (AdvancedGameStatsManager.GetInstance(guid).IsInSession)
                {
                    return AdvancedGameStatsManager.GetInstance(guid).GetCharacterSpecificFlag(flag);
                }
                return AdvancedGameStatsManager.GetInstance(guid).GetCharacterSpecificFlag(PlayableCharacters.Pilot, flag);
            }
            return false;
        }

        /// <summary>
        /// Gets <paramref name="character"/>'s <paramref name="flag"/> value
        /// </summary>
        /// <param name="character">Target character</param>
        /// <param name="flag">The character-specific flag to check</param>
        /// <returns><paramref name="character"/>'s <paramref name="flag"/> value or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static bool GetCharacterSpecificFlag(string guid, PlayableCharacters character, CustomCharacterSpecificGungeonFlags flag)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                return AdvancedGameStatsManager.GetInstance(guid).GetCharacterSpecificFlag(character, flag);
            }
            return false;
        }

        /// <summary>
        /// Gets <paramref name="maximum"/>'s value in total.
        /// </summary>
        /// <param name="maximum">Target maximum</param>
        /// <returns><paramref name="maximum"/> value or 0 if <see cref="AdvancedGameStatsManager"/> doesn't have an instance</returns>
        public static float GetPlayerMaximum(string guid, CustomTrackedMaximums maximum)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                return AdvancedGameStatsManager.GetInstance(guid).GetPlayerMaximum(maximum);
            }
            return 0f;
        }

        /// <summary>
        /// Sets <paramref name="flag"/>'s value to <paramref name="value"/>
        /// </summary>
        /// <param name="flag">The target flag</param>
        /// <param name="value">Value to set</param>
        public static void SetFlag(string guid, CustomDungeonFlags flag, bool value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).SetFlag(flag, value);
            }
        }

        /// <summary>
        /// Sets <paramref name="stat"/>'s value to <paramref name="value"/>
        /// </summary>
        /// <param name="stat">The target stat</param>
        /// <param name="value">Value to set</param>
        public static void SetStat(string guid, CustomTrackedStats stat, float value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).SetStat(stat, value);
            }
        }

        /// <summary>
        /// Increments <paramref name="stat"/> value by <paramref name="value"/>
        /// </summary>
        /// <param name="stat">Target stat</param>
        /// <param name="value">Increment value</param>
        public static void RegisterStatChange(string guid, CustomTrackedStats stat, float value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).RegisterStatChange(stat, value);
            }
        }

        /// <summary>
        /// Sets <paramref name="maximum"/>'s value to <paramref name="value"/> if <paramref name="maximum"/>'s current value is less than <paramref name="value"/>
        /// </summary>
        /// <param name="maximum">Target maximum</param>
        /// <param name="value">Value to set</param>
        public static void UpdateMaximum(string guid, CustomTrackedMaximums maximum, float value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).UpdateMaximum(maximum, value);
            }
        }

        /// <summary>
        /// Sets the session character's or the Pilot's (if the player isn't in a session) <paramref name="flag"/> value
        /// </summary>
        /// <param name="flag">Target flag</param>
        /// <param name="value">Value to set</param>
        public static void SetCharacterSpecificFlag(string guid, CustomCharacterSpecificGungeonFlags flag, bool value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).SetCharacterSpecificFlag(flag, value);
            }
        }

        /// <summary>
        /// Sets <paramref name="character"/>'s <paramref name="flag"/> value
        /// </summary>
        /// <param name="character">Target character</param>
        /// <param name="flag">Target flag</param>
        /// <param name="value">Value to set</param>
        public static void SetCharacterSpecificFlag(string guid, PlayableCharacters character, CustomCharacterSpecificGungeonFlags flag, bool value)
        {
            if (AdvancedGameStatsManager.HasInstance(guid))
            {
                AdvancedGameStatsManager.GetInstance(guid).SetCharacterSpecificFlag(character, flag, value);
            }
        }

        public static void AIActorEngagedHook(Action<AIActor, bool> orig, AIActor self, bool isReinforcement)
        {
            if (!self.HasBeenEngaged)
            {
                if (self.SetsCustomFlagOnActivation())
                {
                    AdvancedGameStatsManager.GetInstance(self.GetCustomGuid()).SetFlag(self.GetCustomFlagToSetOnActivation(), true);
                }
            }
            orig(self, isReinforcement);
        }

        public static void AIActorRewardsHook(Action<AIActor> orig, AIActor self)
        {
            FieldInfo i = typeof(AIActor).GetField("m_hasGivenRewards", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!(bool)i.GetValue(self) && !self.IsTransmogrified)
            {
                if (self.SetsCustomFlagOnDeath())
                {
                    AdvancedGameStatsManager.GetInstance(self.GetCustomGuid()).SetFlag(self.GetCustomFlagToSetOnDeath(), true);
                }
                if (self.SetsCustomCharacterSpecificFlagOnDeath())
                {
                    AdvancedGameStatsManager.GetInstance(self.GetCustomGuid()).SetCharacterSpecificFlag(self.GetCustomCharacterSpecificFlagToSetOnDeath(), true);
                }
            }
            orig(self);
        }

        public static bool SaveHook(Func<bool> orig)
        {
            bool result = orig();
            AdvancedGameStatsManager.Save();
            EnumSaveData.Save();
            return result;
        }

        public static void LoadHook(Action orig)
        {
            AdvancedGameStatsManager.Load();
            EnumSaveData.Load();
            orig();
        }

        public static void ResetHook(Action orig)
        {
            AdvancedGameStatsManager.DANGEROUS_ResetAllStats();
            orig();
        }

        public static void BeginSessionHook(Action<GameStatsManager, PlayerController> orig, GameStatsManager self, PlayerController player)
        {
            orig(self, player);
            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                if (AdvancedGameStatsManager.HasInstance(save.Key))
                {
                    AdvancedGameStatsManager.GetInstance(save.Key).BeginNewSession(player);
                }
            }
        }

        public static void EndSessionHook(Action<GameStatsManager, bool, bool> orig, GameStatsManager self, bool recordSessionStats, bool decrementDifferentiator = true)
        {
            orig(self, recordSessionStats, decrementDifferentiator);
            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                if (AdvancedGameStatsManager.HasInstance(save.Key))
                {
                    AdvancedGameStatsManager.GetInstance(save.Key).EndSession(recordSessionStats);
                }
            }
        }

        public static void ClearAllStatsHook(Action<GameStatsManager> orig, GameStatsManager self)
        {
            orig(self);
            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                if (AdvancedGameStatsManager.HasInstance(save.Key))
                {
                    AdvancedGameStatsManager.GetInstance(save.Key).ClearAllStatsGlobal();
                }
            }
        }
        public static void DeleteMidGameSaveHook(Action<SaveManager.SaveSlot?> orig, SaveManager.SaveSlot? overrideSaveSlot)
        {
            orig(overrideSaveSlot);

            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                if (AdvancedGameStatsManager.HasInstance(save.Key))
                {
                    AdvancedGameStatsManager.GetInstance(save.Key).midGameSaveGuid = null;
                }
            }
            
            string path = string.Format(SaveManager.MidGameSave.filePattern, (overrideSaveSlot == null) ? SaveManager.CurrentSaveSlot : overrideSaveSlot.Value);
            string path2 = Path.Combine(SaveManager.SavePath, path);
            if (File.Exists(path2))
            {
                File.Delete(path2);
            }
        }

        public static void MidgameSaveHook(Action<GlobalDungeonData.ValidTilesets> orig, GlobalDungeonData.ValidTilesets tileset)
        {
            AdvancedGameStatsManager.DoMidgameSave();
            orig(tileset);
        }

        public static void InvalidateSaveHook(Action<bool> orig, bool savestats)
        {
            AdvancedGameStatsManager.InvalidateMidgameSave(false);
            orig(savestats);
        }

        public static void RevalidateSaveHook(Action orig)
        {
            AdvancedGameStatsManager.RevalidateMidgameSave(false);
            orig();
        }

        public static IEnumerator FrameDelayedInitizlizationHook(Func<Dungeon, MidGameSaveData, IEnumerator> orig, Dungeon self, MidGameSaveData data)
        {
            yield return orig(self, data);
            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                AdvancedMidGameSaveData midgameSave;
                if (AdvancedGameStatsManager.VerifyAndLoadMidgameSave(out midgameSave, save.Key, true))
                {
                    midgameSave.LoadDataFromMidGameSave(save.Key);
                }
            }
            yield break;
        }

        public static GameStats MoveSessionStatsHook(Func<GameStatsManager, GameStats> orig, GameStatsManager self)
        {
            foreach (var save in SaveAPIManager.AdvancedGameSaves)
            {
                AdvancedGameStatsManager.GetInstance(save.Key).MoveSessionStatsToSavedSessionStats();
            } 
            return orig(self);
        }

        public static bool PrerequisiteHook(Func<DungeonPrerequisite, bool> orig, DungeonPrerequisite self)
        {
            if (self is CustomDungeonPrerequisite)
            {
                return (self as CustomDungeonPrerequisite).CheckConditionsFulfilled();
            }
            return orig(self);
        }

        public static void ClearActiveGameDataHook(Action<GameManager, bool, bool> orig, GameManager self, bool destroyGameManager, bool endSession)
        {
            orig(self, destroyGameManager, endSession);
            OnActiveGameDataCleared?.Invoke(self, destroyGameManager, endSession);
        }

        private static Hook saveHook;
        private static Hook loadHook;
        private static Hook resetHook;
        private static Hook beginSessionHook;
        private static Hook endSessionHook;
        private static Hook clearAllStatsHook;
        private static Hook deleteMidGameSaveHook;
        private static Hook midgameSaveHook;
        private static Hook invalidateSaveHook;
        private static Hook revalidateSaveHook;
        private static Hook frameDelayedInitizlizationHook;
        private static Hook moveSessionStatsHook;
        private static Hook prerequisiteHook;
        private static Hook clearActiveGameDataHook;
        private static Hook aiactorRewardsHook;
        private static Hook aiactorEngagedHook;
        private static bool m_loaded;
        //public static SaveManager.SaveType AdvancedGameSave;
        //public static SaveManager.SaveType AdvancedMidGameSave;
        public static OnActiveGameDataClearedDelegate OnActiveGameDataCleared;
        public delegate void OnActiveGameDataClearedDelegate(GameManager manager, bool destroyGameManager, bool endSession);
        private static bool FirstLoad;
    }
}
