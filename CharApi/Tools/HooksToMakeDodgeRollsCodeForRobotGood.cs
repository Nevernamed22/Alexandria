using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.CharacterAPI
{
    public static class HooksToMakeDodgeRollsCodeForRobotGood
    {
        public static void Init()
        {
            Hook getValueHook = new Hook(
                typeof(BasicStatPickup).GetMethod("Pickup", BindingFlags.Public | BindingFlags.Instance),
                typeof(Hooks).GetMethod("BasicStatPickupPickupHook")
            );
        }

		//HandleDarkSoulsReset_CR
		//HandleSkullTrigger
		//PassiveItem.Pickup
		//ResetToFactorySettings
		//CoopResurrectInternal
		//HandleCloneEffect
		//TriggerDarkSoulsReset
		//CheckCost
		//ApplyCost
		//BasicStatPickup.Pickup

		public static void BasicStatPickupPickupHook(BasicStatPickup self, PlayerController player)
		{

			FieldInfo _pickedUp = typeof(BasicStatPickup).GetField("m_pickedUp", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo _pickedUpThisRun = typeof(BasicStatPickup).GetField("m_pickedUpThisRun", BindingFlags.NonPublic | BindingFlags.Instance);

			if ((bool)_pickedUp.GetValue(self))
			{
				return;
			}
			if (self.ArmorToGive > 0 && !(bool)_pickedUpThisRun.GetValue(self))
			{
				player.healthHaver.Armor += (float)self.ArmorToGive;
			}
			else if (!(bool)_pickedUpThisRun.GetValue(self) && self.IsMasteryToken && player.AllowZeroHealthState && player.ForceZeroHealthState)
			{
				player.healthHaver.Armor += 1f;
			}
			if (self.ModifiesDodgeRoll)
			{
				player.rollStats.rollDistanceMultiplier *= self.DodgeRollDistanceMultiplier;
				player.rollStats.rollTimeMultiplier *= self.DodgeRollTimeMultiplier;
				player.rollStats.additionalInvulnerabilityFrames += self.AdditionalInvulnerabilityFrames;
			}
			if (!(bool)_pickedUpThisRun.GetValue(self) && self.IsJunk && player.AllowZeroHealthState && player.ForceZeroHealthState)
			{
				StatModifier statModifier = new StatModifier();
				statModifier.statToBoost = PlayerStats.StatType.Damage;
				statModifier.amount = 0.05f;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				player.ownerlessStatModifiers.Add(statModifier);
				player.stats.RecalculateStats(player, false, false);
			}
			if (!(bool)_pickedUpThisRun.GetValue(self) && self.GivesCurrency)
			{
				player.carriedConsumables.Currency += self.CurrencyToGive;
			}
			if (!(bool)_pickedUpThisRun.GetValue(self) && player.AllowZeroHealthState && player.ForceZeroHealthState)
			{
				for (int i = 0; i < self.modifiers.Count; i++)
				{
					if (self.modifiers[i].statToBoost == PlayerStats.StatType.Health && self.modifiers[i].amount > 0f)
					{
						int amountToDrop = Mathf.FloorToInt(self.modifiers[i].amount * (float)UnityEngine.Random.Range(GameManager.Instance.RewardManager.RobotMinCurrencyPerHealthItem, GameManager.Instance.RewardManager.RobotMaxCurrencyPerHealthItem + 1));
						LootEngine.SpawnCurrency(player.CenterPosition, amountToDrop, false);
					}
				}
			}
			self.Pickup(player);
		}
	}
}
