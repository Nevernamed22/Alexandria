using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
namespace Alexandria.CharacterAPI
{
    //This basically does nothing

    public static class StringHandler
    {
        private static FieldInfo strings = typeof(StringTableManager.ComplexStringCollection).GetField("strings", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo singleString = typeof(StringTableManager.SimpleStringCollection).GetField("singleString", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo dfStrings = typeof(dfLanguageManager).GetField("strings", BindingFlags.NonPublic | BindingFlags.Instance);
        public static Dictionary<string, string> customStringDictionary = new Dictionary<string, string>();

        private static string[] fields =
        {
            "m_coreTable", //good 
            "m_backupCoreTable", //good
        };


        public static void AddStringDefinition(string key, string value)
        {
            foreach (string field in fields)
            {
                FieldInfo fi = typeof(StringTableManager).GetField(field, BindingFlags.NonPublic | BindingFlags.Static);
                var dictionary = (Dictionary<string, StringTableManager.StringCollection>)fi.GetValue(null);

                if (dictionary == null) continue;

                var collection = new StringTableManager.SimpleStringCollection();
                collection.AddString(value, 1);

                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, collection);
            }
        }

        public static void AddDFStringDefinition(string key, string value)
        {
            string lowerKey = key.ToLower();
            if (!customStringDictionary.ContainsKey(lowerKey))
                customStringDictionary.Add(lowerKey, value);
        }
    }
}
