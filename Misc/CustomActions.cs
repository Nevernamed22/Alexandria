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
        public static Action<PlayerController> OnRunStart;
        public static Action<Dungeon> PostDungeonTrueStart;
        public static Action<List<DebrisObject>, Chest> SpawnChestContents;
        //public static Action<Projectile, bool, bool, bool, bool> OnProjectileDieInAir;

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
            chestSpawnItemsHook = new Hook(
                typeof(Chest).GetMethod("SpewContentsOntoGround", BindingFlags.NonPublic | BindingFlags.Instance),
                typeof(CustomActions).GetMethod("OnPostOpenBullshit"));
            new Hook(
                typeof(PlayerController).GetMethod("HandleSpinfallSpawn", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("HandleSpinfallSpawnHook")
            );
            new Hook(
                typeof(Dungeon).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("StartHook", BindingFlags.Static | BindingFlags.Public)
            );
        }
        private static Hook pedestalSpawnHook;
        private static Hook chestPostProcessHook;
        private static Hook chestPreOpenHook;
        private static Hook chestBrokenHook;
        private static Hook healthhaverDieHook;
        private static Hook explosionHook;
        private static Hook ShrineUseHook;
        private static Hook chestSpawnItemsHook;
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
        public static void OnPostOpenBullshit(Action<Chest, List<Transform>> orig, Chest self, List<Transform> transforms)
        {
            List<DebrisObject> list = new List<DebrisObject>();
            bool isRainbowRun = GameStatsManager.Instance.IsRainbowRun;
            Type chesttype = typeof(Chest);
            FieldInfo _forceDropOkayForRainbowRun = chesttype.GetField("m_forceDropOkayForRainbowRun", BindingFlags.NonPublic | BindingFlags.Instance);
            if (isRainbowRun && !self.IsRainbowChest && !(bool)_forceDropOkayForRainbowRun.GetValue(self))
            {
                Vector2 a;
                if (self.spawnTransform != null)
                {
                    a = self.spawnTransform.position;
                }
                else
                {
                    Bounds bounds = self.sprite.GetBounds();
                    a = self.transform.position + bounds.extents;
                }
                FieldInfo _room = chesttype.GetField("m_room", BindingFlags.NonPublic | BindingFlags.Instance);
                LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteChest, a + new Vector2(-0.5f, -2.25f), (RoomHandler)_room.GetValue(self), true);
            }
            else
            {
                for (int i = 0; i < self.contents.Count; i++)
                {
                    List<DebrisObject> list2 = LootEngine.SpewLoot(new List<GameObject>
                {
                    self.contents[i].gameObject
                }, transforms[i].position);
                    list.AddRange(list2);
                    for (int j = 0; j < list2.Count; j++)
                    {
                        if (list2[j])
                        {
                            list2[j].PreventFallingInPits = true;
                        }
                        if (!(list2[j].GetComponent<Gun>() != null))
                        {
                            if (!(list2[j].GetComponent<CurrencyPickup>() != null))
                            {
                                if (list2[j].specRigidbody != null)
                                {
                                    list2[j].specRigidbody.CollideWithOthers = false;
                                    DebrisObject debrisObject = list2[j];
                                    MethodInfo _BecomeViableItem = chesttype.GetMethod("BecomeViableItem", BindingFlags.NonPublic | BindingFlags.Instance);
                                    debrisObject.OnTouchedGround += (Action<DebrisObject>)_BecomeViableItem.Invoke(self, new object[] { debrisObject });
                                }
                            }
                        }
                    }
                }
            }
            if (SpawnChestContents != null)
            {
                SpawnChestContents(list, self);
            }
            if (self.IsRainbowChest && isRainbowRun && self.transform.position.GetAbsoluteRoom() == GameManager.Instance.Dungeon.data.Entrance)
            {
                MethodInfo _HandleRainbowRunLootProcessing = chesttype.GetMethod("HandleRainbowRunLootProcessing", BindingFlags.NonPublic | BindingFlags.Instance);
                GameManager.Instance.Dungeon.StartCoroutine((IEnumerator)_HandleRainbowRunLootProcessing.Invoke(self, new object[] { list }));
            }
        }

        public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
