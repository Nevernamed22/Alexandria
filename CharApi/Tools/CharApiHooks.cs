using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using MonoMod.RuntimeDetour;
using Object = UnityEngine.Object;
using IEnumerator = System.Collections.IEnumerator;


using Alexandria.ItemAPI;
using Alexandria.Misc;
using Dungeonator;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using FullInspector;

namespace Alexandria.CharacterAPI
{
    [HarmonyPatch]
    public static class Hooks
    {
        public static void Init()
        {
            try
            {
                Hook foyerCallbacksHook = new Hook(
                   typeof(Foyer).GetMethod("SetUpCharacterCallbacks", BindingFlags.NonPublic | BindingFlags.Instance),
                   typeof(Hooks).GetMethod("FoyerCallbacks2")

               );
                Debug.Log("charapi hooks: 1");
                Hook languageManagerHook = new Hook(
                    typeof(dfControl).GetMethod("getLocalizedValue", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(Hooks).GetMethod("DFGetLocalizedValue")
                );

                var braveSETypes = new Type[]
                {
                    typeof(string),
                    typeof(string),
                };
                Hook braveLoad = new Hook(
                    typeof(BraveResources).GetMethod("Load", BindingFlags.Public | BindingFlags.Static, null, braveSETypes, null),
                    typeof(Hooks).GetMethod("BraveLoadObject")
                );

                Hook playerSwitchHook = new Hook(
                    typeof(Foyer).GetMethod("PlayerCharacterChanged", BindingFlags.Public | BindingFlags.Instance),
                    typeof(Hooks).GetMethod("OnPlayerChanged")
                );
                Debug.Log("charapi hooks: 2");

                Hook clearP2Hook = new Hook(
                    typeof(GameManager).GetMethod("ClearSecondaryPlayer", BindingFlags.Public | BindingFlags.Instance),
                    typeof(Hooks).GetMethod("OnP2Cleared")
                );
            }
            catch (Exception e)
            {
                ToolsCharApi.PrintException(e);
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ResetToFactorySettings))]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.TriggerDarkSoulsReset))]
        [HarmonyILManipulator]
        private static void EnsureZeroHealthCharactersAreTreatedLikeRobotIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
              instr => instr.MatchLdarg(0),
              instr => instr.MatchLdfld<PlayerController>("characterIdentity")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.CallPrivate(typeof(Hooks), nameof(TreatZeroHealthCharacterAsRobot));
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.HandleCloneEffect), MethodType.Enumerator)]
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.CoopResurrectInternal), MethodType.Enumerator)]
        [HarmonyILManipulator]
        private static void EnsureZeroHealthCharactersAreTreatedLikeRobotEnumeratorIL(ILContext il, MethodBase original)
        {
            FieldInfo thisField = original.DeclaringType.GetEnumeratorField("$this");
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
              instr => instr.MatchLdarg(0),
              instr => instr.MatchLdfld(original.DeclaringType.FullName, thisField.Name),
              instr => instr.MatchLdfld<PlayerController>("characterIdentity")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, thisField);
                cursor.CallPrivate(typeof(Hooks), nameof(TreatZeroHealthCharacterAsRobot));
            }
        }

        private static int TreatZeroHealthCharacterAsRobot(int actualId, PlayerController player)
        {
            if (player.ForceZeroHealthState)
                return (int)PlayableCharacters.Robot; // matching against a BNE opcode expecting Robot
            return actualId;
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.SwapToAlternateCostume))]
        [HarmonyPrefix]
        private static void PlayerControllerSwapToAlternateCostumePatch(PlayerController __instance, tk2dSpriteAnimation overrideTargetLibrary)
        {
            if (!__instance.IsCustomCharacter() || __instance.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return;
            if (cc.data == null)
                cc.GetData();
            if (!__instance.IsUsingAlternateCostume && cc.data.altGlowMaterial is Material altGlowMaterial)
            {
                Texture mainTex = __instance.AlternateCostumeLibrary?.clips[0]?.frames[0]?.spriteCollection?.spriteDefinitions[0]?.material.GetTexture("_MainTex");
                if (mainTex != null && altGlowMaterial.GetTexture("_MainTex") != mainTex)
                    altGlowMaterial.SetTexture("_MainTex", mainTex);
                __instance.sprite.renderer.material = altGlowMaterial;
            }
            else if (cc.data.glowMaterial is Material glowMaterial)
            {
                if (glowMaterial.GetTexture("_MainTex") != __instance.sprite.renderer.material.GetTexture("_MainTex"))
                    glowMaterial.SetTexture("_MainTex", __instance.sprite.renderer.material.GetTexture("_MainTex")); //_MainTexture
                __instance.sprite.renderer.material = glowMaterial;
            }
        }

        [HarmonyPatch(typeof(Foyer), nameof(Foyer.ProcessPlayerEnteredFoyer))]
        [HarmonyPostfix]
        private static void FoyerProcessPlayerEnteredFoyerPatch(Foyer __instance, PlayerController p)
        {
            if (Dungeon.ShouldAttemptToLoadFromMidgameSave && GameManager.Instance.IsLoadingLevel)
                return;
            if (!p || p.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return;
            if (cc.data == null && !cc.GetData())
            {
                ETGModConsole.Log($"[Charapi]: custom character data NULLED as it DOES NOT EXIST");
                return;
            }
            p.ForceStaticFaceDirection(Vector2.up);
            if (p.IsUsingAlternateCostume && cc.data.altGlowMaterial is Material altGlowMaterial)
            {
                if (altGlowMaterial.GetTexture("_MainTex") != p.sprite.renderer.material.GetTexture("_MainTex"))
                    altGlowMaterial.SetTexture("_MainTex", p.sprite.renderer.material.GetTexture("_MainTex"));
                p.SetOverrideMaterial(altGlowMaterial);
            }
            else if (cc.data.glowMaterial is Material glowMaterial)
            {
                if (glowMaterial.GetTexture("_MainTex") != p.sprite.renderer.material.GetTexture("_MainTex"))
                    glowMaterial.SetTexture("_MainTex", p.sprite.renderer.material.GetTexture("_MainTex"));
                p.SetOverrideMaterial(glowMaterial);
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.LocalShaderName), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool PlayerControllerLocalShaderNamePatch(PlayerController __instance, ref string __result)
        {
            if (!GameOptions.SupportsStencil)
                return true;
            if (!__instance.IsCustomCharacter() || __instance.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return true;
            if (cc.data == null && !cc.GetData())
            {
                ETGModConsole.Log($"[Charapi]: custom character data NULLED as it DOES NOT EXIST");
                return true;
            }
            if (__instance.IsUsingAlternateCostume && cc.data.altGlowMaterial is Material altGlowMaterial)
            {
                if (altGlowMaterial.GetTexture("_MainTex") != __instance.sprite.renderer.material.GetTexture("_MainTex"))
                    altGlowMaterial.SetTexture("_MainTex", __instance.sprite.renderer.material.GetTexture("_MainTex")); //_MainTexture
                __instance.sprite.renderer.material = altGlowMaterial;
                __result = altGlowMaterial.shader.name;
                return false;
            }
            if (cc.data.glowMaterial is Material glowMaterial)
            {
                if (glowMaterial.GetTexture("_MainTex") != __instance.sprite.renderer.material.GetTexture("_MainTex"))
                    glowMaterial.SetTexture("_MainTex", __instance.sprite.renderer.material.GetTexture("_MainTex"));
                __instance.sprite.renderer.material = glowMaterial;
                __result = glowMaterial.shader.name;
                return false;
            }
            return true;
        }

        [HarmonyPatch]
        private static class CustomPastHandlerPatches
        {
            [HarmonyPatch(typeof(ArkController), nameof(ArkController.HandleClockhair), MethodType.Enumerator)]
            [HarmonyILManipulator]
            private static void ArkControllerHandleClockhairPatchIL(ILContext il, MethodBase original)
            {
                ILCursor cursor = new ILCursor(il);
                ILLabel toggleUICameraLabel = null; // executed if past is valid
                if (!cursor.TryGotoNext(MoveType.After,
                  instr => instr.MatchLdloc(8), // flag
                  instr => instr.MatchBrtrue(out toggleUICameraLabel)
                  ))
                  return;
                cursor.Emit(OpCodes.Ldarg_0); // load enumerator type
                cursor.Emit(OpCodes.Ldfld, original.DeclaringType.GetEnumeratorField("$this")); // load actual "$this" field
                cursor.Emit(OpCodes.Ldarg_0); // load enumerator type
                cursor.Emit(OpCodes.Ldfld, original.DeclaringType.GetEnumeratorField("shotPlayer")); // load shotPlayer field
                cursor.CallPrivate(typeof(CustomPastHandlerPatches), nameof(DoCustomPastChecks));
                cursor.Emit(OpCodes.Brtrue, toggleUICameraLabel);
            }

            private static bool DoCustomPastChecks(ArkController ark, PlayerController shotPlayer)
            {
              if (shotPlayer.GetComponent<CustomCharacter>() is not CustomCharacter cc || string.IsNullOrEmpty(cc.past))
                return false;
              ark.ResetPlayers(false);
              GameManager.Instance.LoadCustomLevel(cc.past);
              return true;
            }
        }

        [HarmonyPatch(typeof(PunchoutPlayerController), nameof(PunchoutPlayerController.UpdateUI))]
        [HarmonyPrefix]
        private static bool PunchoutPlayerControllerUpdateUIPatch(PunchoutPlayerController __instance)
        {
            if (__instance.m_playerId <= 7)
                return true;

            string str = backUpUI[__instance.m_playerId];
            __instance.HealthBarUI.SpriteName = "punch_health_bar_001";
            if (__instance.Health > 66f)
                __instance.PlayerUiSprite.SpriteName = str + "1";
            else if (__instance.Health > 33f)
                __instance.PlayerUiSprite.SpriteName = str + "2";
            else
                __instance.PlayerUiSprite.SpriteName = str + "3";

            if (__instance.IsEevee && __instance.PlayerUiSprite.OverrideMaterial == null)
            {
                Material material = UnityEngine.Object.Instantiate<Material>(__instance.PlayerUiSprite.Atlas.Material);
                material.shader = Shader.Find("Brave/Internal/GlitchEevee");
                material.SetTexture("_EeveeTex", __instance.CosmicTex);
                material.SetFloat("_WaveIntensity", 0.1f);
                material.SetFloat("_ColorIntensity", 0.015f);
                __instance.PlayerUiSprite.OverrideMaterial = material;
            }
            else if (!__instance.IsEevee && __instance.PlayerUiSprite.OverrideMaterial != null)
                __instance.PlayerUiSprite.OverrideMaterial = null;

            return false;
        }

        public static string[] backUp = PunchoutPlayerController.PlayerNames;
        public static string[] backUpUI = PunchoutPlayerController.PlayerUiNames;

        [HarmonyPatch(typeof(PunchoutController), nameof(PunchoutController.Init))]
        [HarmonyPrefix]
        private static bool PunchoutControllerInitPatch(PunchoutController __instance)
        {
            PlayerController player = GameManager.Instance.PrimaryPlayer;
            if (!player.IsCustomCharacter() || player.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return true;

            var name = cc.data.nameShort.ToLower();
            if (!backUp.Contains("eevee"))
            {
                Array.Resize(ref PunchoutPlayerController.PlayerNames, PunchoutPlayerController.PlayerNames.Length + 1);
                PunchoutPlayerController.PlayerNames[PunchoutPlayerController.PlayerNames.Length - 1] = "eevee";
                Array.Resize(ref PunchoutPlayerController.PlayerUiNames, PunchoutPlayerController.PlayerUiNames.Length + 1);
                PunchoutPlayerController.PlayerUiNames[PunchoutPlayerController.PlayerUiNames.Length - 1] = "punch_player_health_eevee_00";
            }

            if (!backUp.Contains(name))
            {
                Array.Resize(ref PunchoutPlayerController.PlayerNames, PunchoutPlayerController.PlayerNames.Length + 1);
                PunchoutPlayerController.PlayerNames[PunchoutPlayerController.PlayerNames.Length - 1] = name;
                Array.Resize(ref PunchoutPlayerController.PlayerUiNames, PunchoutPlayerController.PlayerUiNames.Length + 1);
                PunchoutPlayerController.PlayerUiNames[PunchoutPlayerController.PlayerUiNames.Length - 1] = $"punch_player_health_{name}_00";

                CustomCharacter.punchoutBullShit.Add(name, PunchoutPlayerController.PlayerUiNames.Length - 1);
                backUp = PunchoutPlayerController.PlayerNames;
                backUpUI = PunchoutPlayerController.PlayerUiNames;
            }

            __instance.Player.CustomSwapPlayer(CustomCharacter.punchoutBullShit[name], false);
            __instance.CoopCultist.gameObject.SetActive(GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER);
            __instance.StartCoroutine(__instance.UiFadeInCR());
            __instance.m_isInitialized = true;
            __instance.Player.sprite.usesOverrideMaterial = true;

            if (cc.data.useGlow && cc.data.glowMaterial != null)
            {
                if (cc.data.glowMaterial.GetTexture("_MainTex") != __instance.Player.sprite.renderer.material.GetTexture("_MainTex"))
                    cc.data.glowMaterial.SetTexture("_MainTexture", __instance.Player.sprite.renderer.material.GetTexture("_MainTex"));
                __instance.Player.sprite.renderer.material = cc.data.glowMaterial;
            }
            else
            {
                Material mat = new Material(SpriteHandler.Default_Punchout_Material);
                mat.mainTexture = __instance.Player.sprite.renderer.material.mainTexture;
                mat.SetTexture("_MainTexture", __instance.Player.sprite.renderer.material.GetTexture("_MainTex"));
                __instance.Player.sprite.renderer.material = mat;
            }
            return false;
        }

        private static void CustomSwapPlayer(this PunchoutPlayerController self, int? newPlayerIndex = null, bool keepEevee = false)
        {
            if (newPlayerIndex == null)
            {
                if (self.IsEevee && !keepEevee)
                {
                    newPlayerIndex = new int?(0);
                }
                else
                {
                    newPlayerIndex = new int?((self.m_playerId) % (PunchoutPlayerController.PlayerNames.Length));
                }
            }

            if (!keepEevee)
            {
                bool flag = newPlayerIndex.Value == 7;
                if (flag && !self.IsEevee)
                {
                    self.IsEevee = true;
                    self.sprite.usesOverrideMaterial = true;
                    self.sprite.renderer.material.shader = Shader.Find("Brave/PlayerShaderEevee");
                    self.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", self.CosmicTex);
                    self.sprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
                    self.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
                }
                else if (!flag && self.IsEevee)
                {
                    self.IsEevee = false;
                    self.sprite.usesOverrideMaterial = false;
                }
            }

            if (self.IsEevee)
            {
                newPlayerIndex = new int?(UnityEngine.Random.Range(0, PunchoutPlayerController.PlayerNames.Length));
            }


            string oldName = backUp[self.m_playerId];

            string newName = backUp[newPlayerIndex.Value];


            self.m_playerId = newPlayerIndex.Value;

            self.SwapAnim(self.aiAnimator.IdleAnimation, oldName, newName);

            self.SwapAnim(self.aiAnimator.HitAnimation, oldName, newName);

            for (int i = 0; i < self.aiAnimator.OtherAnimations.Count; i++)
            {
                self.SwapAnim(self.aiAnimator.OtherAnimations[i].anim, oldName, newName);

            }

            self.UpdateUI();
            List<AIAnimator.NamedDirectionalAnimation> otherAnimations = self.aiAnimator.ChildAnimator.OtherAnimations;
            otherAnimations[0].anim.Type = DirectionalAnimation.DirectionType.None;
            otherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.None;
            otherAnimations[2].anim.Type = DirectionalAnimation.DirectionType.None;
        }

        [HarmonyPatch(typeof(AmmonomiconDeathPageController), nameof(AmmonomiconDeathPageController.GetDeathPortraitName))]
        [HarmonyPostfix]
        private static void AmmonomiconDeathPageControllerGetDeathPortraitNamePatch(AmmonomiconDeathPageController __instance, ref string __result)
        {
            if (GameManager.Instance.PrimaryPlayer.IsCustomCharacter() && GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<CustomCharacter>() is CustomCharacter cc)
                __result = $"coop_page_death_{cc.data.nameShort.ToLower()}_001";
        }

        private static bool IsCustomCharacter(this PlayerController player)
        {
            return player.characterIdentity > PlayableCharacters.Gunslinger;
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ChangeSpecialShaderFlag))]
        [HarmonyPrefix]
        private static void PlayerControllerChangeSpecialShaderFlagPatch(PlayerController __instance, int flagIndex, float val)
        {
            if (!__instance.IsCustomCharacter())
                return;
            if (__instance.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return;

            Texture spriteTexture = __instance.sprite.renderer.material.GetTexture("_MainTex");
            if (val == 0)
            {
                if (cc.data.useGlow)
                {
                    if (__instance.IsUsingAlternateCostume && cc.data.altGlowMaterial is Material altGlowMaterial)
                    {
                        if (altGlowMaterial.GetTexture("_MainTex") != spriteTexture)
                            altGlowMaterial.SetTexture("_MainTexture", spriteTexture);
                        __instance.sprite.renderer.material = altGlowMaterial;
                    }
                    else if (cc.data.glowMaterial is Material glowMaterial)
                    {
                        if (glowMaterial.GetTexture("_MainTex") != spriteTexture)
                            glowMaterial.SetTexture("_MainTexture", spriteTexture);
                        __instance.sprite.renderer.material = glowMaterial;
                    }
                    else
                        ETGModConsole.Log($"[Charapi]: glow material NULLED");
                }
            }
            else
            {
                if (cc.data.normalMaterial.GetTexture("_MainTex") != spriteTexture)
                    cc.data.normalMaterial.SetTexture("_MainTexture", spriteTexture);
                __instance.sprite.renderer.material = cc.data.normalMaterial;
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.GetBaseAnimationName))]
        [HarmonyPostfix]
        private static void PlayerControllerGetBaseAnimationNamePatch(PlayerController __instance, ref string __result)
        {
            if (__instance.gameObject.GetComponent<CustomCharacter>() is CustomCharacter cc && !string.IsNullOrEmpty(cc.overrideAnimation))
                __result = cc.overrideAnimation;
        }

        //NOTE: i suspect this code doesn't work quite correctly, but i've ported it to harmony as-is
        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.DoGhostBlank))]
        [HarmonyPrefix]
        private static bool PlayerControllerDoGhostBlankPatch(PlayerController __instance)
        {
            if (CharacterBuilder.storedCharacters.Count() > 0)
                return true;
            if (__instance.gameObject.GetComponent<CustomCharacter>() is not CustomCharacter cc)
                return true;
            var coopBlankReplacement = CharacterBuilder.storedCharacters[cc.data.nameInternal.ToLower()].First.coopBlankReplacement;
            if (coopBlankReplacement != null)
                __instance.m_blankCooldownTimer = coopBlankReplacement(__instance);
            return false;
        }

        [HarmonyPatch(typeof(CharacterSelectIdleDoer), nameof(CharacterSelectIdleDoer.Update))]
        [HarmonyPrefix]
        private static void CharacterSelectIdleDoerUpdatePatch(CharacterSelectIdleDoer __instance)
        {
            if (__instance.GetComponent<CustomCharacterFoyerController>() is CustomCharacterFoyerController ccfc && ccfc.useGlow && __instance.sprite.renderer.material != ccfc.data.glowMaterial)
            {
                __instance.sprite.usesOverrideMaterial = true;
                __instance.sprite.renderer.material = ccfc.data.glowMaterial;
            }
        }

        [HarmonyPatch(typeof(FoyerCharacterSelectFlag), nameof(FoyerCharacterSelectFlag.OnSelectedCharacterCallback))]
        [HarmonyPostfix]
        private static void FoyerCharacterSelectFlagOnSelectedCharacterCallbackPatch(FoyerCharacterSelectFlag __instance, PlayerController newCharacter)
        {
            if (!newCharacter.gameObject.name.ToLower().Contains(__instance.CharacterPrefabPath.ToLower(), false))
                return;
            if (__instance.GetComponent<CustomCharacterFoyerController>() is CustomCharacterFoyerController ccfc && ccfc.metaCost > 0)
                GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, -ccfc.metaCost);
            __instance.gameObject.SetActive(false);
            __instance.GetComponent<SpeculativeRigidbody>().enabled = false;
        }

        [HarmonyPatch(typeof(CharacterSelectIdleDoer), nameof(CharacterSelectIdleDoer.OnEnable))]
        [HarmonyPrefix]
        private static void CharacterSelectIdleDoerOnEnablePatch(CharacterSelectIdleDoer __instance)
        {
            if (__instance.GetComponent<CustomCharacterFoyerController>() is CustomCharacterFoyerController ccfc && ccfc.useGlow && __instance.sprite.renderer.material != ccfc.data.glowMaterial)
            {
                __instance.sprite.usesOverrideMaterial = true;
                __instance.sprite.renderer.material = ccfc.data.glowMaterial;
            }
        }

        [HarmonyPatch(typeof(FoyerCharacterSelectFlag), nameof(FoyerCharacterSelectFlag.CanBeSelected))]
        [HarmonyPrefix]
        private static bool FoyerCharacterSelectFlagCanBeSelectedPatch(FoyerCharacterSelectFlag __instance, ref bool __result)
        {
            if (__instance.GetComponent<CustomCharacterFoyerController>() is not CustomCharacterFoyerController ccfc || ccfc.metaCost <= 0)
                return true;
            __result = (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY) >= __instance.GetComponent<CustomCharacterFoyerController>().metaCost);
            return false;
        }

        [HarmonyPatch(typeof(dfLanguageManager), nameof(dfLanguageManager.GetValue))]
        [HarmonyPrefix]
        private static bool dfLanguageManagerGetValuePatch(dfLanguageManager __instance, string key, ref string __result)
        {
            if (!characterDeathNames.Contains(key))
                return true;
            PlayerController player = GameManager.Instance.PrimaryPlayer;
            if (player && player.GetComponent<CustomCharacter>() is CustomCharacter cc && cc.data != null)
            {
                __result = cc.name;
                return false;
            }
            return true;
        }

        //Triggers FoyerCharacterHandler (called from Foyer.SetUpCharacterCallbacks)
        public static List<FoyerCharacterSelectFlag> FoyerCallbacks2(Func<Foyer, List<FoyerCharacterSelectFlag>> orig, Foyer self)
        {
            var sortedByX = orig(self);

            var sortedByXCustom = FoyerCharacterHandler.AddCustomCharactersToFoyer(sortedByX);

            foreach (var character in sortedByXCustom)
            {
                sortedByX.Add(character);
            }

            return sortedByX;
        }

        //Used to add in strings 
        public static string DFGetLocalizedValue(Func<dfControl, string, string> orig, dfControl self, string key)
        {
            if (StringHandler.customStringDictionary.TryGetValue(key, out string val))
                return val;
            return orig(self, key);
        }

        //Used to set fake player prefabs to active on instantiation (hook doesn't work on this call)
        public static Object BraveLoadObject(Func<string, string, Object> orig, string path, string extension = ".prefab")
        {
            var value = orig(path, extension);
            if (value == null)
            {
                path = path.ToLower();
                if (CharacterBuilder.storedCharacters.ContainsKey(path))
                {
                    var character = CharacterBuilder.storedCharacters[path].Second;
                    return character;
                }
            }
            return value;
        }

        public static void OnPlayerChanged(Action<Foyer, PlayerController> orig, Foyer self, PlayerController player)
        {
            ResetInfiniteGuns();
            orig(self, player);
        }

        public static void OnP2Cleared(Action<GameManager> orig, GameManager self)
        {
            orig(self);
            ResetInfiniteGuns();
        }

        //Resets all the character-specific infinite guns 
        public static Dictionary<int, GunBackupData> gunBackups = new Dictionary<int, GunBackupData>();
        public static void ResetInfiniteGuns()
        {
            var player1 = GameManager.Instance?.PrimaryPlayer?.GetComponent<CustomCharacter>();
            var player2 = GameManager.Instance?.SecondaryPlayer?.GetComponent<CustomCharacter>();
            List<int> removables = new List<int>();
            foreach (var entry in gunBackups)
            {
                if ((player1 && player1.GetInfiniteGunIDs().Contains(entry.Key)) || (player2 && player2.GetInfiniteGunIDs().Contains(entry.Key))) continue;
                var gun = PickupObjectDatabase.GetById(entry.Key) as Gun;
                gun.InfiniteAmmo = entry.Value.InfiniteAmmo;
                gun.CanBeDropped = entry.Value.CanBeDropped;
                gun.PersistsOnDeath = entry.Value.PersistsOnDeath;
                gun.PreventStartingOwnerFromDropping = entry.Value.PreventStartingOwnerFromDropping;
                removables.Add(entry.Key);
                ToolsCharApi.Print($"Reset {gun.EncounterNameOrDisplayName} to infinite = {gun.InfiniteAmmo}");
            }
            foreach (var id in removables)
                gunBackups.Remove(id);

        }
        public static Hook getOrLoadByName_Hook;
        public static Hook setWinPicHook;

        [HarmonyPatch(typeof(AmmonomiconDeathPageController), nameof(AmmonomiconDeathPageController.SetWinPic))]
        [HarmonyPostfix]
        private static void AmmonomiconDeathPageControllerSetWinPicPatch(AmmonomiconDeathPageController __instance)
        {
            GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
            PlayerController player = GameManager.Instance.PrimaryPlayer;
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || player.GetComponent<CustomCharacter>() is not CustomCharacter cc || cc.data == null)
                return;
            if (tilesetId != GlobalDungeonData.ValidTilesets.FINALGEON && __instance.ShouldUseJunkPic())
            {
                if (cc.data.junkanWinPic == null)
                    __instance.photoSprite.Texture = (BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture);
                else
                    __instance.photoSprite.Texture = cc.data.junkanWinPic;

            }
            else if (tilesetId == GlobalDungeonData.ValidTilesets.FINALGEON && cc.data.pastWinPic)
                __instance.photoSprite.Texture = cc.data.pastWinPic;
        }

        public static List<string> characterDeathNames = new List<string>
        {
            "#CHAR_ROGUE_SHORT",
            "#CHAR_CONVICT_SHORT",
            "#CHAR_ROBOT_SHORT",
            "#CHAR_MARINE_SHORT",
            "#CHAR_GUIDE_SHORT",
            "#CHAR_CULTIST_SHORT",
            "#CHAR_BULLET_SHORT",
            "#CHAR_PARADOX_SHORT",
            "#CHAR_GUNSLINGER_SHORT"
        };
        //static bool ab = false;
        public struct GunBackupData
        {
            public bool InfiniteAmmo,
                CanBeDropped,
                PersistsOnDeath,
                PreventStartingOwnerFromDropping;
        }
    }
}

/// <summary>Allows CharAPI characters to be saved / loaded properly by the elevator button.</summary>
[HarmonyPatch]
internal static class MidGameSaveDataGetPlayerOnePrefabPatcher
{
  [HarmonyPatch(typeof(MidGameSaveData), nameof(MidGameSaveData.GetPlayerOnePrefab))]
  [HarmonyPrefix]
  private static bool MidGameSaveDataGetPlayerOnePrefabPatch(MidGameSaveData __instance, ref GameObject __result)
  {
    PlayableCharacters id = __instance.playerOneData.CharacterIdentity;
    if (id <= PlayableCharacters.Gunslinger)
      return true;
    foreach (var tup in Alexandria.CharacterAPI.CharacterBuilder.storedCharacters.Values)
    {
      if (tup.Second.GetComponent<PlayerController>().characterIdentity == id)
      {
        __result = tup.Second;
        return false;
      }
    }
    return true;
  }
}
