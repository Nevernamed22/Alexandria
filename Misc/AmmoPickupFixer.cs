using System;
using System.Collections.Generic;
using System.Text;
using Alexandria.ItemAPI;
using Dungeonator;
using UnityEngine;
using HarmonyLib;

namespace Alexandria.Misc
{
   [HarmonyPatch]
   public static class AmmoPickupFixer
    {
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void Init() { }

        [HarmonyPatch(typeof(AmmoPickup), nameof(AmmoPickup.Interact))]
        [HarmonyPrefix]
        private static bool AmmoPickupInteractPatch(AmmoPickup __instance, PlayerController interactor)
        {
            if (interactor.CurrentGun is Gun gun && gun.GetComponent<AdvancedGunBehavior>() is AdvancedGunBehavior agb && agb.canCollectAmmoAtMaxAmmo)
            {
                if (RoomHandler.unassignedInteractableObjects.Contains(__instance))
                    RoomHandler.unassignedInteractableObjects.Remove(__instance);
                SpriteOutlineManager.RemoveOutlineFromSprite(__instance.sprite, true);
                __instance.Pickup(interactor);
                return false;
            }
            if (interactor.CurrentGun && !interactor.CurrentGun.CanGainAmmo)
            {
                GameUIRoot.Instance.InformNeedsReload(interactor, new Vector3(interactor.specRigidbody.UnitCenter.x - interactor.transform.position.x, 1.25f, 0f), 1f, "#RELOAD_FULL");
                return false;
            }
            return true;
        }

        public static void ForcePickupWithoutGainingAmmo(this AmmoPickup self, PlayerController collectingPlayer, bool neglectSpreadToOtherGuns = false)
        {
            if (self.mode == AmmoPickup.AmmoPickupMode.FULL_AMMO)
            {
                string @string = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
                string description = collectingPlayer.CurrentGun.GetComponent<EncounterTrackable>().journalData.GetPrimaryDisplayName(false) + " " + StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_BODY");
                tk2dBaseSprite sprite = collectingPlayer.CurrentGun.GetSprite();
                if (!GameUIRoot.Instance.BossHealthBarVisible)
                {
                    GameUIRoot.Instance.notificationController.DoCustomNotification(@string, description, sprite.Collection, sprite.spriteId, UINotificationController.NotificationColor.SILVER, false, false);
                }
            }
            else if (self.mode == AmmoPickup.AmmoPickupMode.SPREAD_AMMO)
            {
                for (int i = 0; i < collectingPlayer.inventory.AllGuns.Count; i++)
                {
                    if (!neglectSpreadToOtherGuns && collectingPlayer.inventory.AllGuns[i] && collectingPlayer.CurrentGun != collectingPlayer.inventory.AllGuns[i])
                    {
                        collectingPlayer.inventory.AllGuns[i].GainAmmo(Mathf.FloorToInt((float)collectingPlayer.inventory.AllGuns[i].AdjustedMaxAmmo * self.SpreadAmmoOtherGunsPercent));
                    }
                }
                collectingPlayer.CurrentGun.ForceImmediateReload(false);
                if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                {
                    PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(collectingPlayer);
                    if (!otherPlayer.IsGhost)
                    {
                        for (int j = 0; j < otherPlayer.inventory.AllGuns.Count; j++)
                        {
                            if (!neglectSpreadToOtherGuns && otherPlayer.inventory.AllGuns[j])
                            {
                                otherPlayer.inventory.AllGuns[j].GainAmmo(Mathf.FloorToInt((float)otherPlayer.inventory.AllGuns[j].AdjustedMaxAmmo * self.SpreadAmmoOtherGunsPercent));
                            }
                        }
                        otherPlayer.CurrentGun.ForceImmediateReload(false);
                    }
                }
                string string2 = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
                string string3 = StringTableManager.GetString("#AMMO_SPREAD_REFILLED_BODY");
                tk2dBaseSprite sprite2 = self.sprite;
                if (!GameUIRoot.Instance.BossHealthBarVisible)
                {
                    GameUIRoot.Instance.notificationController.DoCustomNotification(string2, string3, sprite2.Collection, sprite2.spriteId, UINotificationController.NotificationColor.SILVER, false, false);
                }
            }
            self.m_pickedUp = false;
            self.m_isBeingEyedByRat = false;
            self.GetRidOfMinimapIcon();

            if (self.pickupVFX != null) collectingPlayer.PlayEffectOnActor(self.pickupVFX, Vector3.zero, true, false, false);

            UnityEngine.Object.Destroy(self.gameObject);
            AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", self.gameObject);
        }      
    }
}
