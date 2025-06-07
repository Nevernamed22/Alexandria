using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Dungeonator;
using Alexandria.ItemAPI;
using Planetside;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Collections;
using HarmonyLib;
using Alexandria.Misc;
using System.ComponentModel;

namespace Alexandria.DungeonAPI
{

    public static class StaticReferences
    {
        public static List<string> GlitchBossNames = new List<string>();
        public static Dictionary<string, AssetBundle> AssetBundles;
        public static Dictionary<string, GenericRoomTable> RoomTables;
        public static SharedInjectionData subShopTable;

        public static Dictionary<string, GameObject> customObjects = new Dictionary<string, GameObject>();
        public static Dictionary<string, DungeonPlaceable> customPlaceables = new Dictionary<string, DungeonPlaceable>();
        public static Dictionary<string, int> storedItemIDs = new Dictionary<string, int>();


        public static Dictionary<string, string> roomTableMap = new Dictionary<string, string>()
        {
            { "castle", "Castle_RoomTable" },
            { "gungeon", "Gungeon_RoomTable" },
            { "mines", "Mines_RoomTable" },
            { "catacombs", "Catacomb_RoomTable" },
            { "forge", "Forge_RoomTable" },
            { "sewer", "Sewer_RoomTable" },
            { "cathedral", "Cathedral_RoomTable" },
            { "bullethell", "BulletHell_RoomTable" },
            { "resourcefulrat", "Resourceful Rat Maze Rooms" },

            //{ "unknown", "SecretHelpers_RoomTable" },
        };

        public static Dictionary<string, string> specialRoomTableMap = new Dictionary<string, string>()
        {
            { "special", "basic special rooms (shrines, etc)" },
            { "shop", "Shop Room Table" },
            { "secret", "secret_room_table_01" },
            { "winchester", "WinchesterRoomTable" },
        };


        //=================== LIST OF BOSS ROOM POOLS 
        public static Dictionary<string, string> BossRoomTableMap = new Dictionary<string, string>()
        {
            //Floor 1
            { "gull", "bosstable_01_gatlinggull"},
            { "triggertwins", "bosstable_01_bulletbros"},
            { "bulletking", "bosstable_01_bulletking"},
            //Sewer
            { "blobby", "bosstable_01a_blobulord"},
            //Floor 2
            { "gorgun", "bosstable_02_meduzi"},
            { "beholster", "bosstable_02_beholster"},
            { "ammoconda", "bosstable_02_bashellisk"},
            //Abbey
            { "oldking", "bosstable_04_oldking"},

            //Floor 3
            { "tank", "bosstable_03_tank"},
            { "cannonballrog", "bosstable_03_powderskull"},
            { "flayer", "bosstable_03_mineflayer"},
            //Floor 4
            { "priest", "bosstable_02a_highpriest"},
            { "pillars", "bosstable_04_statues"},
            { "monger", "bosstable_04_demonwall"},
            //Door Lord
            { "doorlord", "bosstable_xx_bossdoormimic"}
        };

        public static Dictionary<string, string> MiniBossRoomTableMap = new Dictionary<string, string>()
        {
            {"blockner", "BlocknerMiniboss_Table_01" },
            {"shadeagunim","PhantomAgunim_Table_01" },
            //{"fuselier","fusebombroom01" }
        };


        public static string[] assetBundleNames = new string[]
        {
            "shared_auto_001",
            "shared_auto_002",
            "brave_resources_001",
        };



        public static string[] dungeonPrefabNames = new string[]
        {
            "base_gungeon",
            "base_castle",
            "base_mines",
            "base_catacombs",
            "base_forge",
            "base_sewer",
            "base_cathedral",
            "base_bullethell",
            "base_resourcefulrat"
        };


        public static void Init()
        {
            AssetBundles = new Dictionary<string, AssetBundle>();
            foreach (var name in assetBundleNames)
            {
                try
                {
                    var bundle = ResourceManager.LoadAssetBundle(name);
                    if(bundle == null)
                    {
                        ShrineTools.PrintError($"Failed to load asset bundle: {name}");
                        continue;
                    }
                    //Tools.PrintError($"Loaded assetbundle: {name}");

                    AssetBundles.Add(name, ResourceManager.LoadAssetBundle(name));
                }
                catch (Exception e)
                {
                    ShrineTools.PrintError($"Failed to load asset bundle: {name}");
                    ShrineTools.PrintException(e);
                }
            }
            // InitStaticRoomObjects();
            SetupExoticObjects.InitialiseObjects();
            RoomTables = new Dictionary<string, GenericRoomTable>();
            foreach (var entry in roomTableMap)
            {
                try
                {
                    var table = DungeonDatabase.GetOrLoadByName($"base_{entry.Key}").PatternSettings.flows[0].fallbackRoomTable;
                    if (entry.Key.Contains("resourcefulrat"))
                    {
                        table = OfficialFlows.GetDungeonPrefab("base_resourcefulrat").PatternSettings.flows[0].AllNodes[18].overrideRoomTable; 
                    }
                    RoomTables.Add(entry.Key, table);
                }
                catch (Exception e)
                {
                    ShrineTools.PrintError($"Failed to load room table: {entry.Key}:{entry.Value}");
                    ShrineTools.PrintException(e);
                }
            }

            foreach (var entry in specialRoomTableMap)
            {
                try
                {
                    var table = GetAsset<GenericRoomTable>(entry.Value);
                    RoomTables.Add(entry.Key, table);
                }
                catch (Exception e)
                {
                    ShrineTools.PrintError($"Failed to load special room table: {entry.Key} : {entry.Value}");
                    ShrineTools.PrintException(e);
                }
            }

            /*
            int i = 0;
            foreach (var entry in OfficialFlows.GetDungeonPrefab("base_resourcefulrat").PatternSettings.flows[1].AllNodes)
            {

                if (entry.overrideExactRoom != null)
                {
                    ETGModConsole.Log(entry.overrideExactRoom.name + " : " + i);
                }
                i++;
            }
            `*/
            //================================ Adss Boss Rooms into RoomTables

            foreach (var entry in BossRoomTableMap)
            {
                try
                {
                    var table = GetAsset<GenericRoomTable>(entry.Value);
                    RoomTables.Add(entry.Key, table);
                }
                catch (Exception e)
                {
                    ShrineTools.PrintError($"Failed to load special room table: {entry.Key}:{entry.Value}");
                    ShrineTools.PrintException(e);
                }
            }
            //================================ Adss Mini Boss Rooms into RoomTables
            foreach (var entry in MiniBossRoomTableMap)
            {
                try
                {
                    var table = GetAsset<GenericRoomTable>(entry.Value);
                    RoomTables.Add(entry.Key, table);
                }
                catch (Exception e)
                {
                    ShrineTools.PrintError($"Failed to load special room table: {entry.Key}:{entry.Value}");
                    ShrineTools.PrintException(e);
                }
            }

            //shrineTable-npcTable-blackMarketInjection



            subShopTable = AssetBundles["shared_auto_001"].LoadAsset<SharedInjectionData>("_global injected subshop table");
            /*
            int i = 0;
            foreach(var data in subShopTable.InjectionData)
            {
                ETGModConsole.Log(data.exactRoom.name + " : " + i);
                i++;
                ETGModConsole.Log("====");
                //CharacterAPI.ToolsCharApi.LogPropertiesAndFields(data, data.annotation);
            }
            */
            RoomTables.Add("oldred", ConvertExactRoomIntoNewRoomTable(subShopTable.InjectionData[0], subShopTable.InjectionData[0].exactRoom));
            RoomTables.Add("cursula", ConvertExactRoomIntoNewRoomTable(subShopTable.InjectionData[1], subShopTable.InjectionData[1].exactRoom));
            RoomTables.Add("flynt", ConvertExactRoomIntoNewRoomTable(subShopTable.InjectionData[2], subShopTable.InjectionData[2].exactRoom));
            RoomTables.Add("trorc", ConvertExactRoomIntoNewRoomTable(subShopTable.InjectionData[3], subShopTable.InjectionData[3].exactRoom));
            RoomTables.Add("goopton", ConvertExactRoomIntoNewRoomTable(subShopTable.InjectionData[4], subShopTable.InjectionData[4].exactRoom));


            RoomIcons.LoadRoomIcons();

            RoomIcons.WinchesterRoomIcon = RoomTables["winchester"].includedRooms.elements[0].room.associatedMinimapIcon;

            
            Dungeon keep_ = DungeonDatabase.GetOrLoadByName("base_castle");
            Dungeon sewer_ = DungeonDatabase.GetOrLoadByName("base_sewer");
            Dungeon proper_ = DungeonDatabase.GetOrLoadByName("base_gungeon");
            Dungeon abbey_ = DungeonDatabase.GetOrLoadByName("base_cathedral");
            Dungeon hollow_ = DungeonDatabase.GetOrLoadByName("base_catacombs");
            Dungeon hell_ = DungeonDatabase.GetOrLoadByName("base_bullethell");

            StaticInjections.Keep_Injections_Sewer = keep_.PatternSettings.flows[1].sharedInjectionData[1];
            StaticInjections.Sewer_Injections = sewer_.PatternSettings.flows[0].sharedInjectionData[1];
            StaticInjections.Proper_Injections = proper_.PatternSettings.flows[0].sharedInjectionData[1];
            StaticInjections.Abbey_Injections = abbey_.PatternSettings.flows[0].sharedInjectionData[1];
            StaticInjections.Hollow_Injections = hollow_.PatternSettings.flows[0].sharedInjectionData[1];
            StaticInjections.Hell_Injections = hell_.PatternSettings.flows[0].sharedInjectionData[0];

            
            RoomTables.Add("sewerentrace", ProcessRoomTableThing(StaticInjections.Keep_Injections_Sewer, StaticInjections.Keep_Injections_Sewer.InjectionData[0], 1).roomTable);

            RoomTables.Add("fireplace", keep_.PatternSettings.flows[0].sharedInjectionData[1].InjectionData[1].roomTable);
            RoomTables.Add("miscreward", keep_.PatternSettings.flows[0].sharedInjectionData[0].InjectionData[1].roomTable);


            RoomTables.Add("crestroom", ProcessRoomTableThing(StaticInjections.Sewer_Injections, StaticInjections.Sewer_Injections.InjectionData[1]).roomTable);
            RoomTables.Add("abbeyentrance", ProcessRoomTableThing(StaticInjections.Proper_Injections, StaticInjections.Proper_Injections.InjectionData[0]).roomTable);
            RoomTables.Add("abbeyextrasecret", ProcessRoomTableThing(StaticInjections.Abbey_Injections, StaticInjections.Abbey_Injections.InjectionData[0]).roomTable);
            RoomTables.Add("rng_entry", ProcessRoomTableThing(StaticInjections.Hollow_Injections, StaticInjections.Hollow_Injections.InjectionData[1]).roomTable);
            RoomTables.Add("bullet_hell_secret", ProcessRoomTableThing(StaticInjections.Hell_Injections, StaticInjections.Hell_Injections.InjectionData[0]).roomTable);
            
            keep_ = null;
            sewer_ = null;
            proper_ = null;
            abbey_ = null;
            hollow_ = null;
            hell_ = null;


            //FlowDatabase.GetOrLoadByName("Secret_DoubleBeholster_Flow");

            var glitchedFlow = ResourceManager.LoadAssetBundle("flows_base_001").LoadAsset<DungeonFlow>("Secret_DoubleBeholster_Flow");
            StaticInjections.GlitchFlow = glitchedFlow;
            StaticInjections.Node = glitchedFlow.AllNodes[2];
            RoomTables.Add("glitchedBoss", ConvertNodeToRoomTable(StaticInjections.Node).overrideRoomTable);

            StaticInjections.Shrine_injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("_global injected shrine table");
            
            RoomTables.Add("challenge_shrine_keep", StaticInjections.Shrine_injections.InjectionData[1].roomTable);
            RoomTables.Add("challenge_shrine_proper", StaticInjections.Shrine_injections.InjectionData[2].roomTable);
            RoomTables.Add("challenge_shrine_mines", StaticInjections.Shrine_injections.InjectionData[3].roomTable);
            RoomTables.Add("challenge_shrine_hollow", StaticInjections.Shrine_injections.InjectionData[4].roomTable);
            RoomTables.Add("challenge_shrine_forge", StaticInjections.Shrine_injections.InjectionData[5].roomTable);
            StaticInjections.Shrine_injections.InjectionData[0].roomTable.includedRoomTables = new List<GenericRoomTable>()
            {

            };
            RoomTables.Add("cleanse_shrine", ConvertRoomIntoNewRoomTable(StaticInjections.Shrine_injections.InjectionData[0], StaticInjections.Shrine_injections.InjectionData[0].roomTable, 0));
            RoomTables.Add("cursed_mirror", ConvertRoomIntoNewRoomTable(StaticInjections.Shrine_injections.InjectionData[0], StaticInjections.Shrine_injections.InjectionData[0].roomTable, 1));
            RoomTables.Add("black_market_entrance", ConvertRoomIntoNewRoomTable(StaticInjections.Shrine_injections.InjectionData[0], StaticInjections.Shrine_injections.InjectionData[0].roomTable, 2));
            RoomTables.Add("random_shrine", ConvertRoomIntoNewRoomTable(StaticInjections.Shrine_injections.InjectionData[0], StaticInjections.Shrine_injections.InjectionData[0].roomTable, 3));

            RoomTables.Add("glass_shrine", ConvertRoomIntoNewRoomTable(StaticInjections.Shrine_injections.InjectionData[0], StaticInjections.Shrine_injections.InjectionData[0].roomTable, 4));
            ETGMod.StartGlobalCoroutine(Delay(StaticInjections.Shrine_injections.InjectionData[0].roomTable));

            StaticInjections.NPC_injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("_global injected npc table");
            RoomTables.Add("lost_adventurer", ProcessRoomTableThing(StaticInjections.NPC_injections, StaticInjections.NPC_injections.InjectionData[1]).roomTable);
            RoomTables.Add("gunsling_king", ProcessRoomTableThing(StaticInjections.NPC_injections, StaticInjections.NPC_injections.InjectionData[2]).roomTable);
            RoomTables.Add("mendy_and_patches", ProcessRoomTableThing(StaticInjections.NPC_injections, StaticInjections.NPC_injections.InjectionData[3]).roomTable);
            RoomTables.Add("synergrace", ProcessRoomTableThing(StaticInjections.NPC_injections, StaticInjections.NPC_injections.InjectionData[4]).roomTable);

            StaticInjections.Black_Market_injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("black market injection data");

            ETGModConsole.Log(StaticInjections.Black_Market_injections.AttachedInjectionData.Count);
            RoomTables.Add("black_market", ProcessRoomTableThing(StaticInjections.Black_Market_injections, StaticInjections.Black_Market_injections.InjectionData[0]).roomTable);

            var room =  LoadHelper.LoadAssetFromAnywhere<PrototypeDungeonRoom>("Shrine_DemonFace_Room");
            room.requiredInjectionData = StaticInjections.Black_Market_injections;

            StaticInjections.Miniboss_injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("phantom agunim injection data");
            RoomTables.Add("fuselier", ProcessRoomTableThing(StaticInjections.Miniboss_injections, StaticInjections.Miniboss_injections.InjectionData[2]).roomTable);

            StaticInjections.Fallback_Subshop_Injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("subshop fallback injection data");
            RoomTables.Add("gun_muncher", ProcessRoomTableThing(StaticInjections.Fallback_Subshop_Injections, StaticInjections.Fallback_Subshop_Injections.InjectionData[0]).roomTable);
            RoomTables.Add("vampire", ProcessRoomTableThing(StaticInjections.Fallback_Subshop_Injections, StaticInjections.Fallback_Subshop_Injections.InjectionData[1]).roomTable);


            if (DebugSpawns == true)
            {
                StaticInjections.Shrine_injections.ChanceToSpawnOne = 1;
                StaticInjections.Shrine_injections.OnlyOne = false;
                StaticInjections.Shrine_injections.IgnoreUnmetPrerequisiteEntries = true;

                StaticInjections.NPC_injections.ChanceToSpawnOne = 1;
                StaticInjections.NPC_injections.OnlyOne = false;
                StaticInjections.NPC_injections.IgnoreUnmetPrerequisiteEntries = true;

                StaticInjections.Black_Market_injections.ChanceToSpawnOne = 1;
                StaticInjections.Black_Market_injections.OnlyOne = false;
                StaticInjections.Black_Market_injections.IgnoreUnmetPrerequisiteEntries = true;

                StaticInjections.Miniboss_injections.ChanceToSpawnOne = 1;
                StaticInjections.Miniboss_injections.OnlyOne = false;
                StaticInjections.Miniboss_injections.IgnoreUnmetPrerequisiteEntries = true;

                StaticInjections.Fallback_Subshop_Injections.ChanceToSpawnOne = 1;
                StaticInjections.Fallback_Subshop_Injections.OnlyOne = false;
                StaticInjections.Fallback_Subshop_Injections.IgnoreUnmetPrerequisiteEntries = true;

            }
            StaticInjections.Base_Shared_Injections = LoadHelper.LoadAssetFromAnywhere<SharedInjectionData>("base shared injection data");
            RoomTables.Add("jailedNPC", StaticInjections.Base_Shared_Injections.InjectionData[1].roomTable);

            /*
            if (StaticInjections.Gungeon_Common_Injections != null)
            {
                foreach (var entry in StaticInjections.Gungeon_Common_Injections.InjectionData)
                {
                    CharacterAPI.ToolsCharApi.LogPropertiesAndFields(entry, "");
                }
            }
            */

            ShrineTools.Print("Static references initialized.");
        }


        private static GenericRoomTable ConvertExactRoomIntoNewRoomTable(ProceduralFlowModifierData roomTableToAddTo, PrototypeDungeonRoom roomEntry)
        {
            if (DebugSpawns == true)
            {
                roomTableToAddTo.chanceToLock = 0;
                roomTableToAddTo.chanceToSpawn = 1;
                roomTableToAddTo.selectionWeight = 1;
                roomTableToAddTo.prerequisites = new DungeonPrerequisite[0];
                roomTableToAddTo.OncePerRun = false;
                roomTableToAddTo.RequiresMasteryToken = false;
                roomTableToAddTo.DEBUG_FORCE_SPAWN = true;
            }

            var newTable = ScriptableObject.CreateInstance<GenericRoomTable>();

            newTable.includedRoomTables = new List<GenericRoomTable>() { };

            newTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new List<WeightedRoom>()
                {
                    new WeightedRoom()
                    {
                        room = roomEntry,
                        weight = roomTableToAddTo.chanceToSpawn,
                        additionalPrerequisites = roomTableToAddTo.prerequisites != null&& DebugSpawns == false  ?  roomTableToAddTo.prerequisites.ToArray() : new DungeonPrerequisite[0],
                    }
                },
            };
            roomTableToAddTo.roomTable = ScriptableObject.CreateInstance<GenericRoomTable>();

            roomTableToAddTo.roomTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new List<WeightedRoom>()
                {
                    new WeightedRoom()
                    {
                       
                    }
                },
            };
            roomTableToAddTo.roomTable.includedRoomTables = new List<GenericRoomTable>();
            roomTableToAddTo.roomTable.includedRoomTables.Add(newTable);
            return newTable;

        }
        private static GenericRoomTable ConvertRoomIntoNewRoomTable(ProceduralFlowModifierData roomTableToAddTo, GenericRoomTable roomTableToPullFrom, int entry, bool debug  = false)
        {
            var roomEntry = roomTableToPullFrom.includedRooms.elements[entry];

            if (DebugSpawns == true || debug == true)
            {
                roomTableToAddTo.chanceToLock = 0;
                roomTableToAddTo.chanceToSpawn = 1;
                roomTableToAddTo.selectionWeight = 1;

                roomTableToAddTo.prerequisites = new DungeonPrerequisite[0];
                roomTableToAddTo.OncePerRun = false;
                roomTableToAddTo.RequiresMasteryToken = false;
                roomTableToAddTo.DEBUG_FORCE_SPAWN = true;
                roomTableToAddTo.RandomNodeChildMinDistanceFromEntrance = 0;
                roomTableToAddTo.IsWarpWing = false;
                roomTableToAddTo.placementRules = new List<ProceduralFlowModifierData.FlowModifierPlacementType>() { ProceduralFlowModifierData.FlowModifierPlacementType.BEFORE_ANY_COMBAT_ROOM };
            }



            var newTable = ScriptableObject.CreateInstance<GenericRoomTable>();
            newTable.includedRoomTables = new List<GenericRoomTable>() { };
            newTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new List<WeightedRoom>()
                {
                    new WeightedRoom()
                    {
                        room = roomEntry.room,
                        weight = roomEntry.weight,
                        additionalPrerequisites = roomEntry.room.prerequisites != null&& DebugSpawns == false  ?  roomEntry.room.prerequisites.ToArray() : new DungeonPrerequisite[0],
                    }
                },             
            };

            roomTableToAddTo.roomTable.includedRoomTables.Add(newTable);
            return newTable;

        }
        private static DungeonFlowNode ConvertNodeToRoomTable(DungeonFlowNode node, float overrideWeight = 1)
        {
            node.overrideRoomTable = ScriptableObject.CreateInstance<GenericRoomTable>();
            node.overrideRoomTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new List<WeightedRoom>()
                {
                    new WeightedRoom()
                    {
                        room = node.overrideExactRoom,
                        weight = overrideWeight,
                        additionalPrerequisites = node.overrideExactRoom.prerequisites != null ? node.overrideExactRoom.prerequisites.ToArray() : new DungeonPrerequisite[0],  
                    }
                }
            };
            node.overrideRoomTable.name = "alexandriaGlitchTable";
            node.overrideRoomTable.includedRoomTables = new List<GenericRoomTable>() { };
            ETGMod.StartGlobalCoroutine(Delay(node));
            return node;
        }
        private static IEnumerator Delay(GenericRoomTable flow)
        {
            yield return null;
            flow.includedRooms = new WeightedRoomCollection() { elements = new List<WeightedRoom>() { } };
            yield break;
        }
        private static IEnumerator Delay(DungeonFlowNode flow)
        {
            yield return null;
            flow.overrideExactRoom = null;
            yield break;
        }

        private static bool DebugSpawns = false;

        private static ProceduralFlowModifierData ProcessRoomTableThing(SharedInjectionData shared , ProceduralFlowModifierData data, float defaultWeight = 1, bool debug = false)
        {

            ProceduralFlowModifierData modifierData = new ProceduralFlowModifierData()
            {
                annotation = data.annotation,
                CanBeForcedSecret = data.CanBeForcedSecret,
                chanceToLock = DebugSpawns == true || debug ? 0 : data.chanceToLock,
                chanceToSpawn = DebugSpawns == true || debug == true ? 1 : data.chanceToSpawn,
                selectionWeight = DebugSpawns == true || debug == true ? 1 : data.selectionWeight,
                DEBUG_FORCE_SPAWN = DebugSpawns == true || debug == true ? true : data.DEBUG_FORCE_SPAWN,
                exactRoom = null,
                exactSecondaryRoom = data.exactSecondaryRoom,
                framedCombatNodes = data.framedCombatNodes,
                IsWarpWing = data.IsWarpWing,
                OncePerRun = DebugSpawns == true ? false : data.OncePerRun,
                placementRules = data.placementRules,
                prerequisites = DebugSpawns == true ? new DungeonPrerequisite[0]: data.prerequisites.ToArray(),       
                RandomNodeChildMinDistanceFromEntrance = DebugSpawns == true ? 0 : data.RandomNodeChildMinDistanceFromEntrance,
                RequiredValidPlaceable = data.RequiredValidPlaceable,
                RequiresMasteryToken = DebugSpawns == true ? false : data.RequiresMasteryToken,
                
                roomTable = ScriptableObject.CreateInstance<GenericRoomTable>(),
               
            };
            if (debug == true)
            {
                ETGModConsole.Log(data.exactRoom.name);
                int I = 0;
                foreach (var entryR in data.exactRoom.placedObjects)
                {
                    if (entryR.nonenemyBehaviour != null)
                    {
                        ETGModConsole.Log(entryR.nonenemyBehaviour.name ?? "NULL");
                    }
                    I++;
                }
                I = 0;      
            }

            modifierData.roomTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new ()
                {
                    new WeightedRoom()
                    {
                        room = data.exactRoom,
                        additionalPrerequisites = data.exactRoom.prerequisites != null && DebugSpawns == false ? data.exactRoom.prerequisites.ToArray() : new DungeonPrerequisite[0],
                        weight = defaultWeight
                    }
                }

            };
            modifierData.roomTable.includedRoomTables = new List<GenericRoomTable>() { };

            data.chanceToSpawn = 0;
            data.selectionWeight = 0;
            shared.InjectionData.Add(modifierData);
            return modifierData;
        }




        public static void InitStaticRoomObjects()
        {

            GameObject SingleCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(68).gameObject);
            if (SingleCasingPlaceable.GetComponent<PickupMover>()) UnityEngine.Object.Destroy(SingleCasingPlaceable.GetComponent<PickupMover>());
            GameObject FiveCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(70).gameObject);
            if (FiveCasingPlaceable.GetComponent<PickupMover>()) UnityEngine.Object.Destroy(FiveCasingPlaceable.GetComponent<PickupMover>());
            GameObject FiftyCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(74).gameObject);
            if (FiftyCasingPlaceable.GetComponent<PickupMover>()) UnityEngine.Object.Destroy(FiftyCasingPlaceable.GetComponent<PickupMover>());

            StoredRoomObjects.Add("SingleCasingPlaceable", SingleCasingPlaceable);
            StoredRoomObjects.Add("FiveCasingPlaceable", FiveCasingPlaceable);
            StoredRoomObjects.Add("FiftyCasingPlaceable", FiftyCasingPlaceable);

            var rngDungeon = DungeonDatabase.GetOrLoadByName("base_nakatomi");
            if (rngDungeon)
            {
                if (rngDungeon.PatternSettings.flows[0].name == "FS4_Nakatomi_Flow")
                {
                    if (rngDungeon.PatternSettings.flows[0].AllNodes.Count == 14)
                    {
                        GameObject MopAndBucket = rngDungeon.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
                        GameObject CardboardBox3 = rngDungeon.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[2].nonenemyBehaviour.gameObject;
                        GameObject ACUnit = rngDungeon.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject;
                        GameObject ACVent = rngDungeon.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[2].nonenemyBehaviour.gameObject;
                        StoredRoomObjects.Add("MopAndBucket", MopAndBucket);
                        StoredRoomObjects.Add("CardboardBox3", CardboardBox3);
                        StoredRoomObjects.Add("ACUnit", ACUnit);
                        StoredRoomObjects.Add("ACVent", ACVent);
                    }
                }
            }
            rngDungeon = null;

            var forgeDungeon = DungeonDatabase.GetOrLoadByName("Base_Forge");

              
            foreach (WeightedRoom wRoom in forgeDungeon.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
            {

                if (wRoom.room != null && !string.IsNullOrEmpty(wRoom.room.name))
                {
                    if (wRoom.room.name.ToLower().StartsWith("forge_normal_cubulead_03"))
                    {
                        GameObject VerticalCrusher = wRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
                        GameObject FireBarTrap = wRoom.room.placedObjects[7].nonenemyBehaviour.gameObject;
                        StoredRoomObjects.Add("VerticalCrusher", VerticalCrusher);
                        StoredRoomObjects.Add("FireBarTrap", FireBarTrap);
                    }
                    if (wRoom.room.name.ToLower().StartsWith("forge_connector_flamepipes_01"))
                    {
                        GameObject FlamePipeNorth = wRoom.room.placedObjects[1].nonenemyBehaviour.gameObject;
                        GameObject FlamePipeWest = wRoom.room.placedObjects[3].nonenemyBehaviour.gameObject;
                        GameObject FlamePipeEast = wRoom.room.placedObjects[2].nonenemyBehaviour.gameObject;
                        StoredRoomObjects.Add("FlamePipeNorth", FlamePipeNorth);
                        StoredRoomObjects.Add("FlamePipeWest", FlamePipeWest);
                        StoredRoomObjects.Add("FlamePipeEast", FlamePipeEast);
                    }
                }
            }

            foreach (DungeonFlow flows in forgeDungeon.PatternSettings.flows)
            {
                foreach (DungeonFlowNode node in flows.AllNodes)
                {
                    if (node.overrideExactRoom != null)
                    {
                        if (node.overrideExactRoom.name.ToLower().StartsWith("exit_room_forge"))
                        {
                            GameObject ForgeGearVertical = node.overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
                            GameObject ForgeGearHorizontalLeft = node.overrideExactRoom.placedObjects[7].nonenemyBehaviour.gameObject;
                            GameObject ForgeGearHorizontalRight = node.overrideExactRoom.placedObjects[8].nonenemyBehaviour.gameObject;
                            GameObject BlacksmithLargeTorch = node.overrideExactRoom.placedObjects[19].nonenemyBehaviour.gameObject;

                            if (!StoredRoomObjects.ContainsKey("ForgeGearVertical")){StoredRoomObjects.Add("ForgeGearVertical", ForgeGearVertical);}
                            if (!StoredRoomObjects.ContainsKey("ForgeGearHorizontalLeft")) { StoredRoomObjects.Add("ForgeGearHorizontalLeft", ForgeGearHorizontalLeft); }
                            if (!StoredRoomObjects.ContainsKey("ForgeGearHorizontalRight")) { StoredRoomObjects.Add("ForgeGearHorizontalRight", ForgeGearHorizontalRight); }
                            if (!StoredRoomObjects.ContainsKey("BlacksmithLargeTorch")) { StoredRoomObjects.Add("BlacksmithLargeTorch", BlacksmithLargeTorch); }
                        }
                        if (node.overrideExactRoom.name.ToLower().StartsWith("blacksmith_testroom"))
                        {
                            GameObject BlacksmithLoungeChair = node.overrideExactRoom.placedObjects[9].nonenemyBehaviour.gameObject;
                            GameObject BlacksmithTableAndChair = node.overrideExactRoom.placedObjects[10].nonenemyBehaviour.gameObject;
                            GameObject BlacksmithHotBowl = node.overrideExactRoom.placedObjects[11].nonenemyBehaviour.gameObject;

                            if (!StoredRoomObjects.ContainsKey("BlacksmithLoungeChair")) { StoredRoomObjects.Add("BlacksmithLoungeChair", BlacksmithLoungeChair); }
                            if (!StoredRoomObjects.ContainsKey("BlacksmithTableAndChair")) { StoredRoomObjects.Add("BlacksmithTableAndChair", BlacksmithTableAndChair); }
                            if (!StoredRoomObjects.ContainsKey("BlacksmithHotBowl")) { StoredRoomObjects.Add("BlacksmithHotBowl", BlacksmithHotBowl); }
                        }
                    }
                }
            }
            forgeDungeon = null;

            var sewerDungeon = DungeonDatabase.GetOrLoadByName("Base_Sewer");
            foreach (WeightedRoom wRoom in sewerDungeon.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
            {
                if (wRoom.room != null && !string.IsNullOrEmpty(wRoom.room.name))
                {
                    if (wRoom.room.name.ToLower().StartsWith("sewer_trash_compactor_001"))
                    {
                        GameObject HorizontalCrusher = wRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
                        StoredRoomObjects.Add("HorizontalCrusher", HorizontalCrusher);
                    }
                }
            }
            sewerDungeon = null;
           
        }



        public static GameObject DefineMinecartFromValues(string cartType, float maxSpeed, float timeToMaxSpeed, string storedBody, bool NearestInCart, bool forceActive)
        {
            GameObject asset = RoomFactory.GetExoticGameObject(cartType);
            GameObject gameObject = FakePrefab.Clone(asset);
            MineCartController component = gameObject.GetComponent<MineCartController>();
            GameObject result;
            if (component == null)
            {
                result = asset;
            }
            else
            {
                component.MaxSpeed = maxSpeed;
                component.TimeToMaxSpeed = timeToMaxSpeed;
                component.ForceActive = forceActive;
                if (NearestInCart == true)
                {
                    gameObject.AddComponent<SpecialComponents.ForceNearestToRide>();
                }

                result = gameObject;
            }
            return result;
        }

      

     


        public static GameObject DefineDeadBlowFromValues(bool followsPlayer, bool persistent, bool facesLeft, bool leaveGoop, bool fireBullets, string goopType, float initialDelay, float minDelay, float maxDelay)
        {
            GameObject asset = StaticReferences.GetAsset<GameObject>("Forge_Hammer");
            GameObject gameObject = FakePrefab.Clone(asset);
            ForgeHammerController component = gameObject.GetComponent<ForgeHammerController>();
            bool flag = component == null;
            GameObject result;
            if (flag)
            {
                result = asset;
            }
            else
            {
                component.TracksPlayer = followsPlayer;
                component.DeactivateOnEnemiesCleared = !persistent;
                if (facesLeft)
                {
                    component.ForceLeft = true;
                    component.ForceRight = false;
                }
                else
                {
                    component.ForceLeft = false;
                    component.ForceRight = true;
                }
                component.DoGoopOnImpact = leaveGoop;
                component.DoesBulletsOnImpact = fireBullets;
                component.GoopToDo = StaticReferences.FetchGoopDefinitions(goopType);
                component.InitialDelay = initialDelay;
                component.MinTimeBetweenAttacks = minDelay;
                component.MaxTimeBetweenAttacks = maxDelay;
                result = gameObject;
            }
            return result;
        }

        public static GameObject GenerateCustomBarrel(string baseBarrel, float rollSpeed, bool trailgoop, string trailGoopType, float goopTrailWidth, bool doGoopPuddle, string puddleGoopType, float goopPuddleWidth, bool destroyedByPlayerRoll)
        {
            GameObject gameObject = new GameObject();
            bool flag = baseBarrel == "red_explosive";
            if (flag)
            {
                gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Red Barrel"));
            }
            else
            {
                bool flag2 = baseBarrel == "metal_explosive";
                if (flag2)
                {
                    gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Red Drum"));
                }
                else
                {
                    bool flag3 = baseBarrel == "water_drum";
                    if (flag3)
                    {
                        gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Blue Drum"));
                    }
                    else
                    {
                        bool flag4 = baseBarrel == "oil_drum";
                        if (flag4)
                        {
                            gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Purple Drum"));
                        }
                        else
                        {
                            bool flag5 = baseBarrel == "poison_drum";
                            if (flag5)
                            {
                                gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Yellow Drum"));
                            }
                            else
                            {
                                gameObject = FakePrefab.Clone(RoomFactory.GetGameObjectFromBundles("Blue Drum"));
                            }
                        }
                    }
                }
            }
            KickableObject component = gameObject.GetComponent<KickableObject>();
            MinorBreakable component2 = gameObject.GetComponent<MinorBreakable>();
            bool flag6 = component;
            if (flag6)
            {
                component.rollSpeed = rollSpeed;
                component.RollingDestroysSafely = destroyedByPlayerRoll;
                component.leavesGoopTrail = trailgoop;
                component.goopRadius = goopTrailWidth;
                component.goopFrequency = 0.05f;
                component.goopType = StaticReferences.FetchGoopDefinitions(trailGoopType);
            }
            bool flag7 = component2;
            if (flag7)
            {
                component2.goopsOnBreak = doGoopPuddle;
                component2.goopType = StaticReferences.FetchGoopDefinitions(puddleGoopType);
                component2.goopRadius = goopPuddleWidth;
            }
            return gameObject;
        }

        public static GoopDefinition FetchGoopDefinitions(string desiredGoopType)
        {
            GoopDefinition goopDefinition = null;
            string text = null;
            if (desiredGoopType == "water")
            {
                text = "assets/data/goops/water goop.asset";
            }
            else if (desiredGoopType == "web")
            {
                text = "assets/data/goops/phasewebgoop.asset";
            }
            else if (desiredGoopType == "fire")
            {
                text = "assets/data/goops/napalmgoopthatworks.asset";
            }
            else if (desiredGoopType == "charm")
            {
                PickupObject byId = PickupObjectDatabase.GetById(310);
                GoopDefinition goopDefinition2;
                if (byId == null)
                {
                    goopDefinition2 = null;
                }
                else
                {
                    WingsItem component = byId.GetComponent<WingsItem>();
                    goopDefinition2 = ((component != null) ? component.RollGoop : null);
                }
                goopDefinition = goopDefinition2;
            }
            else if (desiredGoopType == "greenfire")
            {
                goopDefinition = (PickupObjectDatabase.GetById(698) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
            }
            else if (desiredGoopType == "cheese")
            {
                goopDefinition = (PickupObjectDatabase.GetById(808) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
            }

            else if (desiredGoopType == "poison")
            {
                text = "assets/data/goops/poison goop.asset";
            }

            else if (desiredGoopType == "blobulon")
            {
                text = "assets/data/goops/blobulongoop.asset";
            }
            else if (desiredGoopType == "oil")
            {
                text = "assets/data/goops/oil goop.asset";
            }
            if (text != null)
            {
                try
                {
                    GameObject asset = StaticReferences.GetAsset<GameObject>(text);
                    goopDefinition = asset.GetComponent<GoopDefinition>();
                }
                catch
                {
                    goopDefinition = StaticReferences.GetAsset<GoopDefinition>(text);
                }
                goopDefinition.name = text.Replace("assets/data/goops/", "").Replace(".asset", "");
            }
            return goopDefinition;
        }

        /// <summary>
        /// LEGACY VERSION, use customObjects instead.
        /// </summary>
        public static Dictionary<string, GameObject> StoredRoomObjects = new Dictionary<string, GameObject>(){};
        /// <summary>
        /// LEGACY VERSION, use customPlaceables instead.
        /// </summary>
        public static Dictionary<string, DungeonPlaceable> StoredDungeonPlaceables = new Dictionary<string, DungeonPlaceable>() {};


        public static GenericRoomTable GetRoomTable(GlobalDungeonData.ValidTilesets tileset)
        {
            switch (tileset)
            {
                case GlobalDungeonData.ValidTilesets.CASTLEGEON:
                    return RoomTables["castle"];
                case GlobalDungeonData.ValidTilesets.GUNGEON:
                    return RoomTables["gungeon"];
                case GlobalDungeonData.ValidTilesets.MINEGEON:
                    return RoomTables["mines"];
                case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
                    return RoomTables["catacombs"];
                case GlobalDungeonData.ValidTilesets.FORGEGEON:
                    return RoomTables["forge"];
                case GlobalDungeonData.ValidTilesets.SEWERGEON:
                    return RoomTables["sewer"];
                case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
                    return RoomTables["cathedral"];
                case GlobalDungeonData.ValidTilesets.RATGEON:
                    return RoomTables["resourcefulrat"];
                case GlobalDungeonData.ValidTilesets.HELLGEON:
                    return RoomTables["bullethell"];
                default:
                    return RoomTables["gungeon"];
            }
        }
        
        public static T GetAsset<T>(string assetName) where T : UnityEngine.Object
        {
            T item = null;
            foreach (var bundle in AssetBundles.Values)
            {
                item = bundle.LoadAsset<T>(assetName);
                if (item != null)
                    break;
            }
            return item;
        }

    }
}
