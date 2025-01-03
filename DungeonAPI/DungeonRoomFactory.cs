﻿using Dungeonator;

using Alexandria.ItemAPI;
using Planetside;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
//using CustomShrineController = Alexandria.DungeonAPI.ShrineFactory.CustomShrineController;
using FloorType = Dungeonator.CellVisualData.CellFloorType;
using Random = UnityEngine.Random;
namespace Alexandria.DungeonAPI
{
    public static class DungeonRoomFactory
    {
        public static readonly string dataHeader = "***DATA***";

        /*public static AssetBundle sharedAssets;
        public static AssetBundle sharedAssets2;
        public static AssetBundle braveResources;*/

        private static RoomEventDefinition sealOnEnterWithEnemies = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.SEAL_ROOM);
        private static RoomEventDefinition unsealOnRoomClear = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM);

        // private static string [] m_AssetBundleNames = new string[] { "shared_auto_001", "shared_auto_002", "brave_resources_001" };

        public struct RoomData
        {
            public string category;
            public string normalSubCategory;
            public string specialSubCategory;
            public string bossSubCategory;
            public Vector2[] enemyPositions;
            public string[] enemyGUIDs;
            public Vector2[] placeablePositions;
            public string[] placeableGUIDs;
            public int[] enemyReinforcementLayers;
            public Vector2[] exitPositions;
            public string[] exitDirections;
            public string[] floors;
            public float weight;
            public bool isSpecialRoom;
            public bool randomizeEnemyPositions, doFloorDecoration, doWallDecoration, doLighting;
            [NonSerialized]
            public PrototypeDungeonRoom room;
        }


        public static PrototypeDungeonRoom BuildFromResource(string roomPath, bool setRoomCategory = false, bool autoAssignToFloor = false, bool defineFullPath = false, bool assignDecorationSettings = false)
        {
            string RoomPath = (roomPath);
            if (defineFullPath) { RoomPath = roomPath; }
            Texture2D texture = ResourceExtractor.GetTextureFromResource(RoomPath);
            RoomData roomData = ExtractRoomDataFromResource(RoomPath);
            return Build(texture, roomData, setRoomCategory, autoAssignToFloor, assignDecorationSettings, roomData.weight);
        }

        public static PrototypeDungeonRoom Build(Texture2D texture, RoomData roomData, bool SetRoomCategory, bool AutoAssignToFloor, bool AssignDecorationProperties, float? Weight)
        {
            try
            {
                PrototypeDungeonRoom room = CreateRoomFromTexture(texture);
                ApplyRoomData(room, roomData, SetRoomCategory, AutoAssignToFloor, AssignDecorationProperties, Weight);
                room.OnBeforeSerialize();
                room.OnAfterDeserialize();
                room.UpdatePrecalculatedData();
                return room;
            }
            catch (Exception e)
            {
                Tools.PrintError("Failed to build room!");
                Tools.PrintException(e);
            }
            return CreateEmptyRoom(12, 12);
        }

        public static void ApplyRoomData(PrototypeDungeonRoom room, RoomData roomData, bool setRoomCategory, bool autoAssignToFloor, bool assignDecorationProperties, float? Weight)
        {
            // Tools.Print("Building Exits...");
            if (roomData.exitPositions != null)
            {
                for (int i = 0; i < roomData.exitPositions.Length; i++)
                {
                    DungeonData.Direction dir = (DungeonData.Direction)Enum.Parse(typeof(DungeonData.Direction), roomData.exitDirections[i].ToUpper());
                    AddExit(room, roomData.exitPositions[i], dir);
                }
            }
            else
            {
                AddExit(room, new Vector2(room.Width / 2, room.Height), DungeonData.Direction.NORTH);
                AddExit(room, new Vector2(room.Width / 2, 0), DungeonData.Direction.SOUTH);
                AddExit(room, new Vector2(room.Width, room.Height / 2), DungeonData.Direction.EAST);
                AddExit(room, new Vector2(0, room.Height / 2), DungeonData.Direction.WEST);
            }

            // Tools.Print("Adding Enemies...");
            if (roomData.enemyPositions != null)
            {
                for (int i = 0; i < roomData.enemyPositions.Length; i++)
                {
                    AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions);
                }
            }

            // Tools.Print("Adding Objects...");
            if (roomData.placeablePositions != null)
            {
                for (int i = 0; i < roomData.placeablePositions.Length; i++)
                {
                    AddPlaceableToRoom(room, roomData.placeablePositions[i], roomData.placeableGUIDs[i]);
                }
            }

            if (setRoomCategory | autoAssignToFloor)
            {
                // Set categories
                if (!string.IsNullOrEmpty(roomData.category)) { room.category = GetRoomCategory(roomData.category); }
                if (!string.IsNullOrEmpty(roomData.normalSubCategory)) { room.subCategoryNormal = GetRoomNormalSubCategory(roomData.normalSubCategory); }
                if (!string.IsNullOrEmpty(roomData.bossSubCategory)) { room.subCategoryBoss = GetRoomBossSubCategory(roomData.bossSubCategory); }
                if (!string.IsNullOrEmpty(roomData.specialSubCategory)) { room.subCategorySpecial = GetRoomSpecialSubCategory(roomData.specialSubCategory); }
            }
            if (autoAssignToFloor && roomData.floors != null)
            {
                if (!Weight.HasValue)
                {
                    if (room.category == PrototypeDungeonRoom.RoomCategory.SECRET)
                    {
                        Weight = 15; // Normal secret rooms have weight of 15.
                    }
                    else
                    {
                        Weight = 1;
                    }
                }
                if (room.category == PrototypeDungeonRoom.RoomCategory.SECRET)
                {
                    // Secret rooms are generally shared across all floors via a specific room table.
                    // Room Editor doesn't currently set this for secret rooms
                    room.OverrideMusicState = DungeonFloorMusicController.DungeonMusicState.CALM;
                    Planetside.ModPrefabs.SecretRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(room, Weight.Value));
                }
                else
                {
                    foreach (string floor in roomData.floors) { AssignRoomToFloorRoomTable(room, GetTileSet(floor), Weight); }
                    /*ExpandPrefabs.CustomRoomTableSecretGlitchFloor.includedRooms.elements.Add(ExpandRoomPrefabs.GenerateWeightedRoom(room, Weight.Value));
                    ExpandPrefabs.CustomRoomTable.includedRooms.elements.Add(ExpandRoomPrefabs.GenerateWeightedRoom(room, Weight.Value));
                    ExpandPrefabs.CustomRoomTable2.includedRooms.elements.Add(ExpandRoomPrefabs.GenerateWeightedRoom(room, Weight.Value));*/
                }
            }

            if (assignDecorationProperties)
            {
                room.allowFloorDecoration = roomData.doFloorDecoration;
                room.allowWallDecoration = roomData.doWallDecoration;
                room.usesProceduralLighting = roomData.doLighting;
            }
        }

        public static void AssignRoomToFloorRoomTable(PrototypeDungeonRoom Room, GlobalDungeonData.ValidTilesets targetTileSet, float? Weight)
        {
            if (targetTileSet == GlobalDungeonData.ValidTilesets.CASTLEGEON)
            {
                Planetside.ModPrefabs.CastleRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.SEWERGEON)
            {
                Planetside.ModPrefabs.SewersRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.GUNGEON)
            {
                Planetside.ModPrefabs.Gungeon_RoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.CATHEDRALGEON)
            {
                Planetside.ModPrefabs.AbbeyRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.MINEGEON)
            {
                Planetside.ModPrefabs.MinesRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.CATACOMBGEON)
            {
                Planetside.ModPrefabs.CatacombsRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.OFFICEGEON)
            {
                // R&G floor doesn't use room tables yet. If a room has this set, I'll assign to all floors instead.
                return;
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.FORGEGEON)
            {
                Planetside.ModPrefabs.ForgeRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            else if (targetTileSet == GlobalDungeonData.ValidTilesets.HELLGEON)
            {
                Planetside.ModPrefabs.BulletHellRoomTable.includedRooms.elements.Add(Planetside.ModRoomPrefabs.GenerateWeightedRoom(Room, Weight.Value));
            }
            return;
        }


        public static GlobalDungeonData.ValidTilesets GetTileSet(string val)
        {
            return (GlobalDungeonData.ValidTilesets)Enum.Parse(typeof(GlobalDungeonData.ValidTilesets), val.ToUpper());
        }
        public static PrototypeDungeonRoom.RoomCategory GetRoomCategory(string val)
        {
            return (PrototypeDungeonRoom.RoomCategory)Enum.Parse(typeof(PrototypeDungeonRoom.RoomCategory), val.ToUpper());
        }
        public static PrototypeDungeonRoom.RoomNormalSubCategory GetRoomNormalSubCategory(string val)
        {
            return (PrototypeDungeonRoom.RoomNormalSubCategory)Enum.Parse(typeof(PrototypeDungeonRoom.RoomNormalSubCategory), val.ToUpper());
        }
        public static PrototypeDungeonRoom.RoomBossSubCategory GetRoomBossSubCategory(string val)
        {
            return (PrototypeDungeonRoom.RoomBossSubCategory)Enum.Parse(typeof(PrototypeDungeonRoom.RoomBossSubCategory), val.ToUpper());
        }
        public static PrototypeDungeonRoom.RoomSpecialSubCategory GetRoomSpecialSubCategory(string val)
        {
            return (PrototypeDungeonRoom.RoomSpecialSubCategory)Enum.Parse(typeof(PrototypeDungeonRoom.RoomSpecialSubCategory), val.ToUpper());
        }

        public static RoomData ExtractRoomDataFromFile(string path)
        {
            string data = ResourceExtractor.BytesToString(File.ReadAllBytes(path));
            return ExtractRoomData(data);
        }

        public static RoomData ExtractRoomDataFromResource(string path)
        {
            path = path.Replace("/", ".");
            path = path.Replace("\\", ".");
            string data = ResourceExtractor.BytesToString(ResourceExtractor.ExtractEmbeddedResource((path)));
            return ExtractRoomData(data);
        }

        public static RoomData ExtractRoomData(string data)
        {
            int num = (data.Length - dataHeader.Length - 1);
            for (int i = num; i > 0; i--)
            {
                string text = data.Substring(i, dataHeader.Length);
                if (text.Equals(dataHeader)) { return JsonUtility.FromJson<RoomData>(data.Substring(i + dataHeader.Length)); }
            }
            Tools.Log("No room data found at " + data);
            return default(RoomData);
        }

        public static PrototypeDungeonRoom CreateRoomFromTexture(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            PrototypeDungeonRoom room = GetNewPrototypeDungeonRoom(width, height);
            room.FullCellData = new PrototypeDungeonRoomCellData[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    room.FullCellData[x + y * width] = CellDataFromColor(texture.GetPixel(x, y));
                }
            }
            room.name = texture.name;
            return room;
        }

        public static PrototypeDungeonRoomCellData CellDataFromColor(Color32 color)
        {
            // if (color.Equals(Color.magenta)) return null;

            PrototypeDungeonRoomCellData data = new PrototypeDungeonRoomCellData();
            data.state = TypeFromColor(color);
            data.diagonalWallType = DiagonalWallTypeFromColor(color);
            data.appearance = new PrototypeDungeonRoomCellAppearance() { OverrideFloorType = CellVisualData.CellFloorType.Stone };

            if (color == Color.red)
            {
                data.appearance.OverrideFloorType = CellVisualData.CellFloorType.Carpet;
                data.appearance.IsPhantomCarpet = true;
            }

            return data;
        }

        public static CellType TypeFromColor(Color color)
        {
            if (color == Color.black)
            {
                return CellType.PIT;
            }
            else if (color == Color.white | color == Color.red)
            {
                return CellType.FLOOR;
            }
            return CellType.WALL;
        }

        public static DiagonalWallType DiagonalWallTypeFromColor(Color color)
        {
            if (color == Color.red)
                return DiagonalWallType.NORTHEAST;
            else if (color == Color.green)
                return DiagonalWallType.SOUTHEAST;
            else if (color == Color.blue)
                return DiagonalWallType.SOUTHWEST;
            else if (color == Color.yellow)
                return DiagonalWallType.NORTHWEST;
            else
                return DiagonalWallType.NONE;
        }

        public static PrototypeDungeonRoom CreateEmptyRoom(int width = 12, int height = 12)
        {
            try
            {
                Tools.Print("  Create Empty Room...", "5599FF");
                PrototypeDungeonRoom room = GetNewPrototypeDungeonRoom(width, height);
                AddExit(room, new Vector2(width / 2, height), DungeonData.Direction.NORTH);
                AddExit(room, new Vector2(width / 2, 0), DungeonData.Direction.SOUTH);
                AddExit(room, new Vector2(width, height / 2), DungeonData.Direction.EAST);
                AddExit(room, new Vector2(0, height / 2), DungeonData.Direction.WEST);

                room.FullCellData = new PrototypeDungeonRoomCellData[width * height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        room.FullCellData[x + y * width] = new PrototypeDungeonRoomCellData()
                        {
                            state = CellType.FLOOR,
                            appearance = new PrototypeDungeonRoomCellAppearance()
                            {
                                OverrideFloorType = CellVisualData.CellFloorType.Stone,
                            },
                        };
                    }
                }

                room.OnBeforeSerialize();
                room.OnAfterDeserialize();
                room.UpdatePrecalculatedData();
                return room;
            }
            catch (Exception e)
            {
                Tools.PrintException(e);
                return null;
            }
        }

        public static GameObject GetGameObjectFromBundles(string assetPath)
        {
            GameObject gameObject = null;
            foreach (AssetBundle assetBundle in StaticReferences.AssetBundles.Values)
            {
                gameObject = assetBundle.LoadAsset<GameObject>(assetPath);
                if (gameObject) break;
            }
            return gameObject;
        }

        public static DungeonPlaceable GetPlaceableFromBundles(string assetPath)
        {
            DungeonPlaceable dungeonPlaceable = null;
            foreach (AssetBundle assetBundle in StaticReferences.AssetBundles.Values)
            {
                dungeonPlaceable = assetBundle.LoadAsset<DungeonPlaceable>(assetPath);
                if (dungeonPlaceable) break;
            }
            return dungeonPlaceable;
        }

        public static void AddEnemyToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, int layer, bool shuffle)
        {
            DungeonPlaceable placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
            placeableContents.width = 1;
            placeableContents.height = 1;
            placeableContents.respectsEncounterableDifferentiator = true;
            placeableContents.variantTiers = new List<DungeonPlaceableVariant>() {
                new DungeonPlaceableVariant() {
                    percentChance = 1,
                    prerequisites = new DungeonPrerequisite[0],
                    enemyPlaceableGuid = guid,
                    materialRequirements= new DungeonPlaceableRoomMaterialRequirement[0],
                }
            };

            PrototypePlacedObjectData objectData = new PrototypePlacedObjectData()
            {
                contentsBasePosition = location,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = new DungeonPrerequisite[0],
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = placeableContents,
            };

            if (layer > 0)
            {
                AddObjectDataToReinforcementLayer(room, objectData, layer - 1, location, shuffle);
            }
            else
            {
                room.placedObjects.Add(objectData);
                room.placedObjectPositions.Add(location);
            }

            if (!room.roomEvents.Contains(sealOnEnterWithEnemies)) { room.roomEvents.Add(sealOnEnterWithEnemies); }
            if (!room.roomEvents.Contains(unsealOnRoomClear)) { room.roomEvents.Add(unsealOnRoomClear); }
        }

        public static void AddObjectDataToReinforcementLayer(PrototypeDungeonRoom room, PrototypePlacedObjectData objectData, int layer, Vector2 location, bool shuffle)
        {
            if (room.additionalObjectLayers.Count <= layer)
            {
                for (int i = room.additionalObjectLayers.Count; i <= layer; i++)
                {
                    PrototypeRoomObjectLayer newLayer = new PrototypeRoomObjectLayer
                    {
                        layerIsReinforcementLayer = true,
                        placedObjects = new List<PrototypePlacedObjectData>(),
                        placedObjectBasePositions = new List<Vector2>(),
                        shuffle = shuffle
                    };
                    room.additionalObjectLayers.Add(newLayer);
                }
            }
            room.additionalObjectLayers[layer].placedObjects.Add(objectData);
            room.additionalObjectLayers[layer].placedObjectBasePositions.Add(location);
        }

        public static void AddPlaceableToRoom(PrototypeDungeonRoom room, Vector2 location, string assetPath)
        {
            //ETGModConsole.Log(assetPath);
            try
            {
                if (GetGameObjectFromBundles(assetPath) != null)
                {
                    DungeonPrerequisite[] array = new DungeonPrerequisite[0];
                    room.placedObjectPositions.Add(location);
                    DungeonPlaceable dungeonPlaceable = ScriptableObject.CreateInstance<DungeonPlaceable>();
                    dungeonPlaceable.width = 2;
                    dungeonPlaceable.height = 2;
                    dungeonPlaceable.respectsEncounterableDifferentiator = true;
                    dungeonPlaceable.variantTiers = new List<DungeonPlaceableVariant> {
                        new DungeonPlaceableVariant {
                            percentChance = 1f,
                            nonDatabasePlaceable = GetGameObjectFromBundles(assetPath),
                            prerequisites = array,
                            materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    };
                    room.placedObjects.Add(new PrototypePlacedObjectData
                    {
                        contentsBasePosition = location,
                        fieldData = new List<PrototypePlacedObjectFieldData>(),
                        instancePrerequisites = array,
                        linkedTriggerAreaIDs = new List<int>(),
                        placeableContents = dungeonPlaceable
                    });
                    return;
                }
                else if (GetPlaceableFromBundles(assetPath) != null)
                {
                    DungeonPrerequisite[] instancePrerequisites = new DungeonPrerequisite[0];
                    room.placedObjectPositions.Add(location);
                    room.placedObjects.Add(new PrototypePlacedObjectData
                    {
                        contentsBasePosition = location,
                        fieldData = new List<PrototypePlacedObjectFieldData>(),
                        instancePrerequisites = instancePrerequisites,
                        linkedTriggerAreaIDs = new List<int>(),
                        placeableContents = GetPlaceableFromBundles(assetPath)
                    });
                    return;
                }
                else
                {
                    GameObject asset1 = GetGameObjectFromStoredObjects(assetPath);
                    if (asset1)
                    {
                        DungeonPrerequisite[] emptyReqs = new DungeonPrerequisite[0];
                        room.placedObjectPositions.Add(location);

                        var placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
                        placeableContents.width = 2;
                        placeableContents.height = 2;
                        placeableContents.respectsEncounterableDifferentiator = true;
                        placeableContents.variantTiers = new List<DungeonPlaceableVariant>()
                    {
                        new DungeonPlaceableVariant()
                        {
                                percentChance = 1,
                                nonDatabasePlaceable = asset1.gameObject,
                                prerequisites = emptyReqs,
                                materialRequirements= new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    };

                        room.placedObjects.Add(new PrototypePlacedObjectData()
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = emptyReqs,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = placeableContents
                        });
                        //Tools.Print($"Added {asset.name} to room.");
                        return;
                    }
                    DungeonPlaceable placeableOne = GetDungeonPlaceableFromStoredObjects(assetPath);
                    if (placeableOne)
                    {
                        DungeonPrerequisite[] emptyReqs = new DungeonPrerequisite[0];
                        room.placedObjectPositions.Add(location);
                        room.placedObjects.Add(new PrototypePlacedObjectData()
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = emptyReqs,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = placeableOne
                        });
                        return;
                    }
                    else
                    {
                        if (!_AggregateMissingAssetErrors)
                            ShrineTools.PrintError<string>("Unable to find asset in asset bundles: " + assetPath, "FF0000");
                        else if (_MissingAssetsByGuid.ContainsKey(assetPath))
                            ++_MissingAssetsByGuid[assetPath];
                        else
                            _MissingAssetsByGuid[assetPath] = 1;
                    }
                }
                
              
            }
            catch (Exception e)
            {
                Tools.PrintException(e);
            }
        }

        public static GameObject GetGameObjectFromStoredObjects(string assetPath)
        {
            GameObject asset = null;
            foreach (var objectlist in StaticReferences.StoredRoomObjects)
            {
                if (assetPath == objectlist.Key)
                {
                    asset = objectlist.Value;
                    if (asset)
                        break;
                }
            }
            return asset;
        }

        public static DungeonPlaceable GetDungeonPlaceableFromStoredObjects(string assetPath)
        {
            DungeonPlaceable asset = null;
            foreach (var objectlist in StaticReferences.StoredDungeonPlaceables)
            {
                if (assetPath == objectlist.Key)
                {
                    asset = objectlist.Value;
                    if (asset)
                        break;
                }
            }
            return asset;
        }

        public static void AddExit(PrototypeDungeonRoom room, Vector2 location, DungeonData.Direction direction)
        {
            if (room.exitData == null) { room.exitData = new PrototypeRoomExitData(); }
            if (room.exitData.exits == null) { room.exitData.exits = new List<PrototypeRoomExit>(); }

            PrototypeRoomExit exit = new PrototypeRoomExit(direction, location);
            exit.exitType = PrototypeRoomExit.ExitType.NO_RESTRICTION;
            Vector2 margin = (direction == DungeonData.Direction.EAST || direction == DungeonData.Direction.WEST) ? new Vector2(0, 1) : new Vector2(1, 0);
            exit.containedCells.Add(location + margin);
            room.exitData.exits.Add(exit);
        }

        public static PrototypeDungeonRoom GetNewPrototypeDungeonRoom(int width = 12, int height = 12)
        {
            PrototypeDungeonRoom room = ScriptableObject.CreateInstance<PrototypeDungeonRoom>();
            room.injectionFlags = new RuntimeInjectionFlags();
            room.RoomId = UnityEngine.Random.Range(10000, 1000000);
            room.pits = new List<PrototypeRoomPitEntry>();
            room.placedObjects = new List<PrototypePlacedObjectData>();
            room.placedObjectPositions = new List<Vector2>();
            room.additionalObjectLayers = new List<PrototypeRoomObjectLayer>();
            room.eventTriggerAreas = new List<PrototypeEventTriggerArea>();
            room.roomEvents = new List<RoomEventDefinition>();
            room.overriddenTilesets = 0;
            room.paths = new List<SerializedPath>();
            room.prerequisites = new List<DungeonPrerequisite>();
            room.excludedOtherRooms = new List<PrototypeDungeonRoom>();
            room.rectangularFeatures = new List<PrototypeRectangularFeature>();
            room.exitData = new PrototypeRoomExitData();
            room.exitData.exits = new List<PrototypeRoomExit>();
            room.Width = width;
            room.Height = height;

            // Not originally defined as defaults in Kyle's code.
            room.GUID = Guid.NewGuid().ToString();
            room.category = PrototypeDungeonRoom.RoomCategory.NORMAL;
            room.subCategoryBoss = PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS;
            room.subCategoryNormal = PrototypeDungeonRoom.RoomNormalSubCategory.COMBAT;
            room.subCategorySpecial = PrototypeDungeonRoom.RoomSpecialSubCategory.STANDARD_SHOP;
            room.subCategorySecret = PrototypeDungeonRoom.RoomSecretSubCategory.UNSPECIFIED_SECRET;
            room.usesProceduralDecoration = true;
            room.usesProceduralLighting = true;
            room.allowWallDecoration = true;
            room.allowFloorDecoration = true;
            room.PreventMirroring = false;
            room.InvalidInCoop = false;
            room.preventAddedDecoLayering = false;
            room.precludeAllTilemapDrawing = false;
            room.drawPrecludedCeilingTiles = false;
            room.preventBorders = false;
            room.preventFacewallAO = false;
            room.usesCustomAmbientLight = false;
            room.customAmbientLight = Color.white;
            room.ForceAllowDuplicates = false;
            room.injectionFlags = new RuntimeInjectionFlags() { CastleFireplace = false, ShopAnnexed = false };
            room.IsLostWoodsRoom = false;
            room.UseCustomMusic = false;
            room.UseCustomMusicState = false;
            room.CustomMusicEvent = string.Empty;
            room.UseCustomMusicSwitch = false;
            room.CustomMusicSwitch = string.Empty;
            room.overrideRoomVisualTypeForSecretRooms = false;
            room.rewardChestSpawnPosition = new IntVector2(-1, -1);
            room.overrideRoomVisualType = -1;

            return room;
        }

        public static Dictionary<string, DungeonRoomFactory.RoomData> rooms = new Dictionary<string, DungeonRoomFactory.RoomData>();
    }

}
