using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace Alexandria.StatAPI
{
    [HarmonyPatch]
    public static class StatAPIManager
    {
		/// <summary>
		/// Returns the ExtendedPlayerStats present on the specified PlayerStats. If not present, adds an ExtendedPlayerStats component to the PlayerStats and returns it.
		/// </summary>
		/// <param name="s">The target PlayerStats</param>
		/// <returns></returns>
		public static ExtendedPlayerStats GetExtComp(this PlayerStats s)
        {
            if (s == null)
            {
                return null;
            }
            return s.gameObject.GetOrAddComponent<ExtendedPlayerStats>();
        }

		[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.RecalculateStatsInternal))]
		[HarmonyPrefix]
		internal static void RecalculateStatsPrefix(PlayerStats __instance, PlayerController owner)
		{
			var ext = __instance.GetExtComp();
			ext.unprocessedHealthDecrement = 0f;
			ext.unprocessedHealthMult = 1f;
			ext.unprocessedHealthExp = 1f;
			var mults = ext.trueMultiplicativeMults = new float[__instance.BaseStatValues.Count];
			var exps = ext.exponentMods = new float[__instance.BaseStatValues.Count];
			for (int j = 0; j < __instance.BaseStatValues.Count; j++)
			{
				mults[j] = 1f;
				exps[j] = 1f;
			}
			foreach (var s in owner.ActiveExtraSynergies)
			{
				AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[s];
				if (advancedSynergyEntry.SynergyIsActive(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer))
				{
					foreach (var mod in advancedSynergyEntry.statModifiers)
					{
						if (mod.modifyType == ModifyMethodE.TrueMultiplicative)
						{
							mults[(int)mod.statToBoost] *= mod.amount;
						}
						else if (mod.modifyType == ModifyMethodE.Exponent)
						{
							exps[(int)mod.statToBoost] *= mod.amount;
						}
					}
				}
			}
			foreach (var mod in owner.ownerlessStatModifiers)
			{
				if (mod.modifyType == ModifyMethodE.TrueMultiplicative)
				{
					mults[(int)mod.statToBoost] *= mod.amount;
				}
				else if (mod.modifyType == ModifyMethodE.Exponent)
				{
					exps[(int)mod.statToBoost] *= mod.amount;
				}
				if (!mod.hasBeenOwnerlessProcessed)
				{
					ext.AdditionalModifierProcessing(mod, __instance);
				}
			}
			foreach (var passive in owner.passiveItems)
			{
				if (passive.passiveStatModifiers != null && passive.passiveStatModifiers.Length > 0)
				{
					foreach (var mod in passive.passiveStatModifiers)
					{
						if (mod != null && !passive.HasBeenStatProcessed)
						{
							ext.AdditionalModifierProcessing(mod, __instance);
						}
					}
				}
				if (passive is BasicStatPickup statpickup)
				{
					foreach (var mod in statpickup.modifiers)
					{
						if (mod != null && !passive.HasBeenStatProcessed)
						{
							ext.AdditionalModifierProcessing(mod, __instance);
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
						if (mod != null && !active.HasBeenStatProcessed)
						{
							ext.AdditionalModifierProcessing(mod, __instance);
						}
					}
				}
				var holder = active.GetComponent<StatHolder>();
				if (holder && (!holder.RequiresPlayerItemActive || active.IsCurrentlyActive))
				{
					foreach (var mod in holder.modifiers)
					{
						if (mod != null && !active.HasBeenStatProcessed)
						{
							ext.AdditionalModifierProcessing(mod, __instance);
						}
					}
				}
            }
        }
/*
        [HarmonyPatch(typeof(PickupObject), nameof(PickupObject.HandlePickupCurseParticles), new Type[0])]
        [HarmonyPrefix]
        internal static bool BetterCurseCheck(PickupObject __instance)
        {
            if (__instance == null || __instance.sprite == null)
            {
                return false;
            }
            if (__instance is Gun g)
            {
                foreach (var mod in g.passiveStatModifiers)
                {
                    if (IsNegativeCurse(mod))
                    {
                        return true;
                    }
                }
            }
            else if (__instance is PlayerItem a)
            {
                foreach (var mod in a.passiveStatModifiers)
                {
                    if (IsNegativeCurse(mod))
                    {
                        return true;
                    }
                }
            }
            else if (__instance is PassiveItem p && p.passiveStatModifiers != null)
            {
                foreach (var mod in p.passiveStatModifiers)
                {
                    if (IsNegativeCurse(mod))
                    {
                        return true;
                    }
                }
            }
            return false;
        }*/

		[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.ApplyStatModifier))]
		[HarmonyPrefix]
		internal static bool IgnoreCustomStatMods(StatModifier modifier)
        {
			return !addedCustomStats.Contains(modifier.statToBoost);
        }

        [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.ApplyStatModifier))]
		[HarmonyPostfix]
		internal static void ProcessTrueMultiplicativeStat(PlayerStats __instance, StatModifier modifier)
		{
            if (modifier.statToBoost > PlayerStats.StatType.MoneyMultiplierFromEnemies)
            {
				return;
            }
			if (modifier.modifyType == ModifyMethodE.TrueMultiplicative)
			{
				var mults = __instance.GetExtComp().trueMultiplicativeMults;
				if (mults != null)
				{
					int statToBoost = (int)modifier.statToBoost;
					if (mults.Length > statToBoost)
					{
						mults[statToBoost] *= modifier.amount;
					}
				}
			}
			else if (modifier.modifyType == ModifyMethodE.Exponent)
			{
				var exps = __instance.GetExtComp().exponentMods;
				if (exps != null)
				{
					int statToBoost = (int)modifier.statToBoost;
					if (exps.Length > statToBoost)
					{
						exps[statToBoost] *= modifier.amount;
					}
				}
			}
			ProcessStatMod?.Invoke(__instance, modifier);
		}

		[HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.RecalculateStatsInternal))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> StatMods(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.LoadsField(allowzerohealth))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, statsext);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
					yield return new CodeInstruction(OpCodes.Callvirt, extrecalculate);
				}
				yield return instruction;
			}
			yield break;
		}

		internal static MethodInfo statsext = typeof(StatAPIManager).GetMethod(nameof(StatAPIManager.GetExtComp), new Type[] { typeof(PlayerStats) });
		internal static MethodInfo extrecalculate = typeof(ExtendedPlayerStats).GetMethod(nameof(ExtendedPlayerStats.InternalRecalculateStats), BindingFlags.NonPublic | BindingFlags.Instance);
		internal static FieldInfo allowzerohealth = typeof(PlayerController).GetField("AllowZeroHealthState");

		/// <summary>
		/// Returns true if the given StatModifier increases curse, returns false otherwise. For exponent modifiers, returns true if the amount is greater than 1.
		/// </summary>
		/// <param name="mod">The stat modifier to check.</param>
		/// <returns></returns>
		public static bool IsNegativeCurse(StatModifier mod)
		{
			if (mod.modifyType == StatModifier.ModifyMethod.MULTIPLICATIVE || mod.modifyType == ModifyMethodE.TrueMultiplicative || mod.modifyType == ModifyMethodE.Exponent)
			{
				return mod.amount > 1f;
			}
			else if(mod.modifyType == StatModifier.ModifyMethod.ADDITIVE)
            {
				return mod.amount > 0f;
            }
			return false;
		}

		/// <summary>
		/// Creates a stat modifier that modifies a custom stat from a mod with the prefix modPrefix and name statName.
		/// </summary>
		/// <param name="modPrefix">The prefix of the mod that adds the stat.</param>
		/// <param name="statName">The name of the stat.</param>
		/// <param name="amount">The modification amount for the stat modifier.</param>
		/// <param name="method">The modify method for the stat modifier.</param>
		/// <returns></returns>
		public static StatModifier CreateCustomStatModifier(string modPrefix, string statName, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
			var s = ETGModCompatibility.ExtendEnum<PlayerStats.StatType>(modPrefix, statName);
			addedCustomStats.Add(s);
			return StatModifier.Create(s, method, amount);
        }

		internal static readonly List<PlayerStats.StatType> addedCustomStats = new();

		/// <summary>
		/// Base values for custom stats. The key is a tuple with the first element being a mod prefix and the second being the name of a stat and the value is the default value. Stats not in this dictionary will have 1 as their default value.
		/// </summary>
		public static readonly Dictionary<Tuple<string, string>, float> baseStatValues = new();
		/// <summary>
		/// An action that will run after all modifiers have been applied to base stats and before starting custom stat calculation.
		/// </summary>
		public static StatModificationDelegate PreCustomStatModification;
		/// <summary>
		/// An action that will run before applying modifiers to custom stats.
		/// </summary>
		public static StatModificationDelegate PreModifiers;
		/// <summary>
		/// An action that will run after applying all multiplicative modifiers to custom stats. Multiplicative modifiers are the first modifiers to be applied.
		/// </summary>
		public static StatModificationDelegate AfterMultiplicative;
		/// <summary>
		/// An action that will run after applying all additive modifiers to custom stats. Additive modifiers are applied second, after multiplicative modifiers.
		/// </summary>
		public static StatModificationDelegate AfterAdditive;
		/// <summary>
		/// An action that will run after applying all true multiplicative modifiers to custom stats. True multiplicative modifiers are applied third, after additive modifiers.
		/// </summary>
		public static StatModificationDelegate AfterTrueMultiplicative;
		/// <summary>
		/// An action that will run after applying all exponent modifiers to custom stats. Exponent modifiers are the last modifiers to be applied.
		/// </summary>
		public static StatModificationDelegate AfterExponent;
		/// <summary>
		/// An action that will run at the end of custom stat calculation, after all modifiers have been applied and healing modifications have been processed.
		/// </summary>
		public static StatModificationDelegate FinalPostProcessing;
		/// <summary>
		/// An action that will run when a stat modifier is processed.
		/// </summary>
		public static Action<PlayerStats, StatModifier> ProcessStatMod;
		/// <summary>
		/// An action that will run when a stat modifier is processed for the first time in addition to ProcessStatMod.
		/// </summary>
		public static Action<PlayerStats, StatModifier> ProcessUnprocessedMod;
		/// <summary>
		/// A delegate used by actions that happen during custom stat calculation.
		/// </summary>
		/// <param name="stats">The PlayerStats that are being recalculated.</param>
		/// <param name="owner">The owner of the PlayerStats.</param>
		/// <param name="values">The custom stat values that can be modified.</param>
		/// <param name="healAmount">The amount by which the owner will be healed at the end of the recalculation. This is mostly used for healing when picking up health modifiers for the first time.</param>
		public delegate void StatModificationDelegate(PlayerStats stats, PlayerController owner, CustomStatValues values, ref float healAmount);
	}
}
