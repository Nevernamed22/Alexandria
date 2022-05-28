using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
   public static class PickupUtility
    {
        public static void PretendPickup(this AmmoPickup pickup, PlayerController player, bool spreadAmmoGranted = true)
        {
            if (RoomHandler.unassignedInteractableObjects.Contains(pickup))
            {
                RoomHandler.unassignedInteractableObjects.Remove(pickup);
            }
            SpriteOutlineManager.RemoveOutlineFromSprite(pickup.sprite, true);

            if (pickup.mode == AmmoPickup.AmmoPickupMode.FULL_AMMO)
            {
                string @string = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
                string description = player.CurrentGun.GetComponent<EncounterTrackable>().journalData.GetPrimaryDisplayName(false) + " " + StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_BODY");
                tk2dBaseSprite sprite = player.CurrentGun.GetSprite();
                if (!GameUIRoot.Instance.BossHealthBarVisible)
                {
                    GameUIRoot.Instance.notificationController.DoCustomNotification(@string, description, sprite.Collection, sprite.spriteId, UINotificationController.NotificationColor.SILVER, false, false);
                }

            }
            else if (pickup.mode == AmmoPickup.AmmoPickupMode.SPREAD_AMMO)
            {
                for (int i = 0; i < player.inventory.AllGuns.Count; i++)
                {
                    if (player.inventory.AllGuns[i] && player.CurrentGun != player.inventory.AllGuns[i])
                    {
                      if (spreadAmmoGranted)  player.inventory.AllGuns[i].GainAmmo(Mathf.FloorToInt((float)player.inventory.AllGuns[i].AdjustedMaxAmmo * pickup.SpreadAmmoOtherGunsPercent));
                    }
                }
                player.CurrentGun.ForceImmediateReload(false);
                if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                {
                    PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
                    if (!otherPlayer.IsGhost)
                    {
                        for (int j = 0; j < otherPlayer.inventory.AllGuns.Count; j++)
                        {
                            if (otherPlayer.inventory.AllGuns[j])
                            {
                                if (spreadAmmoGranted) otherPlayer.inventory.AllGuns[j].GainAmmo(Mathf.FloorToInt((float)otherPlayer.inventory.AllGuns[j].AdjustedMaxAmmo * pickup.SpreadAmmoOtherGunsPercent));
                            }
                        }
                        otherPlayer.CurrentGun.ForceImmediateReload(false);
                    }
                }
                string string2 = StringTableManager.GetString("#AMMO_SINGLE_GUN_REFILLED_HEADER");
                string string3 = StringTableManager.GetString("#AMMO_SPREAD_REFILLED_BODY");
                tk2dBaseSprite sprite2 = pickup.sprite;
                if (!GameUIRoot.Instance.BossHealthBarVisible)
                {
                    GameUIRoot.Instance.notificationController.DoCustomNotification(string2, string3, sprite2.Collection, sprite2.spriteId, UINotificationController.NotificationColor.SILVER, false, false);
                }
            }

            FieldInfo field = typeof(AmmoPickup).GetField("m_pickedUp", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(pickup, false);

            FieldInfo field2 = typeof(AmmoPickup).GetField("m_isBeingEyedByRat", BindingFlags.Instance | BindingFlags.NonPublic);
            field2.SetValue(pickup, false);

            var type = typeof(AmmoPickup);
            var func = type.GetMethod("GetRidOfMinimapIcon", BindingFlags.Instance | BindingFlags.NonPublic);
            var ret = func.Invoke(pickup.gameObject.GetComponent<AmmoPickup>(), null);

            if (pickup.pickupVFX != null)
            {
                player.PlayEffectOnActor(pickup.pickupVFX, Vector3.zero, true, false, false);
            }
            UnityEngine.Object.Destroy(pickup.gameObject);
            AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", pickup.gameObject);
        }
    }
}
