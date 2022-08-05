using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomCharacters
{
    [fsObject]
    class SaveFileBullShit
    {
        public SaveFileBullShit()
        {
            this.m_customCharacterStats = new Dictionary<CustomPlayableCharacters, GameStats>(new CustomPlayableCharactersComparer());
			SaveSettingThing = new SaveManager.SaveType
			{
				filePattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiSave",
				//legacyFilePattern = "charapiSlot{0}.txt"


				encrypted = false,
				backupCount = 3,
				backupPattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiBackup.{1}",
				backupMinTimeMin = 45,
				legacyFilePattern = CharApiHiveMind.modPrefix.ToLower() + "GameStatsSlot{0}.txt"
			};
		}

		public static bool HasInstance
		{
			get
			{
				return SaveFileBullShit.m_instance != null;
			}
		}

		public static SaveFileBullShit Instance
		{
			get
			{
				if (SaveFileBullShit.m_instance == null)
				{
					Debug.LogError("Trying to access SaveFileBullShit before it has been initialized.");
				}
				return SaveFileBullShit.m_instance;
			}
		}


		public static bool Save()
		{
			bool result = false;
			try
			{
				if (SaveSettingThing == null )
                {
					SaveSettingThing = new SaveManager.SaveType
					{
						filePattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiSave",
						encrypted = false,
						backupCount = 3,
						backupPattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiBackup.{1}",
						backupMinTimeMin = 45,
						legacyFilePattern = CharApiHiveMind.modPrefix.ToLower() + "GameStatsSlot{0}.txt"
					};
				}
				result = SaveManager.Save<SaveFileBullShit>(m_instance, SaveSettingThing, 0, 0u, null);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("SAVE FAILED: {0}", new object[]
				{
					ex
				});
			}
			return result;
		}

		//public static bool Save()
		//{
		//	ETGModConsole.Log($"last played character is: {SaveFileBullShit.m_instance.customLastPlayedCharacter}");
		//	return SaveManager.Save<SaveFileBullShit>(SaveFileBullShit.m_instance, , , 0U, null);
		//}

		public static void Load()
		{
			SaveManager.Init();

			if (SaveSettingThing == null)
			{
				SaveSettingThing = new SaveManager.SaveType
				{
					filePattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiSave",
					encrypted = false,
					backupCount = 3,
					backupPattern = "Slot{0}." + CharApiHiveMind.modPrefix.ToLower() + "CharapiBackup.{1}",
					backupMinTimeMin = 45,
					legacyFilePattern = CharApiHiveMind.modPrefix.ToLower() + "GameStatsSlot{0}.txt"
				};
			}

			if (!SaveManager.Load<SaveFileBullShit>(SaveSettingThing, out m_instance, true))
			{
				m_instance = new SaveFileBullShit();
			}
		}

		//public static void Load()
        //{
		//	SaveManager.Init();
		//	if (!SaveManager.Load<SaveFileBullShit>(SaveSettingThing, out SaveFileBullShit.m_instance, true, 0U, null, null))
		//	{
		//		SaveFileBullShit.m_instance = new SaveFileBullShit();
		//	}
		//}

		public static SaveManager.SaveType SaveSettingThing;

		private static SaveFileBullShit m_instance;

		[fsProperty]
        public Dictionary<CustomPlayableCharacters, GameStats> m_customCharacterStats;
		//[fsProperty]
		//public CustomPlayableCharacters customLastPlayedCharacter;

		[fsIgnore]
		public SaveManager.SaveSlot cachedSaveSlot;
	}
}
