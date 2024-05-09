using System;
using System.Collections.Generic;
using System.Linq;
using Alexandria.ItemAPI;
using Dungeonator;
using FullInspector;
using Planetside;
using UnityEngine;
using static Alexandria.DungeonAPI.SpecialComponents;

namespace Alexandria.DungeonAPI
{
	public static class aa
	{
        public static void LogRoomSubtypes(this Dungeon self)
        {
			Debug.Log(self.DungeonShortName + " : " + self.roomMaterialDefinitions.Count());

        }
    }


	public class SetupExoticObjects
	{
		public static GameObject assignedGameObject;

        public static void InitialiseObjects()
		{
			Dungeon orLoadByName = DungeonDatabase.GetOrLoadByName("base_castle");


			Dungeon dungeon = DungeonDatabase.GetOrLoadByName("base_sewer");
			Dungeon orLoadByName2 = DungeonDatabase.GetOrLoadByName("base_gungeon");
			Dungeon dungeon2 = DungeonDatabase.GetOrLoadByName("base_cathedral");
			Dungeon dungeon3 = DungeonDatabase.GetOrLoadByName("base_mines");
			Dungeon dungeon4 = DungeonDatabase.GetOrLoadByName("base_resourcefulrat");
			Dungeon dungeon5 = DungeonDatabase.GetOrLoadByName("base_catacombs");
			Dungeon dungeon6 = DungeonDatabase.GetOrLoadByName("base_nakatomi");
			Dungeon dungeon7 = DungeonDatabase.GetOrLoadByName("base_forge");
			Dungeon dungeon8 = DungeonDatabase.GetOrLoadByName("base_bullethell");
			Dungeon dungeon9 = DungeonDatabase.GetOrLoadByName("finalscenario_bullet");
			Dungeon dungeon10 = DungeonDatabase.GetOrLoadByName("finalscenario_robot");
			Dungeon dungeon11 = DungeonDatabase.GetOrLoadByName("finalscenario_convict");
			Dungeon dungeon12 = DungeonDatabase.GetOrLoadByName("finalscenario_soldier");
			Dungeon dungeon13 = DungeonDatabase.GetOrLoadByName("finalscenario_guide");
            Dungeon tutorialDungeon = DungeonDatabase.GetOrLoadByName("base_tutorial");


            //assignedGameObject = dungeon4.PatternSettings.flows[0].AllNodes[12].overrideExactRoom.additionalObjectLayers[0].placedObjects[11].nonenemyBehaviour.gameObject;
            Golden_Boss_Pedestal = dungeon4.PatternSettings.flows[0].AllNodes[12].overrideExactRoom.additionalObjectLayers[0].placedObjects[11].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();


            SetupExoticObjects.ForgeGunTrap_North = LoadHelper.LoadAssetFromAnywhere<GameObject>("forge_face_shootssouth");
            SetupExoticObjects.ForgeGunTrap_East = LoadHelper.LoadAssetFromAnywhere<GameObject>("forge_face_shootswest");
            SetupExoticObjects.ForgeGunTrap_West = LoadHelper.LoadAssetFromAnywhere<GameObject>("forge_face_shootseast");

            foreach (WeightedRoom weightedRoom in dungeon7.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
            {
                if (weightedRoom.room != null && !string.IsNullOrEmpty(weightedRoom.room.name))
                {
                    if (weightedRoom.room.name.ToLower().StartsWith("forge_joe_hot_fire_004"))
                    {
                        Moving_Platform_Forge = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;

                        //Moving_Platform_Hollow = weightedRoom.room.placedObjects[6].nonenemyBehaviour.gameObject;
                    }
                }
            }


            foreach (WeightedRoom weightedRoom in dungeon5.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom.room != null && !string.IsNullOrEmpty(weightedRoom.room.name))
				{
					if (weightedRoom.room.name.ToLower().StartsWith("hollow_joeki_004"))
					{
                        Moving_Platform_Hollow = weightedRoom.room.placedObjects[6].nonenemyBehaviour.gameObject;               
                    }
                    if (weightedRoom.room.name.ToLower().StartsWith("hollow_turret_line_001"))
                    {
						HollowGunTrap_North = weightedRoom.room.placedObjects[3].nonenemyBehaviour.gameObject;
                        HollowGunTrap_East = weightedRoom.room.placedObjects[2].nonenemyBehaviour.gameObject;
                        HollowGunTrap_West = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
                    }
                }
			}

            foreach (WeightedRoom weightedRoom in dungeon3.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
            {
                if (weightedRoom.room != null && !string.IsNullOrEmpty(weightedRoom.room.name))
                {
                    if (weightedRoom.room.name.ToLower().StartsWith("mines_trap_dart_room_001"))
					{
                        MinesGunTrap_North = weightedRoom.room.placedObjects[3].nonenemyBehaviour.gameObject;
                        MinesGunTrap_East = weightedRoom.room.placedObjects[7].nonenemyBehaviour.gameObject;
                        MinesGunTrap_West = weightedRoom.room.placedObjects[8].nonenemyBehaviour.gameObject;
                        /*
                        int i = 0;
                        foreach (var placed in weightedRoom.room.placedObjects)
                        {
                            if (placed.nonenemyBehaviour != null)
                            {
                                ETGModConsole.Log(placed.nonenemyBehaviour.name + " : " + i);
                            }
                            i++;
                        }
						*/
                    }

                    if (weightedRoom.room.name.ToLower().StartsWith("mines_ign_normal_shelleton_01_b"))
                    {

                        ExplosiveBarrelMinecart = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<MineCartFactory>().MineCartPrefab.gameObject.InstantiateAndFakeprefab();
                        ExplosiveBarrelMinecart.GetComponent<MineCartController>().MoveCarriedCargoIntoCart = true;

                        MinecarftFactory_Object = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
                        /*
                        int i = 0;
                        foreach (var placed in weightedRoom.room.placedObjects)
                        {
                            if (placed.nonenemyBehaviour != null)
                            {
                                ETGModConsole.Log(placed.nonenemyBehaviour.name + " : " + i);
                            }
                            i++;
                        }
						*/
                        //SetupExoticObjects.HorizontalCrusher = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
                    }
					
					if (weightedRoom.room.name.ToLower().StartsWith("mines_trap_dart_room_001"))
					{
                        Moving_Platform_Mines = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;

						/*
                        int i = 0;
                        foreach (var placed in weightedRoom.room.placedObjects)
                        {
                            if (placed.nonenemyBehaviour != null)
                            {
                                ETGModConsole.Log(placed.nonenemyBehaviour.name + " : " + i);
                            }
                            i++;
                        }
						*/
                    }
					
                }
            }
            foreach (WeightedRoom weightedRoom in dungeon.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom.room != null && !string.IsNullOrEmpty(weightedRoom.room.name))
				{
					if (weightedRoom.room.name.ToLower().StartsWith("sewer_trash_compactor_001"))
					{
						SetupExoticObjects.HorizontalCrusher = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
					}
                    if (weightedRoom.room.name.ToLower().StartsWith("sewer_trap_room_001"))
					{
                        Moving_Platform_Sewer = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
                    }
                }
            }
			foreach (WeightedRoom weightedRoom2 in dungeon7.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom2.room != null && !string.IsNullOrEmpty(weightedRoom2.room.name))
				{
					if (weightedRoom2.room.name.ToLower().StartsWith("forge_normal_cubulead_03"))
					{
						SetupExoticObjects.VerticalCrusher = weightedRoom2.room.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.FireBarTrap = weightedRoom2.room.placedObjects[7].nonenemyBehaviour.gameObject;
					}
					if (weightedRoom2.room.name.ToLower().StartsWith("forge_connector_flamepipes_01"))
					{
						SetupExoticObjects.FlamePipeNorth = weightedRoom2.room.placedObjects[1].nonenemyBehaviour.gameObject;
						SetupExoticObjects.FlamePipeWest = weightedRoom2.room.placedObjects[3].nonenemyBehaviour.gameObject;
						SetupExoticObjects.FlamePipeEast = weightedRoom2.room.placedObjects[2].nonenemyBehaviour.gameObject;
					}
				}
			}
			foreach (WeightedRoom weightedRoom3 in dungeon2.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom3.room != null && !string.IsNullOrEmpty(weightedRoom3.room.name))
				{
					if (weightedRoom3.room.name.ToLower().StartsWith("cathedral_brent_standard_02"))
					{
						SetupExoticObjects.Pew = weightedRoom3.room.placedObjects[0].nonenemyBehaviour.gameObject;
					}
				}
			}
			foreach (WeightedRoom weightedRoom4 in dungeon8.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom4.room != null && !string.IsNullOrEmpty(weightedRoom4.room.name))
				{
					if (weightedRoom4.room.name.ToLower().StartsWith("hell_connector_pathburst_01"))
					{
						SetupExoticObjects.FlameburstTrap = weightedRoom4.room.placedObjects[3].nonenemyBehaviour.gameObject;
					}
				}
			}
			RobotPastController component = dungeon10.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<RobotPastController>();
			foreach (Transform transform in component.GetComponentsInChildren<Transform>())
			{
				if (transform.gameObject.name.Contains("trash fire") && !transform.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RobotsPastTrashfire = transform.gameObject;
				}
				if (transform.gameObject.name.Contains("bomb ass car") && !transform.gameObject.name.Contains("n") && !transform.gameObject.name.Contains("w") && !transform.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RobotsPastCar = transform.gameObject;
				}
				if (transform.gameObject.name.Contains("bomb ass car n") && !transform.gameObject.name.Contains("e") && !transform.gameObject.name.Contains("w") && !transform.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RobotsPastCarNorth = transform.gameObject;
				}
				if (transform.gameObject.name.Contains("bomb ass car nw") && !transform.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RobotsPastCarNorthWest = transform.gameObject;
				};
				if (transform.gameObject.name.Contains("bomb ass car ne") && !transform.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RobotsPastCarNorthEast = transform.gameObject;
				}
			}
			ConvictPastController component2 = dungeon11.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<ConvictPastController>();
			foreach (Transform transform2 in component2.GetComponentsInChildren<Transform>())
			{

				if (transform2.gameObject.name.Contains("Couch") && !transform2.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CouchLeft = transform2.gameObject;
				}
				if (transform2.gameObject.name.Contains("Couch (1)"))		
				{
					SetupExoticObjects.CouchRight = transform2.gameObject;
				}

				if (transform2.gameObject.name.Contains("FernFriend") && !transform2.gameObject.name.Contains("("))		
				{
					SetupExoticObjects.PottedFern = transform2.gameObject;
				}
				if (transform2.gameObject.name.Contains("Henchman") && !transform2.gameObject.name.Contains("("))				
				{
					SetupExoticObjects.NPCHenchman = transform2.gameObject;
				}
			}
			PastLabMarineController component3 = dungeon12.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<PastLabMarineController>();
			foreach (Transform transform3 in component3.GetComponentsInChildren<Transform>())
			{
				if (transform3.gameObject.name.Contains("Terlet") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CellToilet = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("CPU (1)"))
				{
					SetupExoticObjects.DecorativeComputer = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Top") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.MarinesPastMachineDecor = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("PapersPlease") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.DecorativePapers = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Crates_Left") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CratesLeft = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Crates_Right") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CratesRight = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("NPC_Dying_Scientist") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.NPCDyingScientist = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Blair") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistBlair = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Childs") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistChilds = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Copper") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistCopper = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Dukes") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistDukes = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Gene") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistGene = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Macready") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistMacready = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Scientist_Windows") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.ScientistWindows = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("Crazon") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.NPCTootsCraze = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("CrateMarine_RightRed") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RedMarineRight = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("CrateMarine_LeftRed") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RedMarineLeft = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("CrateMarine_RightGreen") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.GreenMarineRight = transform3.gameObject;
				}
				if (transform3.gameObject.name.Contains("CrateMarine_LeftGreen") && !transform3.gameObject.name.Contains("("))
				{
					SetupExoticObjects.GreenMarineLeft = transform3.gameObject;
				}
			}
			GuidePastController component4 = dungeon13.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<GuidePastController>();
			foreach (Transform transform4 in component4.GetComponentsInChildren<Transform>())
			{
				if (transform4.gameObject.name.Contains("Gargoyle") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.GargoyleStatue = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Cryotank_L") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CryotankLeft = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Cryotank_R") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.CryotankRight = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("UnconsciousNazi_01") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.UnconsciousBlackswordSoldier1 = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("UnconsciousNazi_02") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.UnconsciousBlackswordSoldier2 = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("rotato") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.RotatingGreenThing = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Dark_Shelf") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.DarkShelfWithSkullInJar = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Dark_Shelf (1)"))
				{
					SetupExoticObjects.DarkShelfWithGlobe = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Dark_Shelf (2)"))
				{
					SetupExoticObjects.DarkShelfWithMagnifyingGlass = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Dark_Shelf (3)"))
				{
					SetupExoticObjects.DarkShelfWithVials = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("Dark_Shelf (5)"))
				{
					SetupExoticObjects.DarkShelfWithEmptyJar = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("WesleyPipes") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.DecorativeCables = transform4.gameObject;
				}
				if (transform4.gameObject.name.Contains("MachineBox_L") && !transform4.gameObject.name.Contains("("))
				{
					SetupExoticObjects.MachinePedestalWithPurpleThing = transform4.gameObject;
				}
			}
			BulletPastRoomController component5 = dungeon9.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.GetComponent<BulletPastRoomController>();
			foreach (Transform transform5 in component5.GetComponentsInChildren<Transform>())
			{
				if (transform5.gameObject.name.Contains("throne") && !transform5.gameObject.name.Contains("("))
				{
					SetupExoticObjects.GoldenThrone = transform5.gameObject;
				}
			}
			NightclubCrowdController crowdController = component2.crowdController;
			SetupExoticObjects.ConvictPastCrowdNPC_01 = crowdController.Dancers[0].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_02 = crowdController.Dancers[1].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_03 = crowdController.Dancers[2].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_04 = crowdController.Dancers[3].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_05 = crowdController.Dancers[4].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_06 = crowdController.Dancers[5].gameObject;
			SetupExoticObjects.ConvictPastCrowdNPC_07 = crowdController.Dancers[6].gameObject;
			SetupExoticObjects.ConvictPastDancers = new GameObject[]
			{
				SetupExoticObjects.ConvictPastCrowdNPC_01,
				SetupExoticObjects.ConvictPastCrowdNPC_02,
				SetupExoticObjects.ConvictPastCrowdNPC_03,
				SetupExoticObjects.ConvictPastCrowdNPC_04,
				SetupExoticObjects.ConvictPastCrowdNPC_05,
				SetupExoticObjects.ConvictPastCrowdNPC_06,
				SetupExoticObjects.ConvictPastCrowdNPC_07
			};
			SetupExoticObjects.DecorativeAnvilLeft = dungeon7.stampData.objectStamps[6].objectReference;
			SetupExoticObjects.DecorativeAnvilRight = dungeon7.stampData.objectStamps[7].objectReference;
			SetupExoticObjects.DecorativeGoldVase = dungeon5.stampData.objectStamps[4].objectReference;
			SetupExoticObjects.DecorativeStoneUrn = dungeon5.stampData.objectStamps[5].objectReference;
			SetupExoticObjects.DecorativeIceSpikeLarge = dungeon5.stampData.objectStamps[8].objectReference;
			SetupExoticObjects.DecorativeIceSpikeMed = dungeon5.stampData.objectStamps[9].objectReference;
			SetupExoticObjects.DecorativeIceSpikeSmall1 = dungeon5.stampData.objectStamps[10].objectReference;
			SetupExoticObjects.DecorativeIceSpikeSmall2 = dungeon5.stampData.objectStamps[11].objectReference;
			SetupExoticObjects.DecorativeOfficeChairFront = dungeon6.stampData.objectStamps[0].objectReference;
			SetupExoticObjects.DecorativeOfficeChairLeft = dungeon6.stampData.objectStamps[1].objectReference;
			SetupExoticObjects.DecorativeOfficeChairRight = dungeon6.stampData.objectStamps[2].objectReference;
			SetupExoticObjects.WaterCoolerFront = dungeon6.stampData.objectStamps[3].objectReference;
			SetupExoticObjects.WaterCoolerSide = dungeon6.stampData.objectStamps[4].objectReference;
			SetupExoticObjects.PottedPlant = dungeon6.stampData.objectStamps[6].objectReference;
			SetupExoticObjects.CardboardBox1 = dungeon6.stampData.objectStamps[7].objectReference;
			SetupExoticObjects.CardboardBox2 = dungeon6.stampData.objectStamps[8].objectReference;
			SetupExoticObjects.PottedPlantLongHorizontal = dungeon6.stampData.objectStamps[9].objectReference;
			SetupExoticObjects.PottedPlantLongVertical = dungeon6.stampData.objectStamps[11].objectReference;
			SetupExoticObjects.WetFloorSign = dungeon6.stampData.objectStamps[12].objectReference;
			SetupExoticObjects.TrashBag1 = dungeon.stampData.objectStamps[0].objectReference;
			SetupExoticObjects.TrashBag2 = dungeon.stampData.objectStamps[1].objectReference;
			SetupExoticObjects.TrashBag3 = dungeon.stampData.objectStamps[2].objectReference;
			SetupExoticObjects.ManuscriptTableClosed = dungeon2.stampData.objectStamps[11].objectReference;
			SetupExoticObjects.ManuscriptTableOpen = dungeon2.stampData.objectStamps[12].objectReference;
			SetupExoticObjects.ManuscriptTableEmpty = dungeon2.stampData.objectStamps[8].objectReference;
			SetupExoticObjects.ManuscriptTableSide = dungeon2.stampData.objectStamps[9].objectReference;
			SetupExoticObjects.CheeseCandle1 = dungeon4.stampData.objectStamps[22].objectReference;
			SetupExoticObjects.CheeseCandle2 = dungeon4.stampData.objectStamps[23].objectReference;
			SetupExoticObjects.CheeseCandle3 = dungeon4.stampData.objectStamps[24].objectReference;
			SetupExoticObjects.CheeseCandle4 = dungeon4.stampData.objectStamps[25].objectReference;
			SetupExoticObjects.StackOfWood = dungeon3.stampData.objectStamps[2].objectReference;
			SetupExoticObjects.StalagmiteLarge = dungeon3.stampData.objectStamps[3].objectReference;
			SetupExoticObjects.StalagmiteMedium = dungeon3.stampData.objectStamps[4].objectReference;
			SetupExoticObjects.FloorStones1 = dungeon3.stampData.objectStamps[6].objectReference;
			SetupExoticObjects.FloorStones2 = dungeon3.stampData.objectStamps[7].objectReference;
			SetupExoticObjects.FloorStones3 = dungeon3.stampData.objectStamps[8].objectReference;
			SetupExoticObjects.FloorStones4 = dungeon3.stampData.objectStamps[9].objectReference;
			SetupExoticObjects.ShabbyShelf1 = dungeon3.stampData.objectStamps[10].objectReference;
			SetupExoticObjects.ShabbyShelf2 = dungeon3.stampData.objectStamps[11].objectReference;
			SetupExoticObjects.Shovel = dungeon3.stampData.objectStamps[15].objectReference;
			SetupExoticObjects.UpturnedMinecart = dungeon3.stampData.objectStamps[16].objectReference;
			if (dungeon6)
			{
				if (dungeon6.PatternSettings.flows[0].name == "FS4_Nakatomi_Flow")
				{
					if (dungeon6.PatternSettings.flows[0].AllNodes.Count == 14)
					{
						SetupExoticObjects.MopAndBucket = dungeon6.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.CardboardBox3 = dungeon6.PatternSettings.flows[0].AllNodes[0].overrideExactRoom.placedObjects[2].nonenemyBehaviour.gameObject;
						SetupExoticObjects.ACUnit = dungeon6.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject;
						SetupExoticObjects.ACVent = dungeon6.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[2].nonenemyBehaviour.gameObject;
						SetupExoticObjects.KitchenChairFront = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject;
						SetupExoticObjects.KitchenChairLeft = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[8].nonenemyBehaviour.gameObject;
						SetupExoticObjects.KitchenChairRight = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[12].nonenemyBehaviour.gameObject;
						SetupExoticObjects.KitchenCounter = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[16].nonenemyBehaviour.gameObject;
						SetupExoticObjects.SteelTableHorizontal = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[6].nonenemyBehaviour.gameObject;
						SetupExoticObjects.SteelTableVertical = dungeon6.PatternSettings.flows[0].AllNodes[4].overrideExactRoom.placedObjects[3].nonenemyBehaviour.gameObject;
						SetupExoticObjects.BathroomStallDividerNorth = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.BathroomStallDividerEast = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[6].nonenemyBehaviour.gameObject;
						SetupExoticObjects.BathroomStallDividerWest = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[9].nonenemyBehaviour.gameObject;
						SetupExoticObjects.ToiletNorth = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[2].nonenemyBehaviour.gameObject;
						SetupExoticObjects.ToiletEast = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[7].nonenemyBehaviour.gameObject;
						SetupExoticObjects.ToiletWest = dungeon6.PatternSettings.flows[0].AllNodes[6].overrideExactRoom.placedObjects[10].nonenemyBehaviour.gameObject;
						SetupExoticObjects.GlassWallVertical = dungeon6.PatternSettings.flows[0].AllNodes[7].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.GlassWallHorizontal = dungeon6.PatternSettings.flows[0].AllNodes[7].overrideExactRoom.placedObjects[6].nonenemyBehaviour.gameObject;
						SetupExoticObjects.LargeDesk = dungeon6.PatternSettings.flows[0].AllNodes[8].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.TechnoFloorCellEmpty = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.TechnoFloorCellCaterpillar = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[4].nonenemyBehaviour.gameObject;
						SetupExoticObjects.TechnoFloorCellLeever = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[13].nonenemyBehaviour.gameObject;
						SetupExoticObjects.TechnoFloorCellSpider = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[14].nonenemyBehaviour.gameObject;
						SetupExoticObjects.WideComputerBreakable = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[6].nonenemyBehaviour.gameObject;
						SetupExoticObjects.MetalCrate = dungeon6.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[10].nonenemyBehaviour.gameObject;
						SetupExoticObjects.HologramWallHorizontal = dungeon6.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
						SetupExoticObjects.HologramWallVertical = dungeon6.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[7].nonenemyBehaviour.gameObject;
						SetupExoticObjects.VentilationTube = dungeon6.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[8].nonenemyBehaviour.gameObject;
						SetupExoticObjects.TallComputerBreakable = dungeon6.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[13].nonenemyBehaviour.gameObject;
						SetupExoticObjects.AgunimBossMatt = dungeon6.PatternSettings.flows[0].AllNodes[12].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject;
						SetupExoticObjects.AlienTank = dungeon6.PatternSettings.flows[0].AllNodes[13].overrideExactRoom.placedObjects[9].nonenemyBehaviour.gameObject;
						SetupExoticObjects.DecorativeElectricFloor = dungeon6.PatternSettings.flows[0].AllNodes[13].overrideExactRoom.placedObjects[29].nonenemyBehaviour.gameObject;
					}
					else
					{
						ETGModConsole.Log("<color=#ff0000ff>ERROR: R&G DEPARTMENT FLOW 0 HAS AN INCORRECT AMOUNT OF NODES</color>", false);
					}
				}
				else
				{
					ETGModConsole.Log("<color=#ff0000ff>ERROR: R&G DEPARTMENT FLOW 0 HAS AN INCORRECT NAME, AND HAS BEEN ALTERED</color>", false);
				}
			}
			else
			{
				ETGModConsole.Log("<color=#ff0000ff>ERROR: R&G DEPARTMENT DUNGEON PREFAB WAS NULL</color>", false);
			}
			if (dungeon7)
			{
				if (dungeon7.PatternSettings.flows[0].name == "F5_Forge_Flow_00_CriticalBlacksmith")
				{
					if (dungeon7.PatternSettings.flows[0].AllNodes[1].overrideExactRoom && dungeon7.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.name == "Boss Foyer (final)")
					{
						SetupExoticObjects.WallGearWest = dungeon7.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[16].nonenemyBehaviour.gameObject;
						SetupExoticObjects.WallGearEast = dungeon7.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[18].nonenemyBehaviour.gameObject;
						SetupExoticObjects.WallGearNorth = dungeon7.PatternSettings.flows[0].AllNodes[1].overrideExactRoom.placedObjects[20].nonenemyBehaviour.gameObject;
					}
					else
					{
						ETGModConsole.Log("<color=#ff0000ff>ERROR: FORGE FLOW 0 NODE 1 IS NOT THE DRAGUN FOYER</color>", false);
					}
					if (dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom && dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.name == "Blacksmith_TestRoom")
					{
						SetupExoticObjects.NPCBlacksmith = dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[8].nonenemyBehaviour.gameObject;
						SetupExoticObjects.BlacksmithLounger = dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[9].nonenemyBehaviour.gameObject;
						SetupExoticObjects.BlacksmithWorkbench = dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[10].nonenemyBehaviour.gameObject;
						SetupExoticObjects.MoltenMetalWallCrucible = dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[11].nonenemyBehaviour.gameObject;
						SetupExoticObjects.FloatingMagicOrb = dungeon7.PatternSettings.flows[0].AllNodes[10].overrideExactRoom.placedObjects[12].nonenemyBehaviour.gameObject;
					}
					else
					{
						ETGModConsole.Log("<color=#ff0000ff>ERROR: FORGE FLOW 0 NODE 10 IS NOT THE BLACKSMITHS SHOP</color>", false);
					}
				}
				else
				{
					ETGModConsole.Log("<color=#ff0000ff>ERROR: FORGE FLOW 0 HAS AN INCORRECT NAME, AND HAS BEEN ALTERED</color>", false);
				}
			}
			else
			{
				ETGModConsole.Log("<color=#ff0000ff>ERROR: FORGE DUNGEON PREFAB WAS NULL</color>", false);
			}
			SetupExoticObjects.NonRatStealableArmor = FakePrefab.Clone(PickupObjectDatabase.GetById(120).gameObject);
			SetupExoticObjects.NonRatStealableArmor.GetComponent<PickupObject>().IgnoredByRat = true;
			SetupExoticObjects.NonRatStealableAmmo = FakePrefab.Clone(PickupObjectDatabase.GetById(78).gameObject);
			SetupExoticObjects.NonRatStealableAmmo.GetComponent<PickupObject>().IgnoredByRat = true;
			SetupExoticObjects.NonRatStealableSpreadAmmo = FakePrefab.Clone(PickupObjectDatabase.GetById(600).gameObject);
			SetupExoticObjects.NonRatStealableSpreadAmmo.GetComponent<PickupObject>().IgnoredByRat = true;
			SetupExoticObjects.MapPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(137).gameObject);
			SetupExoticObjects.MapPlaceable.GetComponent<PickupObject>().IgnoredByRat = true;
			GameObjectExtensions.GetOrAddComponent<SquishyBounceWiggler>(SetupExoticObjects.MapPlaceable);
			SetupExoticObjects.GlassGuonPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(565).gameObject);
			SetupExoticObjects.GlassGuonPlaceable.GetComponent<PickupObject>().IgnoredByRat = true;
			GameObjectExtensions.GetOrAddComponent<SquishyBounceWiggler>(SetupExoticObjects.GlassGuonPlaceable);
			SetupExoticObjects.FiftyCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(74).gameObject);
			if (SetupExoticObjects.FiftyCasingPlaceable.GetComponent<PickupMover>())
			{
				UnityEngine.Object.Destroy(SetupExoticObjects.FiftyCasingPlaceable.GetComponent<PickupMover>());
			}
			SetupExoticObjects.SingleCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(68).gameObject);
			if (SetupExoticObjects.SingleCasingPlaceable.GetComponent<PickupMover>())
			{
				UnityEngine.Object.Destroy(SetupExoticObjects.SingleCasingPlaceable.GetComponent<PickupMover>());
			}
			SetupExoticObjects.FiveCasingPlaceable = FakePrefab.Clone(PickupObjectDatabase.GetById(70).gameObject);
			if (SetupExoticObjects.FiveCasingPlaceable.GetComponent<PickupMover>())
			{
				UnityEngine.Object.Destroy(SetupExoticObjects.FiveCasingPlaceable.GetComponent<PickupMover>());
			}

			SetupExoticObjects.SawBlade = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[0].nonDatabasePlaceable;
			SetupExoticObjects.Minecart = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[6].nonDatabasePlaceable;
            SetupExoticObjects.TurretMinecart = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[11].nonDatabasePlaceable;

			SetupExoticObjects.KeepSconce = LoadHelper.LoadAssetFromAnywhere<GameObject>("sconce_light");
            SetupExoticObjects.KeepSconceSideLeft = LoadHelper.LoadAssetFromAnywhere<GameObject>("sconce_light_side");
			SetupExoticObjects.KeepSconceSideRight = FakePrefabExtensions.InstantiateAndFakeprefab(LoadHelper.LoadAssetFromAnywhere<GameObject>("sconce_light_side")).gameObject;
			SetupExoticObjects.KeepSconceSideRight.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.KeepSconceSideRight.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            

            SetupExoticObjects.BasicTorch = LoadHelper.LoadAssetFromAnywhere<GameObject>("defaulttorch");
            SetupExoticObjects.BasicTorchSideLeft = LoadHelper.LoadAssetFromAnywhere<GameObject>("defaulttorchside");
            SetupExoticObjects.BasicTorchSideRight = FakePrefabExtensions.InstantiateAndFakeprefab(LoadHelper.LoadAssetFromAnywhere<GameObject>("defaulttorchside")).gameObject;
            SetupExoticObjects.BasicTorchSideRight.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.BasicTorchSideRight.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);


            SetupExoticObjects.CandleabraHorizontal = dungeon2.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;
            SetupExoticObjects.CandleabraLeft = dungeon2.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.CandleabraRight = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon2.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference).gameObject;
            SetupExoticObjects.CandleabraRight.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.CandleabraRight.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            SetupExoticObjects.MineLanternHorizontal = dungeon3.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;
            SetupExoticObjects.MineLanternLeft = dungeon3.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.MineLanternRight = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon3.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference).gameObject;
            SetupExoticObjects.MineLanternRight.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.MineLanternRight.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            SetupExoticObjects.BurningSkeleton_Horizontal = dungeon8.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;
            SetupExoticObjects.BurningSkeleton_Left = dungeon8.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.BurningSkeleton_Right = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon8.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference).gameObject;
            SetupExoticObjects.BurningSkeleton_Right.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.BurningSkeleton_Right.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            SetupExoticObjects.LavaLight_Horizontal = dungeon7.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;
            SetupExoticObjects.LavaLight_Left = dungeon7.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.LavaLight_Right = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon7.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference).gameObject;
            SetupExoticObjects.LavaLight_Right.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.LavaLight_Right.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            SetupExoticObjects.OfficeLight_Horizontal = dungeon6.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;
            SetupExoticObjects.OfficeLight_Left = dungeon6.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.OfficeLight_Right = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon6.roomMaterialDefinitions[0].sidewallLightStamps[0].objectReference.gameObject);
            SetupExoticObjects.OfficeLight_Right.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.OfficeLight_Right.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            SetupExoticObjects.WeirdGreenLight_Horizontal = dungeon6.roomMaterialDefinitions[7].facewallLightStamps[0].objectReference;
            SetupExoticObjects.WeirdGreenLight_Left = dungeon6.roomMaterialDefinitions[7].sidewallLightStamps[0].objectReference;
            SetupExoticObjects.WeirdGreenLight_Right = FakePrefabExtensions.InstantiateAndFakeprefab(dungeon6.roomMaterialDefinitions[7].sidewallLightStamps[0].objectReference);
            SetupExoticObjects.WeirdGreenLight_Right.GetComponent<tk2dBaseSprite>().FlipX = true;
            SetupExoticObjects.WeirdGreenLight_Right.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);

            //abbeyDungeon.roomMaterialDefinitions[0].facewallLightStamps[0].objectReference;



            SetupExoticObjects.LostAdventurer = LoadHelper.LoadAssetFromAnywhere<GameObject>("npc_lostadventurer");
            SetupExoticObjects.BlobulordGrate = LoadHelper.LoadAssetFromAnywhere<GameObject>("blobulordgrate");
            SetupExoticObjects.TreadnoughtPillar = LoadHelper.LoadAssetFromAnywhere<GameObject>("breakablecolumn");
            SetupExoticObjects.TreadnoughtPillarDestroyed = LoadHelper.LoadAssetFromAnywhere<GameObject>("brokencolumn");
			SetupExoticObjects.MedievalCrate = LoadHelper.LoadAssetFromAnywhere<GameObject>("medieval_crate_001");
            SetupExoticObjects.GoldArmor = LoadHelper.LoadAssetFromAnywhere<GameObject>("suit_of_armor_002");
			SetupExoticObjects.TanPot = dungeon2.stampData.objectStamps[14].objectReference;
            StaticReferences.customPlaceables.Add("bossPedestal", new DungeonPlaceable()
            {
                height = 2,
                width = 2,
                variantTiers = new List<DungeonPlaceableVariant>()
                {
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("boss_reward_pedestal"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                },
            });
            StaticReferences.customPlaceables.Add("bossPedestalGolden", new DungeonPlaceable()
            {
                height = 2,
                width = 2,
                variantTiers = new List<DungeonPlaceableVariant>()
                {
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = dungeon4.PatternSettings.flows[0].AllNodes[12].overrideExactRoom.additionalObjectLayers[0].placedObjects[11].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab(),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                },
            });

            var lightObject = FakePrefabExtensions.InstantiateAndFakeprefab(new GameObject("RAT Light Object"));
			var childObject = new GameObject("ChildLight");
            childObject.GetOrAddComponent<AdditionalBraveLight>();
			childObject.transform.parent = lightObject.transform;
			childObject.transform.localPosition = new Vector3(0.5f, 0.5f, 1);
			SetupExoticObjects.GungeonLightSource = lightObject;
            PrototypeDungeonRoom asset = null;
            foreach (var bundle in StaticReferences.AssetBundles.Values)
            {
                asset = bundle.LoadAsset<PrototypeDungeonRoom>("ChallengeShrine_Gungeon_002");
                if (asset)
                    break;
            }
            GameObject ChallengeShrine = FakePrefab.Clone(asset.placedObjects[0].nonenemyBehaviour.gameObject);
            if (ChallengeShrine.GetComponentInChildren<ChallengeShrineController>() == null) { ETGModConsole.Log("HOW THE *FUCK* IS THIS NULL??"); }
            FoolproofedChallengeShrineController newController = ChallengeShrine.AddComponent<FoolproofedChallengeShrineController>();
            ChallengeShrineController baseController = ChallengeShrine.GetComponentInChildren<ChallengeShrineController>();
            newController.acceptOptionKey = baseController.acceptOptionKey;
            newController.AlternativeOutlineTarget = baseController.AlternativeOutlineTarget;
            newController.CustomChestTable = baseController.CustomChestTable;
            newController.declineOptionKey = baseController.declineOptionKey;
            newController.difficulty = baseController.difficulty;
            newController.displayTextKey = baseController.displayTextKey;
            newController.isPassable = baseController.isPassable;
            newController.onPlayerVFX = baseController.onPlayerVFX;
            newController.placeableHeight = baseController.placeableHeight;
            newController.placeableWidth = baseController.placeableWidth;
            newController.playerVFXOffset = baseController.playerVFXOffset;
            newController.talkPoint = baseController.talkPoint;
            newController.usesCustomChestTable = baseController.usesCustomChestTable;
            if (ChallengeShrine.GetComponentInChildren<ChallengeShrineController>()) UnityEngine.Object.Destroy(ChallengeShrine.GetComponentInChildren<ChallengeShrineController>());
            StaticReferences.StoredRoomObjects.Add("ChallengeShrine", ChallengeShrine);
            StaticReferences.customObjects.Add("ChallengeShrine", ChallengeShrine);
            StaticReferences.customPlaceables.Add("FlowerTile", new DungeonPlaceable()
            {
                height = 1,
                width = 1,
                variantTiers = new List<DungeonPlaceableVariant>()
                {
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("flower_floor_001"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("flower_floor_002"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("flower_floor_003"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("flower_floor_004"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("flower_floor_005"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                },
            });
            StaticReferences.customPlaceables.Add("GorgunStatueBulletKin", new DungeonPlaceable()
            {
                height = 1,
                width = 1,
                variantTiers = new List<DungeonPlaceableVariant>()
                {
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_bullet_001"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_bullet_002"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_bullet_003"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                },
            });
            StaticReferences.customPlaceables.Add("GorgunStatueShotgunKin", new DungeonPlaceable()
            {
                height = 1,
                width = 1,
                variantTiers = new List<DungeonPlaceableVariant>()
                {
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_shotgun_001"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_shotgun_002"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_shotgun_003"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                   new DungeonPlaceableVariant()
                   {
                       nonDatabasePlaceable = LoadHelper.LoadAssetFromAnywhere<GameObject>("gungeon_stone_shotgun_004"),
                       prerequisites = new DungeonPrerequisite[0],
                   },
                },
            });
			SetupExoticObjects.PitfallTrap = LoadHelper.LoadAssetFromAnywhere<GameObject>("trap_pit_gungeon_trigger_2x2");
            SetupExoticObjects.Spike_trap = LoadHelper.LoadAssetFromAnywhere<GameObject>("trap_spike_gungeon_2x2");
            SetupExoticObjects.Flame_Trap = LoadHelper.LoadAssetFromAnywhere<GameObject>("trap_flame_poofy_gungeon_1x1");
            SetupExoticObjects.HighPriestBossRoomFloor = LoadHelper.LoadAssetFromAnywhere<GameObject>("highpriestplaceable");
            SetupExoticObjects.LichBossRoomFloor = LoadHelper.LoadAssetFromAnywhere<GameObject>("lichroomplaceable");
			SetupExoticObjects.GunslingKing_NPC = LoadHelper.LoadAssetFromAnywhere<GameObject>("npc_gunslingking");
			SetupExoticObjects.Manservantes_NPC = LoadHelper.LoadAssetFromAnywhere<GameObject>("npc_manservantes");

            SetupExoticObjects.Winchester_NPC = LoadHelper.LoadAssetFromAnywhere<GameObject>("npc_artful_dodger");
            SetupExoticObjects.Winchester_BlueBlock = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue");
            SetupExoticObjects.Winchester_RedBlock = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_red");
            SetupExoticObjects.Winchester_GreenBlock = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_green");
            SetupExoticObjects.Winchester_BlueBlock_Corner_BL = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_corner_topright");
            SetupExoticObjects.Winchester_BlueBlock_Corner_BR = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_corner_topleft");
            SetupExoticObjects.Winchester_BlueBlock_Corner_TL = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_corner_bottomright");
            SetupExoticObjects.Winchester_BlueBlock_Corner_TR = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_corner_bottomleft");
            SetupExoticObjects.Winchester_BlueBlock_Moving_1x3 = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_1x3");
            SetupExoticObjects.Winchester_BlueBlock_Moving_2x2 = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_bumper_blue_2x2");
            SetupExoticObjects.Winchester_NeonSign = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_neonsign");
            SetupExoticObjects.Winchester_MovingTarget = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_target");
            SetupExoticObjects.Winchester_CameraPoint = LoadHelper.LoadAssetFromAnywhere<GameObject>("artfuldodger_camerapoint");
			SetupExoticObjects.Winchester_CameraPanController = FakePrefabExtensions.InstantiateAndFakeprefab(new GameObject("Decoy CamerapanObjects"));
			SetupExoticObjects.Winchester_CameraPanController.AddComponent<WinchesterCameraHelper>();
            var winchesterRoomObject = FakePrefabExtensions.InstantiateAndFakeprefab(new GameObject("Decoy WinchesterRoomController"));
			var art = winchesterRoomObject.AddComponent<ArtfulDodgerRoomController>();
			art.NumberShots = 4;
			art.NumberBounces = 1;
            SetupExoticObjects.Winchester_RoomController = winchesterRoomObject;

            SetupExoticObjects.Skeleton_Standing_Left = dungeon5.stampData.objectStamps[1].objectReference;
            SetupExoticObjects.Skeleton_Standing_Right = dungeon5.stampData.objectStamps[0].objectReference;
            SetupExoticObjects.Skeleton_Hanging_Left = dungeon5.stampData.objectStamps[6].objectReference;
            SetupExoticObjects.Skeleton_Hanging_Right = dungeon5.stampData.objectStamps[7].objectReference;
			var dragunSkull =EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").GetComponent<DraGunDeathController>().skullDebris.InstantiateAndFakeprefab();
			UnityEngine.Object.Destroy(dragunSkull.GetComponent<DebrisObject>());
			dragunSkull.GetComponent<MajorBreakable>().SpawnItemOnBreak = false;
			SetupExoticObjects.Dragun_Skull = dragunSkull;
            var dragunvertabra = EnemyDatabase.GetOrLoadByGuid("465da2bb086a4a88a803f79fe3a27677").GetComponent<DraGunDeathController>().neckDebris.InstantiateAndFakeprefab();
			UnityEngine.Object.Destroy(dragunvertabra.GetComponent<DebrisObject>());
			SetupExoticObjects.Dragun_Vertebra = dragunvertabra;

			SetupExoticObjects.Conveyor_Horizontal = LoadHelper.LoadAssetFromAnywhere<GameObject>("conveyor_horizontal");
            SetupExoticObjects.Conveyor_Vertical = LoadHelper.LoadAssetFromAnywhere<GameObject>("conveyor_vertical");

            GameObject skeletonNote = LoadHelper.LoadAssetFromAnywhere<GameObject>("assets/data/prefabs/interactable objects/notes/skeleton_note_001.prefab");

            SetupExoticObjects.Floor_Skeleton = skeletonNote.transform.Find("skleleton").gameObject.InstantiateAndFakeprefab();

			SetupExoticObjects.Floor_Skeleton_Note = LoadHelper.LoadAssetFromAnywhere<GameObject>("assets/data/prefabs/interactable objects/notes/skeleton_note_001.prefab").InstantiateAndFakeprefab();
			var f = SetupExoticObjects.Floor_Skeleton_Note.transform.Find("skleleton").gameObject;
			f.transform.parent = null;
            UnityEngine.Object.Destroy(f);
			SetupExoticObjects.Floor_Skeleton_Note.gameObject.AddComponent<Repositioner>().reposition = new Vector3(-1, 0);


			SetupExoticObjects.Switch_Chandelier = LoadHelper.LoadAssetFromAnywhere<GameObject>("hanging_chain_attach_wallswitch");
			SetupExoticObjects.Chandelier_Drop = LoadHelper.LoadAssetFromAnywhere<GameObject>("Hanging_Chandelier");

			SetupExoticObjects.Mine_CaveIn = LoadHelper.LoadAssetFromAnywhere<GameObject>("mines_cave_in");
            SetupExoticObjects.Mine_CaveIn_Switch = LoadHelper.LoadAssetFromAnywhere<GameObject>("mines_plunger");

			SetupExoticObjects.BulletPast_PitfallTrap = dungeon9.PatternSettings.flows[0].AllNodes[3].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject;

            SetupExoticObjects.MouseTrap_North = EnemyDatabase.GetOrLoadByGuid("6868795625bd46f3ae3e4377adce288b").GetComponent<ResourcefulRatController>().MouseTraps[0];
            SetupExoticObjects.MouseTrap_East = EnemyDatabase.GetOrLoadByGuid("6868795625bd46f3ae3e4377adce288b").GetComponent<ResourcefulRatController>().MouseTraps[1];
            SetupExoticObjects.MouseTrap_West = EnemyDatabase.GetOrLoadByGuid("6868795625bd46f3ae3e4377adce288b").GetComponent<ResourcefulRatController>().MouseTraps[2];


            SetupExoticObjects.Spinning_Log_Horizontal = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[5].nonDatabasePlaceable;
            SetupExoticObjects.Spinning_Log_Vertical = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[4].nonDatabasePlaceable;

            SetupExoticObjects.Spinning_Ice_Log_Horizontal = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[13].nonDatabasePlaceable;
            SetupExoticObjects.Spinning_Ice_Log_Vertical = ((DungeonPlaceable)BraveResources.Load("RobotDaveTraps", ".asset")).variantTiers[12].nonDatabasePlaceable;

            // Graves
            SetupExoticObjects.TombStone_N = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_top");
            SetupExoticObjects.TombStone_S = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_bot");
            SetupExoticObjects.TombStone_E = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_left");
            SetupExoticObjects.TombStone_W = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_right");
            SetupExoticObjects.TombStone_SE = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_bot_right");
            SetupExoticObjects.TombStone_SW = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_bot_left");
            SetupExoticObjects.TombStone_NW = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_top_right");
            SetupExoticObjects.TombStone_NE = LoadHelper.LoadAssetFromAnywhere<GameObject>("tombstone_top_left");

			SetupExoticObjects.Sewer_Entrance = orLoadByName.PatternSettings.flows[0].sharedInjectionData[1].InjectionData[0].exactRoom.placedObjects[0].nonenemyBehaviour.gameObject;
            SetupExoticObjects.Sewer_Entrace_Angry_Button = orLoadByName.PatternSettings.flows[0].sharedInjectionData[1].InjectionData[0].exactRoom.placedObjects[1].nonenemyBehaviour.gameObject;
            SetupExoticObjects.Keep_Fireplace = LoadHelper.LoadAssetFromAnywhere<GameObject>("fireplace");

			SetupExoticObjects.Moving_Platform_Proper = LoadHelper.LoadAssetFromAnywhere<GameObject>("default_platform_3x3");

            SetupExoticObjects.Bullet_King_Arena_Floor = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingdiasandwall");
            SetupExoticObjects.Bullet_King_Arena_MainWall = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingwall_layer_003").InstantiateAndFakeprefab();
            SetupExoticObjects.Bullet_King_Arena_MainWall.AddComponent<Repositioner>().reposition = new Vector3(-7, -5);
            SetupExoticObjects.Bullet_King_Arena_SideWall_1 = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingwall_layer_001");
            SetupExoticObjects.Bullet_King_Arena_SideWall_2 = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingwall_layer_002").InstantiateAndFakeprefab();
            SetupExoticObjects.Bullet_King_Arena_SideWall_2.AddComponent<Repositioner>().reposition = new Vector3(-4, -4);

            SetupExoticObjects.Wallmonger_Arena_Floor = LoadHelper.LoadAssetFromAnywhere<GameObject>("demonwallroomplaceable");

            SetupExoticObjects.Old_King_Arena_Floor = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingolddiasandwall");
            SetupExoticObjects.Old_King_Arena_MainWall = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingoldwall_layer_003").InstantiateAndFakeprefab();
            SetupExoticObjects.Old_King_Arena_MainWall.AddComponent<Repositioner>().reposition = new Vector3(-7, -5);

            SetupExoticObjects.Old_King_Arena_SideWall_1 = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingoldwall_layer_001");
            SetupExoticObjects.Old_King_Arena_SideWall_2 = LoadHelper.LoadAssetFromAnywhere<GameObject>("bulletkingoldwall_layer_002").InstantiateAndFakeprefab();
			SetupExoticObjects.Old_King_Arena_SideWall_2.AddComponent<Repositioner>().reposition = new Vector3(-4, -4);


			SetupExoticObjects.GatlingGull_Valid_Leap_Position = new GameObject("Gull_Leap_Point").InstantiateAndFakeprefab();
			SetupExoticObjects.GatlingGull_Valid_Leap_Position.AddComponent<GatlingGullLeapPoint>().ForReposition = true;


            SetupExoticObjects.Glitched_Boss_Modifier = new GameObject("Glitch_Boss_Modifier").InstantiateAndFakeprefab();
			SetupExoticObjects.Glitched_Boss_Modifier.AddComponent<Glitched_Boss_Modifier>();
            
			SetupExoticObjects.AttackLeapPoint = new GameObject("Attack Leap Point Dummy").InstantiateAndFakeprefab();
            SetupExoticObjects.AttackLeapPoint.AddComponent<AttackLeapPoint>();

			SetupExoticObjects.DemonFace = LoadHelper.LoadAssetFromAnywhere<GameObject>("shrine_demonface");

            SetupExoticObjects.Wood_Sign_N = tutorialDungeon.PatternSettings.flows[0].AllNodes[9].overrideExactRoom.placedObjects[1].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Wood_Sign_E = tutorialDungeon.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[14].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Wood_Sign_W = tutorialDungeon.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[13].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Wood_Sign_S = tutorialDungeon.PatternSettings.flows[0].AllNodes[11].overrideExactRoom.placedObjects[13].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Wood_Sign_S.GetComponent<tk2dSprite>().spriteId = 132;

            var dummyObject = new GameObject("Dummy");
            dummyObject.AddComponent<tk2dSprite>();
			var p = dummyObject.InstantiateAndFakeprefab();
            var sprite = p.GetComponent<tk2dSprite>();
            sprite.SetSprite(SetupExoticObjects.Wood_Sign_N.GetComponent<tk2dSprite>().sprite.collection, 1);

            SetupExoticObjects.Red_Torn_Carpet = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Red_Torn_Carpet.name = "Red_Torn_Carpet";
            SetupExoticObjects.Red_Torn_Carpet.GetComponent<tk2dSprite>().SetSprite(101);
            SetupExoticObjects.Red_Torn_Carpet.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Stair_Case = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Stair_Case.name = "Stair_Case";
            SetupExoticObjects.Stair_Case.GetComponent<tk2dSprite>().SetSprite(103);
            SetupExoticObjects.Stair_Case.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Start_Room_Decor = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Start_Room_Decor.name = "StartRoom_Decor";
            SetupExoticObjects.Start_Room_Decor.GetComponent<tk2dSprite>().SetSprite(105);
            SetupExoticObjects.Start_Room_Decor.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Start_Room_Floor = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Start_Room_Floor.name = "StartRoom_Floor";
            SetupExoticObjects.Start_Room_Floor.GetComponent<tk2dSprite>().SetSprite(151);
            SetupExoticObjects.Start_Room_Floor.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Special_Dais = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Special_Dais.name = "Special_Dais";
            SetupExoticObjects.Special_Dais.GetComponent<tk2dSprite>().spriteId = 41;
            SetupExoticObjects.Special_Dais.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Hanging_Concrete_Block = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Hanging_Concrete_Block.name = "Hanging_Concrete_Block";
            SetupExoticObjects.Hanging_Concrete_Block.GetComponent<tk2dSprite>().SetSprite(79);
            SetupExoticObjects.Hanging_Concrete_Block.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));

            SetupExoticObjects.Gungeon_Grate = p.InstantiateAndFakeprefab();
            SetupExoticObjects.Gungeon_Grate.name = "Gungeon_Grate";
            SetupExoticObjects.Gungeon_Grate.GetComponent<tk2dSprite>().SetSprite(9);
            SetupExoticObjects.Gungeon_Grate.SetLayerRecursively(LayerMask.NameToLayer("BG_Nonsense"));



            PrototypeDungeonRoom roomPrefab = LoadHelper.LoadAssetFromAnywhere<PrototypeDungeonRoom>("shop02");
            SetupExoticObjects.Shop_TeleporterSign = roomPrefab.placedObjects[10].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();
            
			SetupExoticObjects.ShopLayout = roomPrefab.placedObjects[12].nonenemyBehaviour.gameObject.InstantiateAndFakeprefab();

            SetupExoticObjects.Shop_Crates = ShopLayout.transform.GetChild(1).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Crate = ShopLayout.transform.GetChild(5).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Sack = ShopLayout.transform.GetChild(3).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_ShelfBarrel = ShopLayout.transform.GetChild(10).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Shelf = ShopLayout.transform.GetChild(7).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Mask = ShopLayout.transform.GetChild(6).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Wallsword = ShopLayout.transform.GetChild(11).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_StandingShelf = ShopLayout.transform.GetChild(8).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_AKBarrel = ShopLayout.transform.GetChild(9).gameObject.InstantiateAndFakeprefab();
            SetupExoticObjects.Shop_Stool = ShopLayout.transform.GetChild(12).gameObject.InstantiateAndFakeprefab();

            roomPrefab = null;


            SetupExoticObjects.ShopItemObject = new GameObject("ShopItemObjectDummy").InstantiateAndFakeprefab();
			SetupExoticObjects.ShopItemObject.AddComponent<ShopItemPosition>();

            SetupExoticObjects.Ser_Manuels_Body = tutorialDungeon.PatternSettings.flows[0].AllNodes[15].overrideExactRoom.placedObjects[0].nonenemyBehaviour.gameObject.transform.GetChild(2).gameObject.InstantiateAndFakeprefab();


            SetupExoticObjects.ShopRoundTable = LoadHelper.LoadAssetFromAnywhere<GameObject>("shoptable");
            SetupExoticObjects.ShopRoundTable_Empty = LoadHelper.LoadAssetFromAnywhere<GameObject>("shoptable").InstantiateAndFakeprefab();
			UnityEngine.Object.Destroy(SetupExoticObjects.ShopRoundTable_Empty.GetComponent<ShopSubsidiaryZone>());
			
			SetupExoticObjects.Glass_Case = LoadHelper.LoadAssetFromAnywhere<GameObject>("shop_specialcase").InstantiateAndFakeprefab();

            SetupExoticObjects.Glass_Case_Custom = LoadHelper.LoadAssetFromAnywhere<GameObject>("shop_specialcase").InstantiateAndFakeprefab();
            UnityEngine.Object.Destroy(SetupExoticObjects.Glass_Case_Custom.GetComponent<ShopSubsidiaryZone>());
            SetupExoticObjects.Glass_Case_Custom.AddComponent<ShopItemPosition>().Offset = new Vector3(1, 2.125f, 10);


            SetupExoticObjects.Glass_Case_Empty = LoadHelper.LoadAssetFromAnywhere<GameObject>("shop_specialcase").InstantiateAndFakeprefab();
            UnityEngine.Object.Destroy(SetupExoticObjects.Glass_Case_Empty.GetComponent<ShopSubsidiaryZone>());

			SetupExoticObjects.No_Pickup = new GameObject("No_Pickup").InstantiateAndFakeprefab();
			SetupExoticObjects.No_Pickup.AddComponent<NoPickup>();
            

            SetupExoticObjects.objects = new Dictionary<string, GameObject>
			{
                {
                    "No_Pickup_Object",
                    SetupExoticObjects.No_Pickup
                },
                {
                    "Glass_Case",
                    SetupExoticObjects.Glass_Case
                },
                {
                    "Glass_Case_Custom",
                    SetupExoticObjects.Glass_Case_Custom
                },
                {
                    "Glass_Case_Empty",
                    SetupExoticObjects.Glass_Case_Empty
                },

                {
                    "round_table",
                    SetupExoticObjects.ShopRoundTable
                },
                {
                    "round_table_empty",
                    SetupExoticObjects.ShopRoundTable_Empty
                },

                {
                    "Ser_Manuels_Body",
                    SetupExoticObjects.Ser_Manuels_Body
                },
                {
                    "ShopItemObject",
                    SetupExoticObjects.ShopItemObject
                },
                {
                    "shopLayout",
                    SetupExoticObjects.ShopLayout
                },
                {
                    "Shop_Crates",
                    SetupExoticObjects.Shop_Crates
                },
                {
                    "Shop_Crate",
                    SetupExoticObjects.Shop_Crate
                },
                {
                    "Shop_Sack",
                    SetupExoticObjects.Shop_Sack
                },
                {
                    "Shop_ShelfBarrel",
                    SetupExoticObjects.Shop_ShelfBarrel
                },
                {
                    "Shop_Shelf",
                    SetupExoticObjects.Shop_Shelf
                },

                {
                    "Shop_Mask",
                    SetupExoticObjects.Shop_Mask
                },
                {
                    "Shop_Wallsword",
                    SetupExoticObjects.Shop_Wallsword
                },
                {
                    "Shop_StandingShelf",
                    SetupExoticObjects.Shop_StandingShelf
                },
                {
                    "Shop_AKBarrel",
                    SetupExoticObjects.Shop_AKBarrel
                },
                {
                    "Shop_Stool",
                    SetupExoticObjects.Shop_Stool
                },

                {
                    "ShopSign",
                    SetupExoticObjects.Shop_TeleporterSign
                },
                {
                    "Special_Dais",
                    SetupExoticObjects.Special_Dais
                },
                {
                    "RedTornCarpet",
                    SetupExoticObjects.Red_Torn_Carpet
                },
                {
                    "StairCase",
                    SetupExoticObjects.Stair_Case
                },
                {
                    "GungeonGrate",
                    SetupExoticObjects.Gungeon_Grate
                },
                {
                    "HangingConcreteBlock",
                    SetupExoticObjects.Hanging_Concrete_Block
                },

                {
                    "Start_Room_Decor",
                    SetupExoticObjects.Start_Room_Decor
                },
                {
                    "Start_Room_Floor",
                    SetupExoticObjects.Start_Room_Floor
                },

                {
                    "WoodSignArrow_N",
                    SetupExoticObjects.Wood_Sign_N
                },
                {
                    "WoodSignArrow_E",
                    SetupExoticObjects.Wood_Sign_E
                },
                {
                    "WoodSignArrow_W",
                    SetupExoticObjects.Wood_Sign_W
                },
                {
                    "WoodSignArrow_S",
                    SetupExoticObjects.Wood_Sign_S
                },
                {
                    "demonFace",
                    SetupExoticObjects.DemonFace
                },
                {
                    "land_point",
                    SetupExoticObjects.AttackLeapPoint
                },
                {
                    "bossPedestalGolden",
                    SetupExoticObjects.Golden_Boss_Pedestal
                },
                {
                    "glitch_floor_properties",
                    SetupExoticObjects.Glitched_Boss_Modifier
                },
                {
                    "gullLeapPoint",
                    SetupExoticObjects.GatlingGull_Valid_Leap_Position
                },

                {
                    "minecartFactory",
                    SetupExoticObjects.MinecarftFactory_Object
                },

                {
                    "forge_face_shootssouth",
                    SetupExoticObjects.ForgeGunTrap_North
                },
                {
                    "forge_face_shootswest",
                    SetupExoticObjects.ForgeGunTrap_East
                },
                {
                    "forge_face_shootseast",
                    SetupExoticObjects.ForgeGunTrap_West
                },
                {
                    "mines_face_shootssouth",
                    SetupExoticObjects.MinesGunTrap_North
                },
                {
                    "mines_face_shootswest",
                    SetupExoticObjects.MinesGunTrap_East
                },
                {
                    "mines_face_shootseast",
                    SetupExoticObjects.MinesGunTrap_West
                },

                {
                    "hollow_face_shootssouth",
                    SetupExoticObjects.HollowGunTrap_North
                },
                {
                    "hollow_face_shootswest",
                    SetupExoticObjects.HollowGunTrap_East
                },
                {
                    "hollow_face_shootseast",
                    SetupExoticObjects.HollowGunTrap_West
                },


                {
                    "OmniMovingPlatform_pathing",
                    SetupExoticObjects.Moving_Platform_Proper
                },
                {
                    "OmniMovingPlatformMines_pathing",
                    SetupExoticObjects.Moving_Platform_Mines
                },
                {
                    "OmniMovingPlatformSewer_pathing",
                    SetupExoticObjects.Moving_Platform_Sewer
                },
                {
                    "OmniMovingPlatformHollow_pathing",
                    SetupExoticObjects.Moving_Platform_Hollow
                },
                {
                    "OmniMovingPlatformForge_pathing",
                    SetupExoticObjects.Moving_Platform_Forge
                },
                {
                    "Old_King_Arena_Floor",
                    SetupExoticObjects.Old_King_Arena_Floor
                },
                {
                    "Old_King_Arena_MainWall",
                    SetupExoticObjects.Old_King_Arena_MainWall
                },
                {
                    "Old_King_Arena_SideWall_1",
                    SetupExoticObjects.Old_King_Arena_SideWall_1
                },
                {
                    "Old_King_Arena_SideWall_2",
                    SetupExoticObjects.Old_King_Arena_SideWall_2
                },


                {
                    "Wallmonger_Arena_Floor",
                    SetupExoticObjects.Wallmonger_Arena_Floor
                },

                {
                    "Bullet_King_Arena_Floor",
                    SetupExoticObjects.Bullet_King_Arena_Floor
                },
                {
                    "Bullet_King_Arena_MainWall",
                    SetupExoticObjects.Bullet_King_Arena_MainWall
                },
                {
                    "Bullet_King_Arena_SideWall_1",
                    SetupExoticObjects.Bullet_King_Arena_SideWall_1
                },
                {
                    "Bullet_King_Arena_SideWall_2",
                    SetupExoticObjects.Bullet_King_Arena_SideWall_2
                },

                {
                    "Keep_Fireplace",
                    SetupExoticObjects.Keep_Fireplace
                },
                {
                    "Sewer_Entrace_Angry_Button",
                    SetupExoticObjects.Sewer_Entrace_Angry_Button
                },
                {
                    "Sewer_Entrance",
                    SetupExoticObjects.Sewer_Entrance
                },

                {
                    "TombStone_N",
                    SetupExoticObjects.TombStone_N
                },
                {
                    "TombStone_NE",
                    SetupExoticObjects.TombStone_NE
                },
                {
                    "TombStone_NW",
                    SetupExoticObjects.TombStone_NW
                },
                {
                    "TombStone_E",
                    SetupExoticObjects.TombStone_E
                },
                {
                    "TombStone_W",
                    SetupExoticObjects.TombStone_W
                },
                {
                    "TombStone_SE",
                    SetupExoticObjects.TombStone_SE
                },
                {
                    "TombStone_S",
                    SetupExoticObjects.TombStone_S
                },
                {
                    "TombStone_SW",
                    SetupExoticObjects.TombStone_SW
                },


                {
                    "rollingIceLogHorizontal_pathing",
                    SetupExoticObjects.Spinning_Ice_Log_Horizontal
                },
                {
                    "rollingIceLogVertical_pathing",
                    SetupExoticObjects.Spinning_Ice_Log_Vertical
                },

                {
                    "rollingLogHorizontal_pathing",
                    SetupExoticObjects.Spinning_Log_Horizontal
                },
                {
                    "rollingLogVertical_pathing",
                    SetupExoticObjects.Spinning_Log_Vertical
                },

                {
                    "MouseTrap_North",
                    SetupExoticObjects.MouseTrap_North
                },
                {
                    "MouseTrap_East",
                    SetupExoticObjects.MouseTrap_East
                },
                {
                    "MouseTrap_West",
                    SetupExoticObjects.MouseTrap_West
                },

               

                {
                    "bulletPastPitfalltrap",
                    SetupExoticObjects.BulletPast_PitfallTrap
                },
                {
                    "caveInTrap_DropDowntrap",
                    SetupExoticObjects.Mine_CaveIn
                },
                {
                    "caveInTrap_DropDownswitch",
                    SetupExoticObjects.Mine_CaveIn_Switch
                },

                {
                    "chandelierSwitch_trigger_DropDownswitch",
                    SetupExoticObjects.Switch_Chandelier
                },
                {
                    "chandelierDropTrap_DropDowntrap",
                    SetupExoticObjects.Chandelier_Drop
                },

                {
                    "FloorSkeleton_Flop",
                    SetupExoticObjects.Floor_Skeleton
                },
                {
                    "FloorSkeleton_Note",
                    SetupExoticObjects.Floor_Skeleton_Note
                },

                {
                    "ConveyorHorizontal",
                    SetupExoticObjects.Conveyor_Horizontal
                },
                {
                    "ConveyorVertical",
                    SetupExoticObjects.Conveyor_Vertical
                },

                {
                    "weirdGreenLight_Front",
                    SetupExoticObjects.WeirdGreenLight_Horizontal
                },
                {
                    "weirdGreenLight_left",
                    SetupExoticObjects.WeirdGreenLight_Left
                },

                {
                    "weirdGreenLight_right",
                    SetupExoticObjects.WeirdGreenLight_Right
                },

                {
                    "OfficeLight_Front",
                    SetupExoticObjects.OfficeLight_Horizontal
                },
                {
                    "OfficeLight_Left",
                    SetupExoticObjects.OfficeLight_Left
                },
                {
                    "OfficeLight_Right",
                    SetupExoticObjects.OfficeLight_Right
                },

                {
                    "LavaLight_Front",
                    SetupExoticObjects.LavaLight_Horizontal
                },
                {
                    "LavaLight_Left",
                    SetupExoticObjects.LavaLight_Left
                },
                {
                    "LavaLight_Right",
                    SetupExoticObjects.LavaLight_Right
                },
                {
                    "BurningSkeletonLight_Front",
                    SetupExoticObjects.BurningSkeleton_Horizontal
                },
                {
                    "BurningSkeletonLight_Left",
                    SetupExoticObjects.BurningSkeleton_Left
                },
                {
                    "BurningSkeletonLight_Right",
                    SetupExoticObjects.BurningSkeleton_Right
                },

                {
                    "DragunSkull",
                    SetupExoticObjects.Dragun_Skull
                },
                {
                    "DraunVertabrae",
                    SetupExoticObjects.Dragun_Vertebra
                },
                {
                    "basicSkeletonStandingLeft",
                    SetupExoticObjects.Skeleton_Standing_Left
                },
                {
                    "basicSkeletonStandingRight",
                    SetupExoticObjects.Skeleton_Standing_Right
                },
                {
                    "basicSkeletonHangingLeft",
                    SetupExoticObjects.Skeleton_Hanging_Left
                },
                {
                    "basicSkeletonHangingRight",
                    SetupExoticObjects.Skeleton_Hanging_Right
                },

                {
                    "winchesterCameraPanPlacer",
                    SetupExoticObjects.Winchester_CameraPanController
                },
                {
                    "WinchesterNPC",
                    SetupExoticObjects.Winchester_NPC
                },
                {
                    "WinchesterBlueBumper",
                    SetupExoticObjects.Winchester_BlueBlock
                },
                {
                    "WinchesterGreenBumper",
                    SetupExoticObjects.Winchester_GreenBlock
                },
                {
                    "WinchesterRedBumper",
                    SetupExoticObjects.Winchester_RedBlock
                },

                {
                    "WinchesterBlueBumperDiagonal_BL",
                    SetupExoticObjects.Winchester_BlueBlock_Corner_BL
                },
                {
                    "WinchesterBlueBumperDiagonal_BR",
                    SetupExoticObjects.Winchester_BlueBlock_Corner_BR
                },
                {
                    "WinchesterBlueBumperDiagonal_TL",
                    SetupExoticObjects.Winchester_BlueBlock_Corner_TL
                },
                {
                    "WinchesterBlueBumperDiagonal_TR",
                    SetupExoticObjects.Winchester_BlueBlock_Corner_TR
                },
                {
                    "WinchesterNeonSign",
                    SetupExoticObjects.Winchester_NeonSign
                },

                {
                    "WinchesterRoomController",
                    SetupExoticObjects.Winchester_RoomController
                },
                {
                    "WinchesterCameraController",
                    SetupExoticObjects.Winchester_CameraPoint
                },
                {
                    "winchesterShootyTarget_pathing",
                    SetupExoticObjects.Winchester_MovingTarget
                },
                {
                    "WinchesterMovingBumper1x3_pathing",
                    SetupExoticObjects.Winchester_BlueBlock_Moving_1x3
                },
                {
                    "WinchesterMovingBumper2x2_pathing",
                    SetupExoticObjects.Winchester_BlueBlock_Moving_2x2
                },

                {
                    "GunslingKingNPC",
                    SetupExoticObjects.GunslingKing_NPC
                },
                {
                    "ManservantesNPC",
                    SetupExoticObjects.Manservantes_NPC
                },
                {
                    "HighPriestBossRoomFloorDecal",
                    SetupExoticObjects.HighPriestBossRoomFloor
                },
                {
                    "LichBossRoomFloorDecal",
                    SetupExoticObjects.LichBossRoomFloor
                },
                {
                    "trap_pit_gungeon_trigger_2x2",
                    SetupExoticObjects.PitfallTrap
                },
                {
                    "trap_spike_gungeon_2x2",
                    SetupExoticObjects.Spike_trap
                },
                {
                    "trap_flame_poofy_gungeon_1x1",
                    SetupExoticObjects.Flame_Trap
                },
                {
					"npc_blacksmith",
					SetupExoticObjects.NPCBlacksmith
				},
				{
					"npc_henchman",
					SetupExoticObjects.NPCHenchman
				},
				{
					"npc_tootscraze",
					SetupExoticObjects.NPCTootsCraze
				},
				{
					"npc_dyingscientist",
					SetupExoticObjects.NPCDyingScientist
				},
				{
					"table_steel_vertical",
					SetupExoticObjects.SteelTableVertical
				},
				{
					"table_steel_horizontal",
					SetupExoticObjects.SteelTableHorizontal
				},
				{
					"horizontal_crusher",
					SetupExoticObjects.HorizontalCrusher
				},
				{
					"vertical_crusher",
					SetupExoticObjects.VerticalCrusher
				},
                {
					"flameburst_trap",
					SetupExoticObjects.FlameburstTrap
				},
				{
					"firebar_trap",
					SetupExoticObjects.FireBarTrap
				},
				{
					"flame_pipe_north",
					SetupExoticObjects.FlamePipeNorth
				},
				{
					"flame_pipe_west",
					SetupExoticObjects.FlamePipeWest
				},
				{
					"flame_pipe_east",
					SetupExoticObjects.FlamePipeEast
				},
				{
					"pew",
					SetupExoticObjects.Pew
				},
				{
					"gargoyle_statue",
					SetupExoticObjects.GargoyleStatue
				},
				{
					"cryotank_right",
					SetupExoticObjects.CryotankRight
				},
				{
					"cryotank_left",
					SetupExoticObjects.CryotankLeft
				},
				{
					"unconscious_soldier_1",
					SetupExoticObjects.UnconsciousBlackswordSoldier1
				},
				{
					"unconscious_soldier_2",
					SetupExoticObjects.UnconsciousBlackswordSoldier2
				},
				{
					"rotating_green_thing",
					SetupExoticObjects.RotatingGreenThing
				},
				{
					"dark_shelf_skulljar",
					SetupExoticObjects.DarkShelfWithSkullInJar
				},
				{
					"dark_shelf_globe",
					SetupExoticObjects.DarkShelfWithGlobe
				},
				{
					"dark_shelf_magnifyingglass",
					SetupExoticObjects.DarkShelfWithMagnifyingGlass
				},
				{
					"dark_shelf_vials",
					SetupExoticObjects.DarkShelfWithVials
				},
				{
					"dark_shelf_emptyjar",
					SetupExoticObjects.DarkShelfWithEmptyJar
				},
				{
					"decor_cables",
					SetupExoticObjects.DecorativeCables
				},
				{
					"purple_machine_pedestal",
					SetupExoticObjects.MachinePedestalWithPurpleThing
				},
				{
					"broken_car_horizontal",
					SetupExoticObjects.RobotsPastCar
				},
				{
					"broken_car_vertical",
					SetupExoticObjects.RobotsPastCarNorth
				},
				{
					"broken_car_diagonal_northeast",
					SetupExoticObjects.RobotsPastCarNorthEast
				},
				{
					"broken_car_diagonal_northwest",
					SetupExoticObjects.RobotsPastCarNorthWest
				},
				{
					"burning_barrel",
					SetupExoticObjects.RobotsPastTrashfire
				},
				{
					"couch_left",
					SetupExoticObjects.CouchLeft
				},
				{
					"couch_right",
					SetupExoticObjects.CouchRight
				},
				{
					"potted_fern",
					SetupExoticObjects.PottedFern
				},
				{
					"dancer_1",
					SetupExoticObjects.ConvictPastCrowdNPC_01
				},
				{
					"dancer_2",
					SetupExoticObjects.ConvictPastCrowdNPC_02
				},
				{
					"dancer_3",
					SetupExoticObjects.ConvictPastCrowdNPC_03
				},
				{
					"dancer_4",
					SetupExoticObjects.ConvictPastCrowdNPC_04
				},
				{
					"dancer_5",
					SetupExoticObjects.ConvictPastCrowdNPC_05
				},
				{
					"dancer_6",
					SetupExoticObjects.ConvictPastCrowdNPC_06
				},
				{
					"dancer_7",
					SetupExoticObjects.ConvictPastCrowdNPC_07
				},
				{
					"cell_toilet",
					SetupExoticObjects.CellToilet
				},
				{
					"computer_wall",
					SetupExoticObjects.DecorativeComputer
				},
				{
					"machine_frame_ring",
					SetupExoticObjects.MarinesPastMachineDecor
				},
				{
					"decorative_papers",
					SetupExoticObjects.DecorativePapers
				},
				{
					"scientist_blair",
					SetupExoticObjects.ScientistBlair
				},
				{
					"scientist_childs",
					SetupExoticObjects.ScientistChilds
				},
				{
					"scientist_copper",
					SetupExoticObjects.ScientistCopper
				},
				{
					"scientist_dukes",
					SetupExoticObjects.ScientistDukes
				},
				{
					"scientist_gene",
					SetupExoticObjects.ScientistGene
				},
				{
					"scientist_macready",
					SetupExoticObjects.ScientistMacready
				},
				{
					"scientist_windows",
					SetupExoticObjects.ScientistWindows
				},
				{
					"crate_wall_left",
					SetupExoticObjects.CratesLeft
				},
				{
					"crate_wall_right",
					SetupExoticObjects.CratesRight
				},
				{
					"green_marine_left",
					SetupExoticObjects.GreenMarineLeft
				},
				{
					"green_marine_right",
					SetupExoticObjects.GreenMarineRight
				},
				{
					"red_marine_left",
					SetupExoticObjects.RedMarineLeft
				},
				{
					"red_marine_right",
					SetupExoticObjects.RedMarineRight
				},
				{
					"golden_throne",
					SetupExoticObjects.GoldenThrone
				},
				{
					"anvil_left",
					SetupExoticObjects.DecorativeAnvilLeft
				},
				{
					"anvil_right",
					SetupExoticObjects.DecorativeAnvilRight
				},
				{
					"gold_vase",
					SetupExoticObjects.DecorativeGoldVase
				},
				{
					"stone_urn",
					SetupExoticObjects.DecorativeStoneUrn
				},
				{
					"ice_spike_large",
					SetupExoticObjects.DecorativeIceSpikeLarge
				},
				{
					"ice_spike_medium",
					SetupExoticObjects.DecorativeIceSpikeMed
				},
				{
					"ice_spike_small1",
					SetupExoticObjects.DecorativeIceSpikeSmall1
				},
				{
					"ice_spike_small2",
					SetupExoticObjects.DecorativeIceSpikeSmall2
				},
				{
					"armchair_front",
					SetupExoticObjects.DecorativeOfficeChairFront
				},
				{
					"armchair_left",
					SetupExoticObjects.DecorativeOfficeChairLeft
				},
				{
					"armchair_right",
					SetupExoticObjects.DecorativeOfficeChairRight
				},
				{
					"water_cooler",
					SetupExoticObjects.WaterCoolerFront
				},
				{
					"water_cooler_side",
					SetupExoticObjects.WaterCoolerSide
				},
				{
					"potted_office_plant",
					SetupExoticObjects.PottedPlant
				},
				{
					"potted_plant_long_vertical",
					SetupExoticObjects.PottedPlantLongVertical
				},
				{
					"potted_plant_long_horizontal",
					SetupExoticObjects.PottedPlantLongHorizontal
				},
				{
					"cardboard_box_verticalseam",
					SetupExoticObjects.CardboardBox1
				},
				{
					"cardboard_box_open",
					SetupExoticObjects.CardboardBox2
				},
				{
					"cardboard_box_horizontalseam",
					SetupExoticObjects.CardboardBox3
				},
				{
					"wet_floor_sign",
					SetupExoticObjects.WetFloorSign
				},
				{
					"trashbag_center",
					SetupExoticObjects.TrashBag1
				},
				{
					"trashbag_left",
					SetupExoticObjects.TrashBag2
				},
				{
					"trashbag_right",
					SetupExoticObjects.TrashBag3
				},
				{
					"manuscript_table_closed",
					SetupExoticObjects.ManuscriptTableClosed
				},
				{
					"manuscript_table_open",
					SetupExoticObjects.ManuscriptTableOpen
				},
				{
					"manuscript_table_empty",
					SetupExoticObjects.ManuscriptTableEmpty
				},
				{
					"manuscript_table_side",
					SetupExoticObjects.ManuscriptTableSide
				},
				{
					"cheese_candle_medium",
					SetupExoticObjects.CheeseCandle1
				},
				{
					"cheese_candle_short",
					SetupExoticObjects.CheeseCandle2
				},
				{
					"cheese_candle_tall",
					SetupExoticObjects.CheeseCandle3
				},
				{
					"cheese_candle_red",
					SetupExoticObjects.CheeseCandle4
				},
				{
					"stack_of_wood",
					SetupExoticObjects.StackOfWood
				},
				{
					"stalagmite_large",
					SetupExoticObjects.StalagmiteLarge
				},
				{
					"stalagmite_medium",
					SetupExoticObjects.StalagmiteMedium
				},
				{
					"floor_stones_1",
					SetupExoticObjects.FloorStones1
				},
				{
					"floor_stones_2",
					SetupExoticObjects.FloorStones2
				},
				{
					"floor_stones_3",
					SetupExoticObjects.FloorStones3
				},
				{
					"floor_stones_4",
					SetupExoticObjects.FloorStones4
				},
				{
					"shabby_shelf_1",
					SetupExoticObjects.ShabbyShelf1
				},
				{
					"shabby_shelf_2",
					SetupExoticObjects.ShabbyShelf2
				},
				{
					"shovel",
					SetupExoticObjects.Shovel
				},
				{
					"upturned_minecart",
					SetupExoticObjects.UpturnedMinecart
				},
				{
					"mop_and_bucket",
					SetupExoticObjects.MopAndBucket
				},
				{
					"air_conditioning_unit",
					SetupExoticObjects.ACUnit
				},
				{
					"roof_vent",
					SetupExoticObjects.ACVent
				},
				{
					"kitchen_chair_front",
					SetupExoticObjects.KitchenChairFront
				},
				{
					"kitchen_chair_left",
					SetupExoticObjects.KitchenChairLeft
				},
				{
					"kitchen_chair_right",
					SetupExoticObjects.KitchenChairRight
				},
				{
					"kitchen_counter",
					SetupExoticObjects.KitchenCounter
				},
				{
					"bathroom_stall_divider_north",
					SetupExoticObjects.BathroomStallDividerNorth
				},
				{
					"bathroom_stall_divider_west",
					SetupExoticObjects.BathroomStallDividerWest
				},
				{
					"bathroom_stall_divider_east",
					SetupExoticObjects.BathroomStallDividerEast
				},
				{
					"toilet_north",
					SetupExoticObjects.ToiletNorth
				},
				{
					"toilet_west",
					SetupExoticObjects.ToiletWest
				},
				{
					"toilet_east",
					SetupExoticObjects.ToiletEast
				},
				{
					"glass_wall_vertical",
					SetupExoticObjects.GlassWallVertical
				},
				{
					"glass_wall_horizontal",
					SetupExoticObjects.GlassWallHorizontal
				},
				{
					"large_boss_desk",
					SetupExoticObjects.LargeDesk
				},
				{
					"techno_floor_cell_empty",
					SetupExoticObjects.TechnoFloorCellEmpty
				},
				{
					"techno_floor_cell_caterpillar",
					SetupExoticObjects.TechnoFloorCellCaterpillar
				},
				{
					"techno_floor_cell_leever",
					SetupExoticObjects.TechnoFloorCellLeever
				},
				{
					"techno_floor_cell_spider",
					SetupExoticObjects.TechnoFloorCellSpider
				},
				{
					"wide_computer_breakable",
					SetupExoticObjects.WideComputerBreakable
				},
				{
					"tall_computer_breakable",
					SetupExoticObjects.TallComputerBreakable
				},
				{
					"metal_crate",
					SetupExoticObjects.MetalCrate
				},
				{
					"hologram_wall_horizontal",
					SetupExoticObjects.HologramWallHorizontal
				},
				{
					"hologram_wall_vertical",
					SetupExoticObjects.HologramWallVertical
				},
				{
					"ventilation_pipe",
					SetupExoticObjects.VentilationTube
				},
				{
					"agunim_boss_matt",
					SetupExoticObjects.AgunimBossMatt
				},
				{
					"alien_tank",
					SetupExoticObjects.AlienTank
				},
				{
					"decorative_electric_floor",
					SetupExoticObjects.DecorativeElectricFloor
				},
				{
					"wall_gear_north",
					SetupExoticObjects.WallGearNorth
				},
				{
					"wall_gear_west",
					SetupExoticObjects.WallGearWest
				},
				{
					"wall_gear_east",
					SetupExoticObjects.WallGearEast
				},
				{
					"blacksmith_lounger",
					SetupExoticObjects.BlacksmithLounger
				},
				{
					"blacksmith_workbench",
					SetupExoticObjects.BlacksmithWorkbench
				},
				{
					"molten_metal_wall_bowl",
					SetupExoticObjects.MoltenMetalWallCrucible
				},
				{
					"floating_magic_orb",
					SetupExoticObjects.FloatingMagicOrb
				},
				{
					"armor_pickup",
					SetupExoticObjects.NonRatStealableArmor
				},
				{
					"fifty_casing_pickup",
					SetupExoticObjects.FiftyCasingPlaceable
				},
				{
					"five_casing_pickup",
					SetupExoticObjects.FiveCasingPlaceable
				},
				{
					"glass_guon_pickup",
					SetupExoticObjects.GlassGuonPlaceable
				},
				{
					"half_heart_pickup",
					SetupExoticObjects.HalfHeart
				},
				{
					"heart_pickup",
					SetupExoticObjects.FullHeart
				},
				{
					"map_pickup",
					SetupExoticObjects.MapPlaceable
				},
				{
					"rat_key_pickup",
					SetupExoticObjects.RatKeyPlaceable
				},
				{
					"single_casing_pickup",
					SetupExoticObjects.SingleCasingPlaceable
				},
				{
					"folding_table",
					SetupExoticObjects.FoldingTable
				},
				{
					"saw_blade_pathing",
					SetupExoticObjects.SawBlade
				},
				{
					"minecart_pathing",
					SetupExoticObjects.Minecart
				},
                {
                    "turretminecart_pathing",
                    SetupExoticObjects.TurretMinecart
                },
                {
                    "explosivebarrelminecart_pathing",
                    SetupExoticObjects.ExplosiveBarrelMinecart
                },
                {
                    "sconce_horizontal",
                    SetupExoticObjects.KeepSconce
                },
                {
                    "sconce_left_",
                    SetupExoticObjects.KeepSconceSideLeft
                },
                {
                    "sconce_right_",
                    SetupExoticObjects.KeepSconceSideRight
                },
                {
                    "basictorch_h",
                    SetupExoticObjects.BasicTorch
                },
                {
                    "basictorch_l",
                    SetupExoticObjects.BasicTorchSideLeft
                },
                {
                    "basictorch_r",
                    SetupExoticObjects.BasicTorchSideRight
                },
                {
                    "breakablePillarTreadnought",
                    SetupExoticObjects.TreadnoughtPillar
                },
                {
                    "brokenPillarTreadnought",
                    SetupExoticObjects.TreadnoughtPillarDestroyed
                },
                {
                    "BlobulordGrateFloorDecal",
                    SetupExoticObjects.BlobulordGrate
                },
                {
                    "CustomlightSource",
                    SetupExoticObjects.GungeonLightSource
                },
                {
                    "medievalCrate",
                    SetupExoticObjects.MedievalCrate
                },
                {
                    "suitofarmorGold",
                    SetupExoticObjects.GoldArmor
                },
                {
                    "tanPotAbbey",
                    SetupExoticObjects.TanPot
                },
                {
                    "candlabra_horiz",
                    SetupExoticObjects.CandleabraHorizontal
                },
                {
                    "candlabra_left",
                    SetupExoticObjects.CandleabraLeft
                },
                {
                    "candlabra_right",
                    SetupExoticObjects.CandleabraRight
                },
                {
                    "minelantern_horiz",
                    SetupExoticObjects.MineLanternHorizontal
                },
                {
                    "minelantern_left",
                    SetupExoticObjects.MineLanternLeft
                },
                {
                    "minelantern_right",
                    SetupExoticObjects.MineLanternRight
                },
            };
            dungeon = null;
			dungeon2 = null;
			dungeon3 = null;
			dungeon4 = null;
			dungeon5 = null;
			dungeon6 = null;
			dungeon7 = null;
			dungeon8 = null;
			dungeon9 = null;
			dungeon10 = null;
			dungeon11 = null;
			dungeon12 = null;
			dungeon13 = null;
			tutorialDungeon = null;
		}

		public static List<string> allBasictrapControllerAssetNames = new List<string>()
		{
            "trap_flame_poofy_gungeon_1x1",
            "trap_pit_gungeon_trigger_2x2",
            "trap_spike_gungeon_2x2"
        };

        //ExplosiveBarrelMinecart
        public static GameObject NPCBlacksmith;
		public static GameObject NPCHenchman;
		public static GameObject NPCTootsCraze;
		public static GameObject NPCDyingScientist;
		public static GameObject SteelTableVertical;
		public static GameObject SteelTableHorizontal;
		public static GameObject HorizontalCrusher;
		public static GameObject VerticalCrusher;
		public static GameObject FlameburstTrap;
		public static GameObject FireBarTrap;
		public static GameObject FlamePipeNorth;
		public static GameObject FlamePipeWest;
		public static GameObject FlamePipeEast;
		public static GameObject Pew;
		public static GameObject GargoyleStatue;
		public static GameObject CryotankRight;
		public static GameObject CryotankLeft;
		public static GameObject UnconsciousBlackswordSoldier1;
		public static GameObject UnconsciousBlackswordSoldier2;
		public static GameObject RotatingGreenThing;
		public static GameObject DarkShelfWithSkullInJar;
		public static GameObject DarkShelfWithGlobe;
		public static GameObject DarkShelfWithMagnifyingGlass;
		public static GameObject DarkShelfWithVials;
		public static GameObject DarkShelfWithEmptyJar;
		public static GameObject DecorativeCables;
		public static GameObject MachinePedestalWithPurpleThing;
		public static GameObject RobotsPastCar;
		public static GameObject RobotsPastCarNorth;
		public static GameObject RobotsPastTrashfire;
		public static GameObject RobotsPastCarNorthEast;
		public static GameObject RobotsPastCarNorthWest;
		public static GameObject CouchLeft;
		public static GameObject CouchRight;
		public static GameObject PottedFern;
		public static GameObject ConvictPastCrowdNPC_01;
		public static GameObject ConvictPastCrowdNPC_02;
		public static GameObject ConvictPastCrowdNPC_03;
		public static GameObject ConvictPastCrowdNPC_04;
		public static GameObject ConvictPastCrowdNPC_05;
		public static GameObject ConvictPastCrowdNPC_06;
		public static GameObject ConvictPastCrowdNPC_07;
		public static GameObject[] ConvictPastDancers;
		public static GameObject CellToilet;
		public static GameObject MarinesPastMachineDecor;
		public static GameObject DecorativeComputer;
		public static GameObject DecorativePapers;
		public static GameObject ScientistBlair;
		public static GameObject ScientistChilds;
		public static GameObject ScientistCopper;
		public static GameObject ScientistDukes;
		public static GameObject ScientistGene;
		public static GameObject ScientistMacready;
		public static GameObject ScientistWindows;
		public static GameObject CratesLeft;
		public static GameObject CratesRight;
		public static GameObject GreenMarineLeft;
		public static GameObject GreenMarineRight;
		public static GameObject RedMarineLeft;
		public static GameObject RedMarineRight;
		public static GameObject GoldenThrone;
		public static GameObject DecorativeAnvilLeft;
		public static GameObject DecorativeAnvilRight;
		public static GameObject DecorativeGoldVase;
		public static GameObject DecorativeStoneUrn;
		public static GameObject DecorativeIceSpikeLarge;
		public static GameObject DecorativeIceSpikeMed;
		public static GameObject DecorativeIceSpikeSmall1;
		public static GameObject DecorativeIceSpikeSmall2;
		public static GameObject DecorativeOfficeChairFront;
		public static GameObject DecorativeOfficeChairLeft;
		public static GameObject DecorativeOfficeChairRight;
		public static GameObject WaterCoolerFront;
		public static GameObject WaterCoolerSide;
		public static GameObject PottedPlant;
		public static GameObject PottedPlantLongVertical;
		public static GameObject PottedPlantLongHorizontal;
		public static GameObject CardboardBox1;
		public static GameObject CardboardBox2;
		public static GameObject CardboardBox3;
		public static GameObject WetFloorSign;
		public static GameObject TrashBag1;
		public static GameObject TrashBag2;
		public static GameObject TrashBag3;
		public static GameObject ManuscriptTableClosed;
		public static GameObject ManuscriptTableOpen;
		public static GameObject ManuscriptTableEmpty;
		public static GameObject ManuscriptTableSide;
		public static GameObject CheeseCandle1;
		public static GameObject CheeseCandle2;
		public static GameObject CheeseCandle3;
		public static GameObject CheeseCandle4;
		public static GameObject StackOfWood;
		public static GameObject StalagmiteLarge;
		public static GameObject StalagmiteMedium;
		public static GameObject FloorStones1;
		public static GameObject FloorStones2;
		public static GameObject FloorStones3;
		public static GameObject FloorStones4;
		public static GameObject ShabbyShelf1;
		public static GameObject ShabbyShelf2;
		public static GameObject Shovel;
		public static GameObject UpturnedMinecart;
		public static GameObject MopAndBucket;
		public static GameObject ACUnit;
		public static GameObject ACVent;
		public static GameObject KitchenChairFront;
		public static GameObject KitchenChairLeft;
		public static GameObject KitchenChairRight;
		public static GameObject KitchenCounter;
		public static GameObject BathroomStallDividerNorth;
		public static GameObject BathroomStallDividerWest;
		public static GameObject BathroomStallDividerEast;
		public static GameObject ToiletNorth;
		public static GameObject ToiletWest;
		public static GameObject ToiletEast;
		public static GameObject GlassWallVertical;
		public static GameObject GlassWallHorizontal;
		public static GameObject LargeDesk;
		public static GameObject TechnoFloorCellEmpty;
		public static GameObject TechnoFloorCellCaterpillar;
		public static GameObject TechnoFloorCellLeever;
		public static GameObject TechnoFloorCellSpider;
		public static GameObject WideComputerBreakable;
		public static GameObject TallComputerBreakable;
		public static GameObject MetalCrate;
		public static GameObject HologramWallHorizontal;
		public static GameObject HologramWallVertical;
		public static GameObject VentilationTube;
		public static GameObject AgunimBossMatt;
		public static GameObject AlienTank;
		public static GameObject DecorativeElectricFloor;
		public static GameObject WallGearNorth;
		public static GameObject WallGearWest;
		public static GameObject WallGearEast;
		public static GameObject BlacksmithLounger;
		public static GameObject BlacksmithWorkbench;
		public static GameObject MoltenMetalWallCrucible;
		public static GameObject FloatingMagicOrb;
		public static GameObject NonRatStealableAmmo;
		public static GameObject NonRatStealableSpreadAmmo;
		public static GameObject FullHeart = PickupObjectDatabase.GetById(85).gameObject;
		public static GameObject HalfHeart = PickupObjectDatabase.GetById(73).gameObject;
		public static GameObject NonRatStealableArmor;
		public static GameObject RatKeyPlaceable = PickupObjectDatabase.GetById(727).gameObject;
		public static GameObject GlassGuonPlaceable;
		public static GameObject MapPlaceable;
		public static GameObject SingleCasingPlaceable;
		public static GameObject FiveCasingPlaceable;
		public static GameObject FiftyCasingPlaceable;
		public static GameObject FoldingTable = PickupObjectDatabase.GetById(644).GetComponent<FoldingTableItem>().TableToSpawn.gameObject;
		public static GameObject SawBlade;
		public static GameObject BloodySawBlade;
		public static GameObject Minecart;
        public static GameObject TurretMinecart;
        public static GameObject ExplosiveBarrelMinecart;


		public static GameObject KeepSconce;
        public static GameObject KeepSconceSideLeft;
        public static GameObject KeepSconceSideRight;

        public static GameObject TombStone_N;
        public static GameObject TombStone_NE;
        public static GameObject TombStone_E;
        public static GameObject TombStone_SE;
        public static GameObject TombStone_S;
        public static GameObject TombStone_SW;
        public static GameObject TombStone_W;
        public static GameObject TombStone_NW;

        public static GameObject BasicTorch;
        public static GameObject BasicTorchSideLeft;
        public static GameObject BasicTorchSideRight;
        public static GameObject LostAdventurer;
        public static GameObject BlobulordGrate;
        public static GameObject TreadnoughtPillar;
        public static GameObject TreadnoughtPillarDestroyed;
        public static GameObject GungeonLightSource;
        public static GameObject MedievalCrate;
        public static GameObject GoldArmor;
        public static GameObject TanPot;
        public static GameObject CandleabraHorizontal;
        public static GameObject CandleabraLeft;
        public static GameObject CandleabraRight;
        public static GameObject MineLanternHorizontal;
        public static GameObject MineLanternLeft;
        public static GameObject MineLanternRight;
        public static GameObject Flame_Trap;
        public static GameObject Spike_trap;
        public static GameObject PitfallTrap;
        public static GameObject LichBossRoomFloor;
        public static GameObject HighPriestBossRoomFloor;
        public static GameObject GunslingKing_NPC;
        public static GameObject Manservantes_NPC;

        //MM yess winchester time
        public static GameObject Winchester_NPC;
        public static GameObject Winchester_BlueBlock;
        public static GameObject Winchester_RedBlock;
        public static GameObject Winchester_GreenBlock;
        public static GameObject Winchester_BlueBlock_Corner_BL;
        public static GameObject Winchester_BlueBlock_Corner_BR;
        public static GameObject Winchester_BlueBlock_Corner_TL;
        public static GameObject Winchester_BlueBlock_Corner_TR;
        public static GameObject Winchester_BlueBlock_Moving_1x3;
        public static GameObject Winchester_BlueBlock_Moving_2x2;
        public static GameObject Winchester_NeonSign;
        public static GameObject Winchester_MovingTarget;
        public static GameObject Winchester_CameraPoint;
        public static GameObject Winchester_RoomController;
        public static GameObject Winchester_CameraPanController;

        public static GameObject Skeleton_Standing_Left;
        public static GameObject Skeleton_Standing_Right;
        public static GameObject Skeleton_Hanging_Left;
        public static GameObject Skeleton_Hanging_Right;
        public static GameObject Dragun_Skull;
        public static GameObject Dragun_Vertebra;

        public static GameObject BurningSkeleton_Horizontal;
        public static GameObject BurningSkeleton_Left;
        public static GameObject BurningSkeleton_Right;

        public static GameObject LavaLight_Horizontal;
        public static GameObject LavaLight_Left;
        public static GameObject LavaLight_Right;

        public static GameObject OfficeLight_Horizontal;
        public static GameObject OfficeLight_Left;
        public static GameObject OfficeLight_Right;

        public static GameObject WeirdGreenLight_Horizontal;
        public static GameObject WeirdGreenLight_Left;
        public static GameObject WeirdGreenLight_Right;

        public static GameObject Conveyor_Horizontal;
        public static GameObject Conveyor_Vertical;

        public static GameObject Floor_Skeleton;
        public static GameObject Floor_Skeleton_Note;

        public static GameObject Switch_Chandelier;
        public static GameObject Chandelier_Drop;

        public static GameObject Mine_CaveIn;
        public static GameObject Mine_CaveIn_Switch;

        public static GameObject BulletPast_PitfallTrap;


        public static GameObject MouseTrap_North;
        public static GameObject MouseTrap_East;
        public static GameObject MouseTrap_West;

        public static GameObject Spinning_Log_Horizontal;
        public static GameObject Spinning_Log_Vertical;

        public static GameObject Spinning_Ice_Log_Horizontal;
        public static GameObject Spinning_Ice_Log_Vertical;

        public static GameObject Sewer_Entrance;
        public static GameObject Sewer_Entrace_Angry_Button;
        public static GameObject Keep_Fireplace;

        public static GameObject Moving_Platform_Proper;
        public static GameObject Moving_Platform_Sewer;
        public static GameObject Moving_Platform_Mines;
        public static GameObject Moving_Platform_Hollow;
        public static GameObject Moving_Platform_Forge;


        public static GameObject Bullet_King_Arena_Floor;
        public static GameObject Bullet_King_Arena_MainWall;
        public static GameObject Bullet_King_Arena_SideWall_1;
        public static GameObject Bullet_King_Arena_SideWall_2;

        public static GameObject Wallmonger_Arena_Floor;


        public static GameObject Old_King_Arena_Floor;
        public static GameObject Old_King_Arena_MainWall;
        public static GameObject Old_King_Arena_SideWall_1;
        public static GameObject Old_King_Arena_SideWall_2;
        public static GameObject ForgeGunTrap_North;
        public static GameObject ForgeGunTrap_East;
        public static GameObject ForgeGunTrap_West;
        public static GameObject HollowGunTrap_North;
        public static GameObject HollowGunTrap_East;
        public static GameObject HollowGunTrap_West;
        public static GameObject MinesGunTrap_North;
        public static GameObject MinesGunTrap_East;
        public static GameObject MinesGunTrap_West;
        public static GameObject MinecarftFactory_Object;
		public static GameObject GatlingGull_Valid_Leap_Position;
        public static GameObject Glitched_Boss_Modifier;
        public static GameObject Golden_Boss_Pedestal;
        public static GameObject AttackLeapPoint;
        public static GameObject DemonFace;


        public static GameObject Wood_Sign_N;
        public static GameObject Wood_Sign_E;
        public static GameObject Wood_Sign_W;
        public static GameObject Wood_Sign_S;

        public static GameObject Red_Torn_Carpet;
        public static GameObject Stair_Case;
        public static GameObject Start_Room_Decor;
        public static GameObject Start_Room_Floor;
        public static GameObject Special_Dais;
        public static GameObject Hanging_Concrete_Block;
        public static GameObject Gungeon_Grate;

        public static GameObject Shop_TeleporterSign;
		public static GameObject Shop_Crates;
        public static GameObject Shop_Crate;
        public static GameObject Shop_Sack;
        public static GameObject Shop_ShelfBarrel;
        public static GameObject Shop_Shelf;
        public static GameObject Shop_Mask;
        public static GameObject Shop_Wallsword;
        public static GameObject Shop_StandingShelf;
        public static GameObject Shop_AKBarrel;
        public static GameObject Shop_Stool;
        public static GameObject ShopLayout;

        public static GameObject ShopItemObject;

        public static GameObject Ser_Manuels_Body;

        public static GameObject ShopRoundTable;
        public static GameObject ShopRoundTable_Empty;

        public static GameObject Glass_Case;
        public static GameObject Glass_Case_Custom;
        public static GameObject Glass_Case_Empty;

        public static GameObject No_Pickup;

        public static Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
	}
}
