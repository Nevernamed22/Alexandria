using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    public class AdvancedHoveringGunSynergyProcessor : MonoBehaviour
    {
        public void Awake()
        {
            m_gun = GetComponent<Gun>();
        }
        public bool shouldHaveGunsLastFrame;
        public PlayerController cachedPlayer;
        public void Update()
        {
            if (m_gun)
            {
                if (cachedPlayer != m_gun.GunPlayerOwner())
                {
                    RelinkActions(cachedPlayer, m_gun.GunPlayerOwner());
                    cachedPlayer = m_gun.GunPlayerOwner();
                }
                if (Trigger == TriggerStyle.CONSTANT)
                {
                    bool shouldHaveGunsThisFrame = ShouldHaveGuns;
                    if (shouldHaveGunsLastFrame != shouldHaveGunsThisFrame)
                    {
                        if (shouldHaveGunsLastFrame) { EraseGuns(); }
                        else { SpawnGuns(); }
                        shouldHaveGunsLastFrame = shouldHaveGunsThisFrame;
                    }
                }
            }
        }
        public bool ShouldHaveGuns
        {
            get
            {
                return cachedPlayer != null && m_gun != null && (string.IsNullOrEmpty(RequiredSynergy) || cachedPlayer.PlayerHasActiveSynergy(RequiredSynergy)) && (cachedPlayer.CurrentGun == m_gun || !requiresBaseGunInHand);
            }
        }
        public void SpawnGuns()
        {
            if (currentHoveringGuns.Count > 0) { EraseGuns(); }
            if (!cachedPlayer) { return; }
            for (int i = 0; i < IDsToSpawn.Length; i++)
            {
                if (!requiresTargetGunInInventory || cachedPlayer.inventory.AllGuns.Find(x => x.PickupObjectId == IDsToSpawn[i]) != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ResourceCache.Acquire("Global Prefabs/HoveringGun") as GameObject, cachedPlayer.CenterPosition.ToVector3ZisY(0f), Quaternion.identity);
                    gameObject.transform.parent = cachedPlayer.transform;

                    HoveringGunController controller = gameObject.GetComponent<HoveringGunController>();
                    controller.ShootAudioEvent = this.ShootAudioEvent;
                    controller.OnEveryShotAudioEvent = this.OnEveryShotAudioEvent;
                    controller.FinishedShootingAudioEvent = this.FinishedShootingAudioEvent;
                    controller.ConsumesTargetGunAmmo = ChanceToConsumeTargetGunAmmo > 0;
                    controller.ChanceToConsumeTargetGunAmmo = this.ChanceToConsumeTargetGunAmmo;
                    controller.Position = this.PositionType;
                    controller.Aim = this.AimType;
                    controller.Trigger = this.FireType;


                    controller.CooldownTime = this.FireCooldown;


                    controller.OnlyOnEmptyReload = this.OnlyOnEmptyReload;


                    Gun gun = null;
                    int num = IDsToSpawn[i];
                    for (int j = 0; j < cachedPlayer.inventory.AllGuns.Count; j++)
                    {
                        if (cachedPlayer.inventory.AllGuns[j].PickupObjectId == num) { gun = cachedPlayer.inventory.AllGuns[j]; }
                    }
                    if (!gun) { gun = (PickupObjectDatabase.Instance.InternalGetById(num) as Gun); }

                    if (fireDelayBasedOnGun && gun != null) { controller.CooldownTime = GetProperShootingSpeed(gun); }
                    if (fireDelayBenefitsFromPlayerFirerate) { controller.CooldownTime /= cachedPlayer.stats.GetStatValue(PlayerStats.StatType.RateOfFire); }
                    controller.ShootDuration = (gun.DefaultModule.shootStyle == ProjectileModule.ShootStyle.Beam) ? BeamFireDuration : -1;

                    controller.Initialize(gun, cachedPlayer);
                    currentHoveringGuns.Add(controller);
                }
            }
        }
        public void EraseGuns()
        {
            for (int i = currentHoveringGuns.Count - 1; i >= 0; i--)
            {
                if (currentHoveringGuns[i] != null) { UnityEngine.Object.Destroy(currentHoveringGuns[i].gameObject); }
            }
            currentHoveringGuns.Clear();
        }
        public IEnumerator HandleTimedDuration()
        {
            m_currentlyActive = true;

            SpawnGuns();

            float remainingTime = TriggerDuration;
            while (remainingTime > 0)
            {
                remainingTime -= BraveTime.DeltaTime;
                if (m_resetTimer)
                {
                    if (TriggerStacking == TriggerStackingMode.RESET) { remainingTime = TriggerDuration; }
                    else if (TriggerStacking == TriggerStackingMode.STACK) { remainingTime += TriggerDuration; }
                    m_resetTimer = false;
                }
                yield return null;
            }

            EraseGuns();

            m_currentlyActive = false;
            yield break;
        }
        public bool m_currentlyActive = false;
        public bool m_resetTimer = false;
        private void OnDamaged(PlayerController player)
        {
            if ((string.IsNullOrEmpty(RequiredSynergy) || player.PlayerHasActiveSynergy(RequiredSynergy)) && (!requiresBaseGunInHand || player.CurrentGun == m_gun) && UnityEngine.Random.value < chanceToSpawnOnTrigger)
            {
                if (!m_currentlyActive)
                {
                    StartCoroutine(HandleTimedDuration());
                }
                else if (TriggerStacking != TriggerStackingMode.IGNORE) { m_resetTimer = true; }
            }
        }
        private void OnActiveUsed(PlayerController player, PlayerItem item)
        {
            if ((string.IsNullOrEmpty(RequiredSynergy) || player.PlayerHasActiveSynergy(RequiredSynergy)) && (!requiresBaseGunInHand || player.CurrentGun == m_gun) && UnityEngine.Random.value < chanceToSpawnOnTrigger && (reqActiveItemID == -1 || reqActiveItemID == item.PickupObjectId))
            {
                if (!m_currentlyActive)
                {
                    StartCoroutine(HandleTimedDuration());
                }
                else if (TriggerStacking != TriggerStackingMode.IGNORE) { m_resetTimer = true; }
            }
        }
        private void OnRolled(PlayerController player, Vector2 vec)
        {
            if ((string.IsNullOrEmpty(RequiredSynergy) || player.PlayerHasActiveSynergy(RequiredSynergy)) && (!requiresBaseGunInHand || player.CurrentGun == m_gun) && UnityEngine.Random.value < chanceToSpawnOnTrigger)
            {
                if (!m_currentlyActive)
                {
                    StartCoroutine(HandleTimedDuration());
                }
                else if (TriggerStacking != TriggerStackingMode.IGNORE) { m_resetTimer = true; }
            }
        }
        private void OnBlanked(PlayerController player, int remainingBlanks)
        {
            if ((string.IsNullOrEmpty(RequiredSynergy) || player.PlayerHasActiveSynergy(RequiredSynergy)) && (!requiresBaseGunInHand || player.CurrentGun == m_gun) && UnityEngine.Random.value < chanceToSpawnOnTrigger)
            {
                if (!m_currentlyActive)
                {
                    StartCoroutine(HandleTimedDuration());
                }
                else if (TriggerStacking != TriggerStackingMode.IGNORE) { m_resetTimer = true; }
            }
        }
        public void RelinkActions(PlayerController old, PlayerController target)
        {
            if (old)
            {
                old.OnReceivedDamage -= OnDamaged;
                old.OnUsedPlayerItem -= OnActiveUsed;
                old.OnRollStarted -= OnRolled;
                old.OnUsedBlank -= OnBlanked;
            }
            if (target)
            {
                if (Trigger == TriggerStyle.ON_DAMAGE) { target.OnReceivedDamage += OnDamaged; }
                else if (Trigger == TriggerStyle.ON_ACTIVE_ITEM) { target.OnUsedPlayerItem += OnActiveUsed; }
                else if (Trigger == TriggerStyle.ON_DODGE_ROLL) { target.OnRollStarted += OnRolled; }
                else if (Trigger == TriggerStyle.ON_BLANKED) { target.OnUsedBlank += OnBlanked; }
            }
        }
        private void OnEnable()
        {
            m_currentlyActive = false;
            shouldHaveGunsLastFrame = false;
        }
        private void OnDisable()
        {
            EraseGuns();
        }
        public void OnDestroy()
        {
            RelinkActions(cachedPlayer, null);
            EraseGuns();
        }

        /// <summary>
        /// Returns the fire delay of the gun. If the gun is Charge-style, adds the charge time between shots.
        /// If the gun has one bullet in a clip, includes the reload time.
        /// </summary>
        /// <param name="gun">The gun to be examined.</param>
        public static float GetProperShootingSpeed(Gun gun)
        {
            float start = gun.DefaultModule.cooldownTime;
            if (gun.DefaultModule.shootStyle == ProjectileModule.ShootStyle.Charged)
            {
                if (gun.DefaultModule.chargeProjectiles != null)
                {
                    start += gun.DefaultModule.chargeProjectiles[0].ChargeTime;
                }
            }
            if (gun.DefaultModule.numberOfShotsInClip <= 1)
            {
                start += gun.reloadTime;
            }
            return start;
        }

        /// <summary>
        /// An array of gun IDs that will be spawned when the orbital synergy is activated.
        /// </summary>
        public int[] IDsToSpawn;
        /// <summary>
        /// The synergy required to activate the hovering gun. If left null or empty, no synergy will be required.
        /// </summary>
        public string RequiredSynergy;
        /// <summary>
        /// A list of currently active hovering guns spawned by the synergy controller.
        /// </summary>
        public List<HoveringGunController> currentHoveringGuns = new List<HoveringGunController>();
        /// <summary>
        /// Controls where the hovering guns hover. If OVERHEAD, the gun will appear above the owner. If CIRCULAR, the gun will orbit the player.
        /// </summary>
        public HoveringGunController.HoverPosition PositionType;
        /// <summary>
        /// Controls how the hovering gun will aim. 
        /// If set to NEAREST_ENEMY, the gun will automatically point at the closest target.
        /// If set to PLAYER_AIM, the gun will match the owner's aim direction.
        /// </summary>
        public HoveringGunController.AimType AimType;
        /// <summary>
        /// Controls when the hovering gun will attack. Not to be confused with firerate.
        /// If set to ON_RELOAD, the gun will attack when the player reloads. 
        /// If set to ON_COOLDOWN, the gun will attack continuously.
        /// If set to ON_DODGED_BULLET, the gun will attack when the player dodges a bullet.
        /// If set to ON_FIRED_GUN, the gun will attack when the player presses the fire button.
        /// </summary>
        public HoveringGunController.FireType FireType;
        /// <summary>
        /// If true, orbiting guns will only spawn if the Gun ID they're based on is actually present in the player's inventory.
        /// </summary>
        public bool requiresTargetGunInInventory;
        /// <summary>
        /// If true, orbiting guns will only be active when the player is actively holding the gun that this component is attached to.
        /// </summary>
        public bool requiresBaseGunInHand = true;
        /// <summary>
        /// If true, sets the fire delay of orbital guns to the fire delay of the gun they are based on.
        /// </summary>
        public bool fireDelayBasedOnGun = false;
        /// <summary>
        /// If true, the fire delay of orbital guns will benefit from the player's firerate stat.
        /// </summary>
        public bool fireDelayBenefitsFromPlayerFirerate = false;
        /// <summary>
        /// If Trigger is set to any style other than CONSTANT, this controls the percentage chance that activating a trigger will spawn the orbiting guns.
        /// 0% activation chance at 0, 50% activation chance at 0.5, and 100% activation chance at 1.
        /// </summary>
        public float chanceToSpawnOnTrigger = 1;
        /// <summary>
        /// If Trigger style is set to ON_ACTIVE_ITEM, this integer determines the ID of the active item that must be used to activate the orbiting guns.
        /// If left at -1, all active items will work to activate the orbiting guns.
        /// </summary>
        public int reqActiveItemID = -1;
        /// <summary>
        /// The time between bullets fired from the orbiting gun.
        /// </summary>
        public float FireCooldown = 1f;
        /// <summary>
        /// Controls how long orbital beam weapons will shoot for once triggered.
        /// </summary>
        public float BeamFireDuration = 1f;
        /// <summary>
        /// If true, and firetype is set to ON_RELOAD, orbiting guns will only shoot if the player reloads an EMPTY clip.
        /// </summary>
        public bool OnlyOnEmptyReload;
        /// <summary>
        /// The audio event that plays when the orbiting guns fire.
        /// </summary>
        public string ShootAudioEvent;
        public string OnEveryShotAudioEvent;
        public string FinishedShootingAudioEvent;
        /// <summary>
        /// Determines the trigger that causes the orbiting guns to appear.
        /// If set to CONSTANT, orbiting guns will always be active while the base gun is held.
        /// If set to ON_DAMAGE, orbiting guns will appear after the player takes damage.
        /// If set to ON_ACTIVE_ITEM, orbiting guns will appear after the player uses their active item.
        /// If set to ON_DODGE_ROLL, orbiting guns will appear when the player rolls.
        /// If set to ON_BLANKED, orbiting guns will appear when the player spends a blank.
        /// </summary>
        public TriggerStyle Trigger;
        /// <summary>
        /// Determines how orbiting guns set to any trigger other than CONSTANT respond to that trigger occurring again while orbital guns are already present.
        /// If set to IGNORE, nothing will happen.
        /// If set to RESET, the remaining duration on all orbiting guns will be reset to the value set in the TriggerDuration.
        /// If set to STACK, the value set in the TriggerDuration will be added to the remaining duration of all orbital guns.
        /// </summary>
        public TriggerStackingMode TriggerStacking;
        /// <summary>
        /// If Trigger style is set to any value other than CONSTANT, this float determines how long spawned orbital guns last after being created by the trigger.
        /// </summary>
        public float TriggerDuration = -1f;
        /// <summary>
        /// The chance that shots fired from orbital guns will remove ammo from the gun that the orbital is based on.
        /// Does nothing if the orbital gun's basis is not in the owner's inventory.
        /// If set to 0, no ammo will be taken.
        /// </summary>
        public float ChanceToConsumeTargetGunAmmo = 0f;
        private Gun m_gun;
        public enum TriggerStackingMode
        {
            IGNORE,
            RESET,
            STACK
        }
        public enum TriggerStyle
        {
            CONSTANT,
            ON_DAMAGE,
            ON_ACTIVE_ITEM,
            ON_DODGE_ROLL,
            ON_BLANKED
        }
    }
}
