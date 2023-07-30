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
        public ExtendedPlayerComponent()
        {
            playerHasExperiencedRunStartHook = false;
        }
        public static void Init()
        {
            new Hook(
                typeof(PlayerController).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
                typeof(ExtendedPlayerComponent).GetMethod("DoSetup"));
        }
        public static void DoSetup(Action<PlayerController> action, PlayerController player)
        {
            action(player);
            if (player != null)
            {
                if (CustomActions.OnNewPlayercontrollerSpawned != null) CustomActions.OnNewPlayercontrollerSpawned(player);
                if (player.GetComponent<ExtendedPlayerComponent>() == null) player.gameObject.AddComponent<ExtendedPlayerComponent>();
            }
        }

        #endregion

        public PlayerController attachedPlayer;
        private void Start()
        {
            attachedPlayer = base.GetComponent<PlayerController>();
            if (DoubleDamageStatMod == null)
            {
                DoubleDamageStatMod = new StatModifier()
                {
                    statToBoost = PlayerStats.StatType.Damage,
                    amount = 2,
                    modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,                
                };
            }
        }

        #region Actions
        //Slash Related
        /// <summary>
        /// Runs just before a melee slash belonging to the attached player occurs, containing information about the slash and facilitating modification.
        /// </summary>
        public Action<PlayerController, Vector2, SlashData> PreProcessSlash;
        /// <summary>
        /// Runs after a melee slash belonging to the attached player occurs.
        /// </summary>
        public Action<PlayerController, Vector2, SlashData> PostProcessSlash;
        /// <summary>
        /// Runs when an AIActor is hit by a melee slash belonging to the attached player. Ideal for transferring bullet effects.
        /// </summary>
        public Action<PlayerController, Vector2, SlashData, AIActor> OnSlashHitEnemy;

        //Pickup Based
        /// <summary>
        /// Runs whenever the attached player collects an ammo box.
        /// </summary>
        public Action<PlayerController, AmmoPickup> OnPickedUpAmmo;
        /// <summary>
        /// Runs whenever the attached player collects a key pickup. Note, Rat Keys count as Keys.
        /// </summary>
        public Action<PlayerController, KeyBulletPickup> OnPickedUpKey;
        /// <summary>
        /// Runs whenever the attached player collects HP. Note, Armor is counted as HP.
        /// </summary>
        public Action<PlayerController, HealthPickup> OnPickedUpHP;
        /// <summary>
        /// Runs whenever the attached player touches HP. Occurs before pickup, and will still run if the player nudges a heart pickup at full HP.
        /// </summary>
        public Action<PlayerController, HealthPickup> OnNudgedHP;
        /// <summary>
        /// Runs whenever the attached player collects a blank.
        /// </summary>
        public Action<SilencerItem, PlayerController> OnPickedUpBlank;
        ///// <summary>
        ///// Runs whenever the attached player collects any kind of pickup object.
        ///// Runs AFTER pickup, so be aware of that.
        ///// </summary>
        //public Action<PickupObject, PlayerController> OnCollectedPickup;
        /// <summary>
        /// Runs whenever the attached player drops a passive item.
        /// </summary>
        public Action<PassiveItem, PlayerController, DebrisObject> OnDroppedPassiveItem;

        //Companion Based
        /// <summary>
        /// Runs whenever a companion belonging to the attached player spawns a projectile.
        /// </summary>
        public Action<CompanionController, Projectile> OnCompanionSpawnedBullet;
        /// <summary>
        /// Runs whenever a QueryCompanionStats is called. Useful for modifying companion stats not accessible via OnCompanionSpawnedBullet.
        /// </summary>
        public Action<GameObject, QueriedCompanionStats, PlayerController> OnCompanionStatsQueried;

        //Other
        /// <summary>
        /// Runs whenever the player's gun changes in Blessed Mode.
        /// </summary>
        public Action<PlayerController> OnBlessedGunChanged;
        /// <summary>
        /// Runs just before the attached player drops their active item, for any reason.
        /// </summary>
        public Action<PlayerController, PlayerItem, bool> OnActiveItemPreDrop;
        /// <summary>
        /// Runs whenever a BlankModificationItem belonging to the attached player is processed after a blank.
        /// </summary>
        public Action<PlayerController, SilencerInstance, Vector2, BlankModificationItem> OnBlankModificationItemProcessed;
        /// <summary>
        /// Runs whenever a PlayerOrbital belonging to the attached player is initialised.
        /// </summary>
        public Action<PlayerController, PlayerOrbital> OnNewOrbitalInitialised;

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
            remainingInvulnerabilityTime = incorporealityTime;
            yield return null;

            float subtimer = 0f;
            while (remainingInvulnerabilityTime > 0)
            {
                while (remainingInvulnerabilityTime > 0)
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
                while (remainingInvulnerabilityTime > 0)
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

        #region Queries
        public QueriedCompanionStats QueryCompanionStats(GameObject objectQueried,
            float damage,
            float firerate,
            float range,
            float speed,
            float shotspeed,
            float accuracy,
            float knockback,
            float bossdamage,
            bool doVanillaModifications = true)
        {
            QueriedCompanionStats query = new QueriedCompanionStats()
            {
                initialDamage = damage,
                modifiedDamage = damage,
                initialFirerate = firerate,
                modifiedFirerate = firerate,
                initialRange = range,
                modifiedRange = range,
                initialSpeed = speed,
                modifiedSpeed = speed,
                initialShotSpeed = shotspeed,
                modifiedShotSpeed = shotspeed,
                initialAccuracy = accuracy,
                modifiedAccuracy = accuracy,
                initialKnockback = knockback,
                modifiedKnockback = knockback,
                initialBossDamage = bossdamage,
                modifiedBossDamage = bossdamage
            };
            if (OnCompanionStatsQueried != null) OnCompanionStatsQueried(objectQueried, query, attachedPlayer);
            if (doVanillaModifications)
            {
                if (PassiveItem.IsFlagSetForCharacter(attachedPlayer, typeof(BattleStandardItem))) query.modifiedDamage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
                if (attachedPlayer.CurrentGun && attachedPlayer.CurrentGun.LuteCompanionBuffActive) query.modifiedDamage *= 2;
            }
            return query;
        }

        public class QueriedCompanionStats : EventArgs
        {
            public float initialDamage;
            public float modifiedDamage;

            public float initialFirerate;
            public float modifiedFirerate;

            public float initialRange;
            public float modifiedRange;

            public float initialSpeed;
            public float modifiedSpeed;

            public float initialShotSpeed;
            public float modifiedShotSpeed;

            public float initialAccuracy;
            public float modifiedAccuracy;

            public float initialKnockback;
            public float modifiedKnockback;

            public float initialBossDamage;
            public float modifiedBossDamage;
        }
        #endregion

        public bool playerHasExperiencedRunStartHook;
    }
}
