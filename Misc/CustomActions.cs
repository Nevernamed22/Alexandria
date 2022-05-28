using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;

namespace Alexandria.Misc
{
    class CustomActions
    {
        public static Action<RewardPedestal> OnRewardPedestalSpawned;
        public static Action<Chest> OnChestPostSpawn;
        public static Action<Chest, PlayerController> OnChestPreOpen;
        public static Action<Chest> OnChestBroken;
        public static Action<HealthHaver> OnAnyHealthHaverDie;
        public static Action<HealthHaver, bool> OnBossKilled;
        public static Action<AdvancedShrineController, PlayerController> OnShrineUsed;
        public static Action<Vector3, ExplosionData, Vector2, Action, bool, CoreDamageTypes, bool> OnExplosionComplex;
        public static Action<AmmoPickup, PlayerController> OnAmmoCollected;

        public static void InitHooks()
        {
            pedestalSpawnHook = new Hook(
                typeof(RewardPedestal).GetMethod("MaybeBecomeMimic", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomActions).GetMethod("PostProcessPedestal", BindingFlags.Static | BindingFlags.Public)
            );
            chestPostProcessHook = new Hook(
                typeof(Chest).GetMethod("PossiblyCreateBowler", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("PostProcessChest", BindingFlags.Static | BindingFlags.NonPublic)
            );
            chestPreOpenHook = new Hook(
                typeof(Chest).GetMethod("Open", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("ChestPreOpen", BindingFlags.Static | BindingFlags.NonPublic)
            );
            chestBrokenHook = new Hook(
                typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("OnBroken", BindingFlags.Static | BindingFlags.NonPublic)
            );
            healthhaverDieHook = new Hook(
               typeof(HealthHaver).GetMethod("Die", BindingFlags.Instance | BindingFlags.Public),
               typeof(CustomActions).GetMethod("OnHealthHaverDie", BindingFlags.Static | BindingFlags.NonPublic)
           );
            explosionHook = new Hook(
                typeof(Exploder).GetMethod("Explode", BindingFlags.Static | BindingFlags.Public),
                typeof(CustomActions).GetMethod("ExplosionHook", BindingFlags.Static | BindingFlags.NonPublic)
           );
            ShrineUseHook = new Hook(
                typeof(AdvancedShrineController).GetMethod("DoShrineEffect", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("ShrineUsed", BindingFlags.Static | BindingFlags.NonPublic)
            );
            ammoPickupHook = new Hook(
                typeof(AmmoPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomActions).GetMethod("ammoPickupHookMethod")
            );
            ammoInteractHook = new Hook(
                typeof(AmmoPickup).GetMethod("Interact", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomActions).GetMethod("ammoInteractHookMethod")
            );
        }
        private static Hook pedestalSpawnHook;
        private static Hook chestPostProcessHook;
        private static Hook chestPreOpenHook;
        private static Hook chestBrokenHook;
        private static Hook healthhaverDieHook;
        private static Hook explosionHook;
        private static Hook ShrineUseHook;
        public static Hook ammoPickupHook;
        public static Hook ammoInteractHook;
        public static void PostProcessPedestal(Action<RewardPedestal> orig, RewardPedestal self) { orig(self); if (OnRewardPedestalSpawned != null) { OnRewardPedestalSpawned(self); } }
        private static void OnBroken(Action<Chest> orig, Chest self) { if (OnChestBroken != null) { OnChestBroken(self); } orig(self); }
        private static void PostProcessChest(Action<Chest, bool> orig, Chest self, bool uselssVar) { if (OnChestPostSpawn != null) { OnChestPostSpawn(self); } orig(self, uselssVar); }
        private static void ChestPreOpen(Action<Chest, PlayerController> orig, Chest self, PlayerController opener) { if (OnChestPreOpen != null) { OnChestPreOpen(self, opener); } orig(self, opener); }
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
        private static void ExplosionHook(Action<Vector3, ExplosionData, Vector2, Action, bool, CoreDamageTypes, bool> orig, Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin = null, bool ignoreQueues = false, CoreDamageTypes damageTypes = CoreDamageTypes.None, bool ignoreDamageCaps = false)
        {
            orig(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
            if (OnExplosionComplex != null) OnExplosionComplex(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
        }
        private static void ShrineUsed(Action<AdvancedShrineController, PlayerController> orig, AdvancedShrineController self, PlayerController playa)
        {
            if (OnShrineUsed != null) OnShrineUsed(self, playa);
            orig(self, playa);
        }
        public static void ammoPickupHookMethod(Action<AmmoPickup, PlayerController> orig, AmmoPickup self, PlayerController player)
        {
            bool runOrig = true;
            if (player.CurrentGun && player.CurrentGun.GetComponent<AdvancedGunBehavior>())
            {
                runOrig = player.CurrentGun.GetComponent<AdvancedGunBehavior>().CollectedAmmoPickup(player, player.CurrentGun, self);
            }
            if (runOrig) orig(self, player);
            if (OnAmmoCollected != null) OnAmmoCollected(self, player);
        }
        public static void ammoInteractHookMethod(Action<AmmoPickup, PlayerController> orig, AmmoPickup self, PlayerController player)
        {
            if (player.CurrentGun && player.CurrentGun.GetComponent<AdvancedGunBehavior>() != null && player.CurrentGun.GetComponent<AdvancedGunBehavior>().canCollectAmmoAtMaxAmmo)
            {
                if (RoomHandler.unassignedInteractableObjects.Contains(self))
                {
                    RoomHandler.unassignedInteractableObjects.Remove(self);
                }
                SpriteOutlineManager.RemoveOutlineFromSprite(self.sprite, true);
                self.Pickup(player);
            }
            else if (player.CurrentGun && !player.CurrentGun.CanGainAmmo)
            {
                GameUIRoot.Instance.InformNeedsReload(player, new Vector3(player.specRigidbody.UnitCenter.x - player.transform.position.x, 1.25f, 0f), 1f, "#RELOAD_FULL");
                return;
            }
            else
            {
                orig(self, player);
            }
        }

        public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    }
}
