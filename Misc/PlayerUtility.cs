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
        //Inventory Management. Methods which add, remove, or check items from the player's inventory.

        /// <summary>
        /// Removes the given pickupobject from the player's inventory if it is present in the player's inventory. Works on passives, actives, and guns.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="item">The item or gun to be removed.</param>
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

        /// <summary>
        /// Returns true if the given ID corresponds to one of the target player's starting items.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="id">The item ID being checked.</param>
        public static bool IsStarterItem(this PlayerController player, int id)
        {
            return player.startingActiveItemIds.Contains(id) || player.startingAlternateGunIds.Contains(id) || player.startingGunIds.Contains(id) || player.startingPassiveItemIds.Contains(id);
        }

        /// <summary>
        /// Returns an integer value corresponding to the number of copies of a given passive or active item present in the player's inventory. 
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="itemID">The item ID being counted.</param>
        public static int GetNumberOfItemInInventory(this PlayerController player, int itemID)
        {
            int foundVer = 0;
            foreach (PassiveItem item in player.passiveItems) { if (item.PickupObjectId == itemID) foundVer++; }
            foreach (PlayerItem item in player.activeItems) { if (item.PickupObjectId == itemID) foundVer++; }
            return foundVer;
        }

        /// <summary>
        /// Removes the item at the given index in the target player's inventory.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="index">The index at which the item should be removed.</param>
        public static void RemovePassiveItemAtIndex(this PlayerController player, int index)
        {
            PassiveItem passiveItem = player.passiveItems[index];
            GameUIRoot.Instance.RemovePassiveItemFromDock(passiveItem);
            player.passiveItems.RemoveAt(index);
            UnityEngine.Object.Destroy(passiveItem);
            player.stats.RecalculateStats(player, false, false);
        }

        //Other Informative Methods.

        /// <summary>
        /// Returns the ExtendedPlayerComponent present on the specified player. Returns null if the ExtendedPlayerComponent is not present, however this should never happen.
        /// </summary>
        /// <param name="owner">The target player.</param>
        public static ExtendedPlayerComponent GetExtComp(this PlayerController owner)
        {
            if (owner.GetComponent<ExtendedPlayerComponent>() != null) return owner.GetComponent<ExtendedPlayerComponent>();
            else
            {
                Debug.LogError("Alexandria (PlayerUtility): NO EXTENDEDPLAYERCOMPONENT WAS FOUND ON CHECK!!! - GetExtComp() was unable to find an Extended Player Component on the given PlayerController, so an emergency backup was created instead. This should never happen, and is a serious issue.");
                ExtendedPlayerComponent newComp = owner.gameObject.AddComponent<ExtendedPlayerComponent>();
                return newComp;
            }
        }

        /// <summary>
        /// Returns a Vector2 position corresponding to the specified distance from the player in the direction they are currently aiming.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="distance">The distance from the player that the returned position ought to be.</param>
        public static Vector2 PositionInDistanceFromAimDir(this PlayerController player, float distance)
        {
            Vector2 vector = player.CenterPosition;
            Vector2 normalized = (player.unadjustedAimPoint.XY() - vector).normalized;
            Vector2 final = player.CenterPosition + normalized * distance;
            return final;
        }

        /// <summary>
        /// Returns the position of the target player's cursor. Returned Vector2 is nullable, and will be null if the target player is using a controller.
        /// </summary>
        /// <param name="user">The target player.</param>
        /// <param name="fallbackAimDirDistance">If the player does not have a cursor, and fallbackAimDirDistance is greater than zero, returns a position the specified distance in the direction being aimed.</param>
        public static Vector2 GetCursorPosition(this PlayerController user, float fallbackAimDirDistance = 0)
        {
            Vector2 position = Vector2.zero;
            if (BraveInput.GetInstanceForPlayer(user.PlayerIDX).IsKeyboardAndMouse(false)) { position = user.unadjustedAimPoint.XY() - (user.CenterPosition - user.specRigidbody.UnitCenter); }
            else if (fallbackAimDirDistance != 0) { position = user.PositionInDistanceFromAimDir(fallbackAimDirDistance); }
            if (position != Vector2.zero) position = BraveMathCollege.ClampToBounds(position, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
            return position;
        }

        /// <summary>
        /// Returns true if the specified damage amount will kill the target player. Rudimentary, use with caution.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="damageAmount">The damage amount to be checked against the player's current HP.</param>
        public static bool NextHitWillKillPlayer(this PlayerController player, float damageAmount)
        {
            if (player.healthHaver)
            {
                if (player.healthHaver.NextShotKills == true) return true;
                if (player.characterIdentity != PlayableCharacters.Robot)
                {
                    float currentHearts = player.healthHaver.GetCurrentHealth();
                    if (currentHearts > damageAmount) return false;
                    else if (currentHearts > 0 && player.healthHaver.Armor > 0) return false;
                    else if (player.healthHaver.Armor > 1 && !player.healthHaver.NextDamageIgnoresArmor) return false;
                    else return true;
                }
                else
                {
                    if (player.healthHaver.Armor > 1) return false;
                    else return true;
                }
            }
            else { Debug.LogError("Alexandria (PlayerUtility): Attempted to check for fatal damage on a player with a nonexistent HealthHaver."); return false; }
        }

        //Effect Triggers. Methods which easily trigger effects on the player.

        /// <summary>
        /// Gives the specified amount of ammo to the gun in the target player's inventory with the specified ID. Does nothing if the gun corresponding to the ID is not in the player's inventory.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="idToGive">The target gun ID to restore ammo to.</param>
        /// <param name="AmmoToGive">The amount of ammo to restore.</param>
        public static void GiveAmmoToGunNotInHand(this PlayerController player, int idToGive, int AmmoToGive)
        {
            foreach (Gun gun in player.inventory.AllGuns)
            {
                if (gun.PickupObjectId == idToGive) { gun.GainAmmo(AmmoToGive); }
            }
        }

        /// <summary>
        /// Recalculates the orbital tier and orbital index of the specified player's orbitals. Can resolve issues arising from spawning, deleting, or altering orbitals.
        /// </summary>
        /// <param name="player">The target player.</param>
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

        /// <summary>
        /// Shorthand extension to trigger the invulnerability frame function of the ExtendedPlayerComponent.
        /// </summary>
        /// <param name="player">The target player.</param>
        /// <param name="incorporealityTime">How long the invulnerability frames should last.</param>
        public static void TriggerInvulnerableFrames(this PlayerController player, float incorporealityTime)
        {
            if (player && player.GetExtComp()) player.GetExtComp().TriggerInvulnerableFrames(incorporealityTime);
        }

        /// <summary>
        /// Simple shorthand to trigger a blank effect belonging to the target player at a specified position.
        /// </summary>
        /// <param name="blankOwner">The target player, who the blank effect belongs to.</param>
        /// <param name="blankPosition">The position of the blank effect.</param>
        /// <param name="type">The 'type' of blank. Set to FULL for a full room blank, or MINI for a microblank, like the effect of Blank Bullets.</param>
        public static void DoEasyBlank(this PlayerController blankOwner, Vector2 blankPosition, EasyBlankType type)
        {
            if (type == EasyBlankType.MINI)
            {
                GameObject silencerVFX = (GameObject)ResourceCache.Acquire("Global VFX/BlankVFX_Ghost");
                AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", blankOwner.gameObject);
                GameObject gameObject = new GameObject("silencer");
                SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
                silencerInstance.TriggerSilencer(blankPosition, 25f, 5f, silencerVFX, 0f, 3f, 3f, 3f, 250f, 5f, 0.25f, blankOwner, false, false);
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
    }
    public enum EasyBlankType
    {
        FULL,
        MINI,
    }
}
