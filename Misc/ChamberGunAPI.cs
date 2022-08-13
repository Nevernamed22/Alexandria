using Dungeonator;
using Gungeon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class ChamberGunAPI
    {
        public static void Init()
        {
            Gun ChamberGun = PickupObjectDatabase.GetById(647) as Gun;
            ChamberGunProcessor extantProcessor = ChamberGun.GetComponent<ChamberGunProcessor>();
            if (extantProcessor)
            {

                AdvancedChamberGunController newProcessor = ChamberGun.gameObject.AddComponent<AdvancedChamberGunController>();
                newProcessor.RefillsOnFloorChange = true;

                if (AdvancedChamberGunController.floorFormeDatas == null)
                {
                    AdvancedChamberGunController.floorFormeDatas = new List<AdvancedChamberGunController.ChamberGunData>();
                }

                #region SetupVanillaFloors
                //KEEP
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 2,
                    indexValue = 1,
                    correspondingFormeID = 647,
                    viableMasterRounds = new List<int>()
                    {
                        469,
                    }
                });
                //OUBLIETTE
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 4,
                    indexValue = 1.5f,
                    correspondingFormeID = 657,
                    viableMasterRounds = new List<int>()
                    {

                    }
                });
                //GUNGEON PROPER
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 1,
                    indexValue = 2,
                    correspondingFormeID = 660,
                    viableMasterRounds = new List<int>()
                    {
                        471,
                    }
                });
                //ABBEY
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 8,
                    indexValue = 2.5f,
                    correspondingFormeID = 806,
                    viableMasterRounds = new List<int>()
                    {
                    }
                });
                //MINES
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 16,
                    indexValue = 3,
                    correspondingFormeID = 807,
                    viableMasterRounds = new List<int>()
                    {
                        468,
                    }
                });
                //RAT FLOOR
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 32768,
                    indexValue = 3.5f,
                    correspondingFormeID = 808,
                    viableMasterRounds = new List<int>()
                    {
                    }
                });
                //HOLLOW
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 32,
                    indexValue = 4,
                    correspondingFormeID = 659,
                    viableMasterRounds = new List<int>()
                    {
                        470,
                    }
                });
                //R&G DEPT
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 2048,
                    indexValue = 4.5f,
                    correspondingFormeID = 823,
                    viableMasterRounds = new List<int>()
                    {
                    }
                });
                //FORGE
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 64,
                    indexValue = 5,
                    correspondingFormeID = 658,
                    viableMasterRounds = new List<int>()
                    {
                        467,
                    }
                });
                //BULLET HELL
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = "Vanilla Gungeon",
                    floorTilesetID = 128,
                    indexValue = 6,
                    correspondingFormeID = 763,
                    viableMasterRounds = new List<int>()
                    {
                    }
                });
                #endregion

                UnityEngine.Object.Destroy(extantProcessor);
            }
            else
            {
                Debug.LogError($"Alexandria - ChamberGunAPI: Alexandria did not alter the Chamber Gun, as the Chamber Gun was already altered by someone else. THIS IS BAD.");
            }
        }
        public static void AddAsChamberGunForme(this Gun gun, string modName, int targetFloorTilesetID, List<int> viableMasterRounds, float index)
        {
            if (gun != null && gun.PickupObjectId > 0)
            {
                if (viableMasterRounds == null) viableMasterRounds = new List<int>();
                AdvancedChamberGunController.floorFormeDatas.Add(new AdvancedChamberGunController.ChamberGunData()
                {
                    modName = modName,
                    floorTilesetID = targetFloorTilesetID,
                    indexValue = index,
                    correspondingFormeID = 647,
                    viableMasterRounds = viableMasterRounds
                });
            }
            else Debug.LogWarning($"Alexandria - ChamberGunAPI: {modName} attempted to add a chamber gun form with an invalid id.");
        }
        private static List<MidInitChGunMastery> masteryFormsForDelayedLoad;
        public class MidInitChGunMastery
        {
            public string modName = "Unset";
            public int floorTilesetID = -1;
            public int itemID = -1;
        }
        public static void AddAsChamberGunMastery(this PickupObject item, string modName, int targetFloorTilesetID)
        {
            if (masteryFormsForDelayedLoad == null) masteryFormsForDelayedLoad = new List<MidInitChGunMastery>();
            masteryFormsForDelayedLoad.Add(new MidInitChGunMastery()
            {
                modName = modName,
                floorTilesetID = targetFloorTilesetID,
                itemID = item.PickupObjectId
            });
        }
        public static void DelayedInit()
        {
            foreach (MidInitChGunMastery midinit in masteryFormsForDelayedLoad)
            {
                bool foundTileset = false;
                foreach (AdvancedChamberGunController.ChamberGunData data in AdvancedChamberGunController.floorFormeDatas)
                {
                    if (data.floorTilesetID == midinit.floorTilesetID)
                    {
                        //If the ids match, add the mastery id to that form's list of valid mastery IDs
                        if (data.viableMasterRounds == null) data.viableMasterRounds = new List<int>() { };
                        data.viableMasterRounds.Add(midinit.itemID);
                        foundTileset = true;
                    }
                }
                if (!foundTileset)
                {
                    //If the code is unable to find a valid id in the form list, spit out this error
                    Debug.LogError($"Alexandria - ChamberGunAPI: Mod {midinit.modName} failed to add viable Master Round with ID {midinit.itemID} because the target floor id ({midinit.floorTilesetID}) does not exist in the custom forme list.");
                }
            }
        }

    }
    public class AdvancedChamberGunController : MonoBehaviour
    {
        public AdvancedChamberGunController()
        {
            hyperDebugMode = false;
        }
        public static bool hyperDebugMode;
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
                Debug.LogError($"Alexandria - ChamberGunAPI: Tried to get the tileset on a dungeon that was still loading, or null, so the Keep was chosen as a fallback.");
                return GlobalDungeonData.ValidTilesets.CASTLEGEON;
            }
            if (GameManager.Instance.Dungeon.tileIndices == null)
            {
                Debug.LogError($"Alexandria - ChamberGunAPI: Tried to get the tileset on a dungeon with NULL tile indeces (like a past), so the Keep was chosen as a fallback.");
                return GlobalDungeonData.ValidTilesets.CASTLEGEON;
            }
            return GameManager.Instance.Dungeon.tileIndices.tilesetId;
        } //Returns the Tileset of the current floor
        private bool PlayerHasValidMasterRoundForTileset(PlayerController player, int t)
        {
            foreach (ChamberGunData data in floorFormeDatas)
            {
                if (data.floorTilesetID == t && data.viableMasterRounds != null && data.viableMasterRounds.Count() > 0)
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
            if (gun.CurrentOwner && gun.CurrentOwner is PlayerController)
            {
                PlayerController playerController = gun.CurrentOwner as PlayerController;
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
            if (hyperDebugMode) ETGModConsole.Log($"Attempting to Change to Tileset {t}. FloorFormeDatas is at a count of {floorFormeDatas.Count}.");

            int targetID = -1;
            foreach (ChamberGunData data in floorFormeDatas)
            {
                if (hyperDebugMode) ETGModConsole.Log($"Checking ChamberGunData with tileset id {data.floorTilesetID}.");
                if (data.floorTilesetID == t)
                {
                    if (hyperDebugMode) ETGModConsole.Log($"ChamberGunData with tileset id {data.floorTilesetID} MATCHES {t}, and targetID has been set to it.");
                    targetID = data.correspondingFormeID;
                    break;
                }
                else
                {
                    if (hyperDebugMode) ETGModConsole.Log($"ChamberGunData with tileset id {data.floorTilesetID} does NOT match with {t}.");
                }
            }
            if (targetID != -1)
            {
                ChangeForme(targetID); currentTileset = t;
            }
            else Debug.LogWarning($"Alexandria - ChamberGunAPI: Attempted to change form to a tileset that wasn't valid ({t}).");
        }
        private void ChangeForme(int targetID)
        {
            Gun targetGun = PickupObjectDatabase.GetById(targetID) as Gun;
            if (targetGun == null) Debug.LogError($"Alexandria - ChamberGunAPI: Attempted to change form to an id that is either null or not a gun! ({targetID}).");

            if (hyperDebugMode) ETGModConsole.Log($"Changing to gun id {targetID}, with a gunhandedness of: {targetGun.gunHandedness}.");

            gun.TransformToTargetGun(targetGun);
            gun.gunHandedness = targetGun.gunHandedness;
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
            if (hyperDebugMode) ETGModConsole.Log($"Attempting to remove invalid forme entries from the list of extant data, counting {extantList.Count}.");

            List<ChamberGunData> newDatas = new List<ChamberGunData>();
            foreach (ChamberGunData data in extantList)
            {
                if (hyperDebugMode) ETGModConsole.Log($"Checking tileset id {data.floorTilesetID}...");
                if (IsValidTileset(data.floorTilesetID))
                {
                    if (hyperDebugMode) ETGModConsole.Log("Tileset was Valid!");
                    newDatas.Add(data);
                }
            }
            return newDatas;
        }
        private int FetchNextValidTilesetID(int currentTilesetID)
        {
            if (hyperDebugMode) ETGModConsole.Log("Fetching next forme id");
            List<ChamberGunData> rawData = RemoveInvalidEntriesFromList(floorFormeDatas);
            if (hyperDebugMode) ETGModConsole.Log($"RawData count: {rawData.Count}");

            List<ChamberGunData> validSortedData = ReOrderList(rawData);
            if (hyperDebugMode) ETGModConsole.Log($"Valid Sorted Data count: {validSortedData.Count}");

            //Determines what form the gun is currently in by iterating through all the valid forms and seeing which ones line up
            int selectedIteration = -1;
            for (int i = 0; i < validSortedData.Count(); i++)
            {
                if (validSortedData[i].floorTilesetID == currentTilesetID)
                {
                    selectedIteration = i;
                    if (hyperDebugMode) ETGModConsole.Log($"Gun has been determined to be in position {i} of the heirarchy with form {currentTilesetID}.");
                }

            }
            //If the gun is actually in a form on the list, proceed
            if (selectedIteration > -1)
            {
                if (selectedIteration < (validSortedData.Count - 1))
                {
                    //If the current positioning on the list is less than the sum of all positions (-1 to compensate for the Count/index inconsistency), then we can
                    //   proceed to the next position, if it's not less than, meaning it's either at the highest value or above, we fallback to the first gun.
                    return validSortedData[selectedIteration + 1].floorTilesetID;
                }
                else return validSortedData[0].floorTilesetID; //Fallback to first gun in the heirarchy
            }
            else return validSortedData[0].floorTilesetID; //Return position 0 on the form list as a fallback if the gun's not in a valid position
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

        public static List<ChamberGunData> floorFormeDatas;
        public class ChamberGunData
        {
            public string modName = "Unset";
            public int floorTilesetID = -1;
            public float indexValue = 0;
            public int correspondingFormeID = -1;
            public List<int> viableMasterRounds;
        }
    }
}