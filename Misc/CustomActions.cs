using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using System.Collections;
using Alexandria.NPCAPI;
using HarmonyLib;

namespace Alexandria.Misc
{
    [HarmonyPatch]
    public class CustomActions
    {
        //General dungeon based actions
        /// <summary>
        /// Runs whenever the player begins a new run. Triggers in all gamemodes, including shortcuts and bossrush.
        /// </summary>
        public static Action<PlayerController, PlayerController, GameManager.GameMode> OnRunStart;
        /// <summary>
        /// Runs whenever a new Dungeon instance is created, but BEFORE it is built. Used to modify aspects of Dungeon so they get applied on generation.
        /// </summary>
        public static Action<Dungeon> PostDungeonTrueStart;
        /// <summary>
        /// Runs whenever a new PlayerController is created.
        /// </summary>
        public static Action<PlayerController> OnNewPlayercontrollerSpawned;
        /// <summary>
        /// Runs whenever a new Reward Pedestal (IE: The pedestals created by bosses) is spawned.
        /// </summary>
        public static Action<RewardPedestal> OnRewardPedestalSpawned;
        /// <summary>
        /// Runs whenever a shrine of the type AdvancedShrineController is used. Pending improvement.
        /// </summary>
        public static Action<AdvancedShrineController, PlayerController> OnShrineUsed;
        /// <summary>
        /// Runs whenever a new ShopItemController is created.
        /// </summary>
        public static Action<ShopItemController> OnShopItemStarted;
        /// <summary>
        /// Runs whenever a player fails in the Rat Maze, taking a wrong turn.
        /// </summary>
        public static Action<ResourcefulRatMazeSystemController, PlayerController> OnRatMazeFailed;
        /// <summary>
        /// Runs when a minorbreakable is shattered.
        /// </summary>
        public static Action<MinorBreakable> OnMinorBreakableShattered;
        /// <summary>
        /// Runs just before a reward pedestal determines it's contents. 
        /// Can be used to modify the contents by adding to the 'overrideItemPool' list in 'ValidPedestalContents'.
        /// </summary>
        public static Action<RewardPedestal, PlayerController, ValidPedestalContents> OnRewardPedestalDetermineContents;

        //Chest-based actions
        /// <summary>
        /// Runs when a new chest spawns. With room-reward chests, runs just after the chest has settled itself on the ground.
        /// </summary>
        public static Action<Chest> OnChestPostSpawn;
        /// <summary>
        /// Runs between the player interacting with a chest, and the chest opening.
        /// </summary>
        public static Action<Chest, PlayerController> OnChestPreOpen;
        /// <summary>
        /// Runs whenever a chest is broken.
        /// </summary>
        public static Action<Chest> OnChestBroken;

        //'Global' versions of actions which exist locally on the instance already
        /// <summary>
        /// Runs whenever any HealthHaver is killed.
        /// </summary>
        public static Action<HealthHaver> OnAnyHealthHaverDie;
        /// <summary>
        /// Runs whenever any PlayerController collects an ammo box.
        /// </summary>
        public static Action<AmmoPickup, PlayerController> OnAnyPlayerCollectedAmmo;
        /// <summary>
        /// Runs whenever any player collects a key pickup. Note that Rat Keys count as keys.
        /// </summary>
        public static Action<KeyBulletPickup, PlayerController> OnAnyPlayerCollectedKey;
        /// <summary>
        /// Runs whenever any player collects an HP pickup. Note that Armor pickups count as HP.
        /// </summary>
        public static Action<HealthPickup, PlayerController> OnAnyPlayerCollectedHealth;
        /// <summary>
        /// Runs whenever any player touches an HP Pickup. Occurs before pickup, and will still run even if the player's HP is full and they are unable to collect the pickup.
        /// Note that armor counts as an HP pickup.
        /// </summary>
        public static Action<HealthPickup, PlayerController> OnAnyPlayerNudgedHealth;
        /// <summary>
        /// Runs whenever any player collects a blank.
        /// </summary>
        public static Action<SilencerItem, PlayerController> OnAnyPlayerCollectedBlank;
        /// <summary>
        /// (DOES NOT WORK) Runs whenever any player collects any kind of Pickup Object. Runs AFTER pickup, so be aware of that.
        /// </summary>
        [Obsolete("This action does not work and should never be used; it is public for backwards compatability only.", false)]
        public static Action<PickupObject, PlayerController> OnAnyPlayerCollectedPickup;
        /// <summary>
        /// Runs whenever any player drops a passive item.
        /// </summary>
        public static Action<PassiveItem, PlayerController, DebrisObject> OnAnyPlayerDroppedPassiveItem;

        //Misc
        /// <summary>
        /// Runs whenever any Boss HealthHaver is killed.
        /// </summary>
        public static Action<HealthHaver, bool> OnBossKilled;
        /// <summary>
        /// Runs whenever an explosion occurs. Contains more information about the explosion than the vanilla action.
        /// </summary>
        public static Action<Vector3, ExplosionData, Vector2, Action, bool, CoreDamageTypes, bool> OnExplosionComplex;
        /// <summary>
        /// Runs when the Lord of the Jammed spawns.
        /// </summary>
        public static Action<SuperReaperController> OnLOTJSpawned;
        /// <summary>
        /// Runs when a loot item spawned by the Loot Engine becomes grounded and registered.
        /// </summary>
        public static Action<DebrisObject> OnPostProcessItemSpawn;

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void InitHooks() { }

        //General dungeon based actions
        [HarmonyPatch(typeof(Dungeon), nameof(Dungeon.FloorReached))]
        [HarmonyPostfix]
        private static void DungeonFloorReachedPatch(Dungeon __instance)
        {
            var gameManager = GameManager.Instance;
            var gameStatsManager = GameStatsManager.Instance;
            if (gameManager == null || gameStatsManager == null || gameStatsManager.IsInSession != true)
                return;
            if (gameStatsManager.GetSessionStatValue(TrackedStats.TIME_PLAYED) >= 0.1f)
                return;
            if (gameManager.PrimaryPlayer == null || gameManager.PrimaryPlayer.GetExtComp() == null || gameManager.PrimaryPlayer.GetExtComp().playerHasExperiencedRunStartHook)
                return;
            gameManager.PrimaryPlayer.GetExtComp().playerHasExperiencedRunStartHook = true;
            if (OnRunStart != null)
                OnRunStart(gameManager.PrimaryPlayer, gameManager.SecondaryPlayer, gameManager.CurrentGameMode);
        }

        [HarmonyPatch(typeof(RewardPedestal), nameof(RewardPedestal.MaybeBecomeMimic))]
        [HarmonyPostfix]
        private static void RewardPedestalMaybeBecomeMimicPatch(RewardPedestal __instance)
        {
            if (OnRewardPedestalSpawned != null)
                OnRewardPedestalSpawned(__instance);
        }

        [HarmonyPatch(typeof(Dungeon), nameof(Dungeon.Start))]
        [HarmonyPostfix]
        private static IEnumerator DungeonStartPatch(IEnumerator orig, Dungeon __instance)
        {
            while (orig.MoveNext())
                yield return orig.Current;
            if (PostDungeonTrueStart != null)
                PostDungeonTrueStart(__instance);
            yield break;
        }

        [HarmonyPatch(typeof(AdvancedShrineController), nameof(AdvancedShrineController.DoShrineEffect))]
        [HarmonyPrefix]
        private static void AdvancedShrineControllerDoShrineEffectPatch(AdvancedShrineController __instance, PlayerController player)
        {
            if (OnShrineUsed != null)
                OnShrineUsed(__instance, player);
        }

        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.Initialize), typeof(PickupObject), typeof(BaseShopController))]
        [HarmonyPatch(typeof(ShopItemController), nameof(ShopItemController.Initialize), typeof(PickupObject), typeof(ShopController))]
        [HarmonyPostfix]
        private static void ShopItemControllerInitializeFromBaseShopControllerPatch(ShopItemController __instance)
        {
            if (CustomActions.OnShopItemStarted != null)
                CustomActions.OnShopItemStarted(__instance);
        }

        [HarmonyPatch(typeof(ResourcefulRatMazeSystemController), nameof(ResourcefulRatMazeSystemController.HandleFailure))]
        [HarmonyPostfix]
        private static void ResourcefulRatMazeSystemControllerHandleFailurePatch(ResourcefulRatMazeSystemController __instance, PlayerController cp)
        {
            if (OnRatMazeFailed != null)
                OnRatMazeFailed(__instance, cp);
        }

        [HarmonyPatch(typeof(RewardPedestal), nameof(RewardPedestal.DetermineContents))]
        [HarmonyPrefix]
        private static void RewardPedestalDetermineContentsPatch(RewardPedestal __instance, PlayerController player)
        {
            ValidPedestalContents contentsSet = new ValidPedestalContents();
            contentsSet.overrideItemPool = new List<Tuple<int, float>>();
            if (OnRewardPedestalDetermineContents != null)
                OnRewardPedestalDetermineContents(__instance, player, contentsSet);
            if (contentsSet.overrideItemPool.Count > 0)
            {
                GenericLootTable onTheFlyLootTable = LootUtility.CreateLootTable();
                foreach (Tuple<int, float> entry in contentsSet.overrideItemPool) { onTheFlyLootTable.AddItemToPool(entry.First, entry.Second); }
                __instance.contents = onTheFlyLootTable.SelectByWeightNoExclusions().GetComponent<PickupObject>();
            }
        }

        [HarmonyPatch(typeof(MinorBreakable), nameof(MinorBreakable.FinishBreak))]
        [HarmonyPrefix]
        private static void MinorBreakableFinishBreakPatch(MinorBreakable __instance)
        {
            if (OnMinorBreakableShattered != null)
                OnMinorBreakableShattered(__instance);
        }

        //Chest-based actions
        [HarmonyPatch(typeof(Chest), nameof(Chest.PossiblyCreateBowler))]
        [HarmonyPrefix]
        private static void PossiblyCreateBowlerPatch(Chest __instance, bool mightBeActive)
        {
            __instance.m_cachedSpriteForCoop = __instance.sprite.spriteId;
            if (OnChestPostSpawn != null)
                OnChestPostSpawn(__instance);
        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.Open))]
        [HarmonyPrefix]
        private static void ChestOpenPatch(Chest __instance, PlayerController player)
        {
            if (OnChestPreOpen != null)
                OnChestPreOpen(__instance, player);
        }

        [HarmonyPatch(typeof(Chest), nameof(Chest.OnBroken))]
        [HarmonyPrefix]
        private static void ChestOnBrokenPatch(Chest __instance)
        {
            if (OnChestBroken != null)
                OnChestBroken(__instance);
        }

        //'Global' versions of actions which exist locally on the instance already
        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.Die))]
        [HarmonyPrefix]
        private static void HealthHaverDiePatch(HealthHaver __instance, Vector2 finalDamageDirection)
        {
            if (OnAnyHealthHaverDie != null)
                OnAnyHealthHaverDie(__instance);
            if (__instance && __instance.IsBoss && !GameManager.Instance.InTutorial && OnBossKilled != null)
                OnBossKilled(__instance, __instance.IsSubboss);
        }

        [HarmonyPatch(typeof(AmmoPickup), nameof(AmmoPickup.Pickup))]
        [HarmonyPrefix]
        private static bool AmmoPickupPickupPrefixPatch(AmmoPickup __instance, PlayerController player)
        {
            bool runOrig = true;
            if (player.CurrentGun && player.CurrentGun.GetComponent<AdvancedGunBehavior>() is AdvancedGunBehavior agb)
                runOrig = agb.CollectedAmmoPickup(player, player.CurrentGun, __instance);
            return runOrig;
        }

        [HarmonyPatch(typeof(AmmoPickup), nameof(AmmoPickup.Pickup))]
        [HarmonyPostfix]
        private static void AmmoPickupPickupPostfixPatch(AmmoPickup __instance, PlayerController player)
        {
            if (OnAnyPlayerCollectedAmmo != null)
                OnAnyPlayerCollectedAmmo(__instance, player);
            if (player.GetExtComp() is ExtendedPlayerComponent ext && ext.OnPickedUpAmmo != null)
                ext.OnPickedUpAmmo(player, __instance);
        }

        [HarmonyPatch(typeof(HealthPickup), nameof(HealthPickup.PrePickupLogic))]
        [HarmonyPrefix]
        private static bool PrePickupLogicPatch(HealthPickup __instance, SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody)
        {
            if (__instance && otherRigidbody.gameActor && otherRigidbody.gameActor is PlayerController playerCont)
            {
                if (OnAnyPlayerNudgedHealth != null)
                    OnAnyPlayerNudgedHealth(__instance, playerCont);
                if (playerCont.GetExtComp() is ExtendedPlayerComponent ext && ext.OnNudgedHP != null)
                    ext.OnNudgedHP(playerCont, __instance);
                if (__instance && otherRigidbody && !__instance.m_pickedUp)
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(KeyBulletPickup), nameof(KeyBulletPickup.Pickup))]
        [HarmonyPostfix]
        private static void KeyBulletPickupPickupPatch(KeyBulletPickup __instance, PlayerController player)
        {
            if (OnAnyPlayerCollectedKey != null)
                OnAnyPlayerCollectedKey(__instance, player);
            if (player.GetExtComp() is ExtendedPlayerComponent ext && ext.OnPickedUpKey != null)
                ext.OnPickedUpKey(player, __instance);
        }

        [HarmonyPatch(typeof(SilencerItem), nameof(SilencerItem.Pickup))]
        [HarmonyPostfix]
        private static void SilencerItemPickupPatch(SilencerItem __instance, PlayerController player)
        {
            if (OnAnyPlayerCollectedBlank != null)
                OnAnyPlayerCollectedBlank(__instance, player);
            if (player.GetExtComp() is ExtendedPlayerComponent ext && ext.OnPickedUpBlank != null)
                ext.OnPickedUpBlank(__instance, player);
        }

        [HarmonyPatch(typeof(HealthPickup), nameof(HealthPickup.Pickup))]
        [HarmonyPostfix]
        private static void HealthPickupPickupPatch(HealthPickup __instance, PlayerController player)
        {
            if (OnAnyPlayerCollectedHealth != null)
                OnAnyPlayerCollectedHealth(__instance, player);
            if (player.GetExtComp() is ExtendedPlayerComponent ext && ext.OnPickedUpHP != null)
                ext.OnPickedUpHP(player, __instance);
        }

        [HarmonyPatch(typeof(CompanionController), nameof(CompanionController.HandleCompanionPostProcessProjectile))]
        [HarmonyPostfix]
        private static void CompanionControllerHandleCompanionPostProcessProjectilePatch(CompanionController __instance, Projectile obj)
        {
            if (__instance && __instance.m_owner && __instance.m_owner.GetExtComp() is ExtendedPlayerComponent ext && ext.OnCompanionSpawnedBullet != null)
                ext.OnCompanionSpawnedBullet(__instance, obj);
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.DropActiveItem))]
        [HarmonyPrefix]
        private static void PlayerControllerDropActiveItemPatch(PlayerController __instance, PlayerItem item, float overrideForce, bool isDeathDrop)
        {
            if (__instance && __instance.GetExtComp() is ExtendedPlayerComponent ext && ext.OnActiveItemPreDrop != null)
                ext.OnActiveItemPreDrop(__instance, item, isDeathDrop);
        }

        [HarmonyPatch(typeof(SilencerInstance), nameof(SilencerInstance.ProcessBlankModificationItemAdditionalEffects))]
        [HarmonyPostfix]
        private static void SilencerInstanceProcessBlankModificationItemAdditionalEffectsPatch(SilencerInstance __instance, BlankModificationItem bmi, Vector2 centerPoint, PlayerController user)
        {
            if (user && user.GetExtComp() is ExtendedPlayerComponent ext && ext.OnBlankModificationItemProcessed != null)
                ext.OnBlankModificationItemProcessed(user, __instance, centerPoint, bmi);
        }

        [HarmonyPatch(typeof(PassiveItem), nameof(PassiveItem.Drop))]
        [HarmonyPostfix]
        private static void PassiveItemDropPatch(PassiveItem __instance, PlayerController player, DebrisObject __result)
        {
            if (OnAnyPlayerDroppedPassiveItem != null)
                OnAnyPlayerDroppedPassiveItem(__instance, player, __result);
            if (player && player.GetExtComp() is ExtendedPlayerComponent ext && ext.OnDroppedPassiveItem != null)
                ext.OnDroppedPassiveItem(__instance, player, __result);
        }

        //Misc
        [HarmonyPatch(typeof(Exploder), nameof(Exploder.Explode))]
        [HarmonyPostfix]
        private static void ExploderExplodePatch(Exploder __instance, Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin, bool ignoreQueues, CoreDamageTypes damageTypes, bool ignoreDamageCaps)
        {
            if (OnExplosionComplex != null)
                OnExplosionComplex(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ChangeToRandomGun))]
        [HarmonyPostfix]
        private static void PlayerControllerChangeToRandomGunPatch(PlayerController __instance)
        {
            if (!__instance || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
                return;

            var currentGun = __instance.CurrentGun;
            if (currentGun && currentGun.HasTag("exclude_blessed"))
                __instance.ChangeToRandomGun();
            else if (__instance && __instance.GetExtComp() is ExtendedPlayerComponent ext && ext.OnBlessedGunChanged != null)
                ext.OnBlessedGunChanged(__instance);
        }

        [HarmonyPatch(typeof(SuperReaperController), nameof(SuperReaperController.Start))]
        [HarmonyPostfix]
        private static void SuperReaperControllerStartPatch(SuperReaperController __instance)
        {
            if (OnLOTJSpawned != null)
                OnLOTJSpawned(__instance);
        }

        [HarmonyPatch(typeof(LootEngine), nameof(LootEngine.PostprocessItemSpawn))]
        [HarmonyPostfix]
        private static void LootEnginePostprocessItemSpawnPatch(DebrisObject item)
        {
            if (OnPostProcessItemSpawn != null && item != null)
                OnPostProcessItemSpawn(item);
        }

        [HarmonyPatch(typeof(PlayerOrbital), nameof(PlayerOrbital.Initialize))]
        [HarmonyPostfix]
        private static void PlayerOrbitalInitializePatch(PlayerOrbital __instance, PlayerController owner)
        {
            if (owner && owner.GetExtComp() is ExtendedPlayerComponent ext && ext.OnNewOrbitalInitialised != null)
                ext.OnNewOrbitalInitialised(owner, __instance);
        }

        //Stat Queries
        public class ValidPedestalContents : EventArgs
        {
            /// <summary>
            /// A list of tuples which will be converted into an override loot pool for the pedestal if set. 
            /// The first value is an integer, and represents an item id, while the second is a float an represents the weight.
            /// </summary>
            public List<Tuple<int, float>> overrideItemPool;
        }

        public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
