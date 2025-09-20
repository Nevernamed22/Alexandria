using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.RuntimeDetour;
using HarmonyLib;

namespace Alexandria.ItemAPI
{
    [HarmonyPatch]
    public static class CustomSynergies
    {
        [HarmonyPatch(typeof(StringTableManager), nameof(StringTableManager.GetSynergyString))]
        [HarmonyPostfix]
        private static void StringTableManagerGetSynergyStringPatch(string key, int index, ref string __result)
        {
            if (string.IsNullOrEmpty(__result))
                __result = key;
        }

        public static bool PlayerHasActiveSynergy(this PlayerController player, string synergyNameToCheck)
        {
            foreach (int index in player.ActiveExtraSynergies)
            {
                AdvancedSynergyEntry synergy = GameManager.Instance.SynergyManager.synergies[index];
                if (synergy.NameKey == synergyNameToCheck)
                {
                    return true;
                }
            }
            return false;
        }

        public static AdvancedSynergyEntry Add(string name, List<string> mandatoryConsoleIDs, List<string> optionalConsoleIDs = null, bool affectedByLichesEye = true)
        {
            if (mandatoryConsoleIDs == null || mandatoryConsoleIDs.Count == 0) { ETGModConsole.Log($"Synergy {name} has no mandatory items/guns."); return null; }
            List<int>
                itemIDs = new List<int>(),
                gunIDs = new List<int>(),
                optItemIDs = new List<int>(),
                optGunIDs = new List<int>();
            PickupObject pickup;
            foreach (var id in mandatoryConsoleIDs)
            {
                pickup = Gungeon.Game.Items[id];
                if (pickup && pickup.GetComponent<Gun>())
                    gunIDs.Add(pickup.PickupObjectId);
                else if (pickup && (pickup.GetComponent<PlayerItem>() || pickup.GetComponent<PassiveItem>()))
                    itemIDs.Add(pickup.PickupObjectId);
            }

            if (optionalConsoleIDs != null)
            {
                foreach (var id in optionalConsoleIDs)
                {
                    pickup = Gungeon.Game.Items[id];
                    if (pickup && pickup.GetComponent<Gun>())
                        optGunIDs.Add(pickup.PickupObjectId);
                    else if (pickup && (pickup.GetComponent<PlayerItem>() || pickup.GetComponent<PassiveItem>()))
                        optItemIDs.Add(pickup.PickupObjectId);
                }
            }

            AdvancedSynergyEntry entry = new AdvancedSynergyEntry()
            {
                NameKey = name,
                MandatoryItemIDs = itemIDs,
                MandatoryGunIDs = gunIDs,
                OptionalItemIDs = optItemIDs,
                OptionalGunIDs = optGunIDs,
                bonusSynergies = new List<CustomSynergyType>(),
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = !affectedByLichesEye,
            };
            Add(entry);
            return entry;
        }

        public static void Add(AdvancedSynergyEntry synergyEntry)
        {
            AdvancedSynergyEntry[] array = new AdvancedSynergyEntry[] { synergyEntry };
            GameManager.Instance.SynergyManager.synergies = GameManager.Instance.SynergyManager.synergies.Concat(array).ToArray<AdvancedSynergyEntry>();
        }

        public static bool HasMTGConsoleID(this PlayerController player, string consoleID)
        {
            if (!Gungeon.Game.Items.ContainsID(consoleID)) return false;
            return player.HasPickupID(Gungeon.Game.Items[consoleID].PickupObjectId);
        }
    }
}
