using Alexandria.ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public class ExtendedPlayerComponent : MonoBehaviour
    {
        #region InitAndHooks
        public static void Init()
        {
            playerStartHook = new Hook(
                typeof(PlayerController).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
                typeof(ExtendedPlayerComponent).GetMethod("DoSetup"));
        }
        public static void DoSetup(Action<PlayerController> action, PlayerController player)
        {
            action(player);
            if (player.GetComponent<ExtendedPlayerComponent>() == null) player.gameObject.AddComponent<ExtendedPlayerComponent>();
        }
        
        private static Hook playerStartHook;
        private static Hook activeItemDropHook;
        #endregion

        public PlayerController attachedPlayer;
        private void Start()
        {
            attachedPlayer = base.GetComponent<PlayerController>();
            if (attachedPlayer != null)
            {
                if (CustomActions.OnNewPlayercontrollerSpawned != null) CustomActions.OnNewPlayercontrollerSpawned(attachedPlayer);
            }
        }

        #region Actions
        //Slash Related
        public Action<PlayerController, Vector2, SlashData> PreProcessSlash;
        public Action<PlayerController, Vector2, SlashData> PostProcessSlash;
        public Action<PlayerController, Vector2, SlashData, AIActor> OnSlashHitEnemy;
        //Pickup Based
        public Action<PlayerController, AmmoPickup> OnPickedUpAmmo;
        public Action<PlayerController, KeyBulletPickup> OnPickedUpKey;
        public Action<PlayerController, HealthPickup> OnPickedUpHP;
        public Action<PlayerController, HealthPickup> OnNudgedHP;
        //Other
        public Action<PlayerController> OnBlessedGunChanged;
        public Action<PlayerController, PlayerItem, bool> OnActiveItemPreDrop;
        #endregion

        #region RageHandler
        /// <summary>
        /// Triggers a rage effect (like the Enraging Photo) for the specified duration. Rage gives double damage, and comes with associated visual effects.
        /// </summary>
        /// <param name="dur">The length of the desired rage.</param>
        /// <param name="resetExisting">If true, the given rage duration will override the duration of existing rage (if the player is already enraged) instead of adding to it. Can be used to cancel rage by setting duration to zero.</param>
        public void Enrage(float dur, bool resetExisting)
        {
            if (remainingRageTime > 0)
            {
                if (resetExisting) { remainingRageTime = dur; }
                else { remainingRageTime += dur; }
            }
            else if (dur > 0) attachedPlayer.StartCoroutine(HandleRageDur(dur));
        }
        private IEnumerator HandleRageDur(float dur)
        {
            remainingRageTime = dur;
            this.instanceRageVFX = attachedPlayer.PlayEffectOnActor(RageVFX, new Vector3(0f, 1.375f, 0f), true, true, false);
            attachedPlayer.ownerlessStatModifiers.Add(DoubleDamageStatMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer, true, false);

            float elapsed = 0f;
            float particleCounter = 0f;

            while (remainingRageTime > 0)
            {
                remainingRageTime -= BraveTime.DeltaTime;
                elapsed += BraveTime.DeltaTime;
                attachedPlayer.baseFlatColorOverride = this.flatRageColourOverride.WithAlpha(Mathf.Lerp(this.flatRageColourOverride.a, 0f, Mathf.Clamp01(elapsed - (remainingRageTime - 1f))));
                if (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.LOW && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && attachedPlayer && attachedPlayer.IsVisible && !attachedPlayer.IsFalling)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (instanceRageVFX && elapsed > 1f)
                    {
                        instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
                        instanceRageVFX = null;
                    }
                    if (particleCounter > 1f)
                    {
                        int num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, attachedPlayer.sprite.WorldBottomLeft.ToVector3ZisY(0f), attachedPlayer.sprite.WorldTopRight.ToVector3ZisY(0f), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }
                yield return null;
            }
            if (this.instanceRageVFX) this.instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
            attachedPlayer.ownerlessStatModifiers.Remove(DoubleDamageStatMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer, true, false);
        }

        private float remainingRageTime;
        private static GameObject RageVFX = PickupObjectDatabase.GetById(353).GetComponent<RagePassiveItem>().OverheadVFX.gameObject;
        private GameObject instanceRageVFX;
        private static StatModifier DoubleDamageStatMod;
        private Color flatRageColourOverride = new Color(0.5f, 0f, 0f, 0.75f);
        #endregion

        #region TimedStatHandler
        public void DoTimedStatModifier(PlayerStats.StatType statToBoost, float amount, float time, StatModifier.ModifyMethod modifyMethod = StatModifier.ModifyMethod.MULTIPLICATIVE)
        {
            attachedPlayer.StartCoroutine(HandleTimedStatModifier(statToBoost, amount, time, modifyMethod));
        }
        private IEnumerator HandleTimedStatModifier(PlayerStats.StatType statToBoost, float amount, float dur, StatModifier.ModifyMethod method)
        {
            StatModifier timedMod = new StatModifier()
            {
                amount = amount,
                statToBoost = statToBoost,
                modifyType = method,
            };
            attachedPlayer.ownerlessStatModifiers.Add(timedMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer);
            yield return new WaitForSeconds(dur);
            attachedPlayer.ownerlessStatModifiers.Remove(timedMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer);
            yield break;
        }
        #endregion

        #region IFrameHandler
        private float remainingInvulnerabilityTime;

        /// <summary>
        /// Triggers blinking invulnerability frames for the specified duration. Does not trigger if the player is already invulnerable from basegame I-frames.
        /// </summary>
        /// <param name="incorporealityTime">The length of the desired invulnerability.</param>
        /// <param name="resetExisting">If true, the given incorporeality duration will override the duration of existing incorporeality (if the player is already invulnerable) instead of adding to it. Can be used to cancel I-frames by setting duration to zero.</param>
        public void TriggerInvulnerableFrames(float incorporealityTime, bool resetExisting = false)
        {
            if (attachedPlayer.healthHaver.m_isIncorporeal && !isLocallyIncorporeal) return;

            if (remainingInvulnerabilityTime > 0)
            {
                if (resetExisting) remainingInvulnerabilityTime = incorporealityTime;
                else remainingInvulnerabilityTime += incorporealityTime;
            }
            else if (incorporealityTime > 0) attachedPlayer.StartCoroutine(IncorporealityOnHit(incorporealityTime));
        }
        private IEnumerator IncorporealityOnHit(float incorporealityTime)
        {
            int enemyMask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
            attachedPlayer.specRigidbody.AddCollisionLayerIgnoreOverride(enemyMask);
            attachedPlayer.healthHaver.IsVulnerable = false;
            attachedPlayer.healthHaver.m_isIncorporeal = true;
            isLocallyIncorporeal = true;
            yield return null;

            float subtimer = 0f;
            while (incorporealityTime > 0)
            {
                while (incorporealityTime > 0)
                {
                    remainingInvulnerabilityTime -= BraveTime.DeltaTime;
                    subtimer += BraveTime.DeltaTime;
                    if (subtimer > 0.12f)
                    {
                        attachedPlayer.IsVisible = false;
                        subtimer -= 0.12f;
                        break;
                    }
                    yield return null;
                }
                while (incorporealityTime > 0)
                {
                    remainingInvulnerabilityTime -= BraveTime.DeltaTime;
                    subtimer += BraveTime.DeltaTime;
                    if (subtimer > 0.12f)
                    {
                        attachedPlayer.IsVisible = true;
                        subtimer -= 0.12f;
                        break;
                    }
                    yield return null;
                }
            }

            int mask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
            attachedPlayer.IsVisible = true;
            attachedPlayer.healthHaver.IsVulnerable = true;
            attachedPlayer.specRigidbody.RemoveCollisionLayerIgnoreOverride(mask);
            attachedPlayer.healthHaver.m_isIncorporeal = false;
            isLocallyIncorporeal = false;
            yield break;
        }
        private bool isLocallyIncorporeal;
        #endregion
    }
}
