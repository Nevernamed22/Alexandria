using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Dungeonator;
using Random = UnityEngine.Random;
//using CustomShrineData = GungeonAPI.ShrineFactory.CustomShrineController;
using RoomData = Alexandria.DungeonAPI.RoomFactory.RoomData;
using RoomCategory = PrototypeDungeonRoom.RoomCategory;
using RoomNormalSubCategory = PrototypeDungeonRoom.RoomNormalSubCategory;
using RoomBossSubCategory = PrototypeDungeonRoom.RoomBossSubCategory;
using RoomSpecialSubCategory = PrototypeDungeonRoom.RoomSpecialSubCategory;
using Alexandria.ItemAPI;
namespace Alexandria.DungeonAPI
{

    public static class DungeonHandler
    {
        //public static float GlobalRoomWeight = 1.5f;
        private static bool initialized = false;
        public static bool debugFlow = false;

        public static void Init()
        {
            if (!initialized)
            {

                //RoomFactory.LoadRoomsFromRoomDirectory();

                DungeonHooks.OnPreDungeonGeneration += OnPreDungeonGen;
                initialized = true;
            }
        }

        public static void OnPreDungeonGen(LoopDungeonGenerator generator, Dungeon dungeon, DungeonFlow flow, int dungeonSeed)
        {
           // Tools.Print("Attempting to override floor layout...", "5599FF");
            //CollectDataForAnalysis(flow, dungeon);
            if (flow.name != "Foyer Flow" && !GameManager.IsReturningToFoyerWithPlayer)
            {
                if (debugFlow)
                {
                    flow = SampleFlow.CreateDebugFlow(dungeon);
                    generator.AssignFlow(flow);
                }
                
                /*else
                {
                    flow = DungeonHandler.CreateSpecialFlowFlow(dungeon);
                    generator.AssignFlow(flow);
                }
                */
              //  Tools.Print("Dungeon name: " + dungeon.name);
               // Tools.Print("Override Flow set to: " + flow.name);
            }
            dungeon = null;
        }
        public static DungeonFlow CreateSpecialFlowFlow(Dungeon dungeon)
        {
            var flow = SampleFlow.CreateEntranceExitFlow(dungeon);
            flow.name = "spec_flow";
            DungeonFlowNode
                customRoom,
                hub = new DungeonFlowNode(flow) { overrideExactRoom = RoomFactory.CreateEmptyRoom() },
                lastNode = hub;
            flow.AddNodeToFlow(hub, flow.FirstNode);
            //Tools.Log("Adding room to flow: " + room.room);

            PrototypeDungeonRoom asset = null;
            foreach (var bundle in StaticReferences.AssetBundles.Values)
            {
                asset = bundle.LoadAsset<PrototypeDungeonRoom>("ChallengeShrine_Gungeon_002");
                if (asset)
                    break;
            }

            customRoom = new DungeonFlowNode(flow) { overrideExactRoom = asset };
            flow.AddNodeToFlow(customRoom, lastNode);
            hub = new DungeonFlowNode(flow) { overrideExactRoom = RoomFactory.CreateEmptyRoom() };
            flow.AddNodeToFlow(hub, customRoom);
            lastNode = hub;
            dungeon = null;
            return flow;
        }


        public static void Register(RoomData roomData)
        {
            var room = roomData.room;
            var wRoom = new WeightedRoom()
            {
                room = room,
                additionalPrerequisites = new DungeonPrerequisite[0],
                weight = roomData.weight
            };

            //bool success = false;
            if (roomData.usesAmbientLight == true)
            {
                room.usesCustomAmbientLight = true;
                room.customAmbientLight = new Color(roomData.AmbientLight_R, roomData.AmbientLight_G, roomData.AmbientLight_B);           
            }

            if (roomData.superSpecialRoomType != null && roomData.superSpecialRoomType.ToLower() != "none")
            {
                switch (roomData.superSpecialRoomType.ToLower())
                {
                    case "blockner_miniboss":
                        StaticReferences.RoomTables["blockner"].includedRooms.Add(wRoom);
                        break;
                    case "shadow_magician":
                        StaticReferences.RoomTables["shadeagunim"].includedRooms.Add(wRoom);
                        break;
                    case "winchester":
                        room.associatedMinimapIcon = RoomIcons.WinchesterRoomIcon;
                        StaticReferences.RoomTables["winchester"].includedRooms.Add(wRoom);
                        break;
                    case "misc_reward":
                        StaticReferences.RoomTables["miscreward"].includedRooms.Add(wRoom);
                        break;
                    case "fireplace":
                        StaticReferences.RoomTables["fireplace"].includedRooms.Add(wRoom);
                        break;

                    case "sewer_entrance":
                        if (!room.name.Contains("SewersEntrance"))
                        {
                            room.name += "_SewersEntrance";
                        }
                        room.category = RoomCategory.SECRET;
                        StaticReferences.RoomTables["sewerentrace"].includedRooms.Add(wRoom);
                        break;
                    case "crest_room":
                        room.associatedMinimapIcon = RoomIcons.CrestRoomIcon;
                        StaticReferences.RoomTables["crestroom"].includedRooms.Add(wRoom);
                        break;

                    case "abbey_entrance":
                        StaticReferences.RoomTables["abbeyentrance"].includedRooms.Add(wRoom);
                        break;
                    case "abbey_extra_secret":
                        room.category = RoomCategory.SECRET;
                        StaticReferences.RoomTables["abbeyextrasecret"].includedRooms.Add(wRoom);
                        break;
                    case "hollow_sell_creep":
                        StaticReferences.RoomTables["rng_entry"].includedRooms.Add(wRoom);
                        break;
                    case "bullet_hell_secret":
                        room.category = RoomCategory.SECRET;
                        StaticReferences.RoomTables["bullet_hell_secret"].includedRooms.Add(wRoom);
                        break;
                    case "glitched_boss":
                        room.category = RoomCategory.BOSS;
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.GlitchBossNames.Add(wRoom.room.name);
                        StaticReferences.RoomTables["glitchedBoss"].includedRooms.Add(wRoom);
                        break;
                    case "gatling_gull":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["gull"].includedRooms.Add(wRoom);
                        break;
                    case "bullet_king":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["bulletking"].includedRooms.Add(wRoom);
                        break;
                    case "trigger_twins":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["triggertwins"].includedRooms.Add(wRoom);
                        break;
                    case "blobulord":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["blobby"].includedRooms.Add(wRoom);
                        break;
                    case "gorgun":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["gorgun"].includedRooms.Add(wRoom);
                        break;
                    case "beholster":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["beholster"].includedRooms.Add(wRoom);
                        break;
                    case "ammoconda":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["ammoconda"].includedRooms.Add(wRoom);
                        break;
                    case "old_king":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["oldking"].includedRooms.Add(wRoom);
                        break;
                    case "treadnaught":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["tank"].includedRooms.Add(wRoom);
                        break;
                    case "cannonbalrog":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["cannonballrog"].includedRooms.Add(wRoom);
                        break;
                    case "mine_flayer":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["flayer"].includedRooms.Add(wRoom);
                        break;
                    case "high_priest":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["pillars"].includedRooms.Add(wRoom);
                        break;
                    case "kill_pillars":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["priest"].includedRooms.Add(wRoom);
                        break;
                    case "wallmonger":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["monger"].includedRooms.Add(wRoom);
                        break;
                    case "door_lord":
                        room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                        StaticReferences.RoomTables["doorlord"].includedRooms.Add(wRoom);
                        break;
                }
            }
            else
            {
                switch (room.category)
                {
                    case RoomCategory.SPECIAL:
                        switch (room.subCategorySpecial)
                        {
                            case RoomSpecialSubCategory.STANDARD_SHOP:  //shops
                                StaticReferences.RoomTables["shop"].includedRooms.Add(wRoom);
                                // Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                                //   success = true;
                                break;
                            case RoomSpecialSubCategory.WEIRD_SHOP:    //subshops
                                StaticReferences.subShopTable.InjectionData.AddRange(GetFlowModifier(roomData));
                                room.doorTopDecorable = (ResourceCache.Acquire("Global Prefabs/Purple_Lantern") as GameObject);
                                // Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                                // success = true;
                                break;

                            default:
                                StaticReferences.RoomTables["special"].includedRooms.Add(wRoom);
                                room.doorTopDecorable = (ResourceCache.Acquire("Global Prefabs/Shrine_Lantern") as GameObject);
                                //Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                                // success = true;
                                break;
                        }
                        break;
                    case RoomCategory.SECRET:
                        StaticReferences.RoomTables["secret"].includedRooms.Add(wRoom);
                        //success = true;
                        break;
                    //===========================PUTS YOUR BOSS ROOMS IN THE POOLS DEFINED IN STATICREFERENCES ====================
                    case RoomCategory.BOSS:
                        switch (room.subCategoryBoss)
                        {
                            case RoomBossSubCategory.FLOOR_BOSS:
                                foreach (var p in room.prerequisites)

                                    if (p.requiredTileset == GlobalDungeonData.ValidTilesets.CASTLEGEON)
                                    {
                                        StaticReferences.RoomTables["gull"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["triggertwins"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["bulletking"].includedRooms.Add(wRoom);

                                    }
                                    else if (p.requiredTileset == GlobalDungeonData.ValidTilesets.SEWERGEON)
                                    {
                                        StaticReferences.RoomTables["blobby"].includedRooms.Add(wRoom);
                                    }
                                    else if (p.requiredTileset == GlobalDungeonData.ValidTilesets.GUNGEON)
                                    {
                                        StaticReferences.RoomTables["gorgun"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["beholster"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["ammoconda"].includedRooms.Add(wRoom);
                                    }
                                    else if (p.requiredTileset == GlobalDungeonData.ValidTilesets.CATHEDRALGEON)
                                    {
                                        StaticReferences.RoomTables["oldking"].includedRooms.Add(wRoom);
                                    }
                                    else if (p.requiredTileset == GlobalDungeonData.ValidTilesets.MINEGEON)
                                    {
                                        StaticReferences.RoomTables["tank"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["cannonballrog"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["flayer"].includedRooms.Add(wRoom);
                                    }
                                    else if (p.requiredTileset == GlobalDungeonData.ValidTilesets.CATHEDRALGEON)
                                    {
                                        StaticReferences.RoomTables["pillars"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["priest"].includedRooms.Add(wRoom);
                                        StaticReferences.RoomTables["monger"].includedRooms.Add(wRoom);
                                    }
                                    else
                                    {
                                        //StaticReferences.RoomTables["doorlord"].includedRooms.Add(wRoom);
                                    }
                                room.associatedMinimapIcon = RoomIcons.BossRoomIcon;

                                break;
                            case RoomBossSubCategory.MINI_BOSS:
                                StaticReferences.RoomTables["blockner"].includedRooms.Add(wRoom);
                                StaticReferences.RoomTables["shadeagunim"].includedRooms.Add(wRoom);
                                /*
                                if (room.name.ToLower().Contains("blockner"))
                                {
                                    StaticReferences.RoomTables["blockner"].includedRooms.Add(wRoom);

                                }
                                else if (room.name.ToLower().Contains("agunim"))
                                {
                                    StaticReferences.RoomTables["shadeagunim"].includedRooms.Add(wRoom);
                                }
                                else
                                {
                                    StaticReferences.RoomTables["blockner"].includedRooms.Add(wRoom);
                                    StaticReferences.RoomTables["shadeagunim"].includedRooms.Add(wRoom);
                                }
                                */
                                //StaticReferences.RoomTables["fuselier"].includedRooms.Add(wRoom);
                                room.associatedMinimapIcon = RoomIcons.BossRoomIcon;
                                break;
                            default:
                                //StaticReferences.RoomTables["doorlord"].includedRooms.Add(wRoom);
                                // room.associatedMinimapIcon = iconPrefab;
                                break;
                        }
                        break;


                    //===============================================
                    default:
                        foreach (var p in room.prerequisites)
                            if (p.requireTileset)
                                try
                                {
                                    if (p.requiredTileset == GlobalDungeonData.ValidTilesets.RATGEON)
                                    {
                                        if (wRoom.room.Height == 24 && wRoom.room.Width == 34)
                                        {
                                            wRoom.room.IsLostWoodsRoom = true;
                                            wRoom.room.subCategorySpecial = PrototypeDungeonRoom.RoomSpecialSubCategory.NPC_STORY;
                                            StaticReferences.GetRoomTable(p.requiredTileset).includedRooms.Add(wRoom);
                                        }
                                        else
                                        {
                                            ShrineTools.PrintError($"Room name: " + wRoom.room.name.ToString() + " is of an INVALID height/width, and will NOT be added to the pool! Rat floor rooms should always be 34x | 24y to prevent issues!");
                                        }
                                    }
                                    else
                                    {
                                        StaticReferences.GetRoomTable(p.requiredTileset).includedRooms.Add(wRoom);
                                    }
                                }
                                catch (Exception e)
                                {
                                    ETGModConsole.Log(e.ToString());
                                    ETGModConsole.Log("This Room fucks it up:" + room.name);
                                }
                        //   success = true;
                        break;
                }

            }
            RemoveTilesetPrereqs(room);
        }
        public static GameObject MinimapShrineIconPrefab;

        /*
        public static Dictionary<string, string> SpecificBossKeys = new Dictionary<string, string>()
        {
            {"bulletking","bulletking"},
            {"triggertwins","triggertwins"},
            {"gatlinggull","gull"},
            //========
            {"beholster","beholster"},
            {"ammoconda","ammoconda"},
            {"gorgun","gorgun"},
            //========
            {"tank","tank"},
            {"cannonballrog","cannonballrog"},
            {"mineflayer","flayer"},
            //========
            {"killpillars","pillars"},
            {"highpriest","priest"},
            {"wallmonger","monger"},
        };
        */



        public static void RegisterForShrine(RoomData roomData)
        {
            var room = roomData.room;
            var wRoom = new WeightedRoom()
            {
                room = room,
                additionalPrerequisites = new DungeonPrerequisite[0],
                weight = roomData.weight
            };
            //AssetBundle shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");

            //GameObject iconPrefab = (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon", ".prefab");
            //room.associatedMinimapIcon = iconPrefab;
            
            // bool success = false;
            switch (room.category)
            {
                case RoomCategory.SPECIAL:
                    switch (room.subCategorySpecial)
                    {
                        case RoomSpecialSubCategory.STANDARD_SHOP:  //shops
                            StaticReferences.RoomTables["shop"].includedRooms.Add(wRoom);
                            // Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                        //    success = true;
                            break;
                        case RoomSpecialSubCategory.WEIRD_SHOP:    //subshops
                            StaticReferences.subShopTable.InjectionData.AddRange(GetFlowModifier(roomData));
                            // Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                        //    success = true;
                            break;
                        default:
                            StaticReferences.RoomTables["special"].includedRooms.Add(wRoom);
                            //Tools.Print($"Registering {roomData.room.name} with weight {wRoom.weight} as {roomData.category}:{roomData.specialSubCategory}");
                          //  success = true;
                            break;
                    }
                    break;
                case RoomCategory.SECRET:
                    StaticReferences.RoomTables["secret"].includedRooms.Add(wRoom);
                    //success = true;
                    break;
                case RoomCategory.BOSS:
                    // TODO
                    break;
                default:
                    foreach (var p in room.prerequisites)
                        if (p.requireTileset)
                            StaticReferences.GetRoomTable(p.requiredTileset).includedRooms.Add(wRoom);
                   // success = true;
                    break;
            }
            //success = true;
            RemoveTilesetPrereqs(room);


        }

        public static List<ProceduralFlowModifierData> GetFlowModifier(RoomData roomData)
        {
            var room = roomData.room;
            List<ProceduralFlowModifierData> data = new List<ProceduralFlowModifierData>();
            var tilesetPrereqs = new List<DungeonPrerequisite>();
            foreach (var p in room.prerequisites)
            {
                if (p.requireTileset)
                {
                    data.Add(new ProceduralFlowModifierData()
                    {

                        annotation = room.name,
                        placementRules = new List<ProceduralFlowModifierData.FlowModifierPlacementType>()
                        {
                            ProceduralFlowModifierData.FlowModifierPlacementType.END_OF_CHAIN,
                            ProceduralFlowModifierData.FlowModifierPlacementType.HUB_ADJACENT_NO_LINK,
                        },
                        exactRoom = room,
                        selectionWeight = roomData.weight,
                        chanceToSpawn = 1,
                        prerequisites = new DungeonPrerequisite[] { p }, //doesn't include all the other prereqs, pls fix
                        CanBeForcedSecret = true,
                        
                    });
                }
            }

            RemoveTilesetPrereqs(room);
            if (data.Count == 0)
            {
                data.Add(new ProceduralFlowModifierData()
                {

                    annotation = room.name,
                    placementRules = new List<ProceduralFlowModifierData.FlowModifierPlacementType>()
                        {
                            ProceduralFlowModifierData.FlowModifierPlacementType.END_OF_CHAIN,
                            ProceduralFlowModifierData.FlowModifierPlacementType.HUB_ADJACENT_NO_LINK,
                        },
                    exactRoom = room,
                    selectionWeight = roomData.weight,
                    chanceToSpawn = 1,
                    prerequisites = new DungeonPrerequisite[0], //doesn't include all the other prereqs, pls fix
                    CanBeForcedSecret = true,
                });
            }


            return data;
        }

        public static void RemoveTilesetPrereqs(PrototypeDungeonRoom room)
        {
            var tilesetPrereqs = new List<DungeonPrerequisite>();
            foreach (var p in room.prerequisites)
            {
                if (p.requireTileset)
                    tilesetPrereqs.Add(p);
            }

            foreach (var p in tilesetPrereqs)
                room.prerequisites.Remove(p);
        }

        public static bool BelongsOnThisFloor(RoomData data, string dungeonName)
        {
            if (data.floors == null || data.floors.Length == 0) return true;
            bool onThisFloor = false;
            foreach (var floor in data.floors)
            {
                if (floor.ToLower().Equals(dungeonName.ToLower())) { onThisFloor = true; break; }
            }
            return onThisFloor;
        }

        public static GenericRoomTable GetSpecialRoomTable()
        {
            foreach (var entry in GameManager.Instance.GlobalInjectionData.entries)
                if (entry.injectionData?.InjectionData != null)
                    foreach (var data in entry.injectionData.InjectionData)
                    {
                        if (data.exactRoom != null)
                        {
                            ShrineTools.Log(data.exactRoom.name);

                            if (data.prerequisites != null)
                                foreach (var p in data.prerequisites)
                                    ShrineTools.Log("\t" + p.prerequisiteType);

                            if (data.placementRules != null)
                                foreach (var p in data.placementRules)
                                    ShrineTools.Log("\t" + p);
                        }
                    }
            return null;
        }

        public static void CollectDataForAnalysis(DungeonFlow flow, Dungeon dungeon)
        {
            try
            {
                //GetSpecialRoomTable();
                foreach (var room in flow.fallbackRoomTable.includedRooms.elements)
                {
                   // Tools.Print("Fallback table: " + room?.room?.name);
                }
            }
            catch (Exception e)
            {
                ShrineTools.PrintException(e);
            }
            dungeon = null;
        }

        public static void LogProtoRoomData(PrototypeDungeonRoom room)
        {
            int i = 0;
            ShrineTools.LogPropertiesAndFields(room, "ROOM");

            //room.placedObjects.ForEach(x =>  Tools.Log($"\n----------------Object #{i++}----------------" +x?.placeableContents +"\n" + x?.placeableContents?.variantTiers[0]));
                //Tools.LogPropertiesAndFields(placedObject, "PLACED OBJECT");
                //Tools.LogPropertiesAndFields(placedObject?.placeableContents, "PLACEABLE CONTENT");
                //Tools.LogPropertiesAndFields(placedObject?.placeableContents?.variantTiers[0], "VARIANT TIERS"););

            foreach (var placedObject in room.placedObjects)
            {
                ShrineTools.Log($"\n----------------Object #{i++}----------------");
                ShrineTools.LogPropertiesAndFields(placedObject, "PLACED OBJECT");
                ShrineTools.LogPropertiesAndFields(placedObject?.placeableContents, "PLACEABLE CONTENT");
                ShrineTools.LogPropertiesAndFields(placedObject?.placeableContents?.variantTiers[0], "VARIANT TIERS");
            }

            ShrineTools.Print("==LAYERS==");
            foreach (var layer in room.additionalObjectLayers)
            {
                //Tools.LogPropertiesAndFields(layer);
            }
        }
    }
}