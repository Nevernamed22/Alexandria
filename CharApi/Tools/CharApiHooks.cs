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
    public static class Hooks
    {
        public static void Init()
        {
            try
            {
                Hook getValueHook = new Hook(
                    typeof(dfLanguageManager).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance),
                    typeof(Hooks).GetMethod("GetValueHook")
                );

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

                Hook setWinPicHook = new Hook(
                    typeof(AmmonomiconDeathPageController).GetMethod("SetWinPic", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("SetWinPicHook", BindingFlags.Static | BindingFlags.NonPublic)
                );

                Hook updateHook = new Hook(
                    typeof(CharacterSelectIdleDoer).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("UpdateHook", BindingFlags.Static | BindingFlags.NonPublic)
                );

                Hook onEnableHook = new Hook(
                    typeof(CharacterSelectIdleDoer).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("OnEnableHook", BindingFlags.Static | BindingFlags.NonPublic)
                );

                Hook canBeSelectedHook = new Hook(
                    typeof(FoyerCharacterSelectFlag).GetMethod("CanBeSelected", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("CanBeSelectedHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook onSelectedCharacterCallbackHook = new Hook(
                    typeof(FoyerCharacterSelectFlag).GetMethod("OnSelectedCharacterCallback", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("OnSelectedCharacterCallbackHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook DoGhostBlankHook = new Hook(
                    typeof(PlayerController).GetMethod("DoGhostBlank", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("DoGhostBlankHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook GetBaseAnimationNameHook = new Hook(
                    typeof(PlayerController).GetMethod("GetBaseAnimationName", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("GetBaseAnimationNameHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook ChangeSpecialShaderFlagHook = new Hook(
                    typeof(PlayerController).GetMethod("ChangeSpecialShaderFlag", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("ChangeSpecialShaderFlagHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook GetDeathPortraitNameHook = new Hook(
                    typeof(AmmonomiconDeathPageController).GetMethod("GetDeathPortraitName", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("GetDeathPortraitNameHook", BindingFlags.Static | BindingFlags.NonPublic)
                );

                Hook InitHook = new Hook(
                    typeof(PunchoutController).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("InitHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook ProcessPlayerEnteredFoyerHook = new Hook(
                    typeof(Foyer).GetMethod("ProcessPlayerEnteredFoyer", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("ProcessPlayerEnteredFoyerHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook SwapToAlternateCostumeHook = new Hook(
                    typeof(PlayerController).GetMethod("SwapToAlternateCostume", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("SwapToAlternateCostumeHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook hook3 = new Hook(typeof(PlayerController).GetProperty("LocalShaderName", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Hooks).GetMethod("LocalShaderNameGetHook"));

                Hook punchoutUIHook = new Hook(
                    typeof(PunchoutPlayerController).GetMethod("UpdateUI", BindingFlags.Public | BindingFlags.Instance),
                    typeof(Hooks).GetMethod("PunchoutUpdateUI")
                );

                //Resurrection Based Hooks
                Hook CloneHook = new Hook(
                    typeof(PlayerController).GetMethod("HandleCloneEffect", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("HandleCloneEffectHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook DarkSoulsResetHook = new Hook(
                    typeof(PlayerController).GetMethod("TriggerDarkSoulsReset", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("TriggerDarkSoulsResetHook", BindingFlags.Static | BindingFlags.Public)
                );

                Hook ResetFactorySettingsHook = new Hook(
                    typeof(PlayerController).GetMethod("ResetToFactorySettings", BindingFlags.Instance | BindingFlags.Public),
                    typeof(Hooks).GetMethod("ResetToFactorySettingsHook", BindingFlags.Static | BindingFlags.Public)
                );
                Hook CoopResurrectInternalHook = new Hook(
                    typeof(PlayerController).GetMethod("CoopResurrectInternal", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Hooks).GetMethod("CoopResurrectInternalHook", BindingFlags.Static | BindingFlags.Public)
                );
                //======
            }
            catch (Exception e)
            {
                ToolsCharApi.PrintException(e);
            }
        }

        public static IEnumerator CoopResurrectInternalHook(Func<PlayerController, Vector3, tk2dSpriteAnimationClip, bool, IEnumerator> orig, PlayerController self, Vector3 targetPosition, tk2dSpriteAnimationClip clipToWaitFor, bool isChest = false)
        {
            GameManager.Instance.MainCameraController.IsLerping = true;
            self.m_cloneWaitingForCoopDeath = false;
            self.ForceBlank(5f, 0.5f, true, false, new Vector2?(targetPosition.XY()), false, -1f);
            if (!isChest)
            {
                self.IsCurrentlyCoopReviving = true;
                self.SetInputOverride("revivepause");
                self.PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_GhostRevive"), Vector3.zero, true, true, false);
                float elapsed = 0f;
                while (elapsed < 0.75f)
                {
                    elapsed += BraveTime.DeltaTime;
                    yield return null;
                }
                self.ClearInputOverride("revivepause");
                self.IsCurrentlyCoopReviving = false;
                GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 7.5f;
                GameManager.Instance.MainCameraController.IsLerping = true;
            }
            self.ChangeSpecialShaderFlag(0, 0f);
            self.IsGhost = false;
            self.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
            self.m_blankCooldownTimer = 0f;
            GameUIRoot.Instance.TransitionToGhostUI(self);
            self.CurrentInputState = PlayerInputState.NoInput;
            self.m_cachedAimDirection = -Vector2.up;
            GameUIRoot.Instance.ReenableCoopPlayerUI(self);
            self.stats.RecalculateStats(self, false, false);
            self.transform.position = targetPosition;
            self.specRigidbody.CollideWithTileMap = true;
            self.specRigidbody.CollideWithOthers = true;
            self.specRigidbody.Reinitialize();
            self.healthHaver.FullHeal();
            if (self.ForceZeroHealthState == true)
            {
                self.healthHaver.Armor = 6f;
            }
            self.DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
            self.m_handlingQueuedAnimation = true;
            if (clipToWaitFor != null)
            {
                self.spriteAnimator.Play(clipToWaitFor);
                while (self.spriteAnimator.IsPlaying(clipToWaitFor))
                {
                    yield return null;
                }
            }
            self.m_handlingQueuedAnimation = false;
            self.IsVisible = true;
            if (!SpriteOutlineManager.HasOutline(self.sprite))
            {
                SpriteOutlineManager.AddOutlineToSprite(self.sprite, self.outlineColor, 0.1f, 0f, (self.characterIdentity != PlayableCharacters.Eevee) ? SpriteOutlineManager.OutlineType.NORMAL : SpriteOutlineManager.OutlineType.EEVEE);
            }
            self.m_hideRenderers.ClearOverrides();
            self.m_hideGunRenderers.ClearOverrides();
            self.m_hideHandRenderers.ClearOverrides();
            self.ToggleShadowVisiblity(true);
            self.ToggleRenderer(true, string.Empty);
            self.ToggleRenderer(true, "isVisible");
            self.ToggleGunRenderers(true, string.Empty);
            self.ToggleGunRenderers(true, "isVisible");
            self.ToggleHandRenderers(true, string.Empty);
            self.ToggleHandRenderers(true, "isVisible");
            List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(self.specRigidbody, null, false);
            for (int i = 0; i < overlappingRigidbodies.Count; i++)
            {
                self.specRigidbody.RegisterGhostCollisionException(overlappingRigidbodies[i]);
                overlappingRigidbodies[i].RegisterGhostCollisionException(self.specRigidbody);
            }
            self.m_isFalling = false;
            self.ClearDodgeRollState();
            self.previousMineCart = null;
            self.m_interruptingPitRespawn = false;
            GameManager.Instance.GetOtherPlayer(self).stats.RecalculateStats(GameManager.Instance.GetOtherPlayer(self), false, false);
            self.CurrentInputState = PlayerInputState.AllInput;
            self.healthHaver.IsVulnerable = true;
            yield break;
        }
        public static void ResetToFactorySettingsHook(Action<PlayerController, bool, bool, bool> orig, PlayerController self, bool includeFullHeal = false, bool useFinalFightGuns = false, bool forceAllItems = false)
        {
            if (!self.IsDarkSoulsHollow || useFinalFightGuns)
            {
                self.inventory.DestroyAllGuns();
            }
            if (useFinalFightGuns && self.finalFightGunIds != null && self.finalFightGunIds.Count > 0)
            {
                for (int i = 0; i < self.finalFightGunIds.Count; i++)
                {
                    if (self.finalFightGunIds[i] >= 0)
                    {
                        self.inventory.AddGunToInventory(PickupObjectDatabase.GetById(self.finalFightGunIds[i]) as Gun, true);
                    }
                }
            }
            else if (self.UsingAlternateStartingGuns)
            {
                for (int j = 0; j < self.startingAlternateGunIds.Count; j++)
                {
                    Gun gun = PickupObjectDatabase.GetById(self.startingAlternateGunIds[j]) as Gun;
                    if (forceAllItems || includeFullHeal || useFinalFightGuns || gun.PreventStartingOwnerFromDropping)
                    {
                        Gun gun2 = self.inventory.AddGunToInventory(gun, true);
                    }
                }
            }
            else
            {
                for (int k = 0; k < self.startingGunIds.Count; k++)
                {
                    Gun gun3 = PickupObjectDatabase.GetById(self.startingGunIds[k]) as Gun;
                    if (forceAllItems || includeFullHeal || useFinalFightGuns || gun3.PreventStartingOwnerFromDropping)
                    {
                        Gun gun4 = self.inventory.AddGunToInventory(gun3, true);
                    }
                }
            }
            for (int l = 0; l < self.passiveItems.Count; l++)
            {
                if (!self.passiveItems[l].PersistsOnDeath)
                {
                    DebrisObject debrisObject = self.DropPassiveItem(self.passiveItems[l]);
                    if (debrisObject != null)
                    {
                        UnityEngine.Object.Destroy(debrisObject.gameObject);
                        l--;
                    }
                }
            }
            for (int m = 0; m < self.activeItems.Count; m++)
            {
                if (!self.activeItems[m].PersistsOnDeath)
                {
                    DebrisObject debrisObject2 = self.DropActiveItem(self.activeItems[m], 4f, true);
                    if (debrisObject2 != null)
                    {
                        UnityEngine.Object.Destroy(debrisObject2.gameObject);
                        m--;
                    }
                }
            }
            for (int n = 0; n < self.startingActiveItemIds.Count; n++)
            {
                PlayerItem playerItem = PickupObjectDatabase.GetById(self.startingActiveItemIds[n]) as PlayerItem;
                if (forceAllItems || !playerItem.consumable)
                {
                    if (!self.HasActiveItem(playerItem.PickupObjectId))
                    {
                        if (forceAllItems || includeFullHeal || useFinalFightGuns || playerItem.PreventStartingOwnerFromDropping)
                        {
                            EncounterTrackable.SuppressNextNotification = true;
                            playerItem.Pickup(self);
                            EncounterTrackable.SuppressNextNotification = false;
                        }
                    }
                }
            }
            for (int num = 0; num < self.startingPassiveItemIds.Count; num++)
            {
                PassiveItem passiveItem = PickupObjectDatabase.GetById(self.startingPassiveItemIds[num]) as PassiveItem;
                if (!self.HasPassiveItem(passiveItem.PickupObjectId))
                {
                    EncounterTrackable.SuppressNextNotification = true;
                    LootEngine.GivePrefabToPlayer(passiveItem.gameObject, self);
                    EncounterTrackable.SuppressNextNotification = false;
                }
            }
            if (self.ownerlessStatModifiers != null)
            {
                if (useFinalFightGuns || includeFullHeal)
                {
                    self.ownerlessStatModifiers.Clear();
                }
                else
                {
                    for (int num2 = 0; num2 < self.ownerlessStatModifiers.Count; num2++)
                    {
                        if (!self.ownerlessStatModifiers[num2].PersistsOnCoopDeath)
                        {
                            self.ownerlessStatModifiers.RemoveAt(num2);
                            num2--;
                        }
                    }
                }
            }
            self.stats.RecalculateStats(self, false, false);
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
            {
                GameManager.Instance.GetOtherPlayer(self).stats.RecalculateStats(GameManager.Instance.GetOtherPlayer(self), false, false);
            }
            if (useFinalFightGuns && self.ForceZeroHealthState == true)
            {
                self.healthHaver.Armor = 6f;
            }
            if (includeFullHeal)
            {
                self.healthHaver.FullHeal();
            }
        }
        public static void TriggerDarkSoulsResetHook(Action<PlayerController, bool, int> orig, PlayerController self, bool dropItems = true, int cursedHealthMaximum = 1)
        {
            self.IsOnFire = false;
            self.CurrentFireMeterValue = 0f;
            self.CurrentPoisonMeterValue = 0f;
            self.CurrentCurseMeterValue = 0f;
            self.CurrentDrainMeterValue = 0f;
            AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.GetOtherPlayer(self).IsGhost)
            {
                self.DropPileOfSouls();
                self.HandleDarkSoulsHollowTransition(true);
                self.StartCoroutine(self.HandleCoopDeath(self.m_isFalling));
            }
            else
            {
                self.m_interruptingPitRespawn = true;
                self.healthHaver.FullHeal();
                if (self.ForceZeroHealthState == true)
                {
                    self.healthHaver.Armor = 2f;
                }
                self.specRigidbody.Velocity = Vector2.zero;
                self.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
                if (self.m_returnTeleporter != null)
                {
                    self.m_returnTeleporter.ClearReturnActive();
                    self.m_returnTeleporter = null;
                }
                GameManager.Instance.Dungeon.DarkSoulsReset(self, dropItems, cursedHealthMaximum);
            }
        }
        public static IEnumerator HandleCloneEffectHook(Func<PlayerController, IEnumerator> orig, PlayerController self)
        {
            Pixelator.Instance.FadeToBlack(0.5f, false, 0f);
            GameUIRoot.Instance.ToggleUICamera(false);
            self.healthHaver.FullHeal();
            self.IsOnFire = false;
            self.CurrentFireMeterValue = 0f;
            self.CurrentPoisonMeterValue = 0f;
            self.CurrentCurseMeterValue = 0f;
            self.CurrentDrainMeterValue = 0f;
            if (self.ForceZeroHealthState == true)
            {
                self.healthHaver.Armor = 6f;
            }
            float ela = 0f;
            while (ela < 0.5f)
            {
                ela += GameManager.INVARIANT_DELTA_TIME;
                yield return null;
            }
            int targetLevelIndex = 1;
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
            {
                targetLevelIndex += GameManager.Instance.LastShortcutFloorLoaded;
            }
            GameManager.Instance.SetNextLevelIndex(targetLevelIndex);
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
            {
                GameManager.Instance.DelayedLoadBossrushFloor(0.5f);
            }
            else
            {
                GameManager.Instance.DelayedLoadNextLevel(0.5f);
            }
            self.m_cloneWaitingForCoopDeath = false;
            ExtraLifeItem cloneItem = null;
            for (int i = 0; i < self.passiveItems.Count; i++)
            {
                if (self.passiveItems[i] is ExtraLifeItem)
                {
                    ExtraLifeItem extraLifeItem = self.passiveItems[i] as ExtraLifeItem;
                    if (extraLifeItem.extraLifeMode == ExtraLifeItem.ExtraLifeMode.CLONE)
                    {
                        cloneItem = extraLifeItem;
                    }
                }
            }
            if (cloneItem != null)
            {
                self.RemovePassiveItem(cloneItem.PickupObjectId);
            }
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
            {
                for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
                {
                    PlayerController playerController = GameManager.Instance.AllPlayers[j];
                    if (playerController.IsGhost)
                    {
                        playerController.StartCoroutine(playerController.CoopResurrectInternal(playerController.transform.position, null, true));
                    }
                    playerController.healthHaver.FullHeal();
                    playerController.specRigidbody.Velocity = Vector2.zero;
                    playerController.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
                    if (playerController.m_returnTeleporter != null)
                    {
                        playerController.m_returnTeleporter.ClearReturnActive();
                        playerController.m_returnTeleporter = null;
                    }
                }
                Chest.ToggleCoopChests(false);
            }
            else
            {
                self.healthHaver.FullHeal();
                self.specRigidbody.Velocity = Vector2.zero;
                self.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
                if (self.m_returnTeleporter != null)
                {
                    self.m_returnTeleporter.ClearReturnActive();
                    self.m_returnTeleporter = null;
                }
            }
            yield return new WaitForSeconds(1f);
            self.IsOnFire = false;
            self.CurrentFireMeterValue = 0f;
            self.CurrentPoisonMeterValue = 0f;
            self.CurrentCurseMeterValue = 0f;
            self.CurrentDrainMeterValue = 0f;
            self.healthHaver.FullHeal();
            if (self.ForceZeroHealthState == true)
            {
                self.healthHaver.Armor = 6f;
            }
            yield break;
        }

        public static void SwapToAlternateCostumeHook(Action<PlayerController, tk2dSpriteAnimation> orig, PlayerController self, tk2dSpriteAnimation overrideTargetLibrary = null)
        {

            if (self?.characterIdentity > (PlayableCharacters)10 && self.gameObject.GetComponent<CustomCharacter>() != null)
            {

                if (self.gameObject.GetComponent<CustomCharacter>().data == null) self.gameObject.GetComponent<CustomCharacter>().GetData();

                if (!self.IsUsingAlternateCostume && self.gameObject.GetComponent<CustomCharacter>()?.data?.altGlowMaterial != null)
                {
                    if (self.gameObject.GetComponent<CustomCharacter>()?.data?.altGlowMaterial?.GetTexture("_MainTex") != self.AlternateCostumeLibrary?.clips[0]?.frames[0]?.spriteCollection?.spriteDefinitions[0]?.material.GetTexture("_MainTex"))
                    {
                        self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial.SetTexture("_MainTex", self.AlternateCostumeLibrary.clips[0].frames[0].spriteCollection.spriteDefinitions[0].material.GetTexture("_MainTex"));
                    }
                    self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial;
                }
                else if (self.gameObject.GetComponent<CustomCharacter>()?.data?.glowMaterial != null)
                {
                    if (self.gameObject.GetComponent<CustomCharacter>().data?.glowMaterial?.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                    {
                        //_MainTexture
                        self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial.SetTexture("_MainTex", self.sprite.renderer.material.GetTexture("_MainTex"));
                    }
                    self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial;
                }
            }

            orig(self, overrideTargetLibrary);
        }

        public static void ProcessPlayerEnteredFoyerHook(Action<Foyer, PlayerController> orig, Foyer self, PlayerController p)
        {
            orig(self, p);
            if (Dungeon.ShouldAttemptToLoadFromMidgameSave && GameManager.Instance.IsLoadingLevel)
            {
                return;
            }
            var CCC = p.gameObject.GetComponent<CustomCharacter>();

            if (p && CCC != null)
            {
                if (CCC.data == null)
                {
                    //ETGModConsole.Log($"[Charapi]: custom character data nulled... thats really bad");
                    if (!p.gameObject.GetComponent<CustomCharacter>().GetData())
                    {
                        ETGModConsole.Log($"[Charapi]: custom character data NULLED as it DOES NOT EXIST");
                    }
                }

                p.ForceStaticFaceDirection(Vector2.up);
                if (p.IsUsingAlternateCostume && CCC.data.altGlowMaterial != null)
                {

                    if (CCC.data.altGlowMaterial.GetTexture("_MainTex") != p.sprite.renderer.material.GetTexture("_MainTex"))
                    {
                        CCC.data.altGlowMaterial.SetTexture("_MainTex", p.sprite.renderer.material.GetTexture("_MainTex"));
                    }
                    p.SetOverrideMaterial(CCC.data.altGlowMaterial);

                    //ETGModConsole.Log($"[Charapi]: set shader for alt skin");

                }
                else if (CCC.data.glowMaterial != null)
                {

                    if (CCC.data.glowMaterial.GetTexture("_MainTex") != p.sprite.renderer.material.GetTexture("_MainTex"))
                    {
                        //_MainTexture
                        CCC.data.glowMaterial.SetTexture("_MainTex", p.sprite.renderer.material.GetTexture("_MainTex"));
                    }
                    p.SetOverrideMaterial(CCC.data.glowMaterial);

                    //ETGModConsole.Log($"[Charapi]: set shader for main skin");
                }
            }
        }

        public static string LocalShaderNameGetHook(Func<PlayerController, string> orig, PlayerController self)
        {

            bool flag = !GameOptions.SupportsStencil;
            string result;
            if (flag)
            {
                result = "Brave/PlayerShaderNoStencil";
            }
            else
            {
                if (self.characterIdentity > (PlayableCharacters)10 && self.gameObject.GetComponent<CustomCharacter>() != null)
                {
                    if (self.gameObject.GetComponent<CustomCharacter>().data == null)
                    {
                        //ETGModConsole.Log($"[Charapi]: custom character data nulled... thats really bad");
                        if (!self.gameObject.GetComponent<CustomCharacter>().GetData())
                        {
                            ETGModConsole.Log($"[Charapi]: custom character data NULLED as it DOES NOT EXIST");
                        }
                    }

                    var CCC = self.gameObject.GetComponent<CustomCharacter>();
                    if (CCC != null)
                    {
                        if (self.IsUsingAlternateCostume && CCC.data.altGlowMaterial != null)
                        {
                            if (CCC.data.altGlowMaterial.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                            {
                                //_MainTexture
                                CCC.data.altGlowMaterial.SetTexture("_MainTex", self.sprite.renderer.material.GetTexture("_MainTex"));
                            }
                            self.sprite.renderer.material = CCC.data.altGlowMaterial;
                            result = CCC.data.altGlowMaterial.shader.name;
                        }
                        else if (CCC.data.glowMaterial != null)
                        {
                            if (CCC.data.glowMaterial.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                            {
                                CCC.data.glowMaterial.SetTexture("_MainTex", self.sprite.renderer.material.GetTexture("_MainTex"));
                            }
                            self.sprite.renderer.material = CCC.data.glowMaterial;
                            result = CCC.data.glowMaterial.shader.name;
                        }
                        else
                        {
                            result = orig(self);
                        }
                    }
                    else
                    {
                        result = orig(self);
                    }
                }
                else
                {
                    result = orig(self);
                }
            }
            return result;
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

        public static void PunchoutUpdateUI(Action<PunchoutPlayerController> orig, PunchoutPlayerController self)
        {
            if ((int)self.m_playerId > 7)
            {
                string str = backUpUI[self.m_playerId];

                self.HealthBarUI.SpriteName = "punch_health_bar_001";
                if (self.Health > 66f)
                {
                    self.PlayerUiSprite.SpriteName = str + "1";
                }
                else if (self.Health > 33f)
                {
                    self.PlayerUiSprite.SpriteName = str + "2";
                }
                else
                {
                    self.PlayerUiSprite.SpriteName = str + "3";
                }
                if (self.IsEevee && self.PlayerUiSprite.OverrideMaterial == null)
                {
                    Material material = UnityEngine.Object.Instantiate<Material>(self.PlayerUiSprite.Atlas.Material);
                    material.shader = Shader.Find("Brave/Internal/GlitchEevee");
                    material.SetTexture("_EeveeTex", self.CosmicTex);
                    material.SetFloat("_WaveIntensity", 0.1f);
                    material.SetFloat("_ColorIntensity", 0.015f);
                    self.PlayerUiSprite.OverrideMaterial = material;
                    return;
                }
                if (!self.IsEevee && self.PlayerUiSprite.OverrideMaterial != null)
                {
                    self.PlayerUiSprite.OverrideMaterial = null;
                }
            }
            else
            {
                orig(self);
            }


        }

        public static string[] backUp = PunchoutPlayerController.PlayerNames;
        public static string[] backUpUI = PunchoutPlayerController.PlayerUiNames;


        //one hook in and im already at the point of wanting to punch my screen thats gotta be a new record!! Update its like 3? (i think, ive lost track couldve been a week) days later and i can say it got worse 
        public static void InitHook(Action<PunchoutController> orig, PunchoutController self)
        {
            //ETGModConsole.Log("InitHook 0");
            FieldInfo _isInitialized = typeof(PunchoutController).GetField("m_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo _PlayerNames = typeof(PunchoutPlayerController).GetField("PlayerNames", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo _PlayerUiNames = typeof(PunchoutPlayerController).GetField("PlayerUiNames", BindingFlags.NonPublic | BindingFlags.Static);
            //ETGModConsole.Log("InitHook 1");
            var CCC = GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<CustomCharacter>();
            if ((int)GameManager.Instance.PrimaryPlayer.characterIdentity > 10 && CCC != null)
            {
                var name = CCC.data.nameShort.ToLower();

                if (!backUp.Contains("eevee"))
                {
                    var fuckFuckFuck = (_PlayerNames.GetValue(null) as string[]).ToList();
                    fuckFuckFuck.Add("eevee");
                    _PlayerNames.SetValue(null, fuckFuckFuck.ToArray());

                    var fuckFuckFuckShit = (_PlayerUiNames.GetValue(null) as string[]).ToList();
                    fuckFuckFuckShit.Add($"punch_player_health_eevee_00");
                    _PlayerUiNames.SetValue(null, fuckFuckFuckShit.ToArray());
                }

                if (!backUp.Contains(name)) //.Contains(name)))
                {
                    var fuckFuckFuck = (_PlayerNames.GetValue(null) as string[]).ToList();
                    fuckFuckFuck.Add(name);
                    _PlayerNames.SetValue(null, fuckFuckFuck.ToArray());

                    var fuckFuckFuckShit = (_PlayerUiNames.GetValue(null) as string[]).ToList();
                    fuckFuckFuckShit.Add($"punch_player_health_{name}_00");
                    _PlayerUiNames.SetValue(null, fuckFuckFuckShit.ToArray());


                    CustomCharacter.punchoutBullShit.Add(name, (_PlayerUiNames.GetValue(null) as string[]).Length - 1);
                    backUp = _PlayerNames.GetValue(null) as string[];
                    backUpUI = _PlayerUiNames.GetValue(null) as string[];
                }

                self.Player.CustomSwapPlayer(new int?(CustomCharacter.punchoutBullShit[name]), false);
                self.CoopCultist.gameObject.SetActive(GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER);
                self.StartCoroutine(self.GetType().GetMethod("UiFadeInCR", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null) as IEnumerator);
                _isInitialized.SetValue(self, true);
                self.Player.sprite.usesOverrideMaterial = true;

                bool hasMat = false;
                if ((int)GameManager.Instance.PrimaryPlayer?.characterIdentity > 10 && CCC != null)
                {

                    if (CCC.data.useGlow)
                    {
                        if (CCC.data.glowMaterial != null)
                        {
                            if (CCC.data.glowMaterial.GetTexture("_MainTex") != self.Player.sprite.renderer.material.GetTexture("_MainTex"))
                            {
                                CCC.data.glowMaterial.SetTexture("_MainTexture", self.Player.sprite.renderer.material.GetTexture("_MainTex"));
                            }
                            if (hasMat == false) { hasMat = !hasMat; }
                            self.Player.sprite.renderer.material = CCC.data.glowMaterial;
                        }
                    }
                }
                if (hasMat == false) 
                {
                    Material mat = new Material(SpriteHandler.Default_Punchout_Material);
                    mat.mainTexture = self.Player.sprite.renderer.material.mainTexture;
                    mat.SetTexture("_MainTexture", self.Player.sprite.renderer.material.GetTexture("_MainTex"));
                    self.Player.sprite.renderer.material = mat;
                }
            }
            else
            {
                orig(self);
            }
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




        private static string GetDeathPortraitNameHook(Func<AmmonomiconDeathPageController, string> orig, AmmonomiconDeathPageController self)
        {
            if ((int)GameManager.Instance.PrimaryPlayer.characterIdentity > 10 && GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<CustomCharacter>() != null)
            {
                return $"coop_page_death_{GameManager.Instance.PrimaryPlayer.gameObject.GetComponent<CustomCharacter>().data.nameShort.ToLower()}_001";

            }
            else
            {
                return orig(self);
            }

        }

        //stupid janky hooks that only exist so we can have things glow >:|
        public static void ChangeSpecialShaderFlagHook(Action<PlayerController, int, float> orig, PlayerController self, int flagIndex, float val)
        {
            if ((int)self.characterIdentity > 10 && self.gameObject.GetComponent<CustomCharacter>())
            {
                if (val == 0)
                {
                    if (self.gameObject.GetComponent<CustomCharacter>().data.useGlow)
                    {
                        if (self.IsUsingAlternateCostume && self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial != null)
                        {
                            if (self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                            {
                                self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial.SetTexture("_MainTexture", self.sprite.renderer.material.GetTexture("_MainTex"));
                            }
                            self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial;
                        }
                        else if (self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial != null)
                        {
                            if (self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                            {
                                self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial.SetTexture("_MainTexture", self.sprite.renderer.material.GetTexture("_MainTex"));
                            }
                            self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial;
                        }
                        else
                        {
                            ETGModConsole.Log($"[Charapi]: glow material NULLED");
                        }
                    }
                }
                else
                {
                    if (self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial.GetTexture("_MainTex") != self.sprite.renderer.material.GetTexture("_MainTex"))
                    {
                        self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial.SetTexture("_MainTexture", self.sprite.renderer.material.GetTexture("_MainTex"));
                    }
                    self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial;
                }
            }
            orig(self, flagIndex, val);
        }

        public static void RegisterOverrideColorHook(Action<GameActor, Color, string> orig, GameActor self, Color overrideColor, string source)
        {
            if (self is PlayerController && (int)(self as PlayerController).characterIdentity > 10 && self.gameObject.GetComponent<CustomCharacter>() != null)
            {
                if (self.gameObject.GetComponent<CustomCharacter>().data == null)
                {
                    if (!self.gameObject.GetComponent<CustomCharacter>().GetData())
                    {
                        ETGModConsole.Log($"[Charapi]: custom character data nulled... seems it dosent exists... fuck!");
                    }
                }

                for (int i = 0; i < self.healthHaver.bodySprites.Count; i++)
                {
                    if (self.healthHaver.bodySprites[i])
                    {
                        self.healthHaver.bodySprites[i].usesOverrideMaterial = true;
                        self.healthHaver.bodySprites[i].renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial;
                    }
                }
            }
            orig(self, overrideColor, source);
        }

        public static void DeregisterOverrideColorHook(Action<GameActor, string> orig, GameActor self, string source)
        {
            orig(self, source);
            if (self is PlayerController && (int)(self as PlayerController).characterIdentity > 10 && self.gameObject.GetComponent<CustomCharacter>())
            {
                if (self.gameObject.GetComponent<CustomCharacter>().data.useGlow)
                {
                    if ((self as PlayerController).IsUsingAlternateCostume && self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial != null)
                    {
                        self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial.SetTexture("_MainTexture", self.sprite.renderer.material.GetTexture("_MainTex"));
                        self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.altGlowMaterial;
                    }
                    else if (self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial != null)
                    {
                        self.gameObject.GetComponent<CustomCharacter>().data.normalMaterial.SetTexture("_MainTexture", self.sprite.renderer.material.GetTexture("_MainTex"));
                        self.sprite.renderer.material = self.gameObject.GetComponent<CustomCharacter>().data.glowMaterial;
                    }
                    else
                    {
                        ETGModConsole.Log($"[Charapi]: glow material NULLED");
                    }
                }
            }
        }

        public static float GetPlayerStatValueHook(Func<GameStatsManager, TrackedStats, float> orig, GameStatsManager self, TrackedStats stat)
        {
            float statValue = orig(self, stat);
            foreach (var whydodgerollmustyouhurtmelikethis in self.m_characterStats)
            {
                if (Loader.myPlayableCharacters.Contains(whydodgerollmustyouhurtmelikethis.Key))
                {

                    if (whydodgerollmustyouhurtmelikethis.Value != null)
                    {
                        statValue += whydodgerollmustyouhurtmelikethis.Value.GetStatValue(stat);
                    }
                }
            }
            return statValue;
        }


        public static void ClearStatValueGlobalHook(Action<GameStatsManager, TrackedStats> orig, GameStatsManager self, TrackedStats stat)
        {
            orig(self, stat);
            foreach (var whydodgerollmustyouhurtmelikethis in self.m_characterStats)
            {
                if (Loader.myPlayableCharacters.Contains(whydodgerollmustyouhurtmelikethis.Key))
                {

                    if (whydodgerollmustyouhurtmelikethis.Value != null)
                    {
                        whydodgerollmustyouhurtmelikethis.Value.SetStat(stat, 0);
                    }

                }
            }
        }

        public static void ClearOverheadElementHook(Action<FoyerCharacterSelectFlag> orig, FoyerCharacterSelectFlag self)
        {
            FieldInfo _extantOverheadUIElement = typeof(FoyerCharacterSelectFlag).GetField("m_extantOverheadUIElement", BindingFlags.NonPublic | BindingFlags.Instance);

            if ((_extantOverheadUIElement.GetValue(self) as dfControl) != null && FakePrefab.IsFakePrefab((_extantOverheadUIElement.GetValue(self) as dfControl).gameObject))
            {
                (_extantOverheadUIElement.GetValue(self) as dfControl).gameObject.SetActive(false);
                //UnityEngine.Object.Destroy((_extantOverheadUIElement.GetValue(self) as dfControl).gameObject);
                _extantOverheadUIElement.SetValue(self, null);
            }
            else
            {
                orig(self);
            }
        }

        public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        public static string GetBaseAnimationNameHook(Func<PlayerController, Vector2, float, bool, bool, string> orig, PlayerController self, Vector2 v, float gunAngle, bool invertThresholds = false, bool forceTwoHands = false)
        {
            string s = self.gameObject?.GetComponent<CustomCharacter>()?.overrideAnimation;
            if (!string.IsNullOrEmpty(s))
                return s;
            return orig(self, v, gunAngle, invertThresholds, forceTwoHands);

        }

        public static void DoGhostBlankHook(Action<PlayerController> orig, PlayerController self)
        {
            if (CharacterBuilder.storedCharacters.Count() > 0)
            {
                orig(self);
                return;
            }
            var component = self.gameObject.GetComponent<CustomCharacter>();
            if (component != null)
            {
                if (CharacterBuilder.storedCharacters[component.data.nameInternal.ToLower()].First.coopBlankReplacement != null)
                {
                    self.m_blankCooldownTimer = CharacterBuilder.storedCharacters[self.gameObject.GetComponent<CustomCharacter>()?.data.nameInternal.ToLower()].First.coopBlankReplacement(self);

                }
            }
            else
            {
                orig(self);
            }
        }
        private static void UpdateHook(Action<CharacterSelectIdleDoer> orig, CharacterSelectIdleDoer self)
        {
            if (self.GetComponent<CustomCharacterFoyerController>() != null && self.GetComponent<CustomCharacterFoyerController>().useGlow && self.sprite.renderer.material != self.GetComponent<CustomCharacterFoyerController>().data.glowMaterial)
            {
                var character = self.GetComponent<CustomCharacterFoyerController>();
                self.sprite.usesOverrideMaterial = true;
                self.sprite.renderer.material = character.data.glowMaterial;

            }
            orig(self);
        }

        public static void OnSelectedCharacterCallbackHook(Action<FoyerCharacterSelectFlag, PlayerController> orig, FoyerCharacterSelectFlag self, PlayerController newCharacter)
        {
            orig(self, newCharacter);
            //ETGModConsole.Log($"{newCharacter.gameObject.name} - {self.CharacterPrefabPath}");
            if (newCharacter.gameObject.name.ToLower().Contains(self.CharacterPrefabPath.ToLower(), false))
            {
                if (self.GetComponent<CustomCharacterFoyerController>() != null && self.GetComponent<CustomCharacterFoyerController>().metaCost > 0)
                {

                    GameStatsManager.Instance.RegisterStatChange(TrackedStats.META_CURRENCY, -(self.GetComponent<CustomCharacterFoyerController>().metaCost));
                }
                self.gameObject.SetActive(false);
                self.GetComponent<SpeculativeRigidbody>().enabled = false;
            }
        }

        private static void OnEnableHook(Action<CharacterSelectIdleDoer> orig, CharacterSelectIdleDoer self)
        {
            if (self.GetComponent<CustomCharacterFoyerController>() != null && self.GetComponent<CustomCharacterFoyerController>().useGlow && self.sprite.renderer.material != self.GetComponent<CustomCharacterFoyerController>().data.glowMaterial)
            {
                var character = self.GetComponent<CustomCharacterFoyerController>();
                self.sprite.usesOverrideMaterial = true;
                self.sprite.renderer.material = character.data.glowMaterial;
            }

            orig(self);
        }

        public static bool CanBeSelectedHook(Func<FoyerCharacterSelectFlag, bool> orig, FoyerCharacterSelectFlag self)
        {
            if (self.GetComponent<CustomCharacterFoyerController>() != null && self.GetComponent<CustomCharacterFoyerController>().metaCost > 0)
            {
                return (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY) >= self.GetComponent<CustomCharacterFoyerController>().metaCost);
            }
            else
            {
                return orig(self);
            }

        }

        public static QuickRestartOptions GetNumMetasToQuickRestartHook(Func<QuickRestartOptions> orig)
        {
            QuickRestartOptions result = orig();

            for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
            {
                if (GameManager.Instance.AllPlayers[i].characterIdentity > (PlayableCharacters)10 && GameManager.Instance.AllPlayers[i].gameObject.GetComponent<CustomCharacter>() != null)
                {
                    result.NumMetas += GameManager.Instance.AllPlayers[i].GetComponent<CustomCharacter>().data.metaCost;
                }
            }
            return result;
        }
        #region Dumb Bad Hooks


        public static void InteractHook(Action<ArkController, PlayerController> orig, ArkController self, PlayerController interactor)
        {

            if ((int)interactor.characterIdentity > 10 && ETGModMainBehaviour.Instance.gameObject.GetComponent("CharApiHiveMind") != null)
            {
                FieldInfo _hasBeenInteracted = typeof(ArkController).GetField("m_hasBeenInteracted", BindingFlags.NonPublic | BindingFlags.Instance);

                SpriteOutlineManager.RemoveOutlineFromSprite(self.sprite, false);
                SpriteOutlineManager.RemoveOutlineFromSprite(self.LidAnimator.sprite, false);
                if (!(bool)_hasBeenInteracted.GetValue(self))
                {
                    _hasBeenInteracted.SetValue(self, true);
                }
                for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
                {
                    GameManager.Instance.AllPlayers[i].RemoveBrokenInteractable(self);
                }
                BraveInput.DoVibrationForAllPlayers(Vibration.Time.Normal, Vibration.Strength.Medium);
                if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                {
                    PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(interactor);
                    float num = Vector2.Distance(otherPlayer.CenterPosition, interactor.CenterPosition);
                    if (num > 8f || num < 0.75f)
                    {
                        Vector2 a = Vector2.right;
                        if (interactor.CenterPosition.x < self.ChestAnimator.sprite.WorldCenter.x)
                        {
                            a = Vector2.left;
                        }
                        otherPlayer.WarpToPoint(otherPlayer.transform.position.XY() + a * 2f, true, false);
                    }
                }
                var comp = ETGModMainBehaviour.Instance.gameObject.GetComponent("CharApiHiveMind");

                self.StartCoroutine(comp.GetType().GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { self, interactor }) as IEnumerator);
            }
            else
            {
                orig(self, interactor);
            }
        }






        #endregion
        //Hook for Punchout UI being updated (called when UI updates)

        public static string GetTalkingPlayerNickHook(Func<string> orig)
        {
            PlayerController talkingPlayer = Hooks.GetTalkingPlayer();
            if (talkingPlayer.IsThief)
            {
                return "#THIEF_NAME";
            }
            if (talkingPlayer.GetComponent<CustomCharacter>() != null)
            {
                if (talkingPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return "#PLAYER_NICK_" + talkingPlayer.GetComponent<CustomCharacter>().data.nickname.ToUpper();
                }
            }
            if (talkingPlayer.characterIdentity == PlayableCharacters.Eevee)
            {
                return "#PLAYER_NICK_RANDOM";
            }
            if (talkingPlayer.characterIdentity == PlayableCharacters.Gunslinger)
            {
                return "#PLAYER_NICK_GUNSLINGER";
            }
            return "#PLAYER_NICK_" + talkingPlayer.characterIdentity.ToString().ToUpperInvariant();
        }

        public static string GetValueHook(Func<dfLanguageManager, string, string> orig, dfLanguageManager self, string key)
        {
            if (characterDeathNames.Contains(key))
            {
                if (GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>() != null && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.name;
                }
            }
            return orig(self, key);
        }

        public static string GetTalkingPlayerNameHook(Func<string> orig)
        {
            PlayerController talkingPlayer = Hooks.GetTalkingPlayer();
            if (talkingPlayer.IsThief)
            {
                return "#THIEF_NAME";
            }
            if (talkingPlayer.GetComponent<CustomCharacter>() != null)
            {
                if (talkingPlayer.GetComponent<CustomCharacter>().data != null)
                {
                    return "#PLAYER_NAME_" + talkingPlayer.GetComponent<CustomCharacter>().data.nameShort.ToUpper();
                }
            }
            if (talkingPlayer.characterIdentity == PlayableCharacters.Eevee)
            {
                return "#PLAYER_NAME_RANDOM";
            }
            if (talkingPlayer.characterIdentity == PlayableCharacters.Gunslinger)
            {
                return "#PLAYER_NAME_GUNSLINGER";
            }
            return "#PLAYER_NAME_" + talkingPlayer.characterIdentity.ToString().ToUpperInvariant();
        }

        private static PlayerController GetTalkingPlayer()
        {
            List<TalkDoerLite> allNpcs = StaticReferenceManager.AllNpcs;
            for (int i = 0; i < allNpcs.Count; i++)
            {
                if (allNpcs[i])
                {
                    if (!allNpcs[i].IsTalking || !allNpcs[i].TalkingPlayer || GameManager.Instance.HasPlayer(allNpcs[i].TalkingPlayer))
                    {
                        if (allNpcs[i].IsTalking && allNpcs[i].TalkingPlayer)
                        {
                            return allNpcs[i].TalkingPlayer;
                        }
                    }
                }
            }
            return GameManager.Instance.PrimaryPlayer;
        }


        //Triggers FoyerCharacterHandler (called from Foyer.SetUpCharacterCallbacks)

        public static List<FoyerCharacterSelectFlag> FoyerCallbacks2(Func<Foyer, List<FoyerCharacterSelectFlag>> orig, Foyer self)
        {
            var sortedByX = orig(self);

            var sortedByXCustom = FoyerCharacterHandler.AddCustomCharactersToFoyer(sortedByX);

            foreach (var character in sortedByXCustom)
            {
                sortedByX.Add(character);
                //self.OnPlayerCharacterChanged = character.OnSelectedCharacterCallback;


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

        public static void PrimaryPlayerSwitched(Action<ETGModConsole, string[]> orig, ETGModConsole self, string[] args)
        {
            try
            {
                orig(self, args);
            }
            catch { }
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


        private static void SetWinPicHook(Action<AmmonomiconDeathPageController> orig, AmmonomiconDeathPageController self)
        {
            orig(self);
            GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
            if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>() && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data != null)
            {
                if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FINALGEON && (bool)typeof(AmmonomiconDeathPageController).GetMethod("ShouldUseJunkPic", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null))
                {
                    if (GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.junkanWinPic == null)
                    {
                        self.photoSprite.Texture = (BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture);
                    }
                    else
                    {
                        self.photoSprite.Texture = GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.junkanWinPic;
                    }

                }
                else if (tilesetId == GlobalDungeonData.ValidTilesets.FINALGEON && GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.pastWinPic)
                {
                    self.photoSprite.Texture = GameManager.Instance.PrimaryPlayer.GetComponent<CustomCharacter>().data.pastWinPic;
                }
            }
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
