using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.StatAPI
{
    public class ExtendedPlayerStats : MonoBehaviour
    {
		private void Awake()
		{
			if (baseValueKeys == null)
			{
				baseValueKeys = new();
				baseValueValues = new();
				foreach (var kvp in StatAPIManager.baseStatValues)
				{
					baseValueKeys.Add($"{kvp.Key.First}.{kvp.Key.Second}");
					baseValueValues.Add(kvp.Value);
				}
			}
		}

		internal void InternalRecalculateStats(PlayerController owner, PlayerStats ogstats, ref float healAmount)
		{

			if (statKeys == null)
			{
				statKeys = new();
				statValues = new();
			}
            statValues.Clear();
            statKeys.Clear();
			if (baseValueKeys == null){this.Awake();} // Force Reboot
            var vals = new CustomStatValues(statKeys, statValues);
            for (int i = 0; i < baseValueKeys.Count; i++)
			{
                vals.SetWithoutPrefix(baseValueKeys[i], baseValueValues[i]);
            }
            StatAPIManager.PreCustomStatModification?.Invoke(ogstats, owner, vals, ref healAmount);
			Dictionary<string, float> additive = new();
			Dictionary<string, float> multiplicative = new();
			Dictionary<string, float> tmultiplicative = new();
			Dictionary<string, float> exponent = new();
            foreach (var s in owner.ActiveExtraSynergies)
			{
				if (s >= GameManager.Instance.SynergyManager.synergies.Length)
				{
					continue;
				}
				var entry = GameManager.Instance.SynergyManager.synergies[s];
				if (entry == null || entry.statModifiers == null || !entry.SynergyIsActive(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer))
				{
					continue;
				}
				foreach (var mod in entry.statModifiers)
				{
					if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
					{
						ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
					}
				}
			}
            foreach (var mod in owner.ownerlessStatModifiers)
			{
				if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
				{
					ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
				}
			}
            foreach (var passive in owner.passiveItems)
			{
				if (passive.passiveStatModifiers != null && passive.passiveStatModifiers.Length > 0)
				{
					foreach (var mod in passive.passiveStatModifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
				if (passive is BasicStatPickup statpickup)
				{
					foreach (var mod in statpickup.modifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
				if (passive is CoopPassiveItem coop && (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || (GameManager.Instance.PrimaryPlayer.healthHaver && GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) || owner.HasActiveBonusSynergy(CustomSynergyType.THE_TRUE_HERO)))
				{
					foreach (var mod in coop.modifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
			}
            if (owner.inventory != null && owner.inventory.AllGuns != null)
			{
				if (owner.inventory.CurrentGun != null && owner.inventory.CurrentGun.currentGunStatModifiers != null && owner.inventory.CurrentGun.currentGunStatModifiers.Length > 0)
				{
					foreach (var mod in owner.inventory.CurrentGun.currentGunStatModifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
				foreach (var g in owner.inventory.AllGuns)
				{
					if (g && g.passiveStatModifiers != null && g.passiveStatModifiers.Length > 0)
					{
						foreach (var mod in g.passiveStatModifiers)
						{
							if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
							{
								ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
							}
						}
					}
				}
			}
            foreach (var active in owner.activeItems)
			{
				if (active.passiveStatModifiers != null && active.passiveStatModifiers.Length > 0)
				{
					foreach (var mod in active.passiveStatModifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
				var holder = active.GetComponent<StatHolder>();
				if (holder && (!holder.RequiresPlayerItemActive || active.IsCurrentlyActive))
				{
					foreach (var mod in holder.modifiers)
					{
						if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
						{
							ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
						}
					}
				}
			}
            var currentItem = owner.CurrentItem;
			if (currentItem && currentItem is ActiveBasicStatItem activestat && currentItem.IsActive)
			{
				foreach (var mod in activestat.modifiers)
				{
					if (mod != null && StatAPIManager.addedCustomStats.Contains(mod.statToBoost))
					{
						ProcessMod(mod, additive, multiplicative, tmultiplicative, exponent);
					}
				}
			}
            StatAPIManager.PreModifiers?.Invoke(ogstats, owner, vals, ref healAmount);
			foreach (var kvp in multiplicative)
			{
				vals.SetWithoutPrefix(kvp.Key, vals.GetWithoutPrefix(kvp.Key) * kvp.Value);
			}
            StatAPIManager.AfterMultiplicative?.Invoke(ogstats, owner, vals, ref healAmount);
			foreach (var kvp in additive)
			{
				vals.SetWithoutPrefix(kvp.Key, vals.GetWithoutPrefix(kvp.Key) + kvp.Value);
			}
            StatAPIManager.AfterAdditive?.Invoke(ogstats, owner, vals, ref healAmount);
			foreach (var kvp in tmultiplicative)
			{
				vals.SetWithoutPrefix(kvp.Key, vals.GetWithoutPrefix(kvp.Key) * kvp.Value);
			}
            StatAPIManager.AfterTrueMultiplicative?.Invoke(ogstats, owner, vals, ref healAmount);
			foreach (var kvp in exponent)
			{
				vals.SetWithoutPrefix(kvp.Key, Mathf.Pow(vals.GetWithoutPrefix(kvp.Key), kvp.Value));
            }
            StatAPIManager.AfterExponent?.Invoke(ogstats, owner, vals, ref healAmount);
			var mults = trueMultiplicativeMults;
			var exps = exponentMods;
			healAmount -= unprocessedHealthDecrement;
			var oghealth = ogstats.StatValues[(int)PlayerStats.StatType.Health];
			if (mults != null)
			{
				for (int i = 0; i < ogstats.StatValues.Count && i < mults.Length; i++)
				{
					ogstats.StatValues[i] *= mults[i];
					if (i == (int)PlayerStats.StatType.Health)
					{
						healAmount *= mults[i];
					}
				}
			}
            healAmount += oghealth * (unprocessedHealthMult - 1) + Mathf.Max(Mathf.Pow(oghealth, unprocessedHealthExp) - oghealth, 0f);
			if (exps != null)
			{
				for (int i = 0; i < ogstats.StatValues.Count && i < exps.Length; i++)
				{
					ogstats.StatValues[i] = Mathf.Pow(ogstats.StatValues[i], exps[i]);
				}
			}

            StatAPIManager.FinalPostProcessing?.Invoke(ogstats, owner, vals, ref healAmount);
			ogstats.StatValues[(int)PlayerStats.StatType.Health] = Mathf.Round(ogstats.StatValues[(int)PlayerStats.StatType.Health] * 2) / 2;
			healAmount = Mathf.Round(healAmount * 2) / 2;
        }

        /// <summary>
        /// Returns the current value of a custom stat from a mod with the prefix modPrefix and name statName.
        /// </summary>
        /// <param name="modPrefix">The prefix of the mod that adds the stat.</param>
        /// <param name="statName">The name of the stat.</param>
        /// <returns></returns>
        public float GetStatValue(string modPrefix, string statName)
		{
			if (!statKeys.Contains($"{modPrefix}.{statName}"))
			{
				return 1f;
			}
			return statValues[statKeys.IndexOf($"{modPrefix}.{statName}")];
		}

		/// <summary>
		/// Returns the base value of a custom stat from a mod with the prefix modPrefix and name statName.
		/// </summary>
		/// <param name="modPrefix">The prefix of the mod that adds the stat.</param>
		/// <param name="statName">The name of the stat.</param>
		/// <returns></returns>
		public float GetBaseStatValue(string modPrefix, string statName)
		{
			if (!baseValueKeys.Contains($"{modPrefix}.{statName}"))
			{
				baseValueKeys.Add($"{modPrefix}.{statName}");
				baseValueValues.Add(1f);
			}
			return baseValueValues[baseValueKeys.IndexOf($"{modPrefix}.{statName}")];
		}

		/// <summary>
		/// Sets the base value of a custom stat from a mod with the prefix modPrefix and name statName.
		/// </summary>
		/// <param name="modPrefix">The prefix of the mod that adds the stat.</param>
		/// <param name="statName">The name of the stat.</param>
		/// <param name="value">The new base value for the stat.</param>
		public void SetBaseStatValue(string modPrefix, string statName, float value)
		{
			if (!baseValueKeys.Contains($"{modPrefix}.{statName}"))
			{
				baseValueKeys.Add($"{modPrefix}.{statName}");
				baseValueValues.Add(value);
			}
			else
			{
				baseValueValues[baseValueKeys.IndexOf($"{modPrefix}.{statName}")] = value;
			}
		}

		internal void ProcessMod(StatModifier mod, Dictionary<string, float> additive, Dictionary<string, float> multiplicative, Dictionary<string, float> tmultiplicative, Dictionary<string, float> exponent)
		{
			var realkey = mod.statToBoost.ToString();
			if (mod.modifyType == StatModifier.ModifyMethod.ADDITIVE)
			{
				if (additive.ContainsKey(realkey))
				{
					additive[realkey] += mod.amount;
				}
				else
				{
					additive[realkey] = mod.amount;
				}
			}
			else if (mod.modifyType == StatModifier.ModifyMethod.MULTIPLICATIVE)
			{
				if (multiplicative.ContainsKey(realkey))
				{
					multiplicative[realkey] *= mod.amount;
				}
				else
				{
					multiplicative[realkey] = mod.amount;
				}
			}
			else if (mod.modifyType == ModifyMethodE.TrueMultiplicative)
			{
				if (tmultiplicative.ContainsKey(realkey))
				{
					tmultiplicative[realkey] *= mod.amount;
				}
				else
				{
					tmultiplicative[realkey] = mod.amount;
				}
			}
			else if (mod.modifyType == ModifyMethodE.Exponent)
			{
				if (exponent.ContainsKey(realkey))
				{
					exponent[realkey] *= mod.amount;
				}
				else
				{
					exponent[realkey] = mod.amount;
				}
			}
		}

		internal void AdditionalModifierProcessing(StatModifier mod, PlayerStats stats)
		{
			if (mod.statToBoost == PlayerStats.StatType.Health && (mod.modifyType == ModifyMethodE.TrueMultiplicative || mod.modifyType == ModifyMethodE.Exponent))
			{
				unprocessedHealthDecrement += mod.amount;
				if(mod.amount > 1f && mod.modifyType == ModifyMethodE.TrueMultiplicative)
				{
					unprocessedHealthMult *= mod.amount;
				}
				if(mod.modifyType == ModifyMethodE.Exponent)
                {
					unprocessedHealthExp *= mod.amount;
                }
			}
			StatAPIManager.ProcessUnprocessedMod?.Invoke(stats, mod);
		}

		internal float[] trueMultiplicativeMults;
        internal float unprocessedHealthMult;
		internal float unprocessedHealthExp;
		internal float unprocessedHealthDecrement;
        internal float[] exponentMods;
        internal List<string> statKeys;
        internal List<float> statValues;
        internal List<string> baseValueKeys;
        internal List<float> baseValueValues;
    }
}
