using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
namespace Alexandria.CharacterAPI
{
    //This basically does nothing

    public static class StringHandler
    {
        public static Dictionary<string, string> customStringDictionary = new Dictionary<string, string>();

        public static void AddStringDefinition(string key, string value)
        {
            if (StringTableManager.m_coreTable is var coretable && !coretable.ContainsKey(key))
                coretable.Add(key, new StringTableManager.SimpleStringCollection(){singleString = value});
            if (StringTableManager.m_backupCoreTable is var backupCoretable && !backupCoretable.ContainsKey(key))
                backupCoretable.Add(key, new StringTableManager.SimpleStringCollection(){singleString = value});
        }

        public static void AddDFStringDefinition(string key, string value)
        {
            string lowerKey = key.ToLower();
            if (!customStringDictionary.ContainsKey(lowerKey))
                customStringDictionary.Add(lowerKey, value);
        }
    }
}
