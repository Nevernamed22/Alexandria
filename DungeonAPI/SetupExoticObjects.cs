using System;
using System.Collections.Generic;
using Alexandria.ItemAPI;
using Dungeonator;
using UnityEngine;
namespace Alexandria.DungeonAPI
{
	public class SetupExoticObjects
	{
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
			foreach (WeightedRoom weightedRoom in dungeon.PatternSettings.flows[0].fallbackRoomTable.includedRooms.elements)
			{
				if (weightedRoom.room != null && !string.IsNullOrEmpty(weightedRoom.room.name))
				{
					if (weightedRoom.room.name.ToLower().StartsWith("sewer_trash_compactor_001"))
					{
						SetupExoticObjects.HorizontalCrusher = weightedRoom.room.placedObjects[0].nonenemyBehaviour.gameObject;
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
			SetupExoticObjects.objects = new Dictionary<string, GameObject>
			{
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
		}

		
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
		public static Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();
	}
}
