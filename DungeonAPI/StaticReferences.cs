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

    [HarmonyPatch(typeof(DungeonFlowNode))]
    [HarmonyPatch("UsesGlobalBossData", MethodType.Getter)]
    class TablePatch
    {
        [HarmonyPrefix]
        public static bool Postfix(ref bool __result, DungeonFlowNode __instance)
        {
            bool isGlitchedTable = __instance.overrideRoomTable != null ? __instance.overrideRoomTable.name == "alexandriaGlitchTable" : false;
            return GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SUPERBOSSRUSH && __instance.overrideExactRoom == null  && __instance.roomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && isGlitchedTable == false;
        }
    }


    [HarmonyPatch(typeof(RoomHandler))]
    [HarmonyPatch("HandleBossClearReward", MethodType.Normal)]
    class PedestalPatch
    {
        [HarmonyPrefix]
        public static void Postfix(RoomHandler __instance)
        {
            var name = __instance.GetRoomName();
            if (name != null && StaticReferences.GlitchBossNames.Contains(name))
            {
                for (int j = 0; j < 8; j++)
                {
                    GameObject gameObject = GameManager.Instance.Dungeon.sharedSettingsPrefab.ChestsForBosses.SelectByWeight();
                    RewardPedestal component = gameObject.GetComponent<RewardPedestal>();
                    DungeonData data = GameManager.Instance.Dungeon.data;
                    IntVector2 centeredVisibleClearSpot = __instance.GetCenteredVisibleClearSpot(2, 2);
                    RewardPedestal rewardPedestal2 = RewardPedestal.Spawn(component, centeredVisibleClearSpot, __instance);
                    rewardPedestal2.IsBossRewardPedestal = true;
                    rewardPedestal2.lootTable.lootTable = __instance.OverrideBossRewardTable;
                    data[centeredVisibleClearSpot].isOccupied = true;
                    data[centeredVisibleClearSpot + IntVector2.Right].isOccupied = true;
                    data[centeredVisibleClearSpot + IntVector2.Up].isOccupied = true;
                    data[centeredVisibleClearSpot + IntVector2.One].isOccupied = true;
                }
            }
        }
    }

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
            { "winchester", "WinchesterRoomTable" }
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
                    ShrineTools.PrintError($"Failed to load special room table: {entry.Key}:{entry.Value}");
                    ShrineTools.PrintException(e);
                }
            }


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

            subShopTable = AssetBundles["shared_auto_001"].LoadAsset<SharedInjectionData>("_global injected subshop table");
            //foreach(var data in subShopTable.InjectionData)
            //{
            //    Tools.LogPropertiesAndFields(data, data.annotation);
            //}
                

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

            ShrineTools.Print("Static references initialized.");
        }




        public static DungeonFlowNode ConvertNodeToRoomTable(DungeonFlowNode node, float overrideWeight = 1)
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

        public static IEnumerator Delay(DungeonFlowNode flow)
        {
            yield return null;
            flow.overrideExactRoom = null;
            yield break;
        }

        public static ProceduralFlowModifierData ProcessRoomTableThing(SharedInjectionData shared , ProceduralFlowModifierData data, float defaultWeight = 1)
        {
            ProceduralFlowModifierData modifierData = new ProceduralFlowModifierData()
            {
                annotation = data.annotation,
                CanBeForcedSecret = data.CanBeForcedSecret,
                chanceToLock = data.chanceToLock,
                chanceToSpawn = data.chanceToSpawn,
                DEBUG_FORCE_SPAWN = data.DEBUG_FORCE_SPAWN,
                exactRoom = null,
                exactSecondaryRoom = data.exactSecondaryRoom,
                framedCombatNodes = data.framedCombatNodes,
                IsWarpWing = data.IsWarpWing,
                OncePerRun = data.OncePerRun,
                placementRules = data.placementRules,
                prerequisites = data.prerequisites.ToArray(),
                RandomNodeChildMinDistanceFromEntrance = data.RandomNodeChildMinDistanceFromEntrance,
                RequiredValidPlaceable = data.RequiredValidPlaceable,
                RequiresMasteryToken = data.RequiresMasteryToken,
                roomTable = ScriptableObject.CreateInstance<GenericRoomTable>(),
               
            };

            modifierData.roomTable.includedRooms = new WeightedRoomCollection()
            {
                elements = new()
                {
                            new WeightedRoom()
                            {
                                room = data.exactRoom,
                                additionalPrerequisites = data.exactRoom.prerequisites != null ? data.exactRoom.prerequisites.ToArray() : new DungeonPrerequisite[0],
                                weight = defaultWeight
                            }
                }

            };
            modifierData.roomTable.includedRoomTables = new List<GenericRoomTable>() { };

            data.chanceToSpawn = 0;
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
