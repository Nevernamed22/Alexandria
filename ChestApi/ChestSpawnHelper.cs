using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

<<<<<<< Updated upstream
namespace ChestApi
{
    public static class ChestSpawnHelper
    {
        public static void Init()
        {
            var ConfigureOnPlacementHook = new Hook(
                    typeof(FloorChestPlacer).GetMethod("ConfigureOnPlacement", BindingFlags.Instance | BindingFlags.Public),
                    typeof(ChestSpawnHelper).GetMethod("ConfigureOnPlacementHook", BindingFlags.Static | BindingFlags.Public));
		}

		public static void RegisterCustomChest(Chest chest, float weight = 1)
        {
=======
namespace Alexandria.ChestApi
{
	public static class ChestSpawnHelper
	{
		public static void Init()
		{
			var ConfigureOnPlacementHook = new Hook(
					typeof(FloorChestPlacer).GetMethod("ConfigureOnPlacement", BindingFlags.Instance | BindingFlags.Public),
					typeof(ChestSpawnHelper).GetMethod("ConfigureOnPlacementHook", BindingFlags.Static | BindingFlags.Public));
		}

		public static void RegisterCustomChest(Chest chest, float weight = 1)
		{
>>>>>>> Stashed changes

		}

		//d 0.25
		//c 0.32
		//b 0.2
		//a 0.09
		//s 0.04

		public static void ConfigureOnPlacementHook(Action<FloorChestPlacer, RoomHandler> orig, FloorChestPlacer self, RoomHandler room)
		{

<<<<<<< Updated upstream
			
=======

>>>>>>> Stashed changes
			var chest = GetTargetCustomChest(UnityEngine.Random.value);
			if (chest)
			{
				self.OverrideChestPrefab = chest;
				self.UseOverrideChest = true;
				DungeonPrerequisite dungeonPrerequisite = new DungeonPrerequisite();
				dungeonPrerequisite.requireTileset = true;
				dungeonPrerequisite.requiredTileset = GlobalDungeonData.ValidTilesets.CASTLEGEON;
				dungeonPrerequisite.prerequisiteType = DungeonPrerequisite.PrerequisiteType.TILESET;
<<<<<<< Updated upstream
=======
				self.OverrideChestPrereq = dungeonPrerequisite;
>>>>>>> Stashed changes
			}
			orig(self, room);
		}

		public static Chest GetTargetCustomChest(float fran)
		{
			//float currentMagnificence = GameManager.Instance.RewardManager.CurrentRewardData.DetermineCurrentMagnificence(isGenerationForMagnificence);

			float dumbFuckingChestNumber = 0;
<<<<<<< Updated upstream
			foreach(var chest in customChests)
            {
=======
			foreach (var chest in customChests)
			{
>>>>>>> Stashed changes
				dumbFuckingChestNumber += chest.Value.Second;
				if (fran < dumbFuckingChestNumber)
				{
					return chest.Value.First;
				}
			}
			return null;
		}


		public static Dictionary<string, Tuple<Chest, float>> customChests = new Dictionary<string, Tuple<Chest, float>>();

	}
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes
