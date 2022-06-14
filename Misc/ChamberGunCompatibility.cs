using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Alexandria
{
    class ChamberGunCompatibility
    {
        public static void Init()
        {
            Gun ChamberGun = PickupObjectDatabase.GetById(647) as Gun;
            ChamberGunProcessor extantProcessor = ChamberGun.GetComponent<ChamberGunProcessor>();
            if (extantProcessor)
            {

                AdvancedChamberGunController newProcessor = ChamberGun.gameObject.AddComponent<AdvancedChamberGunController>();
                newProcessor.RefillsOnFloorChange = true;

                //KEEP
                newProcessor.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    floorTilesetID = 2,
                    indexValue = 1,
                    correspondingFormeID = 647,
                    viableMasterRounds = new List<int>()
                    {
                        469,
                    }
                });
                //OUBLIETTE
                newProcessor.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    floorTilesetID = 4,
                    indexValue = 1.5f,
                    correspondingFormeID = 647,
                    viableMasterRounds = new List<int>()
                });
                //GUNGEON PROPER
                //ABBEY
                //MINES
                //RAT FLOOR
                //HOLLOW
                //R&G DEPT
                //FORGE
                //BULLET HELL
            }
            else
            {
                Debug.LogError("AlexandriaLib - Chamber Gun: Attempted to modify the processor of the Chamber Gun, but it did not have one?");
            }
        }
    }
    public class AdvancedChamberGunController : MonoBehaviour
    {
        public AdvancedChamberGunController()
        {
        }
        private Gun gun;
        private int currentTileset;
        private void Awake()
        {
            currentTileset = 2; //Keep Tileset ID
            gun = base.GetComponent<Gun>();
            gun.OnReloadPressed += this.HandleReloadPressed;
        }
        private GlobalDungeonData.ValidTilesets GetFloorTileset()
        {
            if (GameManager.Instance.IsLoadingLevel || !GameManager.Instance.Dungeon)
            {
                return GlobalDungeonData.ValidTilesets.CASTLEGEON;
            }
            if (GameManager.Instance.Dungeon.tileIndices == null)
            {
                Debug.LogError("AlexandriaLib - Chamber Gun: Attempted to fetch the tileset of a floor with null tile Indices!");
                return GlobalDungeonData.ValidTilesets.CASTLEGEON;
            }
            return GameManager.Instance.Dungeon.tileIndices.tilesetId;
        } //Returns the Tileset of the current floor
        private bool PlayerHasValidMasterRoundForTileset(PlayerController player, int t)
        {
            foreach (ChamberGunData data in floorFormeDatas)
            {
                if (data.floorTilesetID == t && data.viableMasterRounds.Count() > 0)
                {
                    foreach (int id in data.viableMasterRounds)
                    {
                        if (player.HasPickupID(id)) return true;
                    }
                }
            }
            return false;
        }
        private bool IsValidTileset(GlobalDungeonData.ValidTilesets t)
        {
            return IsValidTileset((int)t);
        }
        private bool IsValidTileset(int t)
        {
            if (t == (int)GetFloorTileset()) return true;
            PlayerController playerController = gun.CurrentOwner as PlayerController;
            if (playerController)
            {
                foreach (ChamberGunData data in floorFormeDatas)
                {
                    if (data.correspondingFormeID != -1 && data.floorTilesetID == t && PlayerHasValidMasterRoundForTileset(playerController, t))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void ChangeToTileset(int t)
        {
            int targetID = -1;
            foreach (ChamberGunData data in floorFormeDatas)
            {
                if (data.floorTilesetID == t) targetID = data.correspondingFormeID;
            }
            if (targetID != -1) ChangeForme(targetID);
            else Debug.LogError("AlexandriaLib - Chamber Gun: Attempted to change to a tileset form that did not have stored data??");
        }
        private void ChangeForme(int targetID)
        {
            Gun targetGun = PickupObjectDatabase.GetById(targetID) as Gun;
            if (targetGun == null) Debug.LogError("AlexandriaLib - Chamber Gun: Attempted to change form to an ID that was either null or not a gun!"); 
            gun.TransformToTargetGun(targetGun);
        }
        private void Update()
        {
            if (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
            {
                return;
            }
            if (gun && (!gun.CurrentOwner || !this.IsValidTileset(currentTileset)))
            {
                GlobalDungeonData.ValidTilesets validTilesets = this.GetFloorTileset();
                if (!gun.CurrentOwner) validTilesets = GlobalDungeonData.ValidTilesets.CASTLEGEON;
                if (currentTileset != (int)validTilesets) this.ChangeToTileset((int)validTilesets);
            }
            this.JustActiveReloaded = false;
        }
        private List<ChamberGunData> ReOrderList(List<ChamberGunData> extantList)
        {
            List<ChamberGunData> tempDatas = new List<ChamberGunData>();
            tempDatas.AddRange(extantList);

            List<ChamberGunData> orderedDatas = new List<ChamberGunData>();
            foreach (ChamberGunData data in tempDatas)
            {
                if (orderedDatas.Count == 0) { orderedDatas.Add(data); }
                else
                {
                    bool placeFound = false;
                    for (int i = 0; i < orderedDatas.Count(); i++)
                    {
                        ChamberGunData orderedItem = orderedDatas[i];
                        if (data.indexValue <= orderedItem.indexValue)
                        {
                            orderedDatas.Insert(i, data);
                            placeFound = true;
                            break;
                        }
                    }
                    if (!placeFound) orderedDatas.Add(data);
                }
            }
            return orderedDatas;
        }
        private List<ChamberGunData> RemoveInvalidEntriesFromList(List<ChamberGunData> extantList)
        {
            List<ChamberGunData> newDatas = new List<ChamberGunData>();
            for (int i = (extantList.Count - 1); i <= 0; i--)
            {
                if (IsValidTileset(extantList[i].floorTilesetID)) newDatas.Add(extantList[i]);
            }
            return newDatas;
        }
        private int FetchNextValidTilesetID(int currentTilesetID)
        {
            List<ChamberGunData> rawData = RemoveInvalidEntriesFromList(floorFormeDatas);
            List<ChamberGunData> validSortedData = ReOrderList(rawData);
            int selectedIteration = -1;
            for (int i = 0; i < validSortedData.Count(); i++)
            {
                if (validSortedData[i].floorTilesetID == currentTilesetID) selectedIteration = i;
            }
            if (selectedIteration > -1)
            {
                if (validSortedData[selectedIteration + 1] != null) return validSortedData[selectedIteration + 1].floorTilesetID;
                else return validSortedData[0].floorTilesetID;
            }
            else return validSortedData[0].floorTilesetID;
        }
        private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool manual)
        {
            if (this.JustActiveReloaded)
            {
                return;
            }
            if (manual && !sourceGun.IsReloading)
            {
                int nextValidTilesetID = FetchNextValidTilesetID((int)currentTileset);
                if ((int)currentTileset != nextValidTilesetID)
                {
                    this.ChangeToTileset(nextValidTilesetID);
                }
            }
        }
        public void BraveOnLevelWasLoaded()
        {
            if (this.RefillsOnFloorChange && gun && gun.CurrentOwner)
            {
                gun.StartCoroutine(this.DelayedRegainAmmo());
            }
        }
        private IEnumerator DelayedRegainAmmo()
        {
            yield return null;
            while (Dungeon.IsGenerating)
            {
                yield return null;
            }
            if (this.RefillsOnFloorChange && gun && gun.CurrentOwner)
            {
                gun.GainAmmo(gun.AdjustedMaxAmmo);
            }
            yield break;
        }

        public bool RefillsOnFloorChange = true;
        public bool JustActiveReloaded;

        public List<ChamberGunData> floorFormeDatas = new List<ChamberGunData>();
        public class ChamberGunData
        {
            public int floorTilesetID = -1;
            public float indexValue = 0;
            public int correspondingFormeID = -1;
            public List<int> viableMasterRounds;
        }
    }
}
