using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alexandria.ItemAPI;
using Dungeonator;
using UnityEngine;

namespace Alexandria.Misc
{
   public static class AmmoPickupFixer
    {
        public static void Init()
        {
            new Hook(
                typeof(AmmoPickup).GetMethod("Interact", BindingFlags.Instance | BindingFlags.Public),
                typeof(AmmoPickupFixer).GetMethod("ammoInteractHookMethod")
            );
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
        public static void ammoInteractHookMethod(Action<AmmoPickup, PlayerController> orig, AmmoPickup self, PlayerController player)
        {
            if (player.CurrentGun && player.CurrentGun.GetComponent<AdvancedGunBehavior>() && player.CurrentGun.GetComponent<AdvancedGunBehavior>().canCollectAmmoAtMaxAmmo)
            {
                if (RoomHandler.unassignedInteractableObjects.Contains(self)) RoomHandler.unassignedInteractableObjects.Remove(self);
                SpriteOutlineManager.RemoveOutlineFromSprite(self.sprite, true);
                self.Pickup(player);
            }
            else if (player.CurrentGun && !player.CurrentGun.CanGainAmmo)
            {
                GameUIRoot.Instance.InformNeedsReload(player, new Vector3(player.specRigidbody.UnitCenter.x - player.transform.position.x, 1.25f, 0f), 1f, "#RELOAD_FULL");
                return;
            }
            else
            {
                orig(self, player);
            }
        }
    }
}
