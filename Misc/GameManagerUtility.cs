using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Misc
{
   public static class GameManagerUtility
    {
        //Extensions and methods related to the gamemanager

        /// <summary>
        /// Returns the playercontroller with the specified item ID, if there is one. Returns null if no players have the item ID.
        /// </summary>
        /// <param name="managerInstance">The instance Gamemanager.</param>
        /// <param name="id">The item ID being searched for.</param>
        /// <param name="randomIfBoth">If true, and BOTH players have the specified item, the returned player will be random. If false in the same situation of both players having the item, prioritises the Primary Player.</param>
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

        /// <summary>
        /// Returns true if the Primary Player OR the Secondary Player have the specified synergy. Does not check the secondary player if the secondary player does not exist.
        /// </summary>
        /// <param name="managerInstance">The instance Gamemanager.</param>
        /// <param name="synergyID">The synergy name string to check for.</param>        
        public static bool AnyPlayerHasActiveSynergy(this GameManager managerInstance, string synergyID)
        {
            bool synergyDetected = false;
            if (managerInstance.PrimaryPlayer && managerInstance.PrimaryPlayer.PlayerHasActiveSynergy(synergyID)) synergyDetected = true;
            if (managerInstance.SecondaryPlayer && managerInstance.SecondaryPlayer.PlayerHasActiveSynergy(synergyID)) synergyDetected = true;
            return synergyDetected;
        }

        /// <summary>
        /// Returns true if the Primary Player OR the Secondary Player have the specified item ID in their inventory. Does not check the secondary player if the secondary player does not exist.
        /// </summary>
        /// <param name="managerInstance">The instance Gamemanager.</param>
        /// <param name="itemID">The item ID to check for.</param>        
        public static bool AnyPlayerHasPickupID(this GameManager managerInstance, int itemID)
        {
            bool hasBeenDetectedOnPlayer = false;
            if (managerInstance.PrimaryPlayer && managerInstance.PrimaryPlayer.HasPickupID(itemID)) hasBeenDetectedOnPlayer = true;
            if (managerInstance.SecondaryPlayer && managerInstance.SecondaryPlayer.HasPickupID(itemID)) hasBeenDetectedOnPlayer = true;
            return hasBeenDetectedOnPlayer;
        }

        /// <summary>
        /// Returns the total stat value of the specified stat shared between both players. Does not check players that do not exist.
        /// </summary>
        /// <param name="managerInstance">The instance Gamemanager.</param>
        /// <param name="stat">The specified stat to return the combined total of.</param>        
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
    }
}
