using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.VisualAPI;
using Gungeon;
using Alexandria.StatAPI;
using HarmonyLib;

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

        public static GenericLootTable GunslingKingChallengeGuns;
        public static Dictionary<ShopType, GenericLootTable> shopInventories;

        /// <summary>
        /// Initializes hooks and grabs necessary assets for building items
        /// </summary>
        public static void Init()
        {
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

            GunslingKingChallengeGuns = ResourceManager.LoadAssetBundle("shared_auto_001").LoadAsset<GenericLootTable>("gunslingkingshittyguntable");
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
            spriteObject.SetActive(false);

            spriteObject.name = name;
            return spriteObject;
        }

        public static GameObject AddSpriteToObjectAssetbundle(string name, int CollectionID, tk2dSpriteCollectionData data, GameObject obj = null)
        {
            GameObject spriteObject = SpriteFromBundle(name, CollectionID, data, obj);
            FakePrefab.MarkAsFakePrefab(spriteObject);
            spriteObject.SetActive(false);
            spriteObject.name = name;
            return spriteObject;
        }

        public static GameObject SpriteFromBundle(string spriteName, int CollectionID, tk2dSpriteCollectionData data, GameObject obj = null)
        {
            if (obj == null)
            {
                obj = new GameObject();
            }
            tk2dSprite sprite;
            sprite = obj.AddComponent<tk2dSprite>();
            sprite.SetSprite(data, CollectionID);
            sprite.SortingOrder = 0;
            sprite.IsPerpendicular = true;

            obj.GetComponent<BraveBehaviour>().sprite = sprite;

            return obj;
        }

        /// <summary>
        /// Adds a tk2dSprite component to an object and adds that sprite to the ammonomicon for later use. If obj is null, returns a new GameObject with the sprite
        /// Capable of taking an additional argument for the sprite's perpendicular state. 'Flat' sprites will always lay down on the floor, like carpets.
        /// </summary>
        public static GameObject AddSpriteToObjectPerpendicular(string name, string resourcePath, GameObject obj = null, tk2dBaseSprite.PerpendicularState perpendicular = tk2dBaseSprite.PerpendicularState.UNDEFINED, int? sortingLayer = null, Assembly assembly = null)
        {
            GameObject spriteObject = SpriteBuilder.SpriteFromResource(resourcePath, obj, assembly ?? Assembly.GetCallingAssembly());
            FakePrefab.MarkAsFakePrefab(spriteObject);
            spriteObject.SetActive(false);

            tk2dSprite sprite = spriteObject.GetComponent<tk2dSprite>();
            if (sortingLayer != null) { sprite.SortingOrder = (int)sortingLayer; }
            sprite.CachedPerpState = perpendicular;

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
                SpriteBuilder.AddToAmmonomicon(item.sprite.GetCurrentSpriteDef(), idPool);
                //item.encounterTrackable.journalData.AmmonomiconSprite = idPool + item.sprite.GetCurrentSpriteDef().name;
                item.encounterTrackable.journalData.AmmonomiconSprite = item.sprite.GetCurrentSpriteDef().name;

                //item.encounterTrackable.journalData.PrimaryDisplayName = idPool.ToUpper() + "_" + item.encounterTrackable.journalData.PrimaryDisplayName;
                //item.encounterTrackable.journalData.NotificationPanelDescription = idPool.ToUpper() + "_" + item.encounterTrackable.journalData.NotificationPanelDescription;
                //item.encounterTrackable.journalData.AmmonomiconFullEntry = idPool.ToUpper() + "_" + item.encounterTrackable.journalData.AmmonomiconFullEntry;

                item.SetName(item.name);
                item.SetShortDescription(shortDesc);
                item.SetLongDescription(longDesc);

                if (item is PlayerItem) (item as PlayerItem).consumable = false;

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

                ETGMod.Databases.Items.Add(gun, false, "ANY");

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

                ETGMod.Databases.Items.Add(gun, false, "ANY");

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

        public static VFXPool AddCustomMuzzleflash(this Gun target, string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, bool persist = false, VFXAlignment alignment = VFXAlignment.NormalAligned, float emissivePower = -1, Color? emissiveColour = null)
        {
            VFXPool vfx = VFXBuilder.CreateVFXPool(name,spritePaths, fps, Dimensions, anchor, usesZHeight, zHeightOffset, persist, alignment, emissivePower, emissiveColour, Assembly.GetCallingAssembly());
            target.muzzleFlashEffects = vfx;
            return vfx;
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

        public static void AddToGunslingKingTable(this PickupObject gun, int weight = 1)
        {
            GunslingKingChallengeGuns.defaultItemDrops.Add(new WeightedGameObject()
            {
                pickupId = gun.PickupObjectId,
                weight = weight,
                rawGameObject = gun.gameObject,
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
        /// Removes all stat modifiers of the set stat type from a PlayerItem, PassiveItem, or Gun.
        /// </summary>
        ///  /// <param name="po">A PassiveItem, PlayerItem, or Gun to remove the stat from.</param>
        ///  /// <param name="statType">The stat to be wiped.</param>
        public static void RemovePassiveStatModifier(this PickupObject po, PlayerStats.StatType statType)
        {
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null) return;

                var list = item.passiveStatModifiers.ToList();
                for (int i = list.Count() - 1; i >= 0; i--) { if (list[i].statToBoost == statType) { list.RemoveAt(i); } }
                item.passiveStatModifiers = list.ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null) return;

                var list = item.passiveStatModifiers.ToList();
                for (int i = list.Count() - 1; i >= 0; i--) { if (list[i].statToBoost == statType) { list.RemoveAt(i); } }
                item.passiveStatModifiers = list.ToArray();
            }
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null) return;

                var list = item.passiveStatModifiers.ToList();
                for (int i = list.Count() - 1; i >= 0; i--) { if (list[i].statToBoost == statType) { list.RemoveAt(i); } }
                item.passiveStatModifiers = list.ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem, or Gun");
            }
        }

        /// <summary>
        /// Removes all stat modifiers of the set stat type from a PlayerItem, PassiveItem, or Gun.
        /// </summary>
        ///  /// <param name="po">A PassiveItem, PlayerItem, or Gun to remove the stat from.</param>
        ///  /// <param name="modPrefix">The prefix of the mod the custom stat is from.</param>
        ///  /// <param name="customStat">The custom stat to be wiped.</param>
        public static void RemovePassiveCustomStatModifier(this PickupObject po, string modPrefix, string customStat)
        {
            if (po is PlayerItem)
            {
                var item = (po as PlayerItem);
                if (item.passiveStatModifiers == null) return;

                item.passiveStatModifiers = item.passiveStatModifiers.Where(x => x == null || x.statToBoost.ToString() != $"{modPrefix}.{customStat}").ToArray();
            }
            else if (po is PassiveItem)
            {
                var item = (po as PassiveItem);
                if (item.passiveStatModifiers == null) return;

                item.passiveStatModifiers = item.passiveStatModifiers.Where(x => x == null || x.statToBoost.ToString() != $"{modPrefix}.{customStat}").ToArray();
            }
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null) return;

                item.passiveStatModifiers = item.passiveStatModifiers.Where(x => x == null || x.statToBoost.ToString() != $"{modPrefix}.{customStat}").ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem, or Gun");
            }
        }

        /// <summary>
        /// Adds a passive player stat modifier to a PlayerItem, PassiveItem or Gun
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

        /// <summary>
        /// Adds a passive player stat modifier with a custom stat to a PlayerItem, PassiveItem or Gun
        /// </summary>
        public static StatModifier AddPassiveCustomStatModifier(this PickupObject po, string modPrefix, string customStatType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            var modifier = StatAPIManager.CreateCustomStatModifier(modPrefix, customStatType, amount, method);
            po.AddPassiveStatModifier(modifier);
            return modifier;
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
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null)
                    item.passiveStatModifiers = new StatModifier[] { modifier };
                else
                    item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem, or Gun");
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
            else if (po is Gun)
            {
                var item = (po as Gun);
                if (item.passiveStatModifiers == null) return false;

                var list = item.passiveStatModifiers.ToList();
                success = list.Remove(modifier);
                item.passiveStatModifiers = list.ToArray();
            }
            else
            {
                throw new NotSupportedException("Object must be of type PlayerItem, PassiveItem, or Gun");
            }
            return success;
        }
        public static void AddCurrentGunDamageTypeModifier(this Gun gun, CoreDamageTypes damageTypes, float damageMultiplier)
        {
            gun.currentGunDamageTypeModifiers = gun.currentGunDamageTypeModifiers.Concat(new DamageTypeModifier[] { new DamageTypeModifier { damageType = damageTypes, damageMultiplier = damageMultiplier } }).ToArray();
        }

        public static void AddCurrentGunStatModifier(this Gun gun, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod modifyMethod)
        {
            gun.currentGunStatModifiers = gun.currentGunStatModifiers.Concat(new StatModifier[] { new StatModifier { statToBoost = statType, amount = amount, modifyType = modifyMethod } }).ToArray();
        }

        public static void AddCurrentGunCustomStatModifier(this Gun gun, string modPrefix, string customStat, float amount, StatModifier.ModifyMethod modifyMethod)
        {
            gun.currentGunStatModifiers = gun.currentGunStatModifiers.AddToArray(StatAPIManager.CreateCustomStatModifier(modPrefix, customStat, amount, modifyMethod));
        }

        public static void RemoveCurrentGunStatModifier(this Gun gun, PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < gun.currentGunStatModifiers.Length; i++) { if (gun.currentGunStatModifiers[i].statToBoost != statType) { newModifiers.Add(gun.currentGunStatModifiers[i]); } }
            gun.currentGunStatModifiers = newModifiers.ToArray();
        }

        public static void RemoveCurrentGunCustomStatModifier(this Gun gun, string modPrefix, string customStat)
        {
            gun.currentGunStatModifiers = gun.currentGunStatModifiers.Where(x => x == null || x.statToBoost.ToString() != $"{modPrefix}.{customStat}").ToArray();
        }
        public static IEnumerator HandleDuration(PlayerItem item, float duration, PlayerController user, Action<PlayerController> OnFinish)
        {
            if (item.IsCurrentlyActive)
            {
                yield break;
            }

            item.m_isCurrentlyActive = true;
            item.m_activeElapsed = 0f;
            item.m_activeDuration = duration;
            item.OnActivationStatusChanged?.Invoke(item);

            float elapsed = item.m_activeElapsed;
            float dur = item.m_activeDuration;

            while (item.m_activeElapsed < item.m_activeDuration && item.IsCurrentlyActive)
            {
                yield return null;
            }
            item.m_isCurrentlyActive = false;
            item.OnActivationStatusChanged?.Invoke(item);
            OnFinish?.Invoke(user);
            yield break;
        }
    }
}
