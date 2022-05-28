using Alexandria.ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
   public static class PlayerUtility
    {
        public enum EasyBlankType
        {
            FULL,
            MINI,
        }
        public static bool AnyPlayerIsInCombat(this GameManager instance)
        {
            bool toReturn = false;
            if (instance)
            {
                if (instance.PrimaryPlayer != null && instance.PrimaryPlayer.IsInCombat) toReturn = true;
                if (instance.SecondaryPlayer != null && instance.SecondaryPlayer.IsInCombat) toReturn = true;
            }
            return toReturn;
        }
        public static bool IsStarterItem(this PlayerController player, int id)
        {
            return player.startingActiveItemIds.Contains(id) || player.startingAlternateGunIds.Contains(id) || player.startingGunIds.Contains(id) || player.startingPassiveItemIds.Contains(id);
        }
        public static void RemoveItemFromInventory(this PlayerController player, PickupObject item)
        {
            if (item == null) return;
            if (item is PassiveItem)
            {
                if (player.passiveItems.Contains(item as PassiveItem))
                {
                    player.passiveItems.Remove(item as PassiveItem);
                    GameUIRoot.Instance.RemovePassiveItemFromDock(item as PassiveItem);
                    player.stats.RecalculateStats(player, false, false);
                }
            }
            else if (item is PlayerItem)
            {
                if (player.activeItems.Contains(item as PlayerItem))
                {
                    player.activeItems.Remove(item as PlayerItem);
                    player.stats.RecalculateStats(player, false, false);
                }
            }
            else if (item is Gun)
            {
                if (player.inventory.AllGuns.Contains(item as Gun))
                {
                    player.inventory.RemoveGunFromInventory(item as Gun);
                    player.stats.RecalculateStats(player, false, false);
                }
            }
        }
        public static int GetNumberOfItemInInventory(this PlayerController player, int itemID)
        {
            int foundVer = 0;
            foreach (PassiveItem item in player.passiveItems) { if (item.PickupObjectId == itemID) foundVer++; }
            foreach (PlayerItem item in player.activeItems) { if (item.PickupObjectId == itemID) foundVer++; }
            return foundVer;
        }
        public static bool NextHitWillKillPlayer(this PlayerController player, float damageAmount)
        {
            if (player.healthHaver)
            {
                if (player.healthHaver.NextShotKills == true) return true;
                if (player.characterIdentity != PlayableCharacters.Robot)
                {
                    if (player.healthHaver.GetCurrentHealth() > damageAmount) return false;
                    else if (player.healthHaver.Armor > 0 && !player.healthHaver.NextDamageIgnoresArmor) return false;
                    else return true;
                }
                else
                {
                    if (player.healthHaver.Armor > 1) return true;
                    else return false;
                }
            }
            else
            {
                Debug.LogError("PLAYER HAD NO HEALTHHAVER????");

                return false;
            }
        }
        public static void RemovePassiveItemAtIndex(this PlayerController player, int index)
        {
            PassiveItem passiveItem = player.passiveItems[index];
            GameUIRoot.Instance.RemovePassiveItemFromDock(passiveItem);
            player.passiveItems.RemoveAt(index);
            UnityEngine.Object.Destroy(passiveItem);
            player.stats.RecalculateStats(player, false, false);
        }
        public static void DoEasyBlank(this PlayerController blankOwner, Vector2 blankPosition, EasyBlankType type)
        {
            if (type == EasyBlankType.MINI)
            {
                GameObject silencerVFX = (GameObject)ResourceCache.Acquire("Global VFX/BlankVFX_Ghost");
                AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", blankOwner.gameObject);
                GameObject gameObject = new GameObject("silencer");
                SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
                float additionalTimeAtMaxRadius = 0.25f;
                silencerInstance.TriggerSilencer(blankPosition, 25f, 5f, silencerVFX, 0f, 3f, 3f, 3f, 250f, 5f, additionalTimeAtMaxRadius, blankOwner, false, false);
            }
            else if (type == EasyBlankType.FULL)
            {
                GameObject bigSilencerVFX = (GameObject)ResourceCache.Acquire("Global VFX/BlankVFX");
                AkSoundEngine.PostEvent("Play_OBJ_silenceblank_use_01", blankOwner.gameObject);
                GameObject gameObject = new GameObject("silencer");
                SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
                silencerInstance.TriggerSilencer(blankPosition, 50f, 25f, bigSilencerVFX, 0.15f, 0.2f, 50f, 10f, 140f, 15f, 0.5f, blankOwner, true, false);
            }
        }
        public static void GiveAmmoToGunNotInHand(this PlayerController player, int idToGive, int AmmoToGive)
        {
            foreach (Gun gun in player.inventory.AllGuns)
            {
                if (gun.PickupObjectId == idToGive) { gun.GainAmmo(AmmoToGive); }
            }
        }
        public static PlayerController GetPlayerWithItemID(this GameManager managerInstance, int id, bool randomIfBoth = true)
        {
            bool primary = false;
            bool secondary = false;
            if (managerInstance.PrimaryPlayer && managerInstance.PrimaryPlayer.HasPickupID(id)) primary = true;
            if (managerInstance.SecondaryPlayer && managerInstance.SecondaryPlayer.HasPickupID(id)) secondary = true;
            if (primary && secondary)
            {
                if (randomIfBoth)
                {
                    if (UnityEngine.Random.value < 0.5) return managerInstance.PrimaryPlayer;
                    else return managerInstance.SecondaryPlayer;
                }
                else return managerInstance.PrimaryPlayer;
            }
            else if (primary) return managerInstance.PrimaryPlayer;
            else if (secondary) return managerInstance.SecondaryPlayer;
            else return null;
        }
        public static bool AnyPlayerHasActiveSynergy(this GameManager managerInstance, string synergyID)
        {
            bool synergyDetected = false;
            if (managerInstance.PrimaryPlayer && managerInstance.PrimaryPlayer.PlayerHasActiveSynergy(synergyID)) synergyDetected = true;
            if (managerInstance.SecondaryPlayer && managerInstance.SecondaryPlayer.PlayerHasActiveSynergy(synergyID)) synergyDetected = true;
            return synergyDetected;
        }
        public static bool AnyPlayerHasPickupID(this GameManager managerInstance, int itemID)
        {
            bool hasBeenDetectedOnPlayer = false;
            if (managerInstance.PrimaryPlayer && managerInstance.PrimaryPlayer.HasPickupID(itemID)) hasBeenDetectedOnPlayer = true;
            if (managerInstance.SecondaryPlayer && managerInstance.SecondaryPlayer.HasPickupID(itemID)) hasBeenDetectedOnPlayer = true;
            return hasBeenDetectedOnPlayer;
        }
        public static float GetCombinedPlayersStatAmount(this GameManager managerInstance, PlayerStats.StatType stat)
        {
            float amt = 0;
            if (managerInstance.PrimaryPlayer)
            {
                float primary = managerInstance.PrimaryPlayer.stats.GetStatValue(stat);
                amt += primary;
            }
            if (managerInstance.SecondaryPlayer)
            {
                float secondary = managerInstance.SecondaryPlayer.stats.GetStatValue(stat);
                amt += secondary;
            }
            return amt;
        }
        public static Vector2 PositionInDistanceFromAimDir(this PlayerController player, float distance)
        {
            Vector2 vector = player.CenterPosition;
            Vector2 normalized = (player.unadjustedAimPoint.XY() - vector).normalized;
            Vector2 final = player.CenterPosition + normalized * distance;
            return final;
        }
        public static void TriggerInvulnerableFrames(this PlayerController player, float incorporealityTime)
        {
            GameManager.Instance.StartCoroutine(IncorporealityOnHit(player, incorporealityTime));
        }
        private static IEnumerator IncorporealityOnHit(PlayerController player, float incorporealityTime)
        {
            int enemyMask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
            player.specRigidbody.AddCollisionLayerIgnoreOverride(enemyMask);
            player.healthHaver.IsVulnerable = false;
            yield return null;
            float timer = 0f;
            float subtimer = 0f;
            while (timer < incorporealityTime)
            {
                while (timer < incorporealityTime)
                {
                    timer += BraveTime.DeltaTime;
                    subtimer += BraveTime.DeltaTime;
                    if (subtimer > 0.12f)
                    {
                        player.IsVisible = false;
                        subtimer -= 0.12f;
                        break;
                    }
                    yield return null;
                }
                while (timer < incorporealityTime)
                {
                    timer += BraveTime.DeltaTime;
                    subtimer += BraveTime.DeltaTime;
                    if (subtimer > 0.12f)
                    {
                        player.IsVisible = true;
                        subtimer -= 0.12f;
                        break;
                    }
                    yield return null;
                }
            }
            int mask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
            player.IsVisible = true;
            player.healthHaver.IsVulnerable = true;
            player.specRigidbody.RemoveCollisionLayerIgnoreOverride(mask);
            yield break;
        }
        public static void RecalculateOrbitals(this PlayerController player)
        {
            Dictionary<int, int> tiersAndCounts = new Dictionary<int, int>();
            foreach (var o in player.orbitals)
            {
                var orbital = (PlayerOrbital)o;
                int targetTier = PlayerOrbital.CalculateTargetTier(player, o);
                orbital.SetOrbitalTier(targetTier);
                if (tiersAndCounts.ContainsKey(targetTier)) //Count starts at 0
                {
                    int existingCount = tiersAndCounts[targetTier];
                    tiersAndCounts[targetTier] = existingCount + 1;
                }
                else tiersAndCounts.Add(targetTier, 0);
            }
            foreach (var o in player.orbitals)
            {
                var orbital = (PlayerOrbital)o;
                int currentTier = orbital.GetOrbitalTier();
                if (tiersAndCounts.ContainsKey(currentTier))
                {
                    int currentAmtInTier = tiersAndCounts[currentTier];
                    orbital.SetOrbitalTierIndex(tiersAndCounts[currentTier]);
                    tiersAndCounts[currentTier] = currentAmtInTier - 1;

                }
                else
                {
                    orbital.SetOrbitalTierIndex(0);
                }
            }
        }
        public static Vector2? GetCursorPosition(this PlayerController user)
        {
            Vector2 position = Vector2.zero;
            GungeonActions m_activeActions = ReflectionUtility.ReflectGetField<GungeonActions>(typeof(PlayerController), "m_activeActions", user);
            if (BraveInput.GetInstanceForPlayer(user.PlayerIDX).IsKeyboardAndMouse(false)) { position = user.unadjustedAimPoint.XY() - (user.CenterPosition - user.specRigidbody.UnitCenter); }
            else
            {
                return null;
            }
            position = BraveMathCollege.ClampToBounds(position, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
            return position;
        }
    }
}
