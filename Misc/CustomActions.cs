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
        public static Action<PlayerController> OnRunStart;
        public static Action<Dungeon> PostDungeonTrueStart;
        public static Action<PlayerController> OnNewPlayercontrollerSpawned;
        public static Action<RewardPedestal> OnRewardPedestalSpawned;
        public static Action<AdvancedShrineController, PlayerController> OnShrineUsed;
        public static Action<ShopItemController> OnShopItemStarted;

        //Chest-based actions
        public static Action<Chest> OnChestPostSpawn;
        public static Action<Chest, PlayerController> OnChestPreOpen;
        public static Action<Chest> OnChestBroken;

        //'Global' versions of actions which exist locally on the instance already
        public static Action<HealthHaver> OnAnyHealthHaverDie;
        public static Action<AmmoPickup, PlayerController> OnAnyPlayerCollectedAmmo;
        public static Action<KeyBulletPickup, PlayerController> OnAnyPlayerCollectedKey;
        public static Action<HealthPickup, PlayerController> OnAnyPlayerCollectedHealth;
        public static Action<HealthPickup, PlayerController> OnAnyPlayerNudgedHealth;

        //Misc
        public static Action<HealthHaver, bool> OnBossKilled;
        public static Action<Vector3, ExplosionData, Vector2, Action, bool, CoreDamageTypes, bool> OnExplosionComplex;

        public static void InitHooks()
        {
            //General dungeon based actions
            new Hook(typeof(PlayerController).GetMethod("HandleSpinfallSpawn", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("HandleSpinfallSpawnHook"));
            new Hook(typeof(Dungeon).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("StartHook", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(RewardPedestal).GetMethod("MaybeBecomeMimic", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("PostProcessPedestal", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(AdvancedShrineController).GetMethod("DoShrineEffect", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("ShrineUsed", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(ShopItemController).GetMethods().Single(a => a.Name == "Initialize" && a.GetParameters().Length == 2 && a.GetParameters()[1].ParameterType == typeof(BaseShopController)), typeof(CustomActions).GetMethod("InitializeViaBaseShopController", BindingFlags.Static | BindingFlags.Public));
            new Hook(typeof(ShopItemController).GetMethods().Single(a => a.Name == "Initialize" && a.GetParameters().Length == 2 && a.GetParameters()[1].ParameterType == typeof(ShopController)),typeof(CustomActions).GetMethod("InitializeViaShopController", BindingFlags.Static | BindingFlags.Public));

            //Chest-based actions
            new Hook(typeof(Chest).GetMethod("PossiblyCreateBowler", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("PostProcessChest", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(Chest).GetMethod("Open", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("ChestPreOpen", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("OnBroken", BindingFlags.Static | BindingFlags.NonPublic));

            //'Global' versions of actions which exist locally on the instance already
            new Hook(typeof(HealthHaver).GetMethod("Die", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("OnHealthHaverDie", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(AmmoPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("ammoPickupHookMethod"));
            new Hook(typeof(HealthPickup).GetMethod("PrePickupLogic", BindingFlags.Instance | BindingFlags.NonPublic), typeof(CustomActions).GetMethod("healthPrePickupHook"));
            new Hook(typeof(KeyBulletPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public),typeof(CustomActions).GetMethod("keyPickupHookMethod"));
            new Hook(typeof(PlayerController).GetMethod("DropActiveItem", BindingFlags.Public | BindingFlags.Instance),typeof(CustomActions).GetMethod("DropActiveHook", BindingFlags.Public | BindingFlags.Static));

            //Misc
            new Hook(typeof(Exploder).GetMethod("Explode", BindingFlags.Static | BindingFlags.Public), typeof(CustomActions).GetMethod("ExplosionHook", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(PlayerController).GetMethod("ChangeToRandomGun", BindingFlags.Instance | BindingFlags.Public), typeof(CustomActions).GetMethod("ChangeToRandomGunHook", BindingFlags.Static | BindingFlags.Public));
        }

        //General dungeon based actions
        public static IEnumerator HandleSpinfallSpawnHook(Func<PlayerController, float, IEnumerator> orig, PlayerController self, float invisibleDelay)
        {
            IEnumerator origEnum = orig(self, invisibleDelay);
            while (origEnum.MoveNext())
            {
                object obj = origEnum.Current;
                yield return obj;
            }
            if (GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED) <= 0.33f)
            {
                if (OnRunStart != null)
                {
                    OnRunStart(self);
                }
            }
            yield break;
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

        //Chest-based actions
        private static void PostProcessChest(Action<Chest, bool> orig, Chest self, bool uselssVar) { self.m_cachedSpriteForCoop = self.sprite.spriteId;  if (OnChestPostSpawn != null) { OnChestPostSpawn(self); } orig(self, uselssVar);  }
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


        public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
