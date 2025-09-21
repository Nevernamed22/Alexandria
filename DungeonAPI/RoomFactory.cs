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
using Microsoft.Cci;
using System.Linq;
using static Alexandria.DungeonAPI.RoomFactory;
using System.Runtime.Remoting.Lifetime;
using static ShrineCost;
using static Alexandria.DungeonAPI.SpecialComponents;
using HutongGames.PlayMaker.Actions;
using Alexandria.NPCAPI;
using System.Collections;
using static PrototypeRoomExit;
using Alexandria.EnemyAPI;
using HutongGames.Utility;
using FullInspector;
using Alexandria.Misc;
using com.subjectnerd;
using Pathfinding;

namespace Alexandria.DungeonAPI
{
    public static class RoomFactory
    {
        public static List<string> roomNames = new List<string>();
        public static Dictionary<string, RoomData> rooms = new Dictionary<string, RoomData>();
        public static bool FailSafeCheck;
        private static readonly string dataHeader = "***DATA***";
        private static RoomEventDefinition sealOnEnterWithEnemies = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.SEAL_ROOM);
        private static RoomEventDefinition unsealOnRoomClear = new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.UNSEAL_ROOM);
        internal static bool _AggregateMissingAssetErrors = false;
        internal static readonly Dictionary<string, int> _MissingAssetsByGuid = new Dictionary<string, int>();

        /// <summary>
        /// Loads all rooms in a given folder, similar to how Gun Sprites are setup (Example: LoadRoomsFromRoomDirectory("Alex", this.FolderPath() + "/newRooms");
        /// </summary>
        /// <param name="modPrefix"></param>
        /// <param name="roomDirectory"></param>
        /// <returns></returns>
        public static Dictionary<string, RoomData> LoadRoomsFromRoomDirectory(string modPrefix, string roomDirectory)
        {
            _AggregateMissingAssetErrors = true;
            _MissingAssetsByGuid.Clear();
            var loadedRooms = new Dictionary<string, RoomData>();
            Directory.CreateDirectory(roomDirectory);
            foreach (string g in Directory.GetFiles(roomDirectory, "*", SearchOption.AllDirectories))
            {
                if (g.EndsWith(".room", StringComparison.OrdinalIgnoreCase))
                {
                    string name = System.IO.Path.GetFullPath(g).RemovePrefix(roomDirectory).RemoveSuffix(".room");
                    if (RoomUtility.EnableDebugLogging == true) 
                    {
                        ETGModConsole.Log($"Found room: \"{name}\"");
                    }
                    var roomData = BuildFromRoomFile(g);
                    DungeonHandler.Register(roomData);
                    if (!rooms.ContainsKey(modPrefix + ":" + name))
                    {
                        rooms.Add(modPrefix + ":" + name, roomData);
                    }
                    if (!loadedRooms.ContainsKey(modPrefix + ":" + name))
                    {
                        loadedRooms.Add(modPrefix + ":" + name, roomData);
                    }
                }
                else if (g.EndsWith(".newroom", StringComparison.OrdinalIgnoreCase))
                {
                    string name = System.IO.Path.GetFullPath(g).RemovePrefix(roomDirectory).RemoveSuffix(".newroom");
                    if (RoomUtility.EnableDebugLogging == true)
                    {
                        ETGModConsole.Log($"New Found room: \"{name}\"");
                    }
                    var roomData = BuildFromRoomFileWithoutTexture(g);
                    DungeonHandler.Register(roomData);
                    if (!rooms.ContainsKey(modPrefix + ":" + name))
                    {
                        rooms.Add(modPrefix + ":" + name, roomData);
                    }
                    if (!loadedRooms.ContainsKey(modPrefix + ":" + name))
                    {
                        loadedRooms.Add(modPrefix + ":" + name, roomData);
                    }
                }
            }

            foreach (KeyValuePair<string, int> kvp in _MissingAssetsByGuid)
                ShrineTools.PrintError<string>($"Unable to find asset in asset bundles: {kvp.Key} (x{kvp.Value})", "FF0000");
            _MissingAssetsByGuid.Clear();
            _AggregateMissingAssetErrors = false;
            return loadedRooms;
        }

        private static RoomData BuildFromRoomFile(string roomPath)
        {
            var texture = ResourceExtractor.GetTextureFromFile(roomPath, ".room");
            texture.name = System.IO.Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromFile(roomPath);
            roomData.room = Build(texture, roomData);

            return roomData;
        }


        private static RoomData BuildFromRoomFileWithoutTexture(string roomPath)
        {
            //ETGModConsole.Log(roomPath);
            RoomData roomData = ExtractRoomDataFromFile(roomPath);
            roomData.name = System.IO.Path.GetFileName(roomPath);
            roomData.room = Build(roomData);
            PostProcessCells(roomData);

            return roomData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomPath"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static RoomData BuildFromResource(string roomPath, Assembly assembly = null)
        {
            var texture = ResourceExtractor.GetTextureFromResource(roomPath, assembly ?? Assembly.GetCallingAssembly());
            texture.name = System.IO.Path.GetFileName(roomPath);
            RoomData roomData = ExtractRoomDataFromResource(roomPath, assembly ?? Assembly.GetCallingAssembly());
            roomData.room = Build(texture, roomData);
            if (!rooms.ContainsKey(roomData.room.name))
            {
                rooms.Add(roomData.room.name, roomData);
            }
            DungeonHandler.Register(roomData);

            return roomData;
        }

        public static RoomData BuildNewRoomFromResource(string roomPath, Assembly assembly = null)
        {
            RoomData roomData = ExtractRoomDataFromResource(roomPath, assembly ?? Assembly.GetCallingAssembly());
            roomData.name = System.IO.Path.GetFileName(roomPath);
            roomData.room = Build(roomData);
            PostProcessCells(roomData);

            if (!rooms.ContainsKey(roomData.room.name))
            {
                rooms.Add(roomData.room.name, roomData);
            }
            DungeonHandler.Register(roomData);
            return roomData;
        }


        public static PrototypeDungeonRoom Build(RoomData roomData)
        {
            try
            {
                int width = roomData.roomSize.x;
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

        public static void PostProcessCells(RoomData roomData)
        {
            if (roomData.tileInfo.Contains("X"))
            {

                var gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("No_Pickup_Object"));
                var l = gameObject.GetComponent<NoPickup>();
                for (int y = 0; y < roomData.roomSize.y; y++)
                {
                    for (int x = 0; x < roomData.roomSize.x; x++)
                    {
                        if (roomData.tileInfo[x + (y* roomData.roomSize.x)].ToString() == "X")
                        {
                            l.offsets.Add(new Vector2(x, y));
                        }
                    }
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
                            nonDatabasePlaceable = gameObject,
                            prerequisites = new DungeonPrerequisite[0],
                            materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    };
                if (roomData.room.placedObjectPositions == null) { roomData.room.placedObjectPositions = new List<Vector2>() { }; }
                roomData.room.placedObjectPositions.Add(new Vector2(0, 0));
                roomData.room.placedObjects.Add(new PrototypePlacedObjectData()
                {
                    contentsBasePosition = new Vector2(0, 0),
                    fieldData = new List<PrototypePlacedObjectFieldData>(),
                    instancePrerequisites = new DungeonPrerequisite[0],
                    linkedTriggerAreaIDs = new List<int>(),
                    placeableContents = dungeonPlaceable,
                    assignedPathIDx = -1,
                });
            }

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
        //public static GameObject MinimapIconPrefab;



        private static DungeonData.Direction DetermineDirectionType(string data, PrototypeRoomExit.ExitType exitType)
        {
            if (data.ToLower().Contains("west"))
            {
                return exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY ? DungeonData.Direction.EAST : DungeonData.Direction.WEST;
            }
            if (data.ToLower().Contains("east"))
            {
                return exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY ? DungeonData.Direction.WEST : DungeonData.Direction.EAST;
            }
            if (data.ToLower().Contains("north"))
            {
                return exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY ? DungeonData.Direction.SOUTH : DungeonData.Direction.NORTH;
            }
            if (data.ToLower().Contains("south"))
            {
                return exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY ? DungeonData.Direction.NORTH : DungeonData.Direction.SOUTH;
            }
            Debug.LogError("[Alexandria] Somehow failed to get direction of EXIT??? Returning SOUTH and praying shit don't break");
            return DungeonData.Direction.SOUTH;
        }

        private static PrototypeRoomExit.ExitType DetermineExitType(string data)
        {
            if (data.ToLower().Contains("exitonly"))
            {
                return PrototypeRoomExit.ExitType.EXIT_ONLY;
            }
            else if (data.ToLower().Contains("entryonly"))
            {
                return PrototypeRoomExit.ExitType.ENTRANCE_ONLY;
            }
            return PrototypeRoomExit.ExitType.NO_RESTRICTION;
        }



        public static void ApplyRoomData(PrototypeDungeonRoom room, RoomData roomData)
        {

            roomNames.Add(roomData.name);
            room.name = roomData.name;
            if (roomData.exitPositions != null)
            {
                for (int i = 0; i < roomData.exitPositions.Length; i++)
                {
                    string ext = roomData.exitDirections[i].ToUpper();
                    var exitType = DetermineExitType(ext);


                    AddExit(room, roomData.exitPositions[i], DetermineDirectionType(ext, exitType), exitType);
                }
            }
            else
            {
                AddExit(room, new Vector2(room.Width / 2, room.Height), DungeonData.Direction.NORTH);
                AddExit(room, new Vector2(room.Width / 2, 0), DungeonData.Direction.SOUTH);
                AddExit(room, new Vector2(room.Width, room.Height / 2), DungeonData.Direction.EAST);
                AddExit(room, new Vector2(0, room.Height / 2), DungeonData.Direction.WEST);
            }
            //ETGModConsole.Log("cando: " + roomData.randomizeEnemyPositions);
            if (roomData.enemyPositions != null)
            {
                for (int i = 0; i < roomData.enemyPositions.Length; i++)
                {
                    RoomEventTriggerCondition COND = RoomEventTriggerCondition.ON_ENEMIES_CLEARED;
                    if (roomData.waveTriggers != null)
                    {
                        if (roomData.waveTriggers[i] != null)
                        {
                            COND = ReturnTrigger(roomData.waveTriggers[i]);
                        }
                    }

                    //ETGModConsole.Log($"{roomData.enemyGUIDs[i]} | {roomData.enemyReinforcementLayers[i]} | {}");

                    //ETGModConsole.Log("Trigger "+ roomData.waveTriggers[i]);
                    AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], (roomData.enemyAttributes != null && roomData.enemyAttributes.Length > 0 ? roomData.enemyAttributes[i] : ""), roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions, COND);
                    //AddEnemyToRoom(room, roomData.enemyPositions[i], roomData.enemyGUIDs[i], roomData.enemyReinforcementLayers[i], roomData.randomizeEnemyPositions); ///GOES FROM HERE==================================================
                }
            }
            if (RoomUtility.EnableDebugLogging == true)
            {
                ETGModConsole.Log("enemy done");
            }

            Dictionary<int, PrototypeEventTriggerArea> keyValuePairs;
            if (roomData.placeablePositions != null)
            {
                keyValuePairs = new Dictionary<int, PrototypeEventTriggerArea>();
                for (int i = 0; i < roomData.placeablePositions.Length; i++)
                {
                    /*
                    if (roomData.placeableGUIDs[i] == "RoomEventTrigger_Placeable")
                    {
                        var t = roomData.placeablePositions[i];
                        
                        if (room.eventTriggerAreas == null) { room.eventTriggerAreas = new List<PrototypeEventTriggerArea>(); }
                        room.AddEventTriggerArea(new List<IntVector2>() { new IntVector2((int)t.x, (int)t.y) });
                    }
                    else
                    {
                    }
                    */
                    AddPlaceableToRoom(room, roomData.placeablePositions[i], roomData.placeableGUIDs[i], (roomData.placeableAttributes != null && roomData.placeableAttributes.Length > 0 ? roomData.placeableAttributes[i] : ""), keyValuePairs);
                    //AddPlaceableToRoom(room, roomData.placeablePositions[i], roomData.placeableGUIDs[i]);
                }
                List<PrototypeEventTriggerArea> prototypeEventTriggerAreas = new List<PrototypeEventTriggerArea> {};
                foreach (var ebtry in room.eventTriggerAreas)
                {
                    prototypeEventTriggerAreas.Add(ebtry);
                }
                int e = 0;
                foreach (var entry in keyValuePairs)
                {
                    //ETGModConsole.Log(e);
                    if (prototypeEventTriggerAreas.Count > e)
                    {
                        //ETGModConsole.Log(entry.Key);
                        if (room.eventTriggerAreas.Count > entry.Key)
                        {
                            room.eventTriggerAreas[entry.Key] = prototypeEventTriggerAreas[e];
                        }
                    }
                    e++;
                }
            }
            keyValuePairs = null;
            if (RoomUtility.EnableDebugLogging == true)
            {
                ETGModConsole.Log("placeable done");
            }
            if (roomData.nodePositions != null && roomData.nodePositions.Length > 0)

            {
                //ETGModConsole.Log("1");
                Dictionary<string, int> stupidJankyPieceOfShit = new Dictionary<string, int>();
                //ETGModConsole.Log("2");
                //ETGModConsole.Log(roomData.nodeOrder != null ? "Count: " + roomData.nodeOrder.Count() : "NULL");

                for (int j = 0; j < roomData.nodeOrder.Length; j++)
                {

                    //ETGModConsole.Log("3");

                    //ETGModConsole.Log($"{roomData.nodePaths[j]}{roomData.nodeOrder[j]}");
                    stupidJankyPieceOfShit.Add($"{roomData.nodePaths[j]}{roomData.nodeOrder[j]}", j);
                }
                //ETGModConsole.Log("4");

                for (int j = 0; j < roomData.nodePositions.Length; j++)
                {
                    var fuckThisMod = stupidJankyPieceOfShit[$"{roomData.nodePaths[j]}{roomData.nodeOrder[j]}"];
                    SerializedPath.SerializedPathWrapMode wrap = SerializedPath.SerializedPathWrapMode.Loop;
                    if (roomData.nodeWrapModes != null)
                    {
                        if (roomData.nodeWrapModes[fuckThisMod] != null)
                        {
                            wrap = ReturnWrap(roomData.nodeWrapModes[fuckThisMod]);
                        }
                    }

                    RoomFactory.AddNodeToRoom(room, roomData.nodePositions[fuckThisMod], roomData.nodeTypes[fuckThisMod], roomData.nodePaths[fuckThisMod], wrap, roomData.nodePathVisible != null ? roomData.nodePathVisible[fuckThisMod] :false, roomData.additionalPauseDelay != null ? roomData.additionalPauseDelay[j] : 0f);
                }
                //ETGModConsole.Log("5");

            }
            if (RoomUtility.EnableDebugLogging == true)
            {
                ETGModConsole.Log("node done");
            }
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
            room.overrideRoomVisualType = roomData.visualSubtype;
            if  (roomData.visualSubtype > -1)
            {
                room.overrideRoomVisualTypeForSecretRooms = true;
            }

            if (roomData.darkRoom)
            {
                room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES, RoomEventTriggerAction.BECOME_TERRIFYING_AND_DARK));
                room.roomEvents.Add(new RoomEventDefinition(RoomEventTriggerCondition.ON_ENEMIES_CLEARED, RoomEventTriggerAction.END_TERRIFYING_AND_DARK));
            }
        }
        public static SerializedPath.SerializedPathWrapMode ReturnWrap(string s)
        {
            switch (s)
            {
                case "LOOP":
                    return SerializedPath.SerializedPathWrapMode.Loop;
                case "PINGPONG":
                    return SerializedPath.SerializedPathWrapMode.PingPong;
                case "ONCE":
                    return SerializedPath.SerializedPathWrapMode.Once;
                default:
                    return SerializedPath.SerializedPathWrapMode.Loop;

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
            return path.EndsWith(".newroom") ? ExtractRoomDataFromBytesWithoutHeadder(data) : ExtractRoomDataFromBytes(data);
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
            if (RoomUtility.EnableDebugLogging == true)
            {
                ETGModConsole.Log($"{data.name}: {width} x - { height} y");
            }

            for (int y = 0; y < data.roomSize.y; y++)
            {
                for (int x = 0; x < data.roomSize.x; x++)
                {
                    cellData[x + y * width] = CellDataFromNumber(data, data.tileInfo[x + (y * width)].ToString());
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


        private static CellType TypeFromNumber(string type)
        {
            switch (type)
            {
                case "1":
                    return CellType.FLOOR;
                case "2":
                    return CellType.WALL;
                case "3":
                    return CellType.PIT;
                case "4":
                    return CellType.FLOOR;
                case "5":
                    return CellType.FLOOR;
                case "X":
                    return CellType.FLOOR;
                case "G":
                    return CellType.FLOOR;
                default:
                    return CellType.WALL;

            }
        }

        public static PrototypeDungeonRoomCellData CellDataFromNumber(RoomData roomData, string type)
        {
            if (type == null) return null;

            var data = new PrototypeDungeonRoomCellData();
            data.state = TypeFromNumber(type);
            //data.diagonalWallType = DiagonalWallTypeFromColor(color);
            data.diagonalWallType = ReturnDiagonalType(data, roomData, type);
            
            data.appearance = new PrototypeDungeonRoomCellAppearance()
            {
                overrideDungeonMaterialIndex = -1,
                OverrideFloorType = ReturnCellFloorType(data, type),
                IsPhantomCarpet = false,
                ForceDisallowGoop = false,
                globalOverrideIndices = new PrototypeIndexOverrideData() { indices = new List<int>(0) },         
            };
            return data;
        }



        public static Dungeonator.DiagonalWallType ReturnDiagonalType(PrototypeDungeonRoomCellData tile, RoomData roomData, string type)
        {
            switch (type)
            {
                case "6":
                    tile.breakable = false;
                    tile.ForceTileNonDecorated = true;
                    return DiagonalWallType.SOUTHWEST;
                case "7":
                    tile.breakable = false;
                    tile.ForceTileNonDecorated = true;
                    return DiagonalWallType.SOUTHEAST;
                case "8":
                    tile.breakable = false;
                    tile.ForceTileNonDecorated = true;
                    return DiagonalWallType.NORTHEAST;
                case "9":
                    tile.breakable = false;
                    tile.ForceTileNonDecorated = true;
                    return DiagonalWallType.NORTHWEST;
                default:
                    return DiagonalWallType.NONE;
            }
        }


        public static CellVisualData.CellFloorType ReturnCellFloorType(PrototypeDungeonRoomCellData cell, string type)
        {
            switch (type)
            {
                case "1":
                    return FloorType.Stone;
                case "2":
                    return FloorType.Stone;
                case "3":
                    return FloorType.Stone;
                case "4":
                    cell.ForceTileNonDecorated = true;
                    return FloorType.Ice;
                case "5":
                    cell.ForceTileNonDecorated = true;
                    cell.doesDamage = true;         
                    cell.damageDefinition = new CellDamageDefinition()
                    {
                        damageTypes = CoreDamageTypes.Fire,
                        damageToPlayersPerTick = 0.5f,
                        damageToEnemiesPerTick = 0,
                        tickFrequency = 1,
                        respectsFlying = true,
                        isPoison = false
                    };
                    return FloorType.ThickGoop;
                case "G":
                    cell.doesDamage = true;
                    cell.appearance = new PrototypeDungeonRoomCellAppearance() { OverrideFloorType = FloorType.Grass };
                    return FloorType.Grass;
                default:
                    return FloorType.Stone;

            }
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

                PrototypeDungeonRoomCellData[] cellData = room.m_cellData;
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
                room.m_cellData = cellData;

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

        public static Func<string, GameObject, JObject, GameObject> OnCustomProperty;

        public static void AddPlaceableToRoom(PrototypeDungeonRoom room, Vector2 location, string assetPath, string attributes, Dictionary<int, PrototypeEventTriggerArea> prototypeEventTriggerAreas)
        {
            try
            {           
                GameObject gameObject = RoomFactory.GetGameObjectFromBundles(assetPath);
                JObject jobject = null;
                if (!string.IsNullOrEmpty(attributes))
                {
                    jobject = JObject.Parse(attributes);
                    if (OnCustomProperty != null)
                    {
                        gameObject = OnCustomProperty(assetPath, gameObject, jobject);
                    } 
                    if (assetPath == "godray")
                    {
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("godray"));
                        JToken value;
                        float offsetX = jobject.TryGetValue("offsetx", out value) ? ((float)value) : 0f;
                        float offsetY = jobject.TryGetValue("offsety", out value) ? ((float)value) : 0f;

                        gameObject.GetOrAddComponent<Repositioner>().reposition = new Vector3(offsetX, offsetY, 0);
                    }

                    if (assetPath == "saw_blade_pathing")
                    {
                        JToken value;
                        float delay = jobject.TryGetValue("addDelay", out value) ? ((float)value) : 0f;
                        float maxSpeed = jobject.TryGetValue("mS", out value) ? ((float)value) : 9f;



                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("saw_blade_pathing"));
                        var pather = gameObject.GetComponent<PathMover>();
                        pather.AdditionalNodeDelay = delay;
                        pather.PathSpeed = maxSpeed;
                        pather.OriginalPathSpeed = maxSpeed;
                    }

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
                        //ETGModConsole.Log(storedBody);

                        bool storedGUID = jobject.TryGetValue("storedenemyBodyMC", out value2) ? ((bool)value2) : false;
                        bool forceActive = jobject.TryGetValue("cartActive", out value2) ? ((bool)value2) : false;

                        gameObject = StaticReferences.DefineMinecartFromValues("minecart_pathing", maxSpeed, timeToMaxSpeed, "", storedGUID, forceActive);

                    }
                    if (assetPath == "turretminecart_pathing")
                    {
                        JToken value2;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        float timeToMaxSpeed = jobject.TryGetValue("tTMS", out value2) ? ((float)value2) : 1.5f;

                        float initialWait = jobject.TryGetValue("InitialtrapDelay", out value2) ? ((float)value2) : 3;
                        float cooldown = jobject.TryGetValue("TrapTriggerDelay", out value2) ? ((float)value2) : 0.5f;
                        bool forceActive = jobject.TryGetValue("cartActive", out value2) ? ((bool)value2) : false;

                        string type = jobject.TryGetValue("projectileTypeTurret", out value2) ? ((string)value2) : "None.";

                        float projSpeed = jobject.TryGetValue("trapProjSpeed", out value2) ? ((float)value2) : -1f;
                        float range = jobject.TryGetValue("trapProjRange", out value2) ? ((float)value2) : 1000f;

                        bool jammed = jobject.TryGetValue("j", out value2) ? ((bool)value2) : false;

                        gameObject = StaticReferences.DefineMinecartFromValues("turretminecart_pathing", maxSpeed, timeToMaxSpeed, null, false, forceActive);

                        var turretComp = gameObject.GetComponentInChildren<CartTurretController>();

                        if (type != null && turretComp != null)
                        {

                            var b = turretComp.bulletBank;
                            if (type == "Bouncy.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("1a4872dafdb34fd29fe8ac90bd2cea67").bulletBank.GetBullet("default"), "default", "DNC"));
                            }
                            else if (type == "Explosive.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("b4666cb6ef4f4b038ba8924fd8adf38f").bulletBank.GetBullet("self"), "default", "DNC"));
                            }
                            else if (type == "Tank Shell.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("fa76c8cfdf1c4a88b55173666b4bc7fb").bulletBank.GetBullet("fastBullet"), "default", "DNC"));
                            }
                            else if (type == "Bouncy Bullet Kin.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").bulletBank.GetBullet("ricochet"), "default", "DNC"));
                            } //Tank Shell
                            else if (type == "Grenade.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("grenade"), "default", "DNC"));
                            } //Dragun Bouncy
                            else if (type == "Molotov.")
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("molotov"), "default", "DNC"));
                            }
                            else if (type == "Goblet.") 
                            {
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("ffca09398635467da3b1f4a54bcfda80").bulletBank.GetBullet("goblet"), "default", "DNC"));
                            }
                            else 
                            {
                                var bulletCopy = turretComp.bulletBank.Bullets[0];
                                b.Bullets = new List<AIBulletBank.Entry>();
                                b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(bulletCopy, "default", "DNC"));
                                //trapComp.projectileModule.projectiles[0] = trapComp.projectileModule.projectiles[0].gameObject.InstantiateAndFakeprefab().GetComponent<Projectile>();
                            }

                            foreach (var entry in b.Bullets)
                            {
                                if (entry.Name == "default")
                                {
                                    var p = entry.BulletObject.GetComponent<Projectile>();
                                    p.baseData.speed = projSpeed != -1f ? projSpeed : p.baseData.speed;
                                    p.baseData.range = range;
                                    if (jammed == true) { p.gameObject.AddComponent<ProjectileJammer>(); }
                                }
                            }
                        }
 


                        var c = gameObject.GetComponentInChildren<CartTurretController>();
                        c.AwakeTimer = initialWait;
                        c.TimeBetweenShots = cooldown;
                    }
                    if (assetPath == "explosivebarrelminecart_pathing")
                    {
                        JToken value2;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        float timeToMaxSpeed = jobject.TryGetValue("tTMS", out value2) ? ((float)value2) : 1.5f;
                        bool forceActive = jobject.TryGetValue("cartActive", out value2) ? ((bool)value2) : false;

                        gameObject = StaticReferences.DefineMinecartFromValues("explosivebarrelminecart_pathing", maxSpeed, timeToMaxSpeed, null, false, forceActive);          
                    }
                    if (assetPath == "CustomlightSource")
                    {
                        JToken value2;
                        float radius = jobject.TryGetValue("lightRad", out value2) ? ((float)value2) : 3f;
                        float intens = jobject.TryGetValue("lightInt", out value2) ? ((float)value2) : 3;
                        float Red = jobject.TryGetValue("lightColorR", out value2) ? ((float)value2) : 1;
                        float Green = jobject.TryGetValue("lightColorG", out value2) ? ((float)value2) : 1;
                        float Blue = jobject.TryGetValue("lightColorB", out value2) ? ((float)value2) : 1;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("CustomlightSource"));
                        var lightComp = gameObject.GetComponentInChildren<AdditionalBraveLight>();
                        lightComp.LightIntensity = intens;
                        lightComp.LightRadius = radius;
                        lightComp.LightColor = new Color(Red, Green, Blue);
                    }
                    if (assetPath == "bossPedestal" | assetPath == "bossPedestalGolden")
                    {
                        JToken value2;
                        int id = jobject.TryGetValue("bossPdstlItmID", out value2) ? ((int)value2) : -1;
                        string Tag = jobject.TryGetValue("bossPdstlItmStringID", out value2) ? ((string)value2) : "None.";
                        string lootType = jobject.TryGetValue("bossPdstOverrideLootType", out value2) ? ((string)value2) : "N/A";


                        var r = FakePrefab.Clone(RoomFactory.GetCustomDungeonPlaceableObject(assetPath).variantTiers[0].nonDatabasePlaceable);
                        var pedestal = r.GetComponent<RewardPedestal>();
                        var thing  = r.AddComponent<PedestalSetter>();
                        if (lootType == "Fully Random") {thing.myLootType = PedestalSetter.LootType.RANDOM; }
                        else if (lootType == "Random Gun") { thing.myLootType = PedestalSetter.LootType.RANDOM_GUN; }
                        else if(lootType == "Random Item") { thing.myLootType = PedestalSetter.LootType.RANDOM_ITEM; }
                        else if(lootType == "Crest") { thing.myLootType = PedestalSetter.LootType.CREST; }
                        else if (lootType == "Set ID / Tag") { thing.myLootType = PedestalSetter.LootType.SET; }
                        else { thing.myLootType = PedestalSetter.LootType.N_A; }
                        pedestal.pickedUp = false;
                        if (Tag != null && Tag != "None.")
                        {
                            if (StaticReferences.storedItemIDs.ContainsKey(Tag))
                            {
                                thing.Help = StaticReferences.storedItemIDs.Where(self => self.Key == Tag).First().Value;
                            }
                        }
                        else if (id != -1)
                        {
                            thing.Help = id;

                        }
                        gameObject = r;
                    }
                    if (SetupExoticObjects.allBasictrapControllerAssetNames.Contains(assetPath))
                    {
                        JToken value2;
                        
                        string triggerMethod = jobject.TryGetValue("TrapTriggerMethod", out value2) ? ((string)value2) : "Timer";
                        float cooldown = jobject.TryGetValue("TrapTriggerDelay", out value2) ? ((float)value2) : 1f;
                        float initialCooldown = jobject.TryGetValue("InitialtrapDelay", out value2) ? ((float)value2) : 1f;
                        float AttackDelay = jobject.TryGetValue("attackDelatTrap", out value2) ? ((float)value2) : 0.5f;
                        bool trapTriggerOnBlank = jobject.TryGetValue("trapTriggerOnBlank", out value2) ? ((bool)value2) : false;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var trapComp = gameObject.GetComponentInChildren<BasicTrapController>();
                        if (triggerMethod == "Timer") { trapComp.triggerMethod = BasicTrapController.TriggerMethod.Timer; }
                        else if (triggerMethod == "Stepped On") { trapComp.triggerMethod = BasicTrapController.TriggerMethod.PlaceableFootprint; }
                        else if(triggerMethod == "Collisions") { trapComp.triggerMethod = BasicTrapController.TriggerMethod.SpecRigidbody; }
                        else if(triggerMethod == "Script") { trapComp.triggerMethod = BasicTrapController.TriggerMethod.Script; }
                        trapComp.triggerTimerDelay = cooldown; // TrapTriggerDelay
                        trapComp.triggerOnBlank = trapTriggerOnBlank; //trapTriggerOnBlank
                        trapComp.triggerTimerOffset = initialCooldown; //Initial Cooldown
                        trapComp.triggerDelay = AttackDelay; //Attack Delay
                        trapComp.resetDelay = cooldown; 
                    }
                    if (assetPath == "WinchesterRoomController")
                    {
                        JToken value2;
                        int bounces = jobject.TryGetValue("WinchesterBounceCount", out value2) ? ((int)value2) : 1;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("WinchesterRoomController"));
                        gameObject.GetComponent<ArtfulDodgerRoomController>().NumberBounces = bounces;
                    }
                    if (assetPath == "WinchesterCameraController")
                    {
                        JToken value2;
                        float zoom = jobject.TryGetValue("WinCameraZoomOut", out value2) ? ((float)value2) : 0.75f;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("WinchesterCameraController"));
                        gameObject.GetComponent<ArtfulDodgerCameraManipulator>().OverrideZoomScale = zoom;
                    }
                    if (assetPath == "winchesterCameraPanPlacer")
                    {
                        JToken value2;
                        int X = jobject.TryGetValue("TileSizeX_", out value2) ? ((int)value2) : 4;
                        int Y = jobject.TryGetValue("TileSizeY_", out value2) ? ((int)value2) : 4;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("winchesterCameraPanPlacer"));

                        SpeculativeRigidbody specBody = gameObject.GetOrAddComponent<SpeculativeRigidbody>();
                        specBody.CollideWithTileMap = false;
                        specBody.AddCollider(CollisionLayer.Pickup, IntVector2.Zero, new IntVector2(16 * X, 16 * Y), isTrigger: true);
                        gameObject.GetOrAddComponent<WinchesterCameraHelper>();
                    }
                    if (assetPath == "WinchesterNPC")
                    {
                        JToken value2;
                        float X = jobject.TryGetValue("WinchestMoveXTele", out value2) ? ((float)value2) : 0;
                        float Y = jobject.TryGetValue("WinchestMoveYTele", out value2) ? ((float)value2) : 0;
                        float wait = jobject.TryGetValue("WinchestGoneTime", out value2) ? ((float)value2) : 1;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject("WinchesterNPC"));
                        var c = gameObject.AddComponent<SpecialComponents.WinchesterAlterer>();
                        c.goneTime = wait;
                        c.movement = new Vector2(X, Y);
                    }
                    if (assetPath == "winchesterShootyTarget_pathing" | assetPath == "WinchesterMovingBumper1x3_pathing" | assetPath == "WinchesterMovingBumper2x2_pathing")
                    {
                        JToken value2;
                        float speed = jobject.TryGetValue("WinchestTargetSpeed", out value2) ? ((float)value2) : 6f;
                        float delay = jobject.TryGetValue("addDelay", out value2) ? ((float)value2) : 0f;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        gameObject.GetComponent<PathMover>().OriginalPathSpeed = speed;
                        gameObject.GetComponent<PathMover>().PathSpeed = speed;
                        gameObject.GetComponent<PathMover>().AdditionalNodeDelay = delay;

                    }
                    if (assetPath == "ConveyorHorizontal" | assetPath == "ConveyorVertical")
                    {
                        JToken value2;
                        float X = jobject.TryGetValue("TileSizeX_", out value2) ? ((float)value2) : 0;
                        float Y = jobject.TryGetValue("TileSizeY_", out value2) ? ((float)value2) : 0;
                        float VelX = jobject.TryGetValue("ConveyorHorizontalVelocity", out value2) ? ((float)value2) : 0;
                        float VelY = jobject.TryGetValue("ConveyorVerticalVelocity", out value2) ? ((float)value2) : 0;
                        bool Bool = jobject.TryGetValue("ConveyorReversed", out value2) ? ((bool)value2) : false;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var conveyor = gameObject.GetComponent<ConveyorBelt>();
                        conveyor.ConveyorWidth = X;
                        conveyor.ConveyorHeight = Y;
                        conveyor.VelocityX = VelX * (Bool ? -1 : 1);
                        conveyor.VelocityY = VelY * (Bool ? -1 : 1);
                    }
                    if (assetPath == "FloorSkeleton_Note" | assetPath == "ShopSign")
                    {
                        JToken value2;
                        string customText = jobject.TryGetValue("customNoteText", out value2) ? ((string)value2) : "None.";
                        bool isKey = jobject.TryGetValue("customNoteTextIsStringKey", out value2) ? ((bool)value2) : false;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var note = gameObject.GetComponentInChildren<NoteDoer>() ?? gameObject.GetComponent<NoteDoer>();
                        if (customText != "None.")
                        {
                            note.isNormalNote = false;
                            note.useItemsTable = false;
                            note.useAdditionalStrings = false;
                            if (isKey == true)
                            {
                                note.stringKey = customText;
                                note.alreadyLocalized = false;
                            }
                            else
                            {
                                ETGMod.Databases.Strings.Core.Set("#" + customText.ToUpper() + "_CUSTOM", customText);
                                note.stringKey = "#" + customText.ToUpper() + "_CUSTOM";
                            }
                        }
                    }
                    if (assetPath == "bulletPastPitfalltrap")
                    {
                        JToken value2;
                        float AttackDelay = jobject.TryGetValue("attackDelatTrap", out value2) ? ((float)value2) : 3;
                        bool trapTriggerOnBlank = jobject.TryGetValue("trapTriggerOnBlank", out value2) ? ((bool)value2) : false;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var trapComp = gameObject.GetComponentInChildren<BasicTrapController>();
                        trapComp.triggerMethod = BasicTrapController.TriggerMethod.PlaceableFootprint;

                        trapComp.triggerOnBlank = trapTriggerOnBlank; //trapTriggerOnBlank
                        trapComp.triggerDelay = AttackDelay; //Attack Delay
                    }
                    //mines_face_shootssouth-mines_face_shootswest-mines_face_shootseast---mines_face_shootseast-hollow_face_shootssouth-hollow_face_shootswest

                    if (assetPath == "forge_face_shootswest" | assetPath == "forge_face_shootssouth" | assetPath == "forge_face_shootseast" 
                        | assetPath == "mines_face_shootssouth" | assetPath == "mines_face_shootswest" | assetPath == "mines_face_shootseast"
                        | assetPath == "hollow_face_shootseast" | assetPath == "hollow_face_shootssouth" | assetPath == "hollow_face_shootswest")
                    {
                        JToken value2;
                        float cooldown = jobject.TryGetValue("TrapTriggerDelay", out value2) ? ((float)value2) : 1;
                        
                        float projSpeed = jobject.TryGetValue("trapProjSpeed", out value2) ? ((float)value2) : 5f;
                        float range = jobject.TryGetValue("trapProjRange", out value2) ? ((float)value2) : 1000f;
                        string Direction = jobject.TryGetValue("DirectionShoot", out value2) ? ((string)value2) : "SOUTH";
                        string type = jobject.TryGetValue("projectileTypeTurret", out value2) ? ((string)value2) : "None.";
                        bool jammed = jobject.TryGetValue("j", out value2) ? ((bool)value2) : false;


                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var trapComp = gameObject.GetComponentInChildren<ProjectileTrapController>();
                        trapComp.shootDirection = (DungeonData.Direction)Enum.Parse(typeof(DungeonData.Direction), Direction);

                        trapComp.triggerMethod = BasicTrapController.TriggerMethod.Timer;
                        trapComp.triggerTimerDelay = cooldown; //trapTriggerOnBlank
                        trapComp.triggerTimerDelay1 = cooldown; //trapTriggerOnBlank


                        var data = trapComp.overrideProjectileData;
                        data.speed = projSpeed;
                        data.range = range;

                        if (type != null)
                        {
                            //yes i CAN DO this much better but im fucking lazy
                            if (type == "Bouncy.") { trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("1a4872dafdb34fd29fe8ac90bd2cea67").bulletBank.GetBullet("default").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); } //Bouncy
                            else if (type == "Explosive.") { trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("b4666cb6ef4f4b038ba8924fd8adf38f").bulletBank.GetBullet("self").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); } // Small grenade
                            else if(type == "Tank Shell.")
                            {
                                trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("fa76c8cfdf1c4a88b55173666b4bc7fb").bulletBank.GetBullet("fastBullet").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>();
                                trapComp.projectileModule.projectiles[0].gameObject.AddComponent<ProjectileWallUnfuckinator>();
                            } // Bullet King Goblets
                            else if(type == "Bouncy Bullet Kin.") 
                            {
                                trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").bulletBank.GetBullet("ricochet").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>();
                                trapComp.projectileModule.projectiles[0].gameObject.AddComponent<ProjectileWallUnfuckinator>();
                            } //Tank Shell
                            else if(type == "Grenade.") { trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("grenade").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); } //Dragun Bouncy
                            else if(type == "Molotov.") { trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("molotov").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); }
                            else if (type == "Goblet.") { trapComp.projectileModule.projectiles[0] = EnemyDatabase.GetOrLoadByGuid("ffca09398635467da3b1f4a54bcfda80").bulletBank.GetBullet("goblet").BulletObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); }
                            else { trapComp.projectileModule.projectiles[0] = trapComp.projectileModule.projectiles[0].gameObject.InstantiateAndFakeprefab().GetComponent<Projectile>(); }
                        }
                        if (jammed == true)
                        {
                            trapComp.projectileModule.projectiles[0].gameObject.AddComponent<ProjectileJammer>();
                        }
                    }
                    if (assetPath == "pew")
                    {
                        JToken value2;
                        int lentgh = jobject.TryGetValue("pewLength", out value2) ? ((int)value2) : 4;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var collider = gameObject.GetComponentInChildren<ResizableCollider>();
                        collider.NumTiles = lentgh;
                        //gameObject.GetComponent<tk2dSlicedSprite>().dimensions = new Vector2(lentgh * 16, 16);
                    }
                    if (assetPath == "MouseTrap_North" | assetPath == "MouseTrap_East" | assetPath == "MouseTrap_West")
                    {
                        JToken value2;
                        bool trapTriggerOnBlank = jobject.TryGetValue("trapTriggerOnBlank", out value2) ? ((bool)value2) : true;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var collider = gameObject.GetComponent<BasicTrapController>();
                        collider.triggerOnBlank = trapTriggerOnBlank;
                    }

                    if (assetPath == "rollingIceLogVertical_pathing" | assetPath == "rollingLogVertical_pathing")
                    {
                        JToken value2;
                        int lentgh = jobject.TryGetValue("logLength", out value2) ? ((int)value2) : 4;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        float pauseTime = jobject.TryGetValue("addDelay", out value2) ? ((float)value2) : 0f;


                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var collider = gameObject.GetComponent<ResizableCollider>();
                        var path = gameObject.GetComponent<PathMover>();
                        path.OriginalPathSpeed = maxSpeed;
                        path.PathSpeed = maxSpeed;
                        path.AdditionalNodeDelay = pauseTime;

                        collider.NumTiles = lentgh;
                        collider.IsHorizontal = true;
                    }
                    if (assetPath == "rollingIceLogHorizontal_pathing" | assetPath == "rollingLogHorizontal_pathing")
                    {
                        JToken value2;
                        int lentgh = jobject.TryGetValue("logHeight", out value2) ? ((int)value2) : 4;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        float pauseTime = jobject.TryGetValue("addDelay", out value2) ? ((float)value2) : 0f;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var collider = gameObject.GetComponent<ResizableCollider>();

                        var path = gameObject.GetComponent<PathMover>();
                        path.OriginalPathSpeed = maxSpeed;
                        path.PathSpeed = maxSpeed;
                        path.AdditionalNodeDelay = pauseTime;

                        collider.NumTiles = lentgh;
                        collider.IsHorizontal = false;
                    }
                    if (assetPath == "lonk_NPC_pathing")
                    {
                        JToken value2;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var path = gameObject.GetComponent<PathMover>();
                        float pauseTime = jobject.TryGetValue("addDelay", out value2) ? ((float)value2) : 0f;

                        path.OriginalPathSpeed = maxSpeed;
                        path.PathSpeed = maxSpeed;
                        path.AdditionalNodeDelay = pauseTime;

                    }
                    if (assetPath == "flame_pipe_north" | assetPath == "flame_pipe_west" | assetPath == "flame_pipe_east")
                    {
                        JToken value2;
                        float lifetime = jobject.TryGetValue("lf_pipe", out value2) ? ((float)value2) : 10f;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var collider = gameObject.GetComponent<ForgeFlamePipeController>();
                        collider.TimeToSpew = lifetime;
                    }
                    if (assetPath == "OmniMovingPlatform_pathing" | assetPath == "OmniMovingPlatformMines_pathing" | assetPath == "OmniMovingPlatformSewer_pathing" | assetPath == "OmniMovingPlatformHollow_pathing" | assetPath == "OmniMovingPlatformForge_pathing")
                    {
                        JToken value2;
                        float maxSpeed = jobject.TryGetValue("mS", out value2) ? ((float)value2) : 9f;
                        int X = jobject.TryGetValue("TileSizeX_", out value2) ? ((int)value2) : 3;
                        int Y = jobject.TryGetValue("TileSizeY_", out value2) ? ((int)value2) : 3;
                        float pauseTime = jobject.TryGetValue("addDelay", out value2) ? ((float)value2) : 0f;


                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var path = gameObject.GetComponent<PathMover>();
                        path.OriginalPathSpeed = maxSpeed;
                        path.PathSpeed = maxSpeed;
                        path.AdditionalNodeDelay = pauseTime;

                        var mover = gameObject.GetComponent<MovingPlatform>();
                        mover.UsesDwarfConfigurableSize = true;
                        mover.DwarfConfigurableWidth = X;
                        mover.DwarfConfigurableHeight = Y;

                    }
                    if (assetPath == "minecartFactory")
                    {
                        JToken value2;
                        int maxCarts = jobject.TryGetValue("mCSpawner_amount", out value2) ? ((int)value2) : 5;
                        float delayBetweenCarts = jobject.TryGetValue("mCSpawner_cartDelay", out value2) ? ((float)value2) : 3f;
                        float delayDestory = jobject.TryGetValue("mCSpawner_destroyDelay", out value2) ? ((float)value2) : 1f;
                        bool forceActive = jobject.TryGetValue("cartActive", out value2) ? ((bool)value2) : false;
                        string cartType = jobject.TryGetValue("mCSpawner_cartType", out value2) ? ((string)value2) : "Explosive Barrel";
                        bool cartCopy = jobject.TryGetValue("mCSpawner_cartCopy", out value2) ? ((bool)value2) : false;
                        bool cartCopyDestroy = jobject.TryGetValue("mCSpawner_cartDestroyCopy", out value2) ? ((bool)value2) : true;
                        
                        int path = jobject.TryGetValue("tSP", out value2) ? ((int)value2) : 0;
                        int startNode = jobject.TryGetValue("nSP_O", out value2) ? ((int)value2) : 0;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        gameObject.SetActive(false);

                        var factory = gameObject.GetComponent<MineCartFactory>();
                        factory.MaxCarts = maxCarts;
                        factory.DelayBetweenCarts = delayBetweenCarts;
                        factory.DelayUponDestruction = delayDestory;
                        factory.ForceCartActive = forceActive;
                        factory.TargetPathIndex = path;
                        factory.TargetPathNodeIndex = startNode;

                        
                        if (cartCopy == true)
                        {
                            var grabber = gameObject.AddComponent<MineCart_Proximity_Grabber>();
                            grabber.DestroyAfterCopy = cartCopyDestroy;
                            if (cartType == "Explosive Barrel") { grabber.fallBackCart = SetupExoticObjects.ExplosiveBarrelMinecart.GetComponent<MineCartController>(); }
                            else if (cartType == "Default") { grabber.fallBackCart = SetupExoticObjects.Minecart.GetComponent<MineCartController>(); }
                            else if (cartType == "Turret")
                            {
                                var cart = SetupExoticObjects.TurretMinecart.InstantiateAndFakeprefab();
                                cart.gameObject.AddComponent<TurretCartReboot>();
                                grabber.fallBackCart = cart.GetComponent<MineCartController>(); 
                            }
                        }
                        else
                        {
                            if (cartType == "Explosive Barrel") { factory.MineCartPrefab = SetupExoticObjects.ExplosiveBarrelMinecart.GetComponent<MineCartController>(); }
                            else if (cartType == "Default") { factory.MineCartPrefab = SetupExoticObjects.Minecart.GetComponent<MineCartController>(); }
                            else if (cartType == "Turret")
                            {
                                var cart = SetupExoticObjects.TurretMinecart.InstantiateAndFakeprefab();
                                cart.gameObject.AddComponent<TurretCartReboot>();
                                factory.MineCartPrefab = cart.GetComponent<MineCartController>();
                            }
                        }
                        
                    }
                    if (assetPath == "gullLeapPoint")
                    {
                        JToken value2;
                        bool repos = jobject.TryGetValue("gullleap_Repos", out value2) ? ((bool)value2) : true;
                        bool missile = jobject.TryGetValue("gullleap_Missile", out value2) ? ((bool)value2) : true;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var leaper = gameObject.GetComponent<GatlingGullLeapPoint>();
                        leaper.ForReposition = repos;
                        leaper.ForRockets = missile;
                    }
                    if (assetPath == "glitch_floor_properties")
                    {
                        JToken value2;

                        float hpMult = jobject.TryGetValue("glitchHpMult", out value2) ? ((float)value2) : 0.7f;
                        float timeScale = jobject.TryGetValue("glitchtimescaleMult", out value2) ? ((float)value2) : 1f;
                        float Speed = jobject.TryGetValue("glitchspeedMult", out value2) ? ((float)value2) : 1f;
                        bool fly = jobject.TryGetValue("forceFly", out value2) ? ((bool)value2) : false;

                        
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var leaper = gameObject.GetComponent<Glitched_Boss_Modifier>();
                        leaper.DamageMultiplier = hpMult;
                        leaper.TimeScale = timeScale;
                        leaper.MovementSpeed = Speed;
                        leaper.ForceSlight = fly;

                    }
                    if (assetPath == "vertical_crusher"| assetPath == "horizontal_crusher")
                    {
                        JToken value2;

                        float delay = jobject.TryGetValue("crushTrapDelay", out value2) ? ((float)value2) : 0.25f;
                        float crushTrapCloseTime = jobject.TryGetValue("crushTrapCloseTime", out value2) ? ((float)value2) : 1f;
                        float cooldown = jobject.TryGetValue("TrapTriggerDelay", out value2) ? ((float)value2) : 3f;
                        float enemyDamage = jobject.TryGetValue("crushTrapEnemyDamage", out value2) ? ((float)value2) : 30f;
                        float PlayerForce = jobject.TryGetValue("crushTrapPlayerKnockbackForce", out value2) ? ((float)value2) : 50f;
                        float EnemyForce = jobject.TryGetValue("crushTrapEnemyKnockbackForce", out value2) ? ((float)value2) : 50f;

                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var crusher = gameObject.GetComponent<ForgeCrushDoorController>();
                        crusher.CooldownTime = cooldown;
                        crusher.DelayTime = delay;
                        crusher.TimeClosed = crushTrapCloseTime;
                        crusher.DamageToEnemies = enemyDamage;
                        crusher.KnockbackForcePlayers = PlayerForce;
                        crusher.KnockbackForceEnemies = EnemyForce;

                    }
                    if (assetPath == "firebar_trap")
                    {
                        JToken value2;
                        bool jammed = jobject.TryGetValue("j", out value2) ? ((bool)value2) : false;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var bulletBank = gameObject.GetComponent<AIBulletBank>();
                        var newBank = EnemyBuildingTools.CopyBulletBankEntry(bulletBank.Bullets[0], "default", "DNC");
                        bulletBank.Bullets.Clear();
                        bulletBank.Bullets = new List<AIBulletBank.Entry>() { newBank };
                        if (jammed == true)
                        {
                            bulletBank.Bullets[0].BulletObject.GetComponent<Projectile>().gameObject.AddComponent<ProjectileJammer>();
                        }
                    }
                    if (assetPath == "flameburst_trap")
                    {
                        JToken value2;
                        bool jammed = jobject.TryGetValue("j", out value2) ? ((bool)value2) : false;
                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var bulletBank = gameObject.GetComponent<AIBulletBank>();
                        string type = jobject.TryGetValue("projectileTypeTurret", out value2) ? ((string)value2) : "None.";

                        var b = bulletBank;
                        if (type == "Bouncy.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("1a4872dafdb34fd29fe8ac90bd2cea67").bulletBank.GetBullet("default"), "default", "DNC"));
                        }
                        else if (type == "Explosive.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("b4666cb6ef4f4b038ba8924fd8adf38f").bulletBank.GetBullet("self"), "default", "DNC"));
                        }
                        else if (type == "Tank Shell.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("fa76c8cfdf1c4a88b55173666b4bc7fb").bulletBank.GetBullet("fastBullet"), "default", "DNC"));
                        }
                        else if (type == "Bouncy Bullet Kin.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").bulletBank.GetBullet("ricochet"), "default", "DNC"));
                        } //Tank Shell
                        else if (type == "Grenade.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("grenade"), "default", "DNC"));
                        } //Dragun Bouncy
                        else if (type == "Molotov.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("8b913eea3d174184be1af362d441910d").bulletBank.GetBullet("molotov"), "default", "DNC"));
                        }
                        else if (type == "Goblet.")
                        {
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(EnemyDatabase.GetOrLoadByGuid("ffca09398635467da3b1f4a54bcfda80").bulletBank.GetBullet("goblet"), "default", "DNC"));
                        }
                        else
                        {
                            var bulletCopy = b.Bullets[0];
                            b.Bullets = new List<AIBulletBank.Entry>();
                            b.Bullets.Add(EnemyBuildingTools.CopyBulletBankEntry(bulletCopy, "default", "DNC"));
                            //trapComp.projectileModule.projectiles[0] = trapComp.projectileModule.projectiles[0].gameObject.InstantiateAndFakeprefab().GetComponent<Projectile>();
                        }

                        float range = jobject.TryGetValue("trapProjRange", out value2) ? ((float)value2) : 1000f;
                        bulletBank.Bullets[0].BulletObject.GetComponent<Projectile>().baseData.range = range;
                        if (jammed == true)
                        {
                            bulletBank.Bullets[0].BulletObject.GetComponent<Projectile>().gameObject.AddComponent<ProjectileJammer>(); ;
                        }
                    }
                    if (assetPath == "ShopItemObject" | assetPath == "Glass_Case_Custom")
                    {
                        JToken value2;
                        string It = jobject.TryGetValue("shopItemBase", out value2) ? ((string)value2) : "Primary";
                        string shopUser = jobject.TryGetValue("UsedByShop", out value2) ? ((string)value2) : "UsedByShop";
                        float chance = jobject.TryGetValue("shopItemBaseChance", out value2) ? ((float)value2) : 1f;
                        string dir = jobject.TryGetValue("facingDirItem", out value2) ? ((string)value2) : "NORTH";
                        bool she_omni = jobject.TryGetValue("omniDir", out value2) ? ((bool)value2) : true;
                        int id = jobject.TryGetValue("bossPdstlItmID", out value2) ? ((int)value2) : -1;
                        string Tag = jobject.TryGetValue("bossPdstlItmStringID", out value2) ? ((string)value2) : "None.";
                        int overridePrice = jobject.TryGetValue("overridePrice", out value2) ? ((int)value2) : -1;
                        float PriceMult = jobject.TryGetValue("priceMultiplier", out value2) ? ((float)value2) : 1f;



                        gameObject = FakePrefab.Clone(RoomFactory.GetExoticGameObject(assetPath));
                        var fuckYOU = gameObject.GetComponent<ShopSubsidiaryZone>();
                        if (fuckYOU != null) { UnityEngine.Object.Destroy(fuckYOU); }
                        var tiem = gameObject.GetComponent<ShopItemPosition>();
                        tiem.Chance = chance;
                        tiem.OmniDirectional = she_omni;
                        if (overridePrice != -1) { tiem.OverridePrice = overridePrice; }
                        tiem.PriceMultiplier = PriceMult;
                        if (Tag != null && Tag != "None.")
                        {
                            if (StaticReferences.storedItemIDs.ContainsKey(Tag))
                            {
                                tiem.itemID = StaticReferences.storedItemIDs.Where(self => self.Key == Tag).First().Value;
                            }
                        }
                        else if (id != -1)
                        {
                            tiem.itemID = id;

                        }
                        switch (dir)
                        {
                            case "NORTH":
                                tiem.direction = DungeonData.Direction.NORTH;
                                break;
                            case "SOUTH":
                                tiem.direction = DungeonData.Direction.SOUTH;
                                break;
                            case "EAST":
                                tiem.direction = DungeonData.Direction.EAST;
                                break;
                            case "WEST":
                                tiem.direction = DungeonData.Direction.WEST;
                                break;
                            default:
                                tiem.direction = DungeonData.Direction.NORTH;
                                break;
                        }

                        tiem.thisType = It == "Primary" ? ShopItemPosition.TableType.PRIMARY : ShopItemPosition.TableType.SECONDARY;
                        if (shopUser == "Any")
                        {
                            tiem.SeenByAny = true;
                        }
                        else
                        {
                            switch (shopUser)
                            {
                                case "Bello":
                                    tiem.Type = BaseShopController.AdditionalShopType.NONE;
                                    break;
                                case "Goopton":
                                    tiem.Type = BaseShopController.AdditionalShopType.GOOP;
                                    break;
                                case "Old Red":
                                    tiem.Type = BaseShopController.AdditionalShopType.BLANK;
                                    break;
                                case "Flynt":
                                    tiem.Type = BaseShopController.AdditionalShopType.KEY;
                                    break;
                                case "Cursula":
                                    tiem.Type = BaseShopController.AdditionalShopType.CURSE;
                                    break;
                                case "Trorc":
                                    tiem.Type = BaseShopController.AdditionalShopType.TRUCK;
                                    break;
                                case "Blacksmith":
                                    tiem.Type = BaseShopController.AdditionalShopType.BLACKSMITH;
                                    break;
                                case "Shortcut Rat":
                                    tiem.Type = BaseShopController.AdditionalShopType.RESRAT_SHORTCUT;
                                    break;
                                case "Meta Shop":
                                    tiem.Type = BaseShopController.AdditionalShopType.FOYER_META;
                                    break;
                                default:
                                    tiem.SeenByAny = true;
                                    break;
                            }
                        }
                    }
                }

                if (!gameObject && StaticReferences.customObjects.ContainsKey(assetPath))
                {
                    gameObject = RoomFactory.GetCustomGameObject(assetPath);
                }
                else if (!gameObject)
                {
                    gameObject = RoomFactory.GetExoticGameObject(assetPath);
                }

                if (gameObject != null &&!string.IsNullOrEmpty(attributes))
                {
                    jobject = JObject.Parse(attributes);
                    if (OnCustomProperty != null)
                    {
                        foreach (var entry in OnCustomProperty.GetInvocationList())
                        {
                            gameObject = (GameObject)entry.DynamicInvoke(assetPath, gameObject, jobject);
                        }
                    }
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
                    int startNode = 0;

                    if (assetPath.Contains("_pathing") && jobject != null)
                    {
                        JToken value;
                        path = jobject.TryGetValue("tSP", out value) ? ((int)value) : 0;
                        startNode = jobject.TryGetValue("nSP_O", out value) ? ((int)value) : 0;

                        if (RoomUtility.EnableDebugLogging == true)
                        {
                            ETGModConsole.Log($"[{assetPath}]");
                        }
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>(),
                            placeableContents = dungeonPlaceable,
                            assignedPathIDx = path,
                            assignedPathStartNode = startNode
                        });
                    }
                    else if (assetPath.Contains("_DropDowntrap"))
                    {
                        JToken value2;
                        int Order = jobject.TryGetValue("triggerEventValue", out value2) ? ((int)value2) : 0;
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>()
                            {

                            },
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>() { Order },
                            placeableContents = dungeonPlaceable,
                            assignedPathIDx = -1,
                        });
                        //ETGModConsole.Log("_DropDowntrap" + " " + Order + " " + location);
                    }
                    else if (assetPath.Contains("_DropDownswitch"))
                    {
                        JToken value2;
                        int Order = jobject.TryGetValue("triggeredEventValue", out value2) ? ((int)value2) : 0;
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>()
                            {

                            },
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>() { Order },
                            placeableContents = dungeonPlaceable,
                            assignedPathIDx = -1,
                        });
                        
                        if (prototypeEventTriggerAreas.ContainsKey(Order))
                        {
                            prototypeEventTriggerAreas[Order].triggerCells.Add(location);
                        }
                        else
                        {
                            PrototypeEventTriggerArea item2 = room.AddEventTriggerArea(new List<IntVector2>
                            {
                                new IntVector2((int)location.x, (int)location.y),
                            });
                            prototypeEventTriggerAreas.Add(Order, item2);
                        }
                        
                        //ETGModConsole.Log("_DropDownswitch" + " " + Order + " " + location);

                    }
                    else if (assetPath.Contains("gullLeapPoint") || assetPath.Contains("_dungeonPlaceable"))
                    {
                        room.placedObjects.Add(new PrototypePlacedObjectData
                        {
                            contentsBasePosition = location,
                            fieldData = new List<PrototypePlacedObjectFieldData>(),
                            instancePrerequisites = array,
                            linkedTriggerAreaIDs = new List<int>(),
                            assignedPathIDx = -1,
                            nonenemyBehaviour = gameObject.GetComponent<DungeonPlaceableBehaviour>() 
                        });;
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
                            assignedPathIDx = -1,
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
            Chest component = gameObject.GetComponent<Chest>();
            if (component)
            {
                gameObject = FakePrefab.Clone(asset);
                component = gameObject.GetComponent<Chest>(); // sets "component" to the fakeprefabbed chest component instead of the prefab (i had a different crappier fix that worked but this was spapis idea, i just woke up im tired aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa)

                bool flag = attributes.TryGetValue("dI", out jtoken) && !string.IsNullOrEmpty(jtoken.ToObject<string>());
                if (flag)
                {
                    bool flag2 = Game.Items.ContainsID(jtoken.ToObject<string>());
                    if (flag2)
                    {
                        component.forceContentIds = new List<int>
                        {
                            Game.Items[jtoken.ToObject<string>()].PickupObjectId
                        };
                    }
                }
                JToken value = null;

                bool junkie = attributes.TryGetValue("jI", out jtoken) && !string.IsNullOrEmpty(jtoken.ToObject<string>());
                if (junkie)
                {
                    string text = attributes.TryGetValue("jI", out value) ? ((string)value) : "";
                    bool flag3 = Game.Items.ContainsID(text);
                    if (flag3)
                    {
                        component.overrideJunkId = Game.Items[text].PickupObjectId;
                    }
                }

                float overrideMimicChance = attributes.TryGetValue("mC", out value) ? ((float)value) : 0f;
                bool isLocked = !attributes.TryGetValue("cL", out value) || (bool)value;
                bool preventFuse = attributes.TryGetValue("pV", out value) && (bool)value;
                bool isGlitched = attributes.TryGetValue("isGlitched", out value) && (bool)value;
                component.overrideMimicChance = overrideMimicChance;
                component.IsLocked = isLocked;
                component.PreventFuse = preventFuse;
                if (isGlitched == true)
                {
                    component.m_isGlitchChest = true;
                    component.gameObject.AddComponent<Glitchinator>();
                }
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

        public static DungeonPlaceable GetCustomDungeonPlaceableObject(string assetPath)
        {
            DungeonPlaceable result = null;
            DungeonPlaceable gameObject;
            if (StaticReferences.customPlaceables.TryGetValue(assetPath, out gameObject))
            {
                result = gameObject;
            }
            return result;
        }
        public static void AddNodeToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, int layer, SerializedPath.SerializedPathWrapMode wrapMode, bool modifyTilemap, float delay)
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
                node.delayTime = delay;
                serializedPath.nodes.Add(node);
                room.paths.Add(serializedPath);
                serializedPath.wrapMode = wrapMode;//SerializedPath.SerializedPathWrapMode.Loop;
                serializedPath.tilesetPathGrid = modifyTilemap ? 0 : -1;

            }
            else
            {
                serializedPath = room.paths[layer];
                var node = new SerializedPathNode(intLocation);
                serializedPath.wrapMode = wrapMode;
                node.placement = (SerializedPathNode.SerializedNodePlacement)Enum.Parse(typeof(SerializedPathNode.SerializedNodePlacement), guid);
                node.delayTime = delay;
                serializedPath.nodes.Add(node);
                serializedPath.tilesetPathGrid = modifyTilemap ? 0 : -1;
            }

        }


        public static RoomEventTriggerCondition ReturnTrigger(string s)
        {
            switch (s)
            {
                case "ON_ENEMIES_CLEARED":
                    return RoomEventTriggerCondition.ON_ENEMIES_CLEARED;
                case "ENEMY_BEHAVIOR":
                    return RoomEventTriggerCondition.ENEMY_BEHAVIOR;
                case "NPC_TRIGGER_A":
                    return RoomEventTriggerCondition.NPC_TRIGGER_A;
                case "ON_HALF_ENEMY_HP_DEPLETED":
                    return RoomEventTriggerCondition.ON_HALF_ENEMY_HP_DEPLETED;
                case "ON_ONE_QUARTER_ENEMY_HP_DEPLETED":
                    return RoomEventTriggerCondition.ON_ONE_QUARTER_ENEMY_HP_DEPLETED;
                case "ON_THREE_QUARTERS_ENEMY_HP_DEPLETED":
                    return RoomEventTriggerCondition.ON_THREE_QUARTERS_ENEMY_HP_DEPLETED;
                case "SEQUENTIAL_WAVE_TRIGGER":
                    return RoomEventTriggerCondition.SEQUENTIAL_WAVE_TRIGGER;
                case "SHRINE_WAVE_A":
                    return RoomEventTriggerCondition.SHRINE_WAVE_A;
                case "SHRINE_WAVE_B":
                    return RoomEventTriggerCondition.SHRINE_WAVE_B;
                case "SHRINE_WAVE_C":
                    return RoomEventTriggerCondition.SHRINE_WAVE_C;
                case "NPC_TRIGGER_B":
                    return RoomEventTriggerCondition.NPC_TRIGGER_B;
                case "NPC_TRIGGER_C":
                    return RoomEventTriggerCondition.NPC_TRIGGER_C;
                case "ON_ENTER":
                    return RoomEventTriggerCondition.ON_ENTER;
                case "ON_ENTER_WITH_ENEMIES":
                    return RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES;
                case "ON_EXIT":
                    return RoomEventTriggerCondition.ON_EXIT;
                case "ON_NINETY_PERCENT_ENEMY_HP_DEPLETED":
                    return RoomEventTriggerCondition.ON_NINETY_PERCENT_ENEMY_HP_DEPLETED;
                case "TIMER":
                    return RoomEventTriggerCondition.TIMER;
                default:
                    return RoomEventTriggerCondition.ON_ENEMIES_CLEARED;

            }
        }

        public static void AddEnemyToRoom(PrototypeDungeonRoom room, Vector2 location, string guid, string attributes, int layer, bool shuffle, RoomEventTriggerCondition reinforcementType)
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
                RoomFactory.AddObjectDataToReinforcementLayer(room, prototypePlacedObjectData, layer - 1, location, shuffle, reinforcementType);
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
                AddObjectDataToReinforcementLayer(room, objectData, layer - 1, location, shuffle, RoomEventTriggerCondition.ON_ENEMIES_CLEARED); //GOES FROM HERE
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

        public static void AddObjectDataToReinforcementLayer(PrototypeDungeonRoom room, PrototypePlacedObjectData objectData, int layer, Vector2 location, bool shuffle, RoomEventTriggerCondition reinforcementType) //TO HERE
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
                        reinforcementTriggerCondition = reinforcementType
                    };
                    room.additionalObjectLayers.Add(newLayer);

                }
            }
            room.additionalObjectLayers[layer].placedObjects.Add(objectData);
            room.additionalObjectLayers[layer].placedObjectBasePositions.Add(location);
        }

        public static void AddExit(PrototypeDungeonRoom room, Vector2 location, DungeonData.Direction direction, PrototypeRoomExit.ExitType exitType = PrototypeRoomExit.ExitType.NO_RESTRICTION)
        {
            if (room.exitData == null)
                room.exitData = new PrototypeRoomExitData();
            if (room.exitData.exits == null)
                room.exitData.exits = new List<PrototypeRoomExit>();

            PrototypeRoomExit exit = new PrototypeRoomExit(direction, location);
            exit.exitType = exitType;
            exit.containsDoor = true;

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
            public int visualSubtype;

            public string superSpecialRoomType;

            public float AmbientLight_R;
            public float AmbientLight_G;
            public float AmbientLight_B;
            public bool usesAmbientLight;
            public bool[] nodePathVisible;
            
            public string specialRoomPool;

            public float[] additionalPauseDelay;

        }
    }
}
