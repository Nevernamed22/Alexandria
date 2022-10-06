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

namespace Alexandria.Misc
{
    public class CustomActions
    {
        //General dungeon based actions
        /// <summary>
        /// Runs whenever the player begins a new run. Triggers in all gamemodes, including shortcuts and bossrush.
        /// </summary>
        public static Action<PlayerController, PlayerController, GameManager.GameMode> OnRunStart;
        /// <summary>
        /// Runs whenever a new Dungeon is created.
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
        /// Runs whenever the any player player collects a blank.
        /// </summary>
        public static Action<SilencerItem, PlayerController> OnAnyPlayerCollectedBlank;


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

        public static void InitHooks()
        {
            //General dungeon based actions
            new Hook(typeof(Dungeon).GetMethod("FloorReached", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("FloorReachedHook"));
            new Hook(typeof(Dungeon).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("StartHook", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(RewardPedestal).GetMethod("MaybeBecomeMimic", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("PostProcessPedestal", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(AdvancedShrineController).GetMethod("DoShrineEffect", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("ShrineUsed", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(ShopItemController).GetMethods().Single(a => a.Name == "Initialize" && a.GetParameters().Length == 2 && a.GetParameters()[1].ParameterType == typeof(BaseShopController)), typeof(CustomActions).GetMethod("InitializeViaBaseShopController", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(ShopItemController).GetMethods().Single(a => a.Name == "Initialize" && a.GetParameters().Length == 2 && a.GetParameters()[1].ParameterType == typeof(ShopController)), typeof(CustomActions).GetMethod("InitializeViaShopController", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(ResourcefulRatMazeSystemController).GetMethod("HandleFailure", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("OnFailedRatMaze", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(RewardPedestal).GetMethod("DetermineContents", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("RewardPedestalDetermineContents", BindingFlags.Static | BindingFlags.Public));

            //Chest-based actions
            new Hook(typeof(Chest).GetMethod("PossiblyCreateBowler", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("PostProcessChest", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(Chest).GetMethod("Open", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("ChestPreOpen", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("OnBroken", BindingFlags.Static | BindingFlags.NonPublic));

            //'Global' versions of actions which exist locally on the instance already
            new Hook(typeof(HealthHaver).GetMethod("Die", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("OnHealthHaverDie", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(AmmoPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("ammoPickupHookMethod"));
            new Hook(typeof(HealthPickup).GetMethod("PrePickupLogic", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("healthPrePickupHook"));
            new Hook(typeof(KeyBulletPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("keyPickupHookMethod"));
            new Hook(typeof(SilencerItem).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public),typeof(CustomActions).GetMethod("blankPickupHookMethod"));
            new Hook(typeof(CompanionController).GetMethod("HandleCompanionPostProcessProjectile", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("companionSpawnedbullet", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(PlayerController).GetMethod("DropActiveItem", BindingFlags.Public | BindingFlags.Instance), typeof(CustomActions).GetMethod("DropActiveHook", BindingFlags.Public | BindingFlags.Static));

            //Misc
            new Hook(typeof(Exploder).GetMethod("Explode", BindingFlags.Static | BindingFlags.Public), typeof(CustomActions).GetMethod("ExplosionHook", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(PlayerController).GetMethod("ChangeToRandomGun", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("ChangeToRandomGunHook", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(SuperReaperController).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance),typeof(CustomActions).GetMethod("LOTJSpawnHook"));
            new Hook(typeof(LootEngine).GetMethod("PostprocessItemSpawn", BindingFlags.Static | BindingFlags.NonPublic),typeof(CustomActions).GetMethod("PostProcessItemHook", BindingFlags.Static | BindingFlags.Public));

        }

        //General dungeon based actions
        public static void FloorReachedHook(Action<Dungeon> orig, Dungeon self)
        {
            orig(self);
            var gameManager = GameManager.Instance;
            var gameStatsManager = GameStatsManager.Instance;
            if (gameManager != null && gameStatsManager != null && gameStatsManager.IsInSession == true)
            {
                if (gameStatsManager.GetSessionStatValue(TrackedStats.TIME_PLAYED) < 0.1f)
                {
                    if (gameManager.PrimaryPlayer != null && gameManager.PrimaryPlayer.GetExtComp() != null && !gameManager.PrimaryPlayer.GetExtComp().playerHasExperiencedRunStartHook)
                    {
                        gameManager.PrimaryPlayer.GetExtComp().playerHasExperiencedRunStartHook = true;
                        if (OnRunStart != null)
                        {
                            OnRunStart(gameManager.PrimaryPlayer, gameManager.SecondaryPlayer, gameManager.CurrentGameMode);
                        }
                    }
                }
            }
        }
        public static void PostProcessPedestal(Action<RewardPedestal> orig, RewardPedestal self) { orig(self); if (OnRewardPedestalSpawned != null) { OnRewardPedestalSpawned(self); } }
        public static IEnumerator StartHook(Func<Dungeon, IEnumerator> orig, Dungeon self)
        {
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext())
            {
                object obj = origEnum.Current;
                yield return obj;
            }

            if (PostDungeonTrueStart != null)
            {
                PostDungeonTrueStart(self);
            }
            yield break;
        }
        private static void ShrineUsed(Action<AdvancedShrineController, PlayerController> orig, AdvancedShrineController self, PlayerController playa)
        {
            if (OnShrineUsed != null) OnShrineUsed(self, playa);
            orig(self, playa);
        }
        public static void InitializeViaBaseShopController(Action<ShopItemController, PickupObject, BaseShopController> orig, ShopItemController self, PickupObject i, BaseShopController parent)
        {
            orig(self, i, parent);
            if (CustomActions.OnShopItemStarted != null)
            {
                CustomActions.OnShopItemStarted(self);
            }
        }
        public static void InitializeViaShopController(Action<ShopItemController, PickupObject, ShopController> orig, ShopItemController self, PickupObject i, ShopController parent)
        {
            orig(self, i, parent);
            if (CustomActions.OnShopItemStarted != null)
            {
                CustomActions.OnShopItemStarted(self);
            }
        }
        public static void OnFailedRatMaze(Action<ResourcefulRatMazeSystemController, PlayerController> orig, ResourcefulRatMazeSystemController self, PlayerController playa)
        {
            orig(self, playa);
            if (OnRatMazeFailed != null) OnRatMazeFailed(self, playa);
        }
        public static void RewardPedestalDetermineContents(Action<RewardPedestal, PlayerController> orig, RewardPedestal self, PlayerController compareAgainst)
        {
            ValidPedestalContents contentsSet = new ValidPedestalContents();
            contentsSet.overrideItemPool = new List<Tuple<int, float>>();
           
            if (OnRewardPedestalDetermineContents != null) OnRewardPedestalDetermineContents(self, compareAgainst, contentsSet);

            if (contentsSet.overrideItemPool.Count > 0)
            {
                GenericLootTable onTheFlyLootTable = LootUtility.CreateLootTable();
                foreach (Tuple<int, float> entry in contentsSet.overrideItemPool) { onTheFlyLootTable.AddItemToPool(entry.First, entry.Second); }
                self.lootTable.lootTable = onTheFlyLootTable;
                self.UsesSpecificItem = false;
                self.contents = null;
            }
            orig(self, compareAgainst);
        }

        //Chest-based actions
        private static void PostProcessChest(Action<Chest, bool> orig, Chest self, bool uselssVar) { self.m_cachedSpriteForCoop = self.sprite.spriteId; if (OnChestPostSpawn != null) { OnChestPostSpawn(self); } orig(self, uselssVar); }
        private static void ChestPreOpen(Action<Chest, PlayerController> orig, Chest self, PlayerController opener) { if (OnChestPreOpen != null) { OnChestPreOpen(self, opener); } orig(self, opener); }
        private static void OnBroken(Action<Chest> orig, Chest self) { if (OnChestBroken != null) { OnChestBroken(self); } orig(self); }

        //'Global' versions of actions which exist locally on the instance already
        private static void OnHealthHaverDie(Action<HealthHaver, Vector2> orig, HealthHaver self, Vector2 finalDamageDir)
        {
            if (OnAnyHealthHaverDie != null)
            {
                OnAnyHealthHaverDie(self);
            }
            if (self && self.IsBoss && !GameManager.Instance.InTutorial)
            {
                if (OnBossKilled != null) OnBossKilled(self, self.IsSubboss);
            }
            orig(self, finalDamageDir);

        }
        public static void ammoPickupHookMethod(Action<AmmoPickup, PlayerController> orig, AmmoPickup self, PlayerController player)
        {
            bool runOrig = true;
            if (player.CurrentGun && player.CurrentGun.GetComponent<AdvancedGunBehavior>())
            {
                runOrig = player.CurrentGun.GetComponent<AdvancedGunBehavior>().CollectedAmmoPickup(player, player.CurrentGun, self);
            }
            if (self)
            {
                if (runOrig) orig(self, player);
                if (OnAnyPlayerCollectedAmmo != null) OnAnyPlayerCollectedAmmo(self, player);
                if (player.GetExtComp() && player.GetExtComp().OnPickedUpAmmo != null) player.GetExtComp().OnPickedUpAmmo(player, self);
            }
        }
        public static void healthPrePickupHook(Action<HealthPickup, SpeculativeRigidbody, SpeculativeRigidbody> orig, HealthPickup self, SpeculativeRigidbody player, SpeculativeRigidbody selfBody)
        {
            if (self && player.gameActor && player.gameActor is PlayerController)
            {
                PlayerController playerCont = player.gameActor as PlayerController;
                if (OnAnyPlayerNudgedHealth != null) OnAnyPlayerNudgedHealth(self, playerCont);
                if (playerCont.GetExtComp() && playerCont.GetExtComp().OnNudgedHP != null) playerCont.GetExtComp().OnNudgedHP(playerCont, self);
                if (self && player && !self.m_pickedUp) orig(self, player, selfBody);
            }
        }
        public static void keyPickupHookMethod(Action<KeyBulletPickup, PlayerController> orig, KeyBulletPickup self, PlayerController player)
        {
            orig(self, player);
            if (OnAnyPlayerCollectedKey != null) OnAnyPlayerCollectedKey(self, player);
            if (player.GetExtComp() && player.GetExtComp().OnPickedUpKey != null) player.GetExtComp().OnPickedUpKey(player, self);
        }
        public static void blankPickupHookMethod(Action<SilencerItem, PlayerController> orig, SilencerItem self, PlayerController player)
        {
            orig(self, player);
            if (OnAnyPlayerCollectedBlank != null) OnAnyPlayerCollectedBlank(self, player);
            if (player.GetExtComp() && player.GetExtComp().OnPickedUpBlank != null) player.GetExtComp().OnPickedUpBlank(self, player);
        }
        public static void companionSpawnedbullet(Action<CompanionController, Projectile> orig, CompanionController self, Projectile spawnedProjectile)
        {
            orig(self, spawnedProjectile);
            if (self && self.m_owner && self.m_owner.GetExtComp() && self.m_owner.GetExtComp().OnCompanionSpawnedBullet != null) self.m_owner.GetExtComp().OnCompanionSpawnedBullet(self, spawnedProjectile);
        }
        public static DebrisObject DropActiveHook(Func<PlayerController, PlayerItem, float, bool, DebrisObject> orig, PlayerController self, PlayerItem item, float force = 4f, bool deathdrop = false)
        {
            if (self && self.GetExtComp().OnActiveItemPreDrop != null) self.GetExtComp().OnActiveItemPreDrop(self, item, deathdrop);
            return orig(self, item, force, deathdrop);
        }

        //Misc
        private static void ExplosionHook(Action<Vector3, ExplosionData, Vector2, Action, bool, CoreDamageTypes, bool> orig, Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin = null, bool ignoreQueues = false, CoreDamageTypes damageTypes = CoreDamageTypes.None, bool ignoreDamageCaps = false)
        {
            orig(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
            if (OnExplosionComplex != null) OnExplosionComplex(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
        }
        public static void ChangeToRandomGunHook(Action<PlayerController> orig, PlayerController self)
        {
            orig(self);
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
            {
                return;
            }

            var currentGun = self.CurrentGun;

            if (currentGun && currentGun.HasTag("exclude_blessed"))
            {
                self.ChangeToRandomGun();
            }
            else { if (self && self.GetExtComp() != null) self.GetExtComp().OnBlessedGunChanged(self); }
        }
        public static void LOTJSpawnHook(Action<SuperReaperController> orig, SuperReaperController self)
        {
            orig(self);
            if (OnLOTJSpawned != null) OnLOTJSpawned(self);
        }
        public static void PostProcessItemHook(Action<DebrisObject> orig, DebrisObject spawnedItem)
        {
            orig(spawnedItem);
            if (OnPostProcessItemSpawn != null && spawnedItem != null) OnPostProcessItemSpawn(spawnedItem);
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
