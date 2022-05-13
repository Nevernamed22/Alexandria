using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAPI;

namespace SynergyTools
{
    class AdvancedFireOnReloadSynergyProcessor : MonoBehaviour
    {
        public AdvancedFireOnReloadSynergyProcessor()
        {
            synergyToCheck = null;
            projToFire = null;
            numToFire = 1;
            angleVariance = 5;
        }
        private void Awake()
        {
            m_gun = GetComponent<Gun>();
            Gun gun = m_gun;
            gun.OnReloadPressed += Reload;
        }
        private void Reload(PlayerController player, Gun gun, bool manual)
        {
            if (gun.IsReloading && (gun.ClipShotsRemaining < gun.ClipCapacity))
            {
                if (hasFired)
                {
                    return;
                }
                if (player.PlayerHasActiveSynergy(synergyToCheck))
                {
                    for (int i = 0; i < numToFire; i++)
                    {
                        if (projToFire != null)
                        {
                            GameObject gameObject = projToFire.InstantiateAndFireInDirection(gun.barrelOffset.position, gun.CurrentAngle, angleVariance, player);
                            Projectile component = gameObject.GetComponent<Projectile>();
                            if (component != null)
                            {
                                component.Owner = player;
                                component.Shooter = player.specRigidbody;
                                component.PossibleSourceGun = gun;
                                component.baseData.damage *= player.stats.GetStatValue(PlayerStats.StatType.Damage);
                                component.baseData.speed *= player.stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
                                component.baseData.force *= player.stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
                                component.baseData.range *= player.stats.GetStatValue(PlayerStats.StatType.RangeMultiplier);
                                component.BossDamageMultiplier *= player.stats.GetStatValue(PlayerStats.StatType.DamageToBosses);
                                component.AdditionalScaleMultiplier *= player.stats.GetStatValue(PlayerStats.StatType.PlayerBulletScale);
                                component.UpdateSpeed();
                                player.DoPostProcessProjectile(component);
                            }
                        }
                    }
                }
                else
                {
                    //ETGModConsole.Log("Player does not have synergy: " + synergyToCheck);
                }
                hasFired = true;
                if (this.hasFired)
                {
                    player.StartCoroutine(this.HandleReloadDelay(gun));
                }
            }
        }
        private IEnumerator HandleReloadDelay(Gun sourceGun)
        {
            yield return new WaitForSeconds(sourceGun.reloadTime);
            this.hasFired = false;
            yield break;
        }
        private bool hasFired = false;
        [SerializeField]
        public string synergyToCheck;
        private Gun m_gun;
        public Projectile projToFire;
        public int numToFire;
        public float angleVariance;
    }
}
