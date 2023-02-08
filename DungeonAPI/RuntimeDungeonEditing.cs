using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    class RuntimeDungeonEditing
    {
        public static RoomHandler AddCustomRuntimeRoom(Dungeon dungeon, IntVector2 dimensions, GameObject roomPrefab, IntVector2? roomWorldPositionOverride = null, Vector3? roomPrefabPositionOverride = null)
        {
            IntVector2 RoomPosition = new IntVector2(10, 10);
            if (roomWorldPositionOverride.HasValue) { RoomPosition = roomWorldPositionOverride.Value; }
            IntVector2 intVector = new IntVector2(dungeon.data.Width + RoomPosition.x, RoomPosition.y);
            int newWidth = dungeon.data.Width + RoomPosition.x + dimensions.x;
            int newHeight = Mathf.Max(dungeon.data.Height, dimensions.y + RoomPosition.y);
            CellData[][] array = BraveUtility.MultidimensionalArrayResize(dungeon.data.cellData, dungeon.data.Width, dungeon.data.Height, newWidth, newHeight);
            CellArea cellArea = new CellArea(intVector, dimensions, 0);
            cellArea.IsProceduralRoom = true;
            dungeon.data.cellData = array;
            dungeon.data.ClearCachedCellData();
            RoomHandler roomHandler = new RoomHandler(cellArea);
            for (int i = 0; i < dimensions.x; i++)
            {
                for (int j = 0; j < dimensions.y; j++)
                {
                    IntVector2 p = new IntVector2(i, j) + intVector;
                    CellData cellData = new CellData(p, CellType.FLOOR);
                    cellData.parentArea = cellArea;
                    cellData.parentRoom = roomHandler;
                    cellData.nearestRoom = roomHandler;
                    array[p.x][p.y] = cellData;
                    roomHandler.RuntimeStampCellComplex(p.x, p.y, CellType.FLOOR, DiagonalWallType.NONE);

                }
            }
            dungeon.data.rooms.Add(roomHandler);
            if (roomPrefabPositionOverride.HasValue)
            {
                float X = roomPrefabPositionOverride.Value.x;
                float Y = roomPrefabPositionOverride.Value.x;
                UnityEngine.Object.Instantiate(roomPrefab, new Vector3(intVector.x + X, intVector.y + Y, 0f), Quaternion.identity);
            }
            else
            {
                UnityEngine.Object.Instantiate(roomPrefab, new Vector3(intVector.x, intVector.y, 0f), Quaternion.identity);
            }
            DeadlyDeadlyGoopManager.ReinitializeData();
            return roomHandler;
        }
    }
}