using ItemAPI;
using Dungeonator;
using Ionic.Zip;
using ItemAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using FloorType = Dungeonator.CellVisualData.CellFloorType;
using Random = UnityEngine.Random;

namespace GungeonAPI
{
    public static class RoomFactory
    {
        public static Dictionary<string, RoomData> rooms = new Dictionary<string, RoomData>();
        public static bool FailSafeCheck;
        private static readonly string dataHeader = "***DATA***";
        private static FieldInfo m_cellData = typeof(PrototypeDungeonRoom).GetField("m_cellData", BindingFlags.Instance | BindingFlags.NonPublic);
        private static RoomEventDefinition sealOnEnterWithEnemies = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.SEAL_ROOM);
        private static RoomEventDefinition unsealOnRoomClear = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM);

        public static Dictionary<string, RoomData> LoadRoomsFromRoomDirectory(string modPrefix, string roomDirectory, string unzippedDirectory)
        {

            var loadedRooms = new Dictionary<string, RoomData>();

            if (File.Exists(roomDirectory))
            {

                using (ZipFile zip = ZipFile.Read(roomDirectory))

                    foreach (ZipEntry f in zip.Entries)
                    {
                        if (f.FileName.Contains("rooms") && !FailSafeCheck)
                        {
                            FailSafeCheck = true;
                            //ItemAPI.Tools.PrintNoID("this is being read");
                            var roomData = BuildFromZipFile(roomDirectory);
                            foreach (var roomsData in roomData)
                            {
                                string name = roomsData.room.name;
                                //OtherTools.PrintNoID($"Found room: \"{name}\"");
                                DungeonHandler.Register(roomsData);
                                rooms.Add(roomsData.room.name, roomsData);
                                loadedRooms.Add(modPrefix + ":" + roomsData.room.name, roomsData);

                            }
                        }
                    }
            }
            else
            {
                Directory.CreateDirectory(unzippedDirectory);
                foreach (string g in Directory.GetFiles(unzippedDirectory, "*", SearchOption.AllDirectories))
                {
                    if (g.EndsWith(".room", StringComparison.OrdinalIgnoreCase))
                    {
                        string name = Path.GetFullPath(g).RemovePrefix(unzippedDirectory).RemoveSuffix(".room");
                        //Tools.PrintNoID($"Found room: \"{name}\"");
                        var roomData = BuildFromRoomFile(g);
                        DungeonHandler.Register(roomData);
                        rooms.Add(modPrefix + ":" + name, roomData);
                        loadedRooms.Add(name, roomData);
                    }
                }
            }

            return loadedRooms;
        }

        public static IEnumerable<RoomData> BuildFromZipFile(string zipFilePath)
        {
            if (!ZipFile.IsZipFile(zipFilePath))
            {
               Debug.LogError($"(Alexandria) Not a valid zip file!");
                yield break;
            }
            using (var zipFile = ZipFile.Read(zipFilePath))
            {
                //Tools.PrintNoID("did it work?");
                foreach (var entry in zipFile.Entries)
                {
                    var fileName = Path.GetFileNameWithoutExtension(entry.FileName);
                    var extension = Path.GetExtension(entry.FileName);
                    if (!string.Equals(extension, ".room", StringComparison.OrdinalIgnoreCase))
                        continue;

                    byte[] zipData;
                    int capacity = (int)entry.UncompressedSize;
                    if (capacity < 0)
                        continue;

                    using (var ms = new MemoryStream(capacity))
                    {
                        entry.Extract(ms);
                        zipData = ms.ToArray();
                    }

                    var texture = ResourceExtractor.BytesToTexture(zipData, fileName);
                    var roomData = ExtractRoomDataFromBytes(zipData);

                    if (roomData.waveTriggers == null)
                    {
                        roomData.waveTriggers = new string[0];
                    }

                    roomData.room = Build(texture, roomData);

                    yield return roomData;
                }
            }
        }

        public static RoomData BuildFromRoomFile(string roomPath)
        {
            var texture = ResourceExtractor.GetTextureFromFile(roomPath, ".room");
            texture.name = Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromFile(roomPath);

            if (roomData.waveTriggers == null)
            {
                roomData.waveTriggers = new string[0];
            }

            roomData.room = Build(texture, roomData);
            return roomData;
        }



        public static RoomData BuildFromResource(string roomPath, bool setRoomCategory = false, bool autoAssignToFloor = false, bool defineFullPath = false, bool assignDecorationSettings = false)
        {
            var texture = ResourceExtractor.GetTextureFromResource(roomPath, Assembly.GetCallingAssembly());
            texture.name = Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromResource(roomPath, Assembly.GetCallingAssembly());

            if (roomData.waveTriggers == null)
            {
                roomData.waveTriggers = new string[0];
            }

            roomData.room = Build(texture, roomData, setRoomCategory, autoAssignToFloor, assignDecorationSettings);

            return roomData;
        }

        //public static PrototypeDungeonRoom Build(Texture2D texture, RoomData roomData)
        public static PrototypeDungeonRoom Build(Texture2D texture, RoomData roomData, bool SetRoomCategory = false, bool AutoAssignToFloor = false, bool AssignDecorationProperties = false)
        {
            try
            {
                var room = CreateRoomFromTexture(texture);
                ApplyRoomData(room, roomData, SetRoomCategory, AutoAssignToFloor, AssignDecorationProperties);
                room.OnBeforeSerialize();
                room.OnAfterDeserialize();
                room.UpdatePrecalculatedData();

                if (room.roomEvents == null)
                {
                    room.roomEvents = new List<RoomEventDefinition>();
                }
                if (roomData.isDarkRoom)
                {
                    room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.BECOME_TERRIFYING_AND_DARK));
                    room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.END_TERRIFYING_AND_DARK));
                }

                return room;
            }
            catch (Exception e)
            {
                 Debug.LogError("(Alexandria) Failed to build room!");
                Debug.LogException(e);
            }

            return CreateEmptyRoom(12, 12);
        }
        public static GameObject MinimapIconPrefab;

        public static void ApplyRoomData(PrototypeDungeonRoom room, RoomData roomData, bool setRoomCategory, bool autoAssignToFloor, bool assignDecorationProperties)
        {
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

            if (roomData.musicState == "NONE")
            {
                room.UseCustomMusicSwitch = false;
            }
            else if (!string.IsNullOrEmpty(roomData.musicState))
            {
                room.UseCustomMusicSwitch = true;
                room.OverrideMusicState = GetRoomMusicState(roomData.musicState);
            }

            if (roomData.enemyPositions != null)
            {
                if (roomData.waveTriggers != null && roomData.waveTriggers.Length > 0)
                {
                    for (int i = 0; i < roomData.enemyPositions.Length; i++)
                    {
                        AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], roomData.waveTriggers[i], roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions);
                    }
                }
                else
                {
                    for (int i = 0; i < roomData.enemyPositions.Length; i++)
                    {
                        AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], "", roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions);
                    }
                }

            }

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
                if (!string.IsNullOrEmpty(roomData.category)) room.category = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomCategory>(roomData.category);
                if (!string.IsNullOrEmpty(roomData.normalSubCategory)) room.subCategoryNormal = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomNormalSubCategory>(roomData.normalSubCategory);
                if (!string.IsNullOrEmpty(roomData.bossSubCategory)) room.subCategoryBoss = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomBossSubCategory>(roomData.bossSubCategory);
                if (!string.IsNullOrEmpty(roomData.specialSubCategory)) room.subCategorySpecial = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomSpecialSubCategory>(roomData.specialSubCategory);
            }
            if (autoAssignToFloor && roomData.floors != null)
            {
                //if (!Weight.HasValue)
                //{
                //    if (room.category == PrototypeDungeonRoom.RoomCategory.SECRET)
               //     {
                //        Weight = 15; // Normal secret rooms have weight of 15.
                //    }
                 //   else
                 //   {
                 //       Weight = 1;
                  //  }
                //}
                foreach (var floor in roomData.floors)
                {
                    room.prerequisites.Add(new DungeonPrerequisite()
                    {
                        prerequisiteType = DungeonPrerequisite.PrerequisiteType.TILESET,
                        requiredTileset = ShrineTools.GetEnumValue<GlobalDungeonData.ValidTilesets>(floor)
                    });
                }
            }

            room.overrideRoomVisualType = roomData.visualSubtypes;

            if (assignDecorationProperties)
            {
                room.allowFloorDecoration = roomData.doFloorDecoration;
                room.allowWallDecoration = roomData.doWallDecoration;
                room.usesProceduralLighting = roomData.doLighting;
            }


        }

        public static DungeonFloorMusicController.DungeonMusicState GetRoomMusicState(string val)
        {
            return (DungeonFloorMusicController.DungeonMusicState)Enum.Parse(typeof(DungeonFloorMusicController.DungeonMusicState), val.ToUpper());
        }


        public static GameObject Minimap_Maintenance_Icon;
        public static AssetBundle sharedAssets2;



        public static RoomData ExtractRoomDataFromBytes(byte[] data)
        {
            string stringData = ResourceExtractor.BytesToString(data);
            return ExtractRoomData(stringData);
        }

        public static RoomData ExtractRoomDataFromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return ExtractRoomDataFromBytes(data);
        }

        public static RoomData ExtractRoomDataFromResource(string path, Assembly assembly = null)
        {
            byte[] data = ResourceExtractor.ExtractEmbeddedResource(path, assembly ?? Assembly.GetCallingAssembly());
            return ExtractRoomDataFromBytes(data);
        }

        public static RoomData ExtractRoomData(string data)
        {
            int end = data.Length - dataHeader.Length - 1;
            for (int i = end; i > 0; i--)
            {
                string sub = data.Substring(i, dataHeader.Length);
                if (sub.Equals(dataHeader))
                    return JsonUtility.FromJson<RoomData>(data.Substring(i + dataHeader.Length));
            }
            //ShrineTools.Log($"No room data found at {data}");
            return new RoomData();
        }

        public static PrototypeDungeonRoom CreateRoomFromTexture(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            PrototypeDungeonRoom room = GetNewPrototypeDungeonRoom(width, height);
            PrototypeDungeonRoomCellData[] cellData = new PrototypeDungeonRoomCellData[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cellData[x + y * width] = CellDataFromColor(texture.GetPixel(x, y));
                }
            }
            
            room.FullCellData = cellData;
            room.name = texture.name;
            return room;
        }

        public static PrototypeDungeonRoomCellData CellDataFromColor(Color32 color)
        {
            if (color.Equals(Color.magenta)) return null;

            var data = new PrototypeDungeonRoomCellData();
            data.state = TypeFromColor(color);
            data.diagonalWallType = DiagonalWallTypeFromColor(color);
            data.appearance = new PrototypeDungeonRoomCellAppearance()
            {
                OverrideFloorType = FloorType.Stone
            };
            return data;
        }

        public static CellType TypeFromColor(Color color)
        {
            if (color == Color.black)
                return CellType.PIT;
            else if (color == Color.white)
                return CellType.FLOOR;
            else
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

        public static RoomData CreateEmptyRoomData(int width = 12, int height = 12)
        {
            RoomData data = new RoomData()
            {
                room = CreateEmptyRoom(width, height),
                category = "SECRET",
                weight = 9999
            };

            return data;
        }

        public static PrototypeDungeonRoom CreateEmptyRoom(int width = 12, int height = 12)
        {
            try
            {
                PrototypeDungeonRoom room = GetNewPrototypeDungeonRoom(width, height);
                AddExit(room, new Vector2(width / 2, height), DungeonData.Direction.NORTH);
                AddExit(room, new Vector2(width / 2, 0), DungeonData.Direction.SOUTH);
                AddExit(room, new Vector2(width, height / 2), DungeonData.Direction.EAST);
                AddExit(room, new Vector2(0, height / 2), DungeonData.Direction.WEST);

                PrototypeDungeonRoomCellData[] cellData = m_cellData.GetValue(room) as PrototypeDungeonRoomCellData[];
                cellData = new PrototypeDungeonRoomCellData[width * height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        cellData[x + y * width] = new PrototypeDungeonRoomCellData()
                        {
                            state = CellType.FLOOR,
                            appearance = new PrototypeDungeonRoomCellAppearance()
                            {
                                OverrideFloorType = CellVisualData.CellFloorType.Stone,
                            },
                        };
                    }
                }
                m_cellData.SetValue(room, cellData);

                room.UpdatePrecalculatedData();
                return room;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        /*public static int GetStyleValue(string dungeonName, string shrineID)
        {
            if (OldShrineFactory.builtShrines != null && OldShrineFactory.builtShrines.ContainsKey(shrineID))
            {
                var shrineData = OldShrineFactory.builtShrines[shrineID]?.GetComponent<CustomShrineController>();
                if (shrineData != null && shrineData.roomStyles != null && shrineData.roomStyles.ContainsKey(dungeonName))
                    return shrineData.roomStyles[dungeonName];
            }
            return -1;
        }*/

        public static void AddPlaceableToRoom(PrototypeDungeonRoom room, Vector2 location, string assetPath)
        {
            try
            {
                if (assetPath == "Challenge_Shrine")
                {
                    if (ShrineTools.GetRoomFromBundles("ChallengeShrine_Gungeon_002") != null)
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
                            nonDatabasePlaceable = ShrineTools.GetRoomFromBundles("ChallengeShrine_Gungeon_002").placedObjects[0].nonenemyBehaviour.gameObject,
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
                }
                else if (GetGameObjectFromBundles(assetPath) != null)
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
                    Debug.LogError("(Alexandria) Unable to find asset in asset bundles: " + assetPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        

        

        public static DungeonPlaceable GetPlaceableFromBundles(string assetPath)
        {
            DungeonPlaceable placeable = null;
            foreach (var bundle in StaticReferences.AssetBundles.Values)
            {
                placeable = bundle.LoadAsset<DungeonPlaceable>(assetPath);
                if (placeable)
                    break;

            }
            return placeable;
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

        public static GameObject GetGameObjectFromBundles(string assetPath)
        {
            GameObject asset = null;
            foreach (var bundle in StaticReferences.AssetBundles.Values)
            {
                asset = bundle.LoadAsset<GameObject>(assetPath);
                if (asset)
                    break;
            }
            return asset;
        }


        public static void AddEnemyToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, string trigger, int layer, bool shuffle)
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
                if (string.IsNullOrEmpty(trigger))
                {
                    AddObjectDataToReinforcementLayer(room, objectData, layer - 1, location, shuffle);
                }
                else
                {
                    AddObjectDataToReinforcementLayer(room, objectData, layer - 1, location, shuffle, ShrineTools.GetEnumValue<RoomEventTriggerCondition>(trigger));
                }

            }
            else
            {
                room.placedObjects.Add(objectData);
                room.placedObjectPositions.Add(location);
            }

            if (!room.roomEvents.Contains(sealOnEnterWithEnemies)) { room.roomEvents.Add(sealOnEnterWithEnemies); }
            if (!room.roomEvents.Contains(unsealOnRoomClear)) { room.roomEvents.Add(unsealOnRoomClear); }
        }

        public static void AddObjectDataToReinforcementLayer(PrototypeDungeonRoom room, PrototypePlacedObjectData objectData, int layer, Vector2 location, bool shuffle, RoomEventTriggerCondition trigger)
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
                        shuffle = shuffle,
                        reinforcementTriggerCondition = trigger,

                    };
                    room.additionalObjectLayers.Add(newLayer);
                }
            }
            room.additionalObjectLayers[layer].placedObjects.Add(objectData);
            room.additionalObjectLayers[layer].placedObjectBasePositions.Add(location);
        }


        public static void AddEnemyToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, int layer, bool shuffle) ///TO HERE==================================================
        {
            DungeonPrerequisite[] emptyReqs = new DungeonPrerequisite[0];
            var placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
            placeableContents.width = 1;
            placeableContents.height = 1;
            placeableContents.respectsEncounterableDifferentiator = true;
            placeableContents.variantTiers = new List<DungeonPlaceableVariant>()
            {
                new DungeonPlaceableVariant()
                {
                    percentChance = 1,
                    prerequisites = emptyReqs,
                    enemyPlaceableGuid = guid,
                    materialRequirements= new DungeonPlaceableRoomMaterialRequirement[0],
                }
            };

            var objectData = new PrototypePlacedObjectData()
            {
                contentsBasePosition = location,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = emptyReqs,
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = placeableContents,
            };


            if (layer > 0)
                AddObjectDataToReinforcementLayer(room, objectData, layer - 1, location, shuffle); //GOES FROM HERE
            else
            {
                room.placedObjects.Add(objectData);
                room.placedObjectPositions.Add(location);
            }

            if (!room.roomEvents.Contains(sealOnEnterWithEnemies))
                room.roomEvents.Add(sealOnEnterWithEnemies);
            if (!room.roomEvents.Contains(unsealOnRoomClear))
                room.roomEvents.Add(unsealOnRoomClear);
        }

        public static void AddObjectDataToReinforcementLayer(PrototypeDungeonRoom room, PrototypePlacedObjectData objectData, int layer, Vector2 location, bool shuffle) //TO HERE
        {
            if (room.additionalObjectLayers.Count <= layer)
            {
                for (int i = room.additionalObjectLayers.Count; i <= layer; i++)
                {
                    var newLayer = new PrototypeRoomObjectLayer()
                    {
                        layerIsReinforcementLayer = true,
                        placedObjects = new List<PrototypePlacedObjectData>(),
                        placedObjectBasePositions = new List<Vector2>(),
                        shuffle = shuffle,
                        //reinforcementTriggerCondition = RoomEventTriggerCondition.ON_HALF_ENEMY_HP_DEPLETED
                    };
                    room.additionalObjectLayers.Add(newLayer);

                }
            }
            room.additionalObjectLayers[layer].placedObjects.Add(objectData);
            room.additionalObjectLayers[layer].placedObjectBasePositions.Add(location);
        }

        public static void AddExit(PrototypeDungeonRoom room, Vector2 location, DungeonData.Direction direction)
        {
            if (room.exitData == null)
                room.exitData = new PrototypeRoomExitData();
            if (room.exitData.exits == null)
                room.exitData.exits = new List<PrototypeRoomExit>();

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
            room.paths = new List<SerializedPath>();
            room.prerequisites = new List<DungeonPrerequisite>();
            room.excludedOtherRooms = new List<PrototypeDungeonRoom>();
            room.rectangularFeatures = new List<PrototypeRectangularFeature>();
            room.exitData = new PrototypeRoomExitData();
            room.exitData.exits = new List<PrototypeRoomExit>();
            room.allowWallDecoration = false;
            room.allowFloorDecoration = false;
            room.Width = width;
            room.Height = height;
            return room;
        }

        public static void LogExampleRoomData()
        {
            Vector2[] vectorArray = new Vector2[]
            {
                new Vector2(4, 4),
                new Vector2(4, 14),
                new Vector2(14, 4),
                new Vector2(14, 14),
            };
            string[] guids = new string[]
            {
                "01972dee89fc4404a5c408d50007dad5",
                "7b0b1b6d9ce7405b86b75ce648025dd6",
                "ffdc8680bdaa487f8f31995539f74265",
                "01972dee89fc4404a5c408d50007dad5",
            };

            Vector2[] exits = new Vector2[]
            {
                new Vector2(0, 9),
                new Vector2(9, 0),
                new Vector2(20, 9),
                new Vector2(9, 20),
            };

            string[] dirs = new string[]
            {
                "EAST", "SOUTH", "WEST",  "NORTH"
            };

            RoomData rd = new RoomData()
            {
                enemyPositions = vectorArray,
                enemyGUIDs = guids,
                exitPositions = exits,
                exitDirections = dirs,

            };
            Debug.Log("(Alexandria) Data to JSON: " + JsonUtility.ToJson(rd));
        }

        public static void AddInjection(PrototypeDungeonRoom protoroom, string injectionAnnotation, List<ProceduralFlowModifierData.FlowModifierPlacementType> placementRules, float chanceToLock, List<DungeonPrerequisite> prerequisites,
                   string injectorName)
        {


            ProceduralFlowModifierData injection = new ProceduralFlowModifierData()
            {
                annotation = injectionAnnotation,
                DEBUG_FORCE_SPAWN = false,
                OncePerRun = false,
                placementRules = new List<ProceduralFlowModifierData.FlowModifierPlacementType>(placementRules),
                roomTable = null,
                exactRoom = protoroom,
                IsWarpWing = false,
                RequiresMasteryToken = false,
                chanceToLock = chanceToLock,
                selectionWeight = 1,
                chanceToSpawn = 1,
                RequiredValidPlaceable = null,
                prerequisites = prerequisites.ToArray(),
                CanBeForcedSecret = true,
                RandomNodeChildMinDistanceFromEntrance = 0,
                exactSecondaryRoom = null,
                framedCombatNodes = 0,
            };
            SharedInjectionData injector = ScriptableObject.CreateInstance<SharedInjectionData>();
            injector.UseInvalidWeightAsNoInjection = true;
            injector.PreventInjectionOfFailedPrerequisites = false;
            injector.IsNPCCell = false;
            injector.IgnoreUnmetPrerequisiteEntries = false;
            injector.OnlyOne = false;
            injector.ChanceToSpawnOne = 0.5f;
            injector.AttachedInjectionData = new List<SharedInjectionData>();
            injector.InjectionData = new List<ProceduralFlowModifierData>
            {
                injection
            };
            injector.name = injectorName;
            SharedInjectionData baseInjection = StaticReferences.LoadAssetFromAnywhere<SharedInjectionData>("Base Shared Injection Data");
            if (baseInjection.AttachedInjectionData == null)
            {
                baseInjection.AttachedInjectionData = new List<SharedInjectionData>();
            }
            baseInjection.AttachedInjectionData.Add(injector);
            BaseInjection = baseInjection;
        }

        private static SharedInjectionData BaseInjection;

        public static void StraightLine()
        {
            try
            {
                Vector2[] enemyPositions = new Vector2[100];
                string[] enemyGuids = new string[100];
                int[] enemyLayers = new int[100];
                for (int i = 0; i < enemyGuids.Length; i++)
                {
                    var db = EnemyDatabase.Instance.Entries;
                    int r = Random.Range(0, db.Count);
                    enemyGuids[i] = db[r].encounterGuid;
                    enemyPositions[i] = new Vector2(i * 2, 10);
                    enemyLayers[i] = 0;
                }

                Vector2[] exits = new Vector2[]
                {
                new Vector2(0, 9),
                new Vector2(200, 9),
                };

                string[] dirs = new string[]
                {
                    "WEST", "EAST"
                };

                RoomData data = new RoomData()
                {
                    enemyPositions = enemyPositions,
                    enemyGUIDs = enemyGuids,
                    enemyReinforcementLayers = enemyLayers,
                    exitPositions = exits,
                    exitDirections = dirs,
                };
                Debug.Log("(Alexandria) Data to JSON: " + JsonUtility.ToJson(data));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public struct RoomData
        {
            public string category;
            public string normalSubCategory;
            public string specialSubCategory;
            public string bossSubCategory;
            public string musicState;
            public Vector2[] enemyPositions;
            public string[] enemyGUIDs;
            public Vector2[] placeablePositions;
            public string[] placeableGUIDs;
            public int[] enemyReinforcementLayers;
            public Vector2[] exitPositions;
            public string[] exitDirections;
            public string[] floors;
            public float weight;
            public int visualSubtypes;
            public bool isSpecialRoom;
            public bool isDarkRoom;
            public string[] waveTriggers;
            public bool randomizeEnemyPositions, doFloorDecoration, doWallDecoration, doLighting;
            [NonSerialized]
            public PrototypeDungeonRoom room;
        }

    }
}