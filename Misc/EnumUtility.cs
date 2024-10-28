using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
	[Obsolete("EnumUtility is broken and doesn't work, use ETGModCompatibility.ExtendEnum instead.")]
   public static class EnumUtility
    {
        public static string GetFullyQualifiedName(string guid, string value)
        {
            return $"{guid}_{value}";
        }

        private static readonly Dictionary<int, Type> reverseMapper = new Dictionary<int, Type>();

        private static readonly object lockObject = new object();

        public const int START_INDEX = 1000;

        public const string MAX_DATA = "maximumStoredValueForEnum";

        public static Type GetEnumType(int number)
        {
            reverseMapper.TryGetValue(number, out var res);
            return res;
        }

        public static List<T> GetValues<T>() where T : System.Enum
        {
            List<T> itemList = new List<T>();
            foreach (T item in Enum.GetValues(typeof(T)))
                itemList.Add(item);

            string startKey = typeof(T).Name + "_";
            foreach (var item in EnumSaveData.Instance.m_customEnums[ModGUID])
            {
                if (item.Key.StartsWith(startKey))
                {
                    int enumVal = int.Parse((string)item.Value);
                    T convertedEnumVal = (T)(object)enumVal;
                    itemList.Add(convertedEnumVal);
                }
            }

            return itemList;
        }

        public static T GetEnumValue<T>(string guid, string value) where T : System.Enum
        {
            //if (sizeof(T) != sizeof(int))
            //    throw new NotSupportedException($"Cannot manage values of type {typeof(T).Name} in GuidManager.GetEnumValue");

            string saveKey = $"{typeof(T).Name}_{guid}_{value}";

            int enumValue = EnumSaveData.Instance.GetValueAsInt(ModGUID, saveKey);
			if (enumValue == default)
            {
                lock (lockObject)
                {
                    enumValue = EnumSaveData.Instance.GetValueAsInt(ModGUID, MAX_DATA);
                    if (enumValue < START_INDEX)
                        enumValue = START_INDEX;

					EnumSaveData.Instance.SetValue(ModGUID, MAX_DATA, enumValue + 1);
					EnumSaveData.Instance.SetValue(ModGUID, saveKey, enumValue);
					//save savedata
					EnumSaveData.Save();

				}
            }

            reverseMapper[enumValue] = typeof(T);
			return (T)(object)enumValue;

		}

		public static string ModGUID = "AlexandriaAPI";

	}

	[fsObject]
	class EnumSaveData
	{
		public EnumSaveData()
		{
			this.m_customEnums = new Dictionary<string, Dictionary<string, object>>();
			SaveSettingThing = new SaveManager.SaveType
			{
				filePattern = "Slot{0}." + "AlexandriaSave",
				encrypted = false,
				backupCount = 3,
				backupPattern = "Slot{0}." + "AlexandriaBackup.{1}",
				backupMinTimeMin = 45,
				legacyFilePattern = "AlexandriaSave{0}.txt"
			};
		}

		public static bool HasInstance
		{
			get
			{
				return EnumSaveData.m_instance != null;
			}
		}

		public static EnumSaveData Instance
		{
			get
			{
				if (EnumSaveData.m_instance == null)
				{
					Debug.LogError("Trying to access EnumSaveData before it has been initialized.");
				}
				return EnumSaveData.m_instance;
			}
		}


		public static bool Save()
		{
			bool result = false;
			try
			{
				if (SaveSettingThing == null)
				{
					SaveSettingThing = new SaveManager.SaveType
					{
						filePattern = "Slot{0}." + "AlexandriaSave",
						encrypted = false,
						backupCount = 3,
						backupPattern = "Slot{0}." + "AlexandriaBackup.{1}",
						backupMinTimeMin = 45,
						legacyFilePattern = "AlexandriaSave{0}.txt"
					};
				}
				result = SaveManager.Save<EnumSaveData>(m_instance, SaveSettingThing, 0, 0u, null);
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

		public static void Load()
		{
			SaveManager.Init();

			if (SaveSettingThing == null)
			{
				SaveSettingThing = new SaveManager.SaveType
				{
					filePattern = "Slot{0}." + "AlexandriaSave",
					encrypted = false,
					backupCount = 3,
					backupPattern = "Slot{0}." + "AlexandriaBackup.{1}",
					backupMinTimeMin = 45,
					legacyFilePattern = "AlexandriaSave{0}.txt"
				};
			}

			if (!SaveManager.Load<EnumSaveData>(SaveSettingThing, out m_instance, true))
			{
				m_instance = new EnumSaveData();
			}
		}

		/// <summary>
		/// Get the value of a key as an object in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to get the value of.</param>
		/// <returns>The value of the key as an object.</returns>
		/// <typeparam name="T">The type of object you are getting.</typeparam>
		public T GetValueAsObject<T>(string guid, string key)
		{
			if (m_customEnums == null)
				m_customEnums = new Dictionary<string, Dictionary<string, object>>();

			if (!m_customEnums.ContainsKey(guid))
				m_customEnums.Add(guid, new Dictionary<string, object>());

			if (!m_customEnums[guid].ContainsKey(key))
				m_customEnums[guid].Add(key, null);

			return (T)m_customEnums[guid][key];
		}

		/// <summary>
		/// Get the value of a key as a string in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to get the value of.</param>
		/// <returns>The value of the key as a string.</returns>
		public string GetValue(string guid, string key)
		{
			var value = GetValueAsObject<object>(guid, key);
			return value == null ? default(string) : value.ToString();
		}

		/// <summary>
		/// Get the value of a key as an integer in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to get the value of.</param>
		/// <returns>The value of the key as an integer.</returns>
		public int GetValueAsInt(string guid, string key)
		{
			string value = GetValue(guid, key);
			int result;
			int.TryParse(value, out result);
			return result;
		}

		/// <summary>
		/// Get the value of a key as a float in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to get the value of.</param>
		/// <returns>The value of the key as a float.</returns>
		public float GetValueAsFloat(string guid, string key)
		{
			string value = GetValue(guid, key);
			float result;
			float.TryParse(value, out result);
			return result;
		}

		/// <summary>
		/// Get the value of a key as a boolean in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to get the value of.</param>
		/// <returns>The value of the key as a boolean.</returns>
		public bool GetValueAsBoolean(string guid, string key)
		{
			string value = GetValue(guid, key);
			bool result;
			bool.TryParse(value, out result);
			return result;
		}

		/// <summary>
		/// Set the value of a key as an object in the save data,
		/// It's recommended to not save an object that implements Unity's Object class as it can cause a infinite recursion and crash the game.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to set the value of.</param>
		/// <param name="value">The object value to set.</param>
		/// <typeparam name="T">The type of object you are setting.</typeparam>
		public void SetValueAsObject<T>(string guid, string key, T value)
		{
			if (m_customEnums == null)
				m_customEnums = new Dictionary<string, Dictionary<string, object>>();

			if (!m_customEnums.ContainsKey(guid))
				m_customEnums.Add(guid, new Dictionary<string, object>());

			if (!m_customEnums[guid].ContainsKey(key))
				m_customEnums[guid].Add(key, value);
			else
				m_customEnums[guid][key] = value;
		}

		/// <summary>
		/// Set the value of a key in the save data.
		/// </summary>
		/// <param name="guid">The GUID of the mod.</param>
		/// <param name="key">The key to set the value of.</param>
		/// <param name="value">The value to set.</param>
		public void SetValue(string guid, string key, object value)
		{
			SetValueAsObject(guid, key, value == null ? default(string) : value.ToString());
		}


		public static SaveManager.SaveType SaveSettingThing;

		private static EnumSaveData m_instance;

		[fsProperty]
		public Dictionary<string, Dictionary<string, object>> m_customEnums;
		
		[fsIgnore]
		public SaveManager.SaveSlot cachedSaveSlot = default;
	}

}
