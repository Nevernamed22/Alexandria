using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
    public class AdvancedVolleyModificationSynergyProcessor : MonoBehaviour
    {
        public void Awake()
        {
            // ETGModConsole.Log("Awake");
            m_gun = base.GetComponent<Gun>();
            if (!m_gun)
                return;

            //ETGModConsole.Log("is gun");
            m_gun.PostProcessVolley += HandleVolleyRebuild;
            if (synergies.ToList().Find(x => x.ReplacesSourceProjectile) != null)
                m_gun.OnPreFireProjectileModifier += HandlePreFireProjectileReplacement;
        }

        private Projectile HandlePreFireProjectileReplacement(Gun sourceGun, Projectile sourceProjectile, ProjectileModule sourceModule)
        {
            if (synergies == null)
                return sourceProjectile;
            if (sourceGun.GunPlayerOwner() is not PlayerController playerController)
                return sourceProjectile;
            if (sourceGun && sourceGun.IsCharging)
                return sourceProjectile;

            Projectile result = sourceProjectile;
            for (int i = 0; i < synergies.Count; i++)
            {
                AdvancedVolleyModificationSynergyData data = synergies[i];
                if (!data.ReplacesSourceProjectile || !playerController.PlayerHasActiveSynergy(data.RequiredSynergy))
                    continue;
                if (data.OnlyReplacesAdditionalProjectiles && !sourceModule.ignoredForReloadPurposes)
                    continue;
                if (data.ReplacementSkipsChargedShots && sourceModule.shootStyle == ProjectileModule.ShootStyle.Charged
                    && sourceModule.chargeProjectiles != null && sourceModule.chargeProjectiles.Find(x => x.Projectile && x.ChargeTime > 0f) != null)
                    continue;
                if (UnityEngine.Random.value >= data.ReplacementChance)
                    continue;

                if (!data.UsesMultipleReplacementProjectiles)
                    result = data.ReplacementProjectile;
                else if (!data.MultipleReplacementsSequential)
                    result = data.MultipleReplacementProjectiles[UnityEngine.Random.Range(0, data.MultipleReplacementProjectiles.Length)];
                else
                {
                    result = data.MultipleReplacementProjectiles[data.multipleSequentialReplacementIndex];
                    data.multipleSequentialReplacementIndex = (data.multipleSequentialReplacementIndex + 1) % data.MultipleReplacementProjectiles.Length;
                }
            }
            return result;
        }

        private void HandleVolleyRebuild(ProjectileVolleyData targetVolley)
        {
            //ETGModConsole.Log("Volley Rebuild");
            if (!m_gun || synergies == null || m_gun.GunPlayerOwner() is not PlayerController playerController)
                return;

            for (int i = 0; i < synergies.Count; i++)
                if (playerController.PlayerHasActiveSynergy(synergies[i].RequiredSynergy))
                    ApplySynergy(targetVolley, synergies[i], playerController);
        }

        private void ApplySynergy(ProjectileVolleyData volley, AdvancedVolleyModificationSynergyData synergy, PlayerController owner)
        {
            //ETGModConsole.Log("Apply synergy");
            if (synergy.AddsChargeProjectile) { volley.projectiles[0].chargeProjectiles.Add(synergy.ChargeProjectileToAdd); }
            if (synergy.AddsModules)
            {
                //ETGModConsole.Log("Adds modules");
                bool flag = true;
                if (volley != null && volley.projectiles.Count > 0 && volley.projectiles[0].projectiles != null && volley.projectiles[0].projectiles.Count > 0)
                {
                    Projectile projectile = volley.projectiles[0].projectiles[0];
                    if (projectile && projectile.GetComponent<ArtfulDodgerProjectileController>())
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    //ETGModConsole.Log($"flag passed, modules to add count = {synergy.ModulesToAdd.Length}");
                    for (int i = 0; i < synergy.ModulesToAdd.Length; i++)
                    {
                        synergy.ModulesToAdd[i].isExternalAddedModule = true;
                        volley.projectiles.Add(synergy.ModulesToAdd[i]);
                        //ETGModConsole.Log("added module");
                    }
                }
            }

            if (synergy.AddsDuplicatesOfBaseModule) { GunVolleyModificationItem.AddDuplicateOfBaseModule(volley, m_gun.GunPlayerOwner(), synergy.DuplicatesOfBaseModule, synergy.BaseModuleDuplicateAngle, 0f); }
            if (synergy.SetsNumberFinalProjectiles)
            {
                bool hasOptionalFinalProjectile = false;
                for (int j = 0; j < volley.projectiles.Count; j++)
                {
                    if (volley.projectiles[j].usesOptionalFinalProjectile)
                    {
                        volley.projectiles[j].numberOfFinalProjectiles = synergy.NumberFinalProjectiles;
                        continue;
                    }
                    if (hasOptionalFinalProjectile || !synergy.AddsNewFinalProjectile)
                        continue;

                    hasOptionalFinalProjectile = true;
                    this.m_gun.OverrideFinaleAudio = true;
                    volley.projectiles[j].usesOptionalFinalProjectile = true;
                    volley.projectiles[j].numberOfFinalProjectiles = synergy.NumberFinalProjectiles;
                    volley.projectiles[j].finalProjectile = synergy.NewFinalProjectile;
                    volley.projectiles[j].finalAmmoType = GameUIAmmoType.AmmoType.CUSTOM;
                    volley.projectiles[j].finalCustomAmmoType = synergy.NewFinalProjectileAmmoType;
                    if (string.IsNullOrEmpty(this.m_gun.finalShootAnimation))
                    {
                        this.m_gun.finalShootAnimation = this.m_gun.shootAnimation;
                    }
                }
            }
            if (synergy.SetsBurstCount)
            {
                if (synergy.MakesDefaultModuleBurst && volley.projectiles.Count > 0 && volley.projectiles[0].shootStyle != ProjectileModule.ShootStyle.Burst)
                {
                    volley.projectiles[0].shootStyle = ProjectileModule.ShootStyle.Burst;
                }
                for (int k = 0; k < volley.projectiles.Count; k++)
                {
                    if (volley.projectiles[k].shootStyle != ProjectileModule.ShootStyle.Burst)
                        continue;

                    int burstShotCount = volley.projectiles[k].burstShotCount;
                    int num = volley.projectiles[k].GetModNumberOfShotsInClip(owner);
                    if (num < 0)
                    {
                        num = int.MaxValue;
                    }
                    int burstShotCount2 = Mathf.Clamp(Mathf.RoundToInt((float)burstShotCount * synergy.BurstMultiplier) + synergy.BurstShift, 1, num);
                    volley.projectiles[k].burstShotCount = burstShotCount2;
                }
            }
            if (synergy.AddsPossibleProjectileToPrimaryModule)
            {
                volley.projectiles[0].projectiles.Add(synergy.AdditionalModuleProjectile);
            }
        }
        public List<AdvancedVolleyModificationSynergyData> synergies = new List<AdvancedVolleyModificationSynergyData>();
        private Gun m_gun;
    }

    public class AdvancedVolleyModificationSynergyData : ScriptableObject
    {
        public string RequiredSynergy;
        public bool AddsChargeProjectile;
        public ProjectileModule.ChargeProjectile ChargeProjectileToAdd;
        public bool AddsModules;
        public ProjectileModule[] ModulesToAdd;
        public bool AddsDuplicatesOfBaseModule;
        public int DuplicatesOfBaseModule;
        public float BaseModuleDuplicateAngle = 10f;
        public bool ReplacesSourceProjectile;
        public float ReplacementChance = 1f;
        public bool OnlyReplacesAdditionalProjectiles;
        public Projectile ReplacementProjectile;
        public bool UsesMultipleReplacementProjectiles;
        public bool MultipleReplacementsSequential;
        public Projectile[] MultipleReplacementProjectiles;
        public bool ReplacementSkipsChargedShots;
        public bool SetsNumberFinalProjectiles;
        public int NumberFinalProjectiles = 1;
        public bool AddsNewFinalProjectile;
        public Projectile NewFinalProjectile;
        public string NewFinalProjectileAmmoType;
        public bool SetsBurstCount;
        public bool MakesDefaultModuleBurst;
        public float BurstMultiplier = 1f;
        public int BurstShift;
        public bool AddsPossibleProjectileToPrimaryModule;
        public Projectile AdditionalModuleProjectile;
        public int multipleSequentialReplacementIndex;
    }
}

