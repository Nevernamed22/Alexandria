using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections;
using MonoMod.RuntimeDetour;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Gungeon;

namespace Alexandria.ItemAPI
{
    public static class ItemBuilder
    {
        public enum CooldownType
        {
            Timed, Damage, PerRoom, None
        }

        public enum ShopType
        {
            Goopton, Flynt, Cursula, Trorc, OldRed
        }

        public static Dictionary<ShopType, GenericLootTable> shopInventories;

        /// <summary>
        /// Initializes hooks and grabs necessary assets for building items
        /// </summary>
        public static void Init()
        {
            FakePrefabHooks.Init();
            CompanionBuilder.Init();
            LoadShopTables();
        }

        /// <summary>
        /// Loads the loot tables of shops for later modification.
        /// </summary>
        private static void LoadShopTables()
        {
            shopInventories = new Dictionary<ShopType, GenericLootTable>();
            shopInventories.Add(ShopType.Flynt, LoadShopTable("Shop_Key_Items_01"));
            shopInventories.Add(ShopType.Trorc, LoadShopTable("Shop_Truck_Items_01"));
            shopInventories.Add(ShopType.Cursula, LoadShopTable("Shop_Curse_Items_01"));
            shopInventories.Add(ShopType.Goopton, LoadShopTable("Shop_Goop_Items_01"));
            shopInventories.Add(ShopType.OldRed, LoadShopTable("Shop_Blank_Items_01"));
        }

        /// <summary>
        /// Gets a loot table from shared_auto_001 from name
        /// </summary>
        public static GenericLootTable LoadShopTable(string assetName)
        {
            return ResourceManager.LoadAssetBundle("shared_auto_001").LoadAsset<GenericLootTable>(assetName);
        }

        /// <summary>
        /// Adds a tk2dSprite component to an object and adds that sprite to the 
        /// ammonomicon for later use. If obj is null, returns a new GameObject with the sprite
        /// </summary>
        public static GameObject AddSpriteToObject(string name, string resourcePath, GameObject obj = null, Assembly assembly = null)
        {
            GameObject spriteObject = SpriteBuilder.SpriteFromResource(resourcePath, obj, assembly ?? Assembly.GetCallingAssembly());
            FakePrefab.MarkAsFakePrefab(spriteObject);
            obj.SetActive(false);

            spriteObject.name = name;
            return spriteObject;
        }

        /// <summary>
        /// Finishes the item setup, adds it to the item databases, adds an encounter trackable 
        /// blah, blah, blah
        /// </summary>
        public static void SetupItem(this PickupObject item, string shortDesc, string longDesc, string idPool = "ItemAPI")
        {
            try
            {
                item.encounterTrackable = null;

                ETGMod.Databases.Items.SetupItem(item, item.name);
                SpriteBuilder.AddToAmmonomicon(item.sprite.GetCurrentSpriteDef());
                item.encounterTrackable.journalData.AmmonomiconSprite = item.sprite.GetCurrentSpriteDef().name;

                item.SetName(item.name);
                item.SetShortDescription(shortDesc);
                item.SetLongDescription(longDesc);

                if (item is PlayerItem)
                    (item as PlayerItem).consumable = false;
                Gungeon.Game.Items.Add(idPool + ":" + item.name.ToLower().Replace(" ", "_"), item);
                ETGMod.Databases.Items.Add(item);
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
            }
        }


        public static T BuildItem<T>(string name, string prefix, string spritePath, string shortDesc, string longDesc, PickupObject.ItemQuality quality, Assembly assembly = null) where T : PickupObject
        {
            try
            {
                GameObject obj = new GameObject(name);
                var item = obj.AddComponent<T>();

                ItemBuilder.AddSpriteToObject(name, spritePath, obj, assembly ?? Assembly.GetCallingAssembly());

                ItemBuilder.SetupItem(item, shortDesc, longDesc, prefix);

                item.quality = quality;

                return item;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up \"{name}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }

        public static T BuildItem<T>(string name, string prefix, string spritePath, string shortDesc, string longDesc, ItemBuilder.CooldownType cooldownType, float rechargeTime, bool consumable, PickupObject.ItemQuality quality, Assembly assembly = null) where T : PlayerItem
        {
            try
            {
                GameObject obj = new GameObject(name);
                var item = obj.AddComponent<T>();

                ItemBuilder.AddSpriteToObject(name, spritePath, obj, assembly ?? Assembly.GetCallingAssembly());

                ItemBuilder.SetupItem(item, shortDesc, longDesc, prefix);

                ItemBuilder.SetCooldownType(item, cooldownType, rechargeTime);
                item.consumable = consumable;
                item.quality = quality;

                return item;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up \"{name}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }

        public static Gun BuildGun<T>(string name, string prefix, string spriteName, string shortDesc, string longDesc, int idleFps, int shootFps, int reloadFps, PickupObject.ItemQuality quality, int gunIdToCopyModuleFrom, int moduleCount, ProjectileModule.ShootStyle shootStyle,
            Projectile projectile, float reloadTime, float coolDown, float spread, int clipSize, int maxAmmo, Vector2 barrelOffset, GunClass gunClass = GunClass.NONE, GunHandedness gunHandedness = GunHandedness.OneHanded, bool otherModulesCostZero = true) where T : GunBehaviour
        {
            try
            {
                Gun gun = ETGMod.Databases.Items.NewGun(name, spriteName);

                var consoleName = name.ToLower().Replace(" ", "_");

                Game.Items.Rename($"outdated_gun_mods:{consoleName}", $"{prefix}:{consoleName}");
                gun.gameObject.AddComponent<T>();
                gun.SetShortDescription(shortDesc);
                gun.SetLongDescription(longDesc);
                GunExt.SetupSprite(gun, null, spriteName + "_idle_001", idleFps);
                gun.SetAnimationFPS(gun.idleAnimation, idleFps);
                gun.SetAnimationFPS(gun.shootAnimation, shootFps);
                gun.SetAnimationFPS(gun.reloadAnimation, reloadFps);

                gun.barrelOffset.position = barrelOffset;

                for (int i = 0; i < moduleCount; i++)
                {
                    var module = gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(gunIdToCopyModuleFrom) as Gun);
                    module.shootStyle = shootStyle;
                    module.cooldownTime = coolDown;
                    module.numberOfShotsInClip = clipSize;
                    module.angleVariance = spread;
                    module.ammoCost = 1;
                    module.projectiles = new List<Projectile> { projectile };
                    if (i > 0)
                    {
                        if (otherModulesCostZero) module.ammoCost = 0;
                        module.ignoredForReloadPurposes = true;
                    }
                }
                

                
                gun.DefaultModule.shootStyle = shootStyle;
                gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                gun.reloadTime = reloadTime;

                
                gun.InfiniteAmmo = false;

                gun.SetBaseMaxAmmo(maxAmmo);
                gun.gunHandedness = gunHandedness;

                gun.quality = quality;

                gun.gunClass = gunClass;

                ETGMod.Databases.Items.Add(gun, null, "ANY");

                return gun;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up \"{name}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }

        public static Gun BuildGun<T>(string name, string prefix, string spriteName, string shortDesc, string longDesc, int idleFps, int shootFps, int reloadFps, PickupObject.ItemQuality quality, int gunIdToCopyModuleFrom, ProjectileModule.ShootStyle shootStyle,
    Projectile projectile, float reloadTime, float coolDown, float spread, int clipSize, int maxAmmo, Vector2 barrelOffset, GunClass gunClass = GunClass.NONE, GunHandedness gunHandedness = GunHandedness.OneHanded) where T : GunBehaviour
        {
            try
            {
                Gun gun = ETGMod.Databases.Items.NewGun(name, spriteName);

                var consoleName = name.ToLower().Replace(" ", "_");

                Game.Items.Rename($"outdated_gun_mods:{consoleName}", $"{prefix}:{consoleName}");
                gun.gameObject.AddComponent<T>();
                gun.SetShortDescription(shortDesc);
                gun.SetLongDescription(longDesc);
                GunExt.SetupSprite(gun, null, spriteName + "_idle_001", idleFps);
                gun.SetAnimationFPS(gun.idleAnimation, idleFps);
                gun.SetAnimationFPS(gun.shootAnimation, shootFps);
                gun.SetAnimationFPS(gun.reloadAnimation, reloadFps);

                gun.barrelOffset.position = barrelOffset;
                
                gun.AddProjectileModuleFrom(PickupObjectDatabase.GetById(gunIdToCopyModuleFrom) as Gun);
                gun.DefaultModule.shootStyle = shootStyle;
                gun.DefaultModule.cooldownTime = coolDown;
                gun.DefaultModule.numberOfShotsInClip = clipSize;
                gun.DefaultModule.angleVariance = spread;
                gun.DefaultModule.ammoCost = 1;
                gun.DefaultModule.projectiles = new List<Projectile> { projectile };



                gun.DefaultModule.shootStyle = shootStyle;
                gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
                gun.reloadTime = reloadTime;


                gun.InfiniteAmmo = false;

                gun.SetBaseMaxAmmo(maxAmmo);
                gun.gunHandedness = gunHandedness;

                gun.quality = quality;

                gun.gunClass = gunClass;

                ETGMod.Databases.Items.Add(gun, null, "ANY");

                return gun;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up \"{name}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }


        public static Projectile BuildProjectile(int baseProj, string spriteName, IntVector2 spriteSize, bool shouldProjectileSpriteRotate, float damage, float speed, float force, float range)
        {
            try
            {
                Projectile projectile = ProjectileUtility.SetupProjectile(baseProj);

                projectile.baseData.damage = damage;
                projectile.baseData.speed = speed;
                projectile.baseData.force = force;
                projectile.baseData.range = range;
                projectile.shouldRotate = shouldProjectileSpriteRotate;
                projectile.SetProjectileSpriteRight(spriteName, spriteSize.x, spriteSize.y, false, tk2dBaseSprite.Anchor.LowerLeft);

                return projectile;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up projectile with sprite name \"{spriteName}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }

        public static Projectile BuildProjectile(int baseProj, float damage, float speed, float force, float range)
        {
            try
            {
                Projectile projectile = ProjectileUtility.SetupProjectile(baseProj);

                projectile.baseData.damage = damage;
                projectile.baseData.speed = speed;
                projectile.baseData.force = force;
                projectile.baseData.range = range;

                return projectile;
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"An error occurred while setting up projectile with base of \"{baseProj}\"");
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
                return null;
            }
        }



        public static void AddToSubShop(this PickupObject po, ShopType type, float weight = 1)
        {
            shopInventories[type].defaultItemDrops.Add(new WeightedGameObject()
            {
                pickupId = po.PickupObjectId,
                weight = weight,
                rawGameObject = po.gameObject,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
        }

        /// <summary>
        /// Sets the cooldown type and length of a PlayerItem, and resets all other cooldown types
        /// </summary>
        public static void SetCooldownType(this PlayerItem item, CooldownType cooldownType, float value)
        {
            item.damageCooldown = -1;
            item.roomCooldown = -1;
            item.timeCooldown = -1;

            switch (cooldownType)
            {
                case CooldownType.Timed:
                    item.timeCooldown = value;
                    break;
                case CooldownType.Damage:
                    item.damageCooldown = value;
                    break;
                case CooldownType.PerRoom:
                    item.roomCooldown = (int)value;
                    break;
            }
        }

        /// <summary>
        /// Adds a passive player stat modifier to a PlayerItem or PassiveItem
        /// </summary>
        public static StatModifier AddPassiveStatModifier(this PickupObject po, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier();
            modifier.amount = amount;
            modifier.statToBoost = statType;
            modifier.modifyType = method;

            po.AddPassiveStatModifier(modifier);
            return modifier;
        }
        public static GameObject InstantiateAndFakeprefab(this GameObject target)
        {
            GameObject instantiatedTarget = UnityEngine.Object.Instantiate<GameObject>(target);
            instantiatedTarget.SetActive(false);
            FakePrefab.MarkAsFakePrefab(instantiatedTarget);
            UnityEngine.Object.DontDestroyOnLoad(instantiatedTarget);
            return instantiatedTarget;
        }
        public static void MakeFakePrefab(this GameObject target)
        {
            target.SetActive(false);
            FakePrefab.MarkAsFakePrefab(target);
            UnityEngine.Object.DontDestroyOnLoad(target);
        }
        public static void AddPassiveStatModifier(this PickupObject po, StatModifier modifier)
        {
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem or PassiveItem");
            }
        }

        public static bool RemovePassiveStatModifier(this PickupObject po, StatModifier modifier)
        {
            bool success = false;
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null) return false;

                var list = item.passiveStatModifiers.ToList();
                success = list.Remove(modifier);
                item.passiveStatModifiers = list.ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null) return false;

                var list = item.passiveStatModifiers.ToList();
                success = list.Remove(modifier);
                item.passiveStatModifiers = list.ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem or PassiveItem");
            }
            return success;
        }


        public static IEnumerator HandleDuration(PlayerItem item, float duration, PlayerController user, Action<PlayerController> OnFinish)
        {
            if (item.IsCurrentlyActive)
            {
                yield break;
            }

            SetPrivateType<PlayerItem>(item, "m_isCurrentlyActive", true);
            SetPrivateType<PlayerItem>(item, "m_activeElapsed", 0f);
            SetPrivateType<PlayerItem>(item, "m_activeDuration", duration);
            item.OnActivationStatusChanged?.Invoke(item);

            float elapsed = GetPrivateType<PlayerItem, float>(item, "m_activeElapsed");
            float dur = GetPrivateType<PlayerItem, float>(item, "m_activeDuration");

            while (GetPrivateType<PlayerItem, float>(item, "m_activeElapsed") < GetPrivateType<PlayerItem, float>(item, "m_activeDuration") && item.IsCurrentlyActive)
            {
                yield return null;
            }
            SetPrivateType<PlayerItem>(item, "m_isCurrentlyActive", false);
            item.OnActivationStatusChanged?.Invoke(item);
            OnFinish?.Invoke(user);
            yield break;
        }

        private static void SetPrivateType<T>(T obj, string field, bool value)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(obj, value);
        }

        private static void SetPrivateType<T>(T obj, string field, float value)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            f.SetValue(obj, value);
        }

        private static T2 GetPrivateType<T, T2>(T obj, string field)
        {
            FieldInfo f = typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T2)f.GetValue(obj);
        }
    }
}
