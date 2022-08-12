using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Misc
{
    public static class GunUtility
    {
        public static bool IsCurrentGun(this Gun gun)
        {
            if (gun && gun.CurrentOwner)
            {
                if (gun.CurrentOwner.CurrentGun == gun) return true;
                else return false;
            }
            else return false;
        }
        public static PlayerController GunPlayerOwner(this Gun gun)
        {
            if (gun && gun.CurrentOwner && gun.CurrentOwner is PlayerController) return gun.CurrentOwner as PlayerController;
            else return null;
        }
        public static void AddStatToGun(this Gun item, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod method = StatModifier.ModifyMethod.ADDITIVE)
        {
            StatModifier modifier = new StatModifier
            {
                amount = amount,
                statToBoost = statType,
                modifyType = method
            };

            if (item.passiveStatModifiers == null)
                item.passiveStatModifiers = new StatModifier[] { modifier };
            else
                item.passiveStatModifiers = item.passiveStatModifiers.Concat(new StatModifier[] { modifier }).ToArray();
        }
        public static void RemoveStatFromGun(this Gun item, PlayerStats.StatType statType)
        {
            var newModifiers = new List<StatModifier>();
            for (int i = 0; i < item.passiveStatModifiers.Length; i++)
            {
                if (item.passiveStatModifiers[i].statToBoost != statType)
                    newModifiers.Add(item.passiveStatModifiers[i]);
            }
            item.passiveStatModifiers = newModifiers.ToArray();
        }
        public static ProjectileModule RawDefaultModule(this Gun self)
        {
            if (self.RawSourceVolley)
            {
                if (self.RawSourceVolley.ModulesAreTiers)
                {
                    for (int i = 0; i < self.RawSourceVolley.projectiles.Count; i++)
                    {
                        ProjectileModule projectileModule = self.RawSourceVolley.projectiles[i];
                        if (projectileModule != null)
                        {
                            int num = (projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex;
                            if (num == self.CurrentStrengthTier)
                            {
                                return projectileModule;
                            }
                        }
                    }
                }
                return self.RawSourceVolley.projectiles[0];
            }
            return self.singleModule;
        }
        public static ProjectileModule AddProjectileModuleToRawVolley(this Gun gun, ProjectileModule projectile)
        {
            gun.RawSourceVolley.projectiles.Add(projectile);
            return projectile;
        }
        public static ProjectileModule AddProjectileModuleToRawVolleyFrom(this Gun gun, Gun other, bool cloned = true)
        {
            ProjectileModule defaultModule = other.DefaultModule;
            if (!cloned)
            {
                return gun.AddProjectileModuleToRawVolley(defaultModule);
            }
            ProjectileModule projectileModule = ProjectileModule.CreateClone(defaultModule, false, -1);
            projectileModule.projectiles = new List<Projectile>(defaultModule.projectiles.Capacity);
            for (int i = 0; i < defaultModule.projectiles.Count; i++)
            {
                projectileModule.projectiles.Add(defaultModule.projectiles[i]);
            }
            return gun.AddProjectileModuleToRawVolley(projectileModule);
        }
    }
}
