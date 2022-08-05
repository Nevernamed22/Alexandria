using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomCharacters
{
    
    class CharApiHiveMind : MonoBehaviour
    {
        public static void Init(string prefix)
        {
			modPrefix = prefix;
            if (ETGModMainBehaviour.Instance?.gameObject != null)
            {
                bool foundIt = false;
                //foreach (Component component in ETGModMainBehaviour.Instance.gameObject.GetComponents<Component>())
                if (ETGModMainBehaviour.Instance.gameObject.GetComponent("CharApiHiveMind") != null)
                {
                    //if (component.GetType().ToString().ToLower().Contains("charapihivemind"))
                    //{
                        var component = ETGModMainBehaviour.Instance.gameObject.GetComponent("CharApiHiveMind");

                        foundIt = true;

                        var _versionInternal = (float)ReflectionHelper.GetValue(component.GetType().GetField("versionInternal"), component);
                        var _modPrefix = (string)ReflectionHelper.GetValue(component.GetType().GetField("modPrefix"), component);
                        
                        if (version != _versionInternal)
                        {
                            ETGModConsole.Log($"CharApi ({prefix}) - ({_modPrefix}): Warning! this mod's charapi version ({version}) dose not match the main CharApiHiveMind version ({_versionInternal})");
                        }
                        //this.trueHiveMindVersion = _trueHiveMindVersion;
                        ETGModConsole.Log($"CharApi ({prefix}): Hivemind has been found!");
                    //}
                }

                if (!foundIt)
                {
                    var hivemind = ETGModMainBehaviour.Instance?.gameObject.AddComponent<CharApiHiveMind>();
                    hivemind.modPrefixInternal = prefix;
                    hivemind.versionInternal = version;
                    ETGModConsole.Log($"CharApi ({prefix}): No Hivemind found so we're creating one");

                }
            }
        }



        public static bool AddNewCharacter(string prefix, PlayableCharacters character)
        {
            bool status = false;
            if (ETGModMainBehaviour.Instance?.gameObject != null)
            {
                foreach (Component component in ETGModMainBehaviour.Instance.gameObject.GetComponents<Component>())
                {
                    if (component.GetType().ToString().ToLower().Contains("charapihivemind"))
                    {                        
                        var _characters = (Dictionary<PlayableCharacters, string>)ReflectionHelper.GetValue(component.GetType().GetField("characters"), component);
                        var _modPrefix = (string)ReflectionHelper.GetValue(component.GetType().GetField("modPrefix"), component);


                        if (_characters.ContainsKey(character))
                        {
                            ETGModConsole.Log($"CharApi ({prefix}): Warning! two characters have the same id ({(int)character})! this is very very bad please inform {prefix}/{_characters[character]}");
                        }
                        else
                        {
                            _characters.Add(character, prefix);
                            status = true;
                        }
                    }
                }
            }
            return status;
        }

        public static float version = 1.3f;

        public static string modPrefix;
        public string modPrefixInternal;
        public float versionInternal;
        public Dictionary<PlayableCharacters, string> characters = new Dictionary<PlayableCharacters, string>();
    }

	
}
