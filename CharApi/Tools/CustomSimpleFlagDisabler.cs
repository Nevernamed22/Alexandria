//using SaveAPI;
using System;
using System.Collections;
using UnityEngine;

namespace CustomCharacters
{
	public class CustomSimpleFlagDisabler : MonoBehaviour
	{
		public CustomSimpleFlagDisabler()
		{
			UseNumberOfAttempts = true;
			this.minStatValue = 1;
		}

		private IEnumerator Start()
		{
			while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
			{
				yield return null;
			}
			if (!string.IsNullOrEmpty(this.ChangeSpriteInstead))
			{
				this.UsesStatComparisonInstead = true;
			}
			if (this.DisableIfNotFoyer)
			{
				if (!GameManager.Instance.IsFoyer)
				{
					this.Disable();
				}
				yield break;
			}
			if (this.UsesStatComparisonInstead && this.transform.parent != null && this.transform.parent.name.Contains("Livery") && GameStatsManager.Instance.AnyPastBeaten() && this.UseNumberOfAttempts)
			{
				yield break;
			}
			if (this.UsesStatComparisonInstead)
			{
				//if (AdvancedGameStatsManager.Instance.GetPlayerStatValue(this.RelevantStat) < (float)this.minStatValue || (UseNumberOfAttempts && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS) < (float)this.minStatValue))
				//{
				//	this.Disable();
				//}
			}
			//else if (this.FlagToCheckFor != CustomDungeonFlags.NONE && AdvancedGameStatsManager.Instance.GetFlag(this.FlagToCheckFor) == this.DisableOnThisFlagValue)
			//{
			//	this.Disable();
			//}
			yield break;
		}

		private void Update()
		{
			if (this.EnableOnGunGameMode && !GameManager.Instance.IsSelectingCharacter && GameManager.Instance.PrimaryPlayer != null && (GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns || ChallengeManager.CHALLENGE_MODE_ACTIVE))
			{
				SpeculativeRigidbody component = base.GetComponent<SpeculativeRigidbody>();
				if (!component.enabled)
				{
					component.enabled = true;
					component.Reinitialize();
					base.GetComponent<MeshRenderer>().enabled = true;
				}
			}
		}

		private void Disable()
		{
			SpeculativeRigidbody component = base.GetComponent<SpeculativeRigidbody>();
			if (!string.IsNullOrEmpty(this.ChangeSpriteInstead))
			{
				base.GetComponent<tk2dBaseSprite>().SetSprite(this.ChangeSpriteInstead);
				if (component)
				{
					component.Reinitialize();
				}
			}
			else
			{
				if (component)
				{
					component.enabled = false;
				}
				if (!this.EnableOnGunGameMode)
				{
					base.gameObject.SetActive(false);
				}
				else
				{
					base.GetComponent<MeshRenderer>().enabled = false;
				}
			}
		}

		[LongEnum]
		public CustomDungeonFlags FlagToCheckFor;
		public bool DisableOnThisFlagValue;
		public bool UsesStatComparisonInstead;
		public CustomTrackedStats RelevantStat;
		public int minStatValue;
		public string ChangeSpriteInstead;
		public bool EnableOnGunGameMode;
		public bool DisableIfNotFoyer;
		public bool UseNumberOfAttempts;
	}
}