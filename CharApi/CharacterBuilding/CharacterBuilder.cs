using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using Alexandria.ItemAPI;
using System.Collections;
using System.Reflection;

namespace Alexandria.CharacterAPI
{
    /*
     * Creates a prefab for the custom character and applies 
     * all the metadata to it
     */
    public static class CharacterBuilder
    {
        public static Dictionary<string, Tuple<CustomCharacterData, GameObject>> storedCharacters = new Dictionary<string, Tuple<CustomCharacterData, GameObject>>();
        public static List<Gun> guns = new List<Gun>();
        public static Dictionary<string, tk2dSpriteCollectionData> storedCollections = new Dictionary<string, tk2dSpriteCollectionData>();

        private static void BuildCharacterCommon(CustomCharacterData data, bool hasAltSkin, bool paradoxUsesSprites, bool removeFoyerExtras,
            bool hasArmourlessAnimations, bool usesArmourNotHealth, bool hasCustomPast, string customPast, int metaCost, bool useGlow,
            GlowMatDoer glowVars, GlowMatDoer altGlowVars, tk2dSpriteCollectionData d1, tk2dSpriteAnimation SteveData1, tk2dSpriteCollectionData d2,
            tk2dSpriteAnimation SteveData2, Assembly assembly, bool isBundle)
        {
            var basePrefab = GetPlayerPrefab(data.baseCharacter);
            if (basePrefab == null)
            {
                ToolsCharApi.PrintError("Could not find prefab for: " + data.baseCharacter.ToString());
                return;
            }
            if (ToolsCharApi.EnableDebugLogging == true)
            {
                ToolsCharApi.Print("");
                ToolsCharApi.Print("--Building Character: " + data.nameShort + "--", "0000FF");
            }

            PlayerController playerController;
            GameObject gameObject = GameObject.Instantiate(basePrefab);

            playerController = gameObject.GetComponent<PlayerController>();
            var customCharacter = gameObject.AddComponent<CustomCharacter>();

            customCharacter.data = data;
            data.characterID = storedCharacters.Count;

            playerController.AllowZeroHealthState = usesArmourNotHealth;
            playerController.ForceZeroHealthState = usesArmourNotHealth;

            playerController.hasArmorlessAnimations = hasArmourlessAnimations;

            playerController.altHandName = "hand_alt";
            playerController.SwapHandsOnAltCostume = true;

            GameObject.DontDestroyOnLoad(gameObject);

            if (isBundle)
                CustomizeCharacterNoSprites(playerController, data, d1, SteveData1, d2, SteveData2, paradoxUsesSprites, assembly ?? Assembly.GetCallingAssembly());
            else
                CustomizeCharacter(playerController, data, paradoxUsesSprites, assembly ?? Assembly.GetCallingAssembly());

            data.useGlow = useGlow;

            if (useGlow)
            {
                data.emissiveColor = glowVars.emissiveColor;
                data.emissiveColorPower = glowVars.emissiveColorPower;
                data.emissivePower = glowVars.emissivePower;
                data.emissiveThresholdSensitivity = glowVars.emissiveThresholdSensitivity;
            }
            data.removeFoyerExtras = removeFoyerExtras;
            data.metaCost = metaCost;

            if (useGlow)
            {
                var material = new Material(EnemyDatabase.GetOrLoadByName("GunNut").sprite.renderer.material);
                material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
                material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
                material.SetTexture("_MainTexture", material.GetTexture("_MainTex"));
                material.SetColor("_EmissiveColor", glowVars.emissiveColor);
                material.SetFloat("_EmissiveColorPower", glowVars.emissiveColorPower);
                material.SetFloat("_EmissivePower", glowVars.emissivePower);
                material.SetFloat("_EmissiveThresholdSensitivity", glowVars.emissiveThresholdSensitivity);

                data.glowMaterial = material;
            }

            if (useGlow && hasAltSkin)
            {
                var material = new Material(EnemyDatabase.GetOrLoadByName("GunNut").sprite.renderer.material);
                material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
                material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
                material.SetTexture("_MainTexture", material.GetTexture("_MainTex"));
                material.SetColor("_EmissiveColor", altGlowVars.emissiveColor);
                material.SetFloat("_EmissiveColorPower", altGlowVars.emissiveColorPower);
                material.SetFloat("_EmissivePower", altGlowVars.emissivePower);
                material.SetFloat("_EmissiveThresholdSensitivity", altGlowVars.emissiveThresholdSensitivity);

                data.altGlowMaterial = material;
            }

            data.normalMaterial = new Material(ShaderCache.Acquire("Brave/PlayerShader"));

            basePrefab = null;
            storedCharacters.Add(data.nameInternal.ToLower(), new Tuple<CustomCharacterData, GameObject>(data, gameObject));

            customCharacter.past = customPast;
            customCharacter.hasPast = hasCustomPast;
            data.hasPast = hasCustomPast;

            //NOTE: we need to update the punchout arrays here to make sure Harmony's cached values for PlayerNames and PlayerUiNames
            //      are up to date by the time we actually start a punchout fight
            CharacterAPI.Hooks.RegisterCharacterForPunchout(customCharacter);

            gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(gameObject);

            ETGModConsole.Characters.Add(data.nameShort.ToLowerInvariant(), data.nameShort); //Adds characters to MTGAPIs character database
        }

        public static void BuildCharacter(CustomCharacterData data, bool hasAltSkin, bool paradoxUsesSprites, bool removeFoyerExtras, bool hasArmourlessAnimations = false,
            bool usesArmourNotHealth = false, bool hasCustomPast = false, string customPast = "", int metaCost = 0, bool useGlow = false,
            GlowMatDoer glowVars = null, GlowMatDoer altGlowVars = null, Assembly assembly = null)
        {
            BuildCharacterCommon(data, hasAltSkin, paradoxUsesSprites, removeFoyerExtras, hasArmourlessAnimations, usesArmourNotHealth,
                hasCustomPast, customPast, metaCost, useGlow, glowVars, altGlowVars, null, null, null, null,
                assembly ??= Assembly.GetCallingAssembly(), isBundle: false);
        }

        public static void BuildCharacterBundle(CustomCharacterData data, tk2dSpriteCollectionData d1, tk2dSpriteAnimation SteveData1, tk2dSpriteCollectionData d2,
            tk2dSpriteAnimation SteveData2, bool hasAltSkin, bool paradoxUsesSprites, bool removeFoyerExtras, bool hasArmourlessAnimations = false,
            bool usesArmourNotHealth = false, bool hasCustomPast = false, string customPast = "", int metaCost = 0, bool useGlow = false,
            GlowMatDoer glowVars = null, GlowMatDoer altGlowVars = null, Assembly assembly = null)
        {
            BuildCharacterCommon(data, hasAltSkin, paradoxUsesSprites, removeFoyerExtras, hasArmourlessAnimations, usesArmourNotHealth,
                hasCustomPast, customPast, metaCost, useGlow, glowVars, altGlowVars, d1, SteveData1, d2, SteveData2,
                assembly ??= Assembly.GetCallingAssembly(), isBundle: true);
        }

        private static void CustomizeCharacterCommon(PlayerController player, CustomCharacterData data, bool paradoxUsesSprites, Assembly assembly)
        {
            if (data.loadout != null)
                HandleLoadout(player, data.loadout, data.altGun);

            if (data.stats != null)
                HandleStats(player, data.stats);

            player.healthHaver.ForceSetCurrentHealth(data.health);
            player.healthHaver.Armor = (int)data.armor;

            player.characterIdentity = (PlayableCharacters)data.identity;

            StringHandler.AddStringDefinition("#PLAYER_NAME_" + player.characterIdentity.ToString().ToUpperInvariant(), data.name);
            StringHandler.AddStringDefinition("#PLAYER_NICK_" + player.characterIdentity.ToString().ToUpperInvariant(), data.nickname);

            StringHandler.AddDFStringDefinition("#CHAR_" + data.nameShort.ToString().ToUpper(), data.name);
            StringHandler.AddDFStringDefinition("#CHAR_" + data.nameShort.ToString().ToUpper() + "_SHORT", data.nameShort);

            if (paradoxUsesSprites)
            {
                var eevee = (GameObject)ResourceCache.Acquire("PlayerEevee");
                if (player.spriteAnimator.Library != null && eevee != null)
                    eevee.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(player.spriteAnimator.Library);
                if (player.AlternateCostumeLibrary != null && eevee != null)
                    eevee.GetComponent<CharacterAnimationRandomizer>().AddOverrideAnimLibrary(player.AlternateCostumeLibrary);
            }
        }

        public static void CustomizeCharacterNoSprites(PlayerController player, CustomCharacterData data, tk2dSpriteCollectionData d1, tk2dSpriteAnimation tk2DSpriteAnimation1, tk2dSpriteCollectionData d2, tk2dSpriteAnimation tk2DSpriteAnimation2, bool paradoxUsesSprites, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            HandleStrings(player, data);

            ToolsCharApi.StartTimer("    Sprite Handling");
            SpriteHandler.HandleSpritesBundle(player, tk2DSpriteAnimation1, d1, tk2DSpriteAnimation2, d2, data, assembly);
            ToolsCharApi.StopTimerAndReport("    Sprite Handling");

            CustomizeCharacterCommon(player, data, paradoxUsesSprites, assembly);
        }
        
        public static void CustomizeCharacter(PlayerController player, CustomCharacterData data, bool paradoxUsesSprites, Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            HandleStrings(player, data);

            ToolsCharApi.StartTimer("    Sprite Handling");
            SpriteHandler.HandleSprites(player, data, assembly);
            ToolsCharApi.StopTimerAndReport("    Sprite Handling");

            CustomizeCharacterCommon(player, data, paradoxUsesSprites, assembly);
        }

        public static void HandleStrings(PlayerController player, CustomCharacterData data)
        {
            player.name = data.nameInternal;
            if (data.faceCard != null)
                player.uiPortraitName = data.nameInternal + "_facecard";
        }

        public static void HandleDictionaries(CustomCharacterData data)
        {
            string keyBase = data.nameShort.ToUpper();
            StringHandler.AddStringDefinition("#PLAYER_NAME_" + keyBase, data.name); //TODO override the get methods instead of overwriting!
            StringHandler.AddStringDefinition("#PLAYER_NICK_" + data.nickname.ToUpper(), data.nickname);
            StringHandler.AddDFStringDefinition("#CHAR_" + keyBase, data.name);
            StringHandler.AddDFStringDefinition("#CHAR_" + keyBase + "_SHORT", data.nameShort);
        }

        public static void HandleLoadout(PlayerController player, List<Tuple<PickupObject, bool>> loadout, List<Tuple<PickupObject, bool>> altGun)
        {
            if (loadout == null)
                ToolsCharApi.PrintError("loadout is NULL, please verify it exists!");
            if (altGun == null)
                ToolsCharApi.PrintError("altGun is NULL, please verify it exists!");
            StripPlayer(player);
            foreach (var tuple in loadout)
            {
                var item = tuple.First;
                int id = item.PickupObjectId;
                if (item.GetComponent<PassiveItem>())
                    player.startingPassiveItemIds.Add(id);
                else if (item.GetComponent<PlayerItem>())
                    player.startingActiveItemIds.Add(id);
                else if (item.GetComponent<Gun>())
                    player.startingGunIds.Add(id);
                else
                    ToolsCharApi.PrintError("Is this even an item? It has no passive, active or gun component! " + item.EncounterNameOrDisplayName);
            }
            foreach (var tuple in altGun)
            {
                var item = tuple.First;
                int id = item.PickupObjectId;
                if (item.GetComponent<Gun>())
                    player.startingAlternateGunIds.Add(id);
                else
                    ToolsCharApi.PrintError("Is this even an gun? It has no gun component! " + item.EncounterNameOrDisplayName);
            }
        }

        public static void StripPlayer(PlayerController player)
        {
            if (player.passiveItems != null)
                player.RemoveAllPassiveItems();
            player.passiveItems = new List<PassiveItem>();
            player.startingPassiveItemIds = new List<int>();

            if (player.activeItems != null)
                player.RemoveAllActiveItems();
            player.activeItems = new List<PlayerItem>();
            player.startingActiveItemIds = new List<int>();

            if (player.inventory != null)
                player.inventory.DestroyAllGuns();
            player.startingGunIds = new List<int>();
            player.startingAlternateGunIds = new List<int>();
        }

        public static void HandleStats(PlayerController player, Dictionary<PlayerStats.StatType, float> stats)
        {
            foreach (var stat in stats)
            {
                player.stats.SetBaseStatValue(stat.Key, stat.Value, player);
                if (stat.Key == PlayerStats.StatType.DodgeRollDistanceMultiplier)
                    player.rollStats.distance *= stat.Value;
                if (stat.Key == PlayerStats.StatType.DodgeRollSpeedMultiplier)
                    player.rollStats.time *= 1f / (stat.Value + Mathf.Epsilon);
            }
        }

        public static GameObject GetPlayerPrefab(PlayableCharacters character)
        {
            string resourceName;
            if (character == PlayableCharacters.Soldier)
                resourceName = "marine";
            else if (character == PlayableCharacters.Pilot)
                resourceName = "rogue";
            else
                resourceName = character.ToString().ToLower();
            return (GameObject)BraveResources.Load("player" + resourceName);
        }
    }
}
