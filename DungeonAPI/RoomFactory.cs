using Dungeonator;

using Alexandria.ItemAPI;
using Planetside;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
//using CustomShrineController = GungeonAPI.ShrineFactory.CustomShrineController;
using FloorType = Dungeonator.CellVisualData.CellFloorType;
using Random = UnityEngine.Random;
using Newtonsoft.Json.Linq;
using Gungeon;

namespace Alexandria.DungeonAPI
{
    public static class RoomFactory
    {
        public static Dictionary<string, RoomData> rooms = new Dictionary<string, RoomData>();
        public static bool FailSafeCheck;
        private static readonly string dataHeader = "***DATA***";
        private static FieldInfo m_cellData = typeof(PrototypeDungeonRoom).GetField("m_cellData", BindingFlags.Instance | BindingFlags.NonPublic);
        private static RoomEventDefinition sealOnEnterWithEnemies = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.SEAL_ROOM);
        private static RoomEventDefinition unsealOnRoomClear = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM);

        public static Dictionary<string, RoomData> LoadRoomsFromRoomDirectory(string modPrefix, string roomDirectory)
        {

            var loadedRooms = new Dictionary<string, RoomData>();

            Directory.CreateDirectory(roomDirectory);
            foreach (string g in Directory.GetFiles(roomDirectory, "*", SearchOption.AllDirectories))
            {
                if (g.EndsWith(".room", StringComparison.OrdinalIgnoreCase))
                {
                    string name = Path.GetFullPath(g).RemovePrefix(roomDirectory).RemoveSuffix(".room");
                    ETGModConsole.Log($"Found room: \"{name}\"");
                    var roomData = BuildFromRoomFile(g);
                    DungeonHandler.Register(roomData);
                    rooms.Add(modPrefix + ":" + name, roomData);
                    loadedRooms.Add(name, roomData);
                }
                else if (g.EndsWith(".newroom", StringComparison.OrdinalIgnoreCase))
                {
                    string name = Path.GetFullPath(g).RemovePrefix(roomDirectory).RemoveSuffix(".newroom");
                    ETGModConsole.Log($"New Found room: \"{name}\"");
                    var roomData = BuildFromRoomFileWithoutTexture(g);
                    DungeonHandler.Register(roomData);
                    rooms.Add(modPrefix + ":" + name, roomData);
                    loadedRooms.Add(name, roomData);
                }
            }

            return loadedRooms;
        }

        public static RoomData BuildFromRoomFile(string roomPath)
        {
            var texture = ResourceExtractor.GetTextureFromFile(roomPath, ".room");
            texture.name = Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromFile(roomPath);
            roomData.room = Build(texture, roomData);
            return roomData;
        }

        public static RoomData BuildFromRoomFileWithoutTexture(string roomPath)
        {
            //ETGModConsole.Log(roomPath);
            RoomData roomData = ExtractRoomDataFromFile(roomPath);
            roomData.name = Path.GetFileName(roomPath);
            roomData.room = Build(roomData);
            return roomData;
        }


        public static RoomData BuildFromResource(string roomPath, Assembly assembly = null)
        {
            var texture = ResourceExtractor.GetTextureFromResource(roomPath, assembly ?? Assembly.GetCallingAssembly());
            texture.name = Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromResource(roomPath, assembly ?? Assembly.GetCallingAssembly());
            roomData.room = Build(texture, roomData);
            return roomData;
        }

        public static PrototypeDungeonRoom Build(RoomData roomData)
        {
            try
            {
                var room = CreateRoomFromData(roomData);
                ApplyRoomData(room, roomData);
                room.UpdatePrecalculatedData();
                return room;
            }
            catch (Exception e)
            {
                // Tools.PrintError("Failed to build room!");
                ShrineTools.PrintException(e);
            }

            return CreateEmptyRoom(12, 12);
        }

        public static PrototypeDungeonRoom Build(Texture2D texture, RoomData roomData)
        {
            try
            {
                var room = CreateRoomFromTexture(texture);
                ApplyRoomData(room, roomData);
                room.UpdatePrecalculatedData();
                return room;
            }
            catch (Exception e)
            {
                // Tools.PrintError("Failed to build room!");
                ShrineTools.PrintException(e);
            }

            return CreateEmptyRoom(12, 12);
        }
        public static GameObject MinimapIconPrefab;



        public static void ApplyRoomData(PrototypeDungeonRoom room, RoomData roomData)
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
            if (roomData.enemyPositions != null)
            {
                for (int i = 0; i < roomData.enemyPositions.Length; i++)
                {
                    AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], (roomData.enemyAttributes != null && roomData.enemyAttributes.Length > 0 ? roomData.enemyAttributes[i] : ""), roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions);
                    //AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions); ///GOES FROM HERE==================================================
                }
            }
            ETGModConsole.Log("enemy done");
            if (roomData.placeablePositions != null)
            {
                for (int i = 0; i < roomData.placeablePositions.Length; i++)
                {
                    
                    AddPlaceableToRoom(room, roomData.placeablePositions[i], roomData.placeableGUIDs[i], (roomData.placeableAttributes != null && roomData.placeableAttributes.Length > 0 ? roomData.placeableAttributes[i] : ""));
                    //AddPlaceableToRoom(room, roomData.placeablePositions[i], roomData.placeableGUIDs[i]);
                }
            }
            ETGModConsole.Log("placeable done");
            if (roomData.nodePositions != null)
            {

                Dictionary<string, int> stupidJankyPieceOfShit = new Dictionary<string, int>();

                for (int j = 0; j < roomData.nodeOrder.Length; j++)
                {
                    stupidJankyPieceOfShit.Add($"{roomData.nodePaths[roomData.nodeOrder[j]]}{roomData.nodeOrder[j]}", j);
                }

                for (int j = 0; j < roomData.nodePositions.Length; j++)
                {
                    RoomFactory.AddNodeToRoom(room, roomData.nodePositions[stupidJankyPieceOfShit[$"{roomData.nodePaths[j]}{j}"]], roomData.nodeTypes[stupidJankyPieceOfShit[$"{roomData.nodePaths[j]}{j}"]], roomData.nodePaths[stupidJankyPieceOfShit[$"{roomData.nodePaths[j]}{j}"]]);
                }   
            }
            ETGModConsole.Log("node done");
            if (roomData.floors != null)
            {
                foreach (var floor in roomData.floors)
                {
                    room.prerequisites.Add(new DungeonPrerequisite()
                    {
                        prerequisiteType = DungeonPrerequisite.PrerequisiteType.TILESET,
                        requiredTileset = ShrineTools.GetEnumValue<GlobalDungeonData.ValidTilesets>(floor)
                    });
                }
            }
            //Set categories
            if (!string.IsNullOrEmpty(roomData.category)) room.category = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomCategory>(roomData.category);
            if (!string.IsNullOrEmpty(roomData.normalSubCategory)) room.subCategoryNormal = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomNormalSubCategory>(roomData.normalSubCategory);
            if (!string.IsNullOrEmpty(roomData.bossSubCategory)) room.subCategoryBoss = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomBossSubCategory>(roomData.bossSubCategory);
            if (!string.IsNullOrEmpty(roomData.specialSubCategory)) room.subCategorySpecial = ShrineTools.GetEnumValue<PrototypeDungeonRoom.RoomSpecialSubCategory>(roomData.specialSubCategory);
            room.usesProceduralDecoration = true;
            room.allowFloorDecoration = roomData.doFloorDecoration;
            room.allowWallDecoration = roomData.doWallDecoration;
            room.usesProceduralLighting = roomData.doLighting;

            if (roomData.darkRoom)
            {
                room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.BECOME_TERRIFYING_AND_DARK));
                room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.END_TERRIFYING_AND_DARK));
            }
        }


        public static GameObject Minimap_Maintenance_Icon;
        public static AssetBundle sharedAssets2;



        public static RoomData ExtractRoomDataFromBytes(byte[] data)
        {
            string stringData = ResourceExtractor.BytesToString(data);
            return ExtractRoomData(stringData);
        }

        public static RoomData ExtractRoomDataFromBytesWithoutHeadder(byte[] data)
        {
            string stringData = ResourceExtractor.BytesToString(data);
            return ExtractRoomDataWithoutHeader(stringData);
        }

        public static RoomData ExtractRoomDataFromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return path.EndsWith(".newroom") ? ExtractRoomDataFromBytesWithoutHeadder(data) : ExtractRoomDataFromBytes(data);
        }

        public static RoomData ExtractRoomDataFromResource(string path, Assembly assembly = null)
        {
            byte[] data = ResourceExtractor.ExtractEmbeddedResource(path, assembly ?? Assembly.GetCallingAssembly());
            return ExtractRoomDataFromBytes(data);
        }

        public static RoomData ExtractRoomDataWithoutHeader(string data)
        {
            return JsonUtility.FromJson<RoomData>(data);
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
            ShrineTools.Log($"No room data found at {data}");
            return new RoomData();
        }

        public static PrototypeDungeonRoom CreateRoomFromData(RoomData data)
        {
            

            int width = data.roomSize.x;
            int height = data.roomSize.y;
            PrototypeDungeonRoom room = GetNewPrototypeDungeonRoom(width, height);
            PrototypeDungeonRoomCellData[] cellData = new PrototypeDungeonRoomCellData[width * height];
            //if (data.tilePositions == null) ETGModConsole.Log($"tilePositions not found for room \"{data.name}\"");

            ETGModConsole.Log($"{data.name}: {data.tileInfo.Length} - {width * height}");

            for (int y = 0; y < data.roomSize.y; y++)
            {
                for (int x = 0; x < data.roomSize.x; x++)
                {
                    cellData[x + y * width] = CellDataFromNumber(int.Parse(data.tileInfo[x + (y * width)].ToString()));
                }
            }

            room.FullCellData = cellData;
            room.name = data.name;
            return room;
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


        private static CellType TypeFromNumber(int type)
        {
            Dictionary<int, CellType> types = new Dictionary<int, CellType> { { 1, CellType.FLOOR }, { 2, CellType.WALL }, { 3, CellType.PIT } };

            return types[type];
        }

        public static PrototypeDungeonRoomCellData CellDataFromNumber(int type)
        {
            if (type <= 0) return null;

            var data = new PrototypeDungeonRoomCellData();
            data.state = TypeFromNumber(type);
            //data.diagonalWallType = DiagonalWallTypeFromColor(color);
            data.diagonalWallType = DiagonalWallType.NONE;
            data.appearance = new PrototypeDungeonRoomCellAppearance()
            {
                OverrideFloorType = FloorType.Stone
            };
            return data;
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
                ShrineTools.PrintException(e);
                return null;
            }
        }

        /*public static int GetStyleValue(string dungeonName, string shrineID)
        {
            if (ShrineFactory.registeredShrines != null && ShrineFactory.registeredShrines.ContainsKey(shrineID))
            {
                var shrineData = ShrineFactory.registeredShrines[shrineID]?.GetComponent<CustomShrineController>();
                if (shrineData != null && shrineData.roomStyles != null && shrineData.roomStyles.ContainsKey(dungeonName))
                    return shrineData.roomStyles[dungeonName];
            }
            return -1;
        }*/

        public static void AddPlaceableToRoom(PrototypeDungeonRoom room, Vector2 location, string assetPath, string attributes)
        {
            try
            {
                GameObject gameObject = RoomFactory.GetGameObjectFromBundles(assetPath);
                JObject jobject = null;
                if (!string.IsNullOrEmpty(attributes))
                {
                    jobject = JObject.Parse(attributes);
                    if (assetPath == "customsetupdeadblow")
                    {
                        JToken value;
                        bool followsPlayer = jobject.TryGetValue("fP", out value) && (bool)value;
                        bool persistent = !jobject.TryGetValue("pOOC", out value) || (bool)value;
                        bool facesLeft = jobject.TryGetValue("fL", out value) && (bool)value;
                        bool leaveGoop = !jobject.TryGetValue("lG", out value) || (bool)value;
                        bool fireBullets = !jobject.TryGetValue("fB", out value) || (bool)value;
                        string goopType = jobject.TryGetValue("gT", out value) ? ((string)value) : "fire";
                        float initialDelay = jobject.TryGetValue("iD", out value) ? ((float)value) : 1f;
                        float minDelay = jobject.TryGetValue("minD", out value) ? ((float)value) : 1f;
                        float maxDelay = jobject.TryGetValue("maxD", out value) ? ((float)value) : 1f;
                        gameObject = StaticReferences.DefineDeadBlowFromValues(followsPlayer, persistent, facesLeft, leaveGoop, fireBullets, goopType, initialDelay, minDelay, maxDelay);
                    }
                    if (assetPath == "custom_barrelcustom_barrel")
                    {
                        JToken value2;
                        string baseBarrel = jobject.TryGetValue("bB", out value2) ? ((string)value2) : "water_drum";
                        float rollSpeed = jobject.TryGetValue("rS", out value2) ? ((float)value2) : 3f;
                        bool trailgoop = !jobject.TryGetValue("tG", out value2) || (bool)value2;
                        string trailGoopType = jobject.TryGetValue("tGT", out value2) ? ((string)value2) : "water";
                        float goopTrailWidth = jobject.TryGetValue("tGW", out value2) ? ((float)value2) : 1f;
                        bool doGoopPuddle = !jobject.TryGetValue("pG", out value2) || (bool)value2;
                        string puddleGoopType = jobject.TryGetValue("pGT", out value2) ? ((string)value2) : "water";
                        float goopPuddleWidth = jobject.TryGetValue("pGW", out value2) ? ((float)value2) : 3f;
                        bool destroyedByPlayerRoll = jobject.TryGetValue("dBPR", out value2) && (bool)value2;
                        gameObject = StaticReferences.GenerateCustomBarrel(baseBarrel, rollSpeed, trailgoop, trailGoopType, goopTrailWidth, doGoopPuddle, puddleGoopType, goopPuddleWidth, destroyedByPlayerRoll);
                    }
                    if (assetPath == "minecart_pathing")
                    {
                        JToken value2;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        float timeToMaxSpeed = jobject.TryGetValue("tTMS", out value2) ? ((float)value2) : 1.5f;

                        gameObject = StaticReferences.DefineMinecartFromValues(maxSpeed, timeToMaxSpeed);
                    }

                }
                
                if (!gameObject && StaticReferences.customObjects.ContainsKey(assetPath))
                {
                    gameObject = RoomFactory.GetCustomGameObject(assetPath);
                    //ETGModConsole.Log(gameObject.name);
                }
                else if (!gameObject)
                {
                    gameObject = RoomFactory.GetExoticGameObject(assetPath);
                }



                if (gameObject)
                {
                    if (jobject != null) gameObject = RoomFactory.MaybeModifyAsset(assetPath, jobject, gameObject);
                    DungeonPrerequisite[] array = new DungeonPrerequisite[0];
                    room.placedObjectPositions.Add(location);
                    DungeonPlaceable dungeonPlaceable = ScriptableObject.CreateInstance<DungeonPlaceable>();
                    dungeonPlaceable.width = 2;
                    dungeonPlaceable.height = 2;
                    dungeonPlaceable.respectsEncounterableDifferentiator = true;
                    dungeonPlaceable.variantTiers = new List<DungeonPlaceableVariant>
                    {
                        new DungeonPlaceableVariant
                        {
                            percentChance = 1f,
                            nonDatabasePlaceable = gameObject,
                            prerequisites = array,
                            materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    };
                    int path = 0;

                    if (assetPath.Contains("_pathing") && jobject != null)
                    {
                        JToken value;
                        path = jobject.TryGetValue("tSP", out value) ? ((int)value) : 0;
                        ETGModConsole.Log($"[{assetPath}]");
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = dungeonPlaceable,
                            assignedPathIDx = path
                        });
                    }
                    else
                    {
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = dungeonPlaceable,
                            assignedPathIDx = -1
                        });
                    }

                }
                else
                {
                    DungeonPlaceable placeableFromBundles = RoomFactory.GetPlaceableFromBundles(assetPath);
                    if (placeableFromBundles)
                    {
                        DungeonPrerequisite[] instancePrerequisites = new DungeonPrerequisite[0];
                        room.placedObjectPositions.Add(location);
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = instancePrerequisites,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = placeableFromBundles
                        });
                    }
                    else if (placeableFromBundles == null && StaticReferences.customPlaceables.ContainsKey(assetPath))
                    {
                        DungeonPrerequisite[] instancePrerequisites = new DungeonPrerequisite[0];
                        room.placedObjectPositions.Add(location);
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = instancePrerequisites,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = StaticReferences.customPlaceables[assetPath]
                        });
                    }
                    else
                    {
                        ShrineTools.PrintError<string>("Unable to find asset in asset bundles: " + assetPath, "FF0000");
                    }
                }
            }
            catch (Exception e)
            {
                ShrineTools.PrintException(e, "FF0000");
            }
        }

        public static void AddPlaceableToRoomLegecy(PrototypeDungeonRoom room, Vector2 location, string assetPath)
        {
            try
            {
                //ETGModConsole.Log("LOADING ASSETPATH:" + assetPath);
               


                GameObject asset = GetGameObjectFromBundles(assetPath);
                if (asset)
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
                            nonDatabasePlaceable = asset,
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
                DungeonPlaceable placeable = GetPlaceableFromBundles(assetPath);
                if (placeable)
                {
                    DungeonPrerequisite[] emptyReqs = new DungeonPrerequisite[0];
                    room.placedObjectPositions.Add(location);
                    room.placedObjects.Add(new PrototypePlacedObjectData()
                    {
                        contentsBasePosition = location,
                        fieldData = new List<PrototypePlacedObjectFieldData>(),
                        instancePrerequisites = emptyReqs,
                        linkedTriggerAreaIDs = new List<int>(),
                        placeableContents = placeable
                    });
                    return;
                }
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
                ShrineTools.PrintError($"Unable to find asset in asset bundles OR stored object list: {assetPath}");

            }
            catch (Exception e)
            {
                ShrineTools.PrintException(e);
            }
        }

        public static GameObject MaybeModifyAsset(string assetPath, JObject attributes, GameObject asset)
        {
            GameObject gameObject = asset;
            JToken jtoken;
            bool flag = attributes.TryGetValue("dI", out jtoken) && !string.IsNullOrEmpty(jtoken.ToObject<string>());
            if (flag)
            {
                JToken value = null;
                string text = attributes.TryGetValue("jI", out value) ? ((string)value) : "";
                float overrideMimicChance = attributes.TryGetValue("mC", out value) ? ((float)value) : 0f;
                bool isLocked = !attributes.TryGetValue("cL", out value) || (bool)value;
                bool preventFuse = attributes.TryGetValue("pV", out value) && (bool)value;
                gameObject = FakePrefab.Clone(asset);
                Chest component = gameObject.GetComponent<Chest>();
                bool flag2 = Game.Items.ContainsID(jtoken.ToObject<string>());
                if (flag2)
                {
                    component.forceContentIds = new List<int>
                    {
                        Game.Items[jtoken.ToObject<string>()].PickupObjectId
                    };
                }
                bool flag3 = Game.Items.ContainsID(text);
                if (flag3)
                {
                    component.overrideJunkId = Game.Items[text].PickupObjectId;
                }
                component.overrideMimicChance = overrideMimicChance;
                component.IsLocked = isLocked;
                component.PreventFuse = preventFuse;
            }
            return gameObject;
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


        public static GameObject GetExoticGameObject(string assetPath)
        {
            GameObject result = null;
            GameObject gameObject;
            if (SetupExoticObjects.objects.TryGetValue(assetPath, out gameObject))
            {
                result = gameObject;
            }
            return result;
        }

        public static GameObject GetCustomGameObject(string assetPath)
        {
            GameObject result = null;
            GameObject gameObject;
            if (StaticReferences.customObjects.TryGetValue(assetPath, out gameObject))
            {
                result = gameObject;
            }
            return result;
        }


        public static void AddNodeToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, int layer)
        {
            IntVector2 intLocation = location.ToIntVector2();
            SerializedPath serializedPath = null;

            if (room.paths == null) room.paths = new List<SerializedPath>();

            if (room.paths.Count < layer + 1)
            {
                serializedPath = new SerializedPath(intLocation);

                var node = new SerializedPathNode(intLocation);
                node.placement = (SerializedPathNode.SerializedNodePlacement)Enum.Parse(typeof(SerializedPathNode.SerializedNodePlacement), guid);
                //serializedPath.nodes[0] = node;

                room.paths.Add(serializedPath);
                serializedPath.wrapMode = SerializedPath.SerializedPathWrapMode.Loop;
                serializedPath.tilesetPathGrid = 0;

            }
            else
            {
                serializedPath = room.paths[layer];
                var node = new SerializedPathNode(intLocation);
                node.placement = (SerializedPathNode.SerializedNodePlacement)Enum.Parse(typeof(SerializedPathNode.SerializedNodePlacement), guid);
                serializedPath.nodes.Add(node);
            }

        }

        public static void AddEnemyToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, string attributes, int layer, bool shuffle)
        {
            DungeonPrerequisite[] array = new DungeonPrerequisite[0];

            bool forceBlackPhantom = false;
            if (!string.IsNullOrEmpty(attributes))
            {
                JObject jobject = JObject.Parse(attributes);
                JToken value;
                forceBlackPhantom = jobject.TryGetValue("j", out value) && (bool)value;
            }

            
            DungeonPlaceable dungeonPlaceable = ScriptableObject.CreateInstance<DungeonPlaceable>();
            dungeonPlaceable.width = 1;
            dungeonPlaceable.height = 1;
            dungeonPlaceable.respectsEncounterableDifferentiator = true;
            dungeonPlaceable.variantTiers = new List<DungeonPlaceableVariant>
            {
                new DungeonPlaceableVariant
                {
                    percentChance = 1f,
                    prerequisites = array,
                    forceBlackPhantom = forceBlackPhantom,
                    enemyPlaceableGuid = guid,
                    materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                }
            };
            PrototypePlacedObjectData prototypePlacedObjectData = new PrototypePlacedObjectData
            {
                contentsBasePosition = location,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = array,
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = dungeonPlaceable
            };
            if (layer > 0)
            {
                //ETGModConsole.Log("Adding object to reinforcement layer " + layer, false);
                RoomFactory.AddObjectDataToReinforcementLayer(room, prototypePlacedObjectData, layer - 1, location, shuffle);
            }
            else
            {
                //ETGModConsole.Log("Adding object to standard layer.", false);
                room.placedObjects.Add(prototypePlacedObjectData);
                room.placedObjectPositions.Add(location);
            }
            if (!room.roomEvents.Contains(RoomFactory.sealOnEnterWithEnemies))
            {
                room.roomEvents.Add(RoomFactory.sealOnEnterWithEnemies);
            }
            if (!room.roomEvents.Contains(RoomFactory.unsealOnRoomClear))
            {
                room.roomEvents.Add(RoomFactory.unsealOnRoomClear);
            }
        }

        public static void AddEnemyToRoomLegecy(PrototypeDungeonRoom room, Vector2 location, string guid, int layer, bool shuffle) //TO HERE==================================================
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
            ShrineTools.Print("Data to JSON: " + JsonUtility.ToJson(rd));
        }

        public static void AddInjection(PrototypeDungeonRoom protoroom, string injectionAnnotation, List<ProceduralFlowModifierData.FlowModifierPlacementType> placementRules, float chanceToLock, List<DungeonPrerequisite> prerequisites,
           string injectorName, float selectionWeight = 1, float chanceToSpawn = 1, GameObject addSingularPlaceable = null, float XFromCenter = 0, float YFromCenter = 0)
        {
            if (addSingularPlaceable != null)
            {
                Vector2 offset = new Vector2(-0.75f, -0.75f);
                Vector2 vector = new Vector2((float)(protoroom.Width / 2) + offset.x, (float)(protoroom.Height / 2) + offset.y);

                protoroom.placedObjectPositions.Add(vector);
                DungeonPrerequisite[] array = new DungeonPrerequisite[0];

                GameObject original = addSingularPlaceable;
                DungeonPlaceable placeableContents = ScriptableObject.CreateInstance<DungeonPlaceable>();
                placeableContents.width = 2;
                placeableContents.height = 2;
                placeableContents.respectsEncounterableDifferentiator = true;
                placeableContents.variantTiers = new List<DungeonPlaceableVariant>
            {
                new DungeonPlaceableVariant
                {
                    percentChance = 1f,
                    nonDatabasePlaceable = original,
                    prerequisites = array,
                    materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                }
            };

                protoroom.placedObjects.Add(new PrototypePlacedObjectData
                {

                    contentsBasePosition = vector,
                    fieldData = new List<PrototypePlacedObjectFieldData>(),
                    instancePrerequisites = array,
                    linkedTriggerAreaIDs = new List<int>(),
                    placeableContents = placeableContents

                });
            }
            
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
                selectionWeight = selectionWeight,
                chanceToSpawn = chanceToSpawn,
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
            SharedInjectionData baseInjection = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("Base Shared Injection Data");
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
                ShrineTools.Log("Data to JSON: " + JsonUtility.ToJson(data));
            }
            catch (Exception e)
            {
                ShrineTools.PrintException(e);
            }
        }

        public struct RoomData
        {
            public string tileInfo;
            //public Vector2[] tilePositions;

            public Vector2Int roomSize;

            public string[] waveTriggers;
            public string[] nodeTypes;
            public string[] nodeWrapModes;
            public Vector2[] nodePositions;
            public int[] nodePaths;
            public int[] nodeOrder;
            public string category;
            public string normalSubCategory;
            public string specialSubCategory;
            public string bossSubCategory;
            public Vector2[] enemyPositions;
            public string[] enemyGUIDs;
            public string[] enemyAttributes;
            public Vector2[] placeablePositions;
            public string[] placeableGUIDs;
            public string[] placeableAttributes;
            public int[] enemyReinforcementLayers;
            public Vector2[] exitPositions;
            public string[] exitDirections;
            public string[] floors;
            public float weight;
            public bool isSpecialRoom;
            public bool randomizeEnemyPositions;
            public bool doFloorDecoration;
            public bool doWallDecoration;
            public bool doLighting;
            public bool darkRoom;
            [NonSerialized]
            public string name;
            [NonSerialized]
            public PrototypeDungeonRoom room;
        }

    }
}