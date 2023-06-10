using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    public class AdvancedGunFormeSynergyProcessor : MonoBehaviour
    {

        private void Awake()
        {
            m_gun = base.GetComponent<Gun>();
            m_gun.OnReloadPressed += HandleReloadPressed;
            currentFormeData = Formes[0];
        }
        private void Update()
        {
            if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave || m_gun == null || !m_gun.enabled) { return; }
            if (m_gun)
            {
                if (m_gun.GunPlayerOwner() != null)
                {
                    if (FunctioningAsTransform && CurrentForme == 0)
                    {
                        int nextValidForme = GetNextValidForme(m_gun.GunPlayerOwner());
                        ChangeForme(Formes[nextValidForme]);
                        CurrentForme = nextValidForme;
                    }
                    if (currentFormeData != null && m_gun.GunPlayerOwner() && !currentFormeData.IsValid(m_gun.GunPlayerOwner()) && CurrentForme != 0)
                    {
                        ChangeForme(Formes[0]);
                        CurrentForme = 0;
                    }

                }
                else if (CurrentForme != 0)
                {
                    ChangeForme(Formes[0]);
                    CurrentForme = 0;
                }
            }
            JustActiveReloaded = false;
        }

        private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool manual)
        {
            if (JustActiveReloaded || FunctioningAsTransform) { return; }

            if (manual && !sourceGun.IsReloading)
            {
                int nextValidForme = GetNextValidForme(ownerPlayer);
                if (nextValidForme != CurrentForme)
                {
                    ChangeForme(Formes[nextValidForme]);
                    CurrentForme = nextValidForme;
                }
            }
        }

        public bool FunctioningAsTransform
        {
            get
            {
                AdvancedGunFormeData forme = GetFirstNonDefaultValidForme;
                if (NumberOfValidFormes == 2 && forme != null && forme.defaultInvalidIfOnlyForm) return true;
                else return false;
            }
        }
        public AdvancedGunFormeData GetFirstNonDefaultValidForme
        {
            get
            {
                for (int i = 0; i < Formes.Count; i++) { if (i != 0 && Formes[i].IsValid(m_gun.GunPlayerOwner())) { return Formes[i]; } }
                return null;
            }
        }
        public int NumberOfValidFormes { get { return Formes.ToList().FindAll(x => x.IsValid(m_gun.GunPlayerOwner())).Count(); } }
        public int GetNextValidForme(PlayerController ownerPlayer)
        {
            for (int i = 0; i < Formes.Count; i++)
            {
                int num = (i + CurrentForme) % Formes.Count;
                if (num != CurrentForme)
                {
                    if (Formes[num].IsValid(ownerPlayer)) { return num; }
                }
            }
            return CurrentForme;
        }
        private void ChangeForme(AdvancedGunFormeData targetForme)
        {
            Gun gun = PickupObjectDatabase.GetById(targetForme.FormeID) as Gun;
            m_gun.TransformToTargetGun(gun);
            if (m_gun.encounterTrackable && gun.encounterTrackable)
            {
                m_gun.encounterTrackable.journalData.PrimaryDisplayName = gun.encounterTrackable.journalData.PrimaryDisplayName;
                m_gun.encounterTrackable.journalData.ClearCache();
                PlayerController playerController = m_gun.CurrentOwner as PlayerController;
                if (playerController) { GameUIRoot.Instance.TemporarilyShowGunName(playerController.IsPrimaryPlayer); }
            }
            currentFormeData = targetForme;
        }

        public List<AdvancedGunFormeData> Formes;
        public AdvancedGunFormeData currentFormeData;
        private Gun m_gun;
        private int CurrentForme;
        public bool JustActiveReloaded;
    }
    public class AdvancedGunFormeData : ScriptableObject
    {
        public bool IsValid(PlayerController p)
        {
            if (!RequiresSynergy) return true;
            if (EnumTypeSynergy) { return p.HasActiveBonusSynergy(RequiredSynergyEnum, false); }
            else { return p.PlayerHasActiveSynergy(RequiredSynergyString); }
        }


        public bool defaultInvalidIfOnlyForm = true;
        public bool RequiresSynergy = true;
        public bool EnumTypeSynergy = false;
        public string RequiredSynergyString;
        public CustomSynergyType RequiredSynergyEnum;
        public int FormeID;
    }
}
