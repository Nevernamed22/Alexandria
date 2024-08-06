using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace Alexandria.Misc
{
    public static class VolleyRebuildHelpers
    {
        private static bool skipRebuildingGunVolleys = false;

        [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.RebuildGunVolleys))]
        private class PlayerStatsRebuildGunVolleysPatch
        {
            static bool Prefix(PlayerController owner)
            {
                return !skipRebuildingGunVolleys; // skip original method iff skipRebuildingGunVolleys is true
            }
        }

        /// <summary>Prevent gun volleys from being rebuilt when recalculating the players' stats</summary>
        public static void RecalculateStatsWithoutRebuildingGunVolleys(this PlayerStats stats, PlayerController player)
        {
            skipRebuildingGunVolleys = true;
            stats.RecalculateStats(player);
            skipRebuildingGunVolleys = false;
        }
    }
}
