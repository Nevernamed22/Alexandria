using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using UnityEngine;
using Dungeonator;
using MonoMod.RuntimeDetour;

namespace GungeonAPI
{
    //Utility methods
    public static class ShrineTools
    {
        public static bool verbose = false;
       // private static string defaultLog = Path.Combine(ETGMod.ResourcesDirectory, "PSOG.txt");
        public static string modID = "Alexandria";

        public static PrototypeDungeonRoom GetRoomFromBundles(string assetPath)
        {
            return StaticReferences.LoadAssetFromAnywhere<PrototypeDungeonRoom>(assetPath);
        }

        public static GameObject GetGameObjectFromBundles(string assetPath)
        {
            return StaticReferences.LoadAssetFromAnywhere<GameObject>(assetPath);
        }

        public static DungeonPlaceable GetPlaceableFromBundles(string assetPath)
        {
            return StaticReferences.LoadAssetFromAnywhere<DungeonPlaceable>(assetPath);
        }


        private static Dictionary<string, float> timers = new Dictionary<string, float>();

        public static void Init()
        {
            //if (File.Exists(defaultLog)) File.Delete(defaultLog);
        }

        public static void Print<T>(T obj, string color = "FFFFFF", bool force = false)
        {
            if (verbose || force)
            {
                string[] lines = obj.ToString().Split('\n');
                foreach (var line in lines)
                    LogToConsole($"<color=#{color}>[{modID}] {line}</color>");
            }

            //Log(obj.ToString());
        }

        public static void PrintRaw<T>(T obj, bool force = false)
        {
            if (verbose || force)
                LogToConsole(obj.ToString());

            //Log(obj.ToString());
        }

        public static void PrintError<T>(T obj, string color = "FF0000")
        {
            string[] lines = obj.ToString().Split('\n');
            foreach (var line in lines)
                LogToConsole($"<color=#{color}>[{modID}] {line}</color>");

            //Log(obj.ToString());
        }

        public static void PrintException(Exception e, string color = "FF0000")
        {
            string message = e.Message + "\n" + e.StackTrace;
            {
                string[] lines = message.Split('\n');
                foreach (var line in lines)
                    LogToConsole($"<color=#{color}>[{modID}] {line}</color>");
            }

            //Log(e.Message);
            //Log("\t" + e.StackTrace);
        }

        //public static void Log<T>(T obj)
        //{
        //    using (StreamWriter writer = new StreamWriter(Path.Combine(ETGMod.ResourcesDirectory, defaultLog), true))
        //    {
        //        writer.WriteLine(obj.ToString());
        //    }
        //}

        public static void Log<T>(T obj, string fileName)
        {
            if (!verbose) return;
            using (StreamWriter writer = new StreamWriter(Path.Combine(ETGMod.ResourcesDirectory, fileName), true))
            {
                writer.WriteLine(obj.ToString());
            }
        }

        public static void LogToConsole(string message)
        {
            message.Replace("\t", "    ");
            ETGModConsole.Log(message);
        }

        private static void BreakdownComponentsInternal(this GameObject obj, int lvl = 0)
        {
            string space = "";
            for (int i = 0; i < lvl; i++)
            {
                space += "\t";
            }

            //Log(space + obj.name + "...");
            foreach (var comp in obj.GetComponents<Component>())
            {
                //Log(space + "    -" + comp.GetType());
            }

            foreach (var child in obj.GetComponentsInChildren<Transform>())
            {
                if (child != obj.transform)
                    child.gameObject.BreakdownComponentsInternal(lvl + 1);
            }
        }

        public static void BreakdownComponents(this GameObject obj)
        {
            BreakdownComponentsInternal(obj, 0);
        }

        public static void ExportTexture(Texture texture, string folder = "")
        {
            string path = Path.Combine(ETGMod.ResourcesDirectory, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(Path.Combine(path, texture.name + ".png"), ((Texture2D)texture).EncodeToPNG());
        }

        public static T GetEnumValue<T>(string val) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), val.ToUpper());
        }

		public static void LogRoomToPNGFile(PrototypeDungeonRoom room)
		{
			int width = room.Width;
			int height = room.Height;

			Texture2D m_NewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
			if (!string.IsNullOrEmpty(room.name)) { m_NewImage.name = room.name; }

			Color WhitePixel = new Color32(255, 255, 255, 255); // Wall Cell
			Color PinkPixel = new Color32(255, 0, 255, 255); // Diagonal Wall Cell (North East)
			Color YellowPixel = new Color32(255, 255, 0, 255); // Diagonal Wall Cell (North West)
			Color HalfPinkPixel = new Color32(127, 0, 127, 255); // Diagonal Wall Cell (South East)
			Color HalfYellowPixel = new Color32(127, 127, 0, 255); // Diagonal Wall Cell (South West)

			Color BluePixel = new Color32(0, 0, 255, 255); // Floor Cell

			Color BlueHalfGreenPixel = new Color32(0, 127, 255, 255); // Floor Cell (Ice Override)
			Color HalfBluePixel = new Color32(0, 0, 127, 255); // Floor Cell (Water Override)
			Color HalfRedPixel = new Color32(0, 0, 127, 255); // Floor Cell (Carpet Override)
			Color GreenHalfRBPixel = new Color32(127, 255, 127, 255); // Floor Cell (Grass Override)
			Color HalfWhitePixel = new Color32(127, 127, 127, 255); // Floor Cell (Bone Override)
			Color OrangePixel = new Color32(255, 127, 0, 255); // Floor Cell (Flesh Override)
			Color RedHalfGBPixel = new Color32(255, 127, 127, 255); // Floor Cell (ThickGoop Override)

			Color GreenPixel = new Color32(0, 255, 0, 255); // Damage Floor Cell

			Color RedPixel = new Color32(255, 0, 0, 255); // Pit Cell

			Color BlackPixel = new Color32(0, 0, 0, 255); // NULL Cell

			for (int X = 0; X < width; X++)
			{
				for (int Y = 0; Y < height; Y++)
				{
					CellType? cellData = room.GetCellDataAtPoint(X, Y).state;
					bool DamageCell = false;
					DiagonalWallType diagonalWallType = DiagonalWallType.NONE;
					if (room.GetCellDataAtPoint(X, Y) != null && cellData.HasValue)
					{
						DamageCell = room.GetCellDataAtPoint(X, Y).doesDamage;
						diagonalWallType = room.GetCellDataAtPoint(X, Y).diagonalWallType;
					}
					if (room.GetCellDataAtPoint(X, Y) == null | !cellData.HasValue)
					{
						m_NewImage.SetPixel(X, Y, BlackPixel);
					}
					else if (cellData.Value == CellType.FLOOR)
					{
						if (DamageCell)
						{
							m_NewImage.SetPixel(X, Y, GreenPixel);
						}
						else if (room.GetCellDataAtPoint(X, Y).appearance != null)
						{
							CellVisualData.CellFloorType overrideFloorType = room.GetCellDataAtPoint(X, Y).appearance.OverrideFloorType;
							if (overrideFloorType == CellVisualData.CellFloorType.Stone)
							{
								m_NewImage.SetPixel(X, Y, BluePixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Ice)
							{
								m_NewImage.SetPixel(X, Y, BlueHalfGreenPixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Water)
							{
								m_NewImage.SetPixel(X, Y, HalfBluePixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Carpet)
							{
								m_NewImage.SetPixel(X, Y, HalfRedPixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Grass)
							{
								m_NewImage.SetPixel(X, Y, GreenHalfRBPixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Bone)
							{
								m_NewImage.SetPixel(X, Y, HalfWhitePixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.Flesh)
							{
								m_NewImage.SetPixel(X, Y, OrangePixel);
							}
							else if (overrideFloorType == CellVisualData.CellFloorType.ThickGoop)
							{
								m_NewImage.SetPixel(X, Y, RedHalfGBPixel);
							}
							else
							{
								m_NewImage.SetPixel(X, Y, BluePixel);
							}
						}
						else
						{
							m_NewImage.SetPixel(X, Y, BluePixel);
						}
					}
					else if (cellData.Value == CellType.WALL)
					{
						if (diagonalWallType == DiagonalWallType.NORTHEAST)
						{
							m_NewImage.SetPixel(X, Y, PinkPixel);
						}
						else if (diagonalWallType == DiagonalWallType.NORTHWEST)
						{
							m_NewImage.SetPixel(X, Y, YellowPixel);
						}
						else if (diagonalWallType == DiagonalWallType.SOUTHEAST)
						{
							m_NewImage.SetPixel(X, Y, HalfPinkPixel);
						}
						else if (diagonalWallType == DiagonalWallType.SOUTHWEST)
						{
							m_NewImage.SetPixel(X, Y, HalfYellowPixel);
						}
						else
						{
							m_NewImage.SetPixel(X, Y, WhitePixel);
						}
					}
					else if (cellData.Value == CellType.PIT)
					{
						m_NewImage.SetPixel(X, Y, RedPixel);
					}
				}
			}

			m_NewImage.Apply();

			string basePath = "DumpedRoomLayouts/";

			string fileName = (basePath + m_NewImage.name);
			if (string.IsNullOrEmpty(m_NewImage.name)) { fileName += ("RoomLayout_" + Guid.NewGuid().ToString()); }

			fileName += "_Layout";

			string path = Path.Combine(ETGMod.ResourcesDirectory, fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");

			if (!File.Exists(path)) { Directory.GetParent(path).Create(); }

			File.WriteAllBytes(path, ImageConversion.EncodeToPNG(m_NewImage));
		}

		public static void LogRoomHandlerToPNGFile(RoomHandler room)
		{
			int width = room.area.dimensions.x + 1;
			int height = room.area.dimensions.y + 1;
			IntVector2 basePosition = room.area.basePosition;

			Texture2D m_NewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
			if (!string.IsNullOrEmpty(room.GetRoomName())) { m_NewImage.name = room.GetRoomName(); }

			Color WhitePixel = Color.grey; // Wall Cell -=
			Color PinkPixel = Color.red; // Diagonal Wall Cell (North East) -=
			Color YellowPixel = Color.yellow; // Diagonal Wall Cell (North West) -=
			Color HalfPinkPixel = Color.green; // Diagonal Wall Cell (South East) -=
			Color HalfYellowPixel = Color.blue; // Diagonal Wall Cell (South West) -=

			Color BluePixel = Color.white; // Floor Cell -=

			Color BlueHalfGreenPixel = new Color32(0, 127, 255, 255); // Floor Cell (Ice Override) -=
			Color HalfBluePixel = new Color32(0, 0, 127, 255); // Floor Cell (Water Override) -=
			Color HalfRedPixel = new Color32(127, 0, 0, 255); // Floor Cell (Carpet Override) -=
			Color GreenHalfRBPixel = new Color32(127, 255, 127, 255); // Floor Cell (Grass Override) -=
			Color HalfWhitePixel = new Color32(127, 127, 127, 255); // Floor Cell (Bone Override) -=
			Color OrangePixel = new Color32(255, 127, 0, 255); // Floor Cell (Flesh Override) -=
			Color RedHalfGBPixel = new Color32(255, 127, 127, 255); // Floor Cell (ThickGoop Override) -=

			Color GreenPixel = new Color32(0, 255, 0, 255); // Damage Floor Cell -=

			Color RedPixel = Color.black; // Pit Cell -=

			Color BlackPixel = Color.magenta; // NULL Cell -=

			for (int X = -1; X < width; X++)
			{
				for (int Y = -1; Y < height; Y++)
				{
					IntVector2 m_CellPosition = (new IntVector2(X, Y) + basePosition);
					if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(m_CellPosition.x, m_CellPosition.y))
					{
						CellType? cellData = GameManager.Instance.Dungeon.data[m_CellPosition].type;
						CellData localDungeonData = GameManager.Instance.Dungeon.data[m_CellPosition];
						bool DamageCell = false;
						DiagonalWallType diagonalWallType = DiagonalWallType.NONE;
						if (localDungeonData != null)
						{
							DamageCell = localDungeonData.doesDamage;
							diagonalWallType = localDungeonData.diagonalWallType;
						}
						if (localDungeonData == null | !cellData.HasValue)
						{
							m_NewImage.SetPixel(X, Y, BlackPixel);
						}
						else if (cellData.Value == CellType.FLOOR)
						{
							if (DamageCell)
							{
								m_NewImage.SetPixel(X, Y, GreenPixel);
							}
							else
							{
								CellVisualData.CellFloorType overrideFloorType = localDungeonData.cellVisualData.floorType;
								if (overrideFloorType == CellVisualData.CellFloorType.Stone)
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Ice)
								{
									m_NewImage.SetPixel(X, Y, BlueHalfGreenPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Water)
								{
									m_NewImage.SetPixel(X, Y, HalfBluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Carpet)
								{
									m_NewImage.SetPixel(X, Y, HalfRedPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Grass)
								{
									m_NewImage.SetPixel(X, Y, GreenHalfRBPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Bone)
								{
									m_NewImage.SetPixel(X, Y, HalfWhitePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Flesh)
								{
									m_NewImage.SetPixel(X, Y, OrangePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.ThickGoop)
								{
									m_NewImage.SetPixel(X, Y, RedHalfGBPixel);
								}
								else
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
							}
						}
						else if (cellData.Value == CellType.WALL)
						{
							if (diagonalWallType == DiagonalWallType.NORTHEAST)
							{
								m_NewImage.SetPixel(X, Y, PinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.NORTHWEST)
							{
								m_NewImage.SetPixel(X, Y, YellowPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHEAST)
							{
								m_NewImage.SetPixel(X, Y, HalfPinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHWEST)
							{
								m_NewImage.SetPixel(X, Y, HalfYellowPixel);
							}
							else
							{
								m_NewImage.SetPixel(X, Y, WhitePixel);
							}
						}
						else if (cellData.Value == CellType.PIT)
						{
							m_NewImage.SetPixel(X, Y, RedPixel);
						}
					}
					else
					{
						m_NewImage.SetPixel(X, Y, BlackPixel);
					}
				}
			}

			m_NewImage.Apply();

			string basePath = "DumpedRoomLayouts/";

			string fileName = (basePath + m_NewImage.name);
			if (string.IsNullOrEmpty(m_NewImage.name)) { fileName += ("RoomLayout_" + Guid.NewGuid().ToString()); }

			fileName += "_Layout";

			string path = Path.Combine(ETGMod.ResourcesDirectory, fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");

			if (!File.Exists(path)) { Directory.GetParent(path).Create(); }

			File.WriteAllBytes(path, ImageConversion.EncodeToPNG(m_NewImage));
		}

		public static void LogDungeonToPNGFile()
		{
			// int width = GameManager.Instance.Dungeon.data.Height;
			// int height = GameManager.Instance.Dungeon.data.Width;
			int width = 1000;
			int height = 1000;

			Texture2D m_NewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
			m_NewImage.name = GameManager.Instance.Dungeon.gameObject.name;

			Color WhitePixel = new Color32(255, 255, 255, 255); // Wall Cell
			Color PinkPixel = new Color32(255, 0, 255, 255); // Diagonal Wall Cell (North East)
			Color YellowPixel = new Color32(255, 255, 0, 255); // Diagonal Wall Cell (North West)
			Color HalfPinkPixel = new Color32(127, 0, 127, 255); // Diagonal Wall Cell (South East)
			Color HalfYellowPixel = new Color32(127, 127, 0, 255); // Diagonal Wall Cell (South West)

			Color BluePixel = new Color32(0, 0, 255, 255); // Floor Cell

			Color BlueHalfGreenPixel = new Color32(0, 127, 255, 255); // Floor Cell (Ice Override)
			Color HalfBluePixel = new Color32(0, 0, 127, 255); // Floor Cell (Water Override)
			Color HalfRedPixel = new Color32(0, 0, 127, 255); // Floor Cell (Carpet Override)
			Color GreenHalfRBPixel = new Color32(127, 255, 127, 255); // Floor Cell (Grass Override)
			Color HalfWhitePixel = new Color32(127, 127, 127, 255); // Floor Cell (Bone Override)
			Color OrangePixel = new Color32(255, 127, 0, 255); // Floor Cell (Flesh Override)
			Color RedHalfGBPixel = new Color32(255, 127, 127, 255); // Floor Cell (ThickGoop Override)

			Color GreenPixel = new Color32(0, 255, 0, 255); // Damage Floor Cell

			Color RedPixel = new Color32(255, 0, 0, 255); // Pit Cell

			Color BlackPixel = new Color32(0, 0, 0, 255); // NULL Cell

			for (int X = 0; X < width; X++)
			{
				for (int Y = 0; Y < height; Y++)
				{
					if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(X, Y))
					{
						IntVector2 m_CellPosition = new IntVector2(X, Y);
						CellType? cellData = GameManager.Instance.Dungeon.data[m_CellPosition].type;
						CellData localDungeonData = GameManager.Instance.Dungeon.data[m_CellPosition];
						bool DamageCell = false;
						DiagonalWallType diagonalWallType = DiagonalWallType.NONE;
						if (localDungeonData != null)
						{
							DamageCell = localDungeonData.doesDamage;
							diagonalWallType = localDungeonData.diagonalWallType;
						}
						if (localDungeonData == null | !cellData.HasValue)
						{
							m_NewImage.SetPixel(X, Y, BlackPixel);
						}
						else if (cellData.Value == CellType.FLOOR)
						{
							if (DamageCell)
							{
								m_NewImage.SetPixel(X, Y, GreenPixel);
							}
							else
							{
								CellVisualData.CellFloorType overrideFloorType = localDungeonData.cellVisualData.floorType;
								if (overrideFloorType == CellVisualData.CellFloorType.Stone)
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Ice)
								{
									m_NewImage.SetPixel(X, Y, BlueHalfGreenPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Water)
								{
									m_NewImage.SetPixel(X, Y, HalfBluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Carpet)
								{
									m_NewImage.SetPixel(X, Y, HalfRedPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Grass)
								{
									m_NewImage.SetPixel(X, Y, GreenHalfRBPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Bone)
								{
									m_NewImage.SetPixel(X, Y, HalfWhitePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Flesh)
								{
									m_NewImage.SetPixel(X, Y, OrangePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.ThickGoop)
								{
									m_NewImage.SetPixel(X, Y, RedHalfGBPixel);
								}
								else
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
							}
						}
						else if (cellData.Value == CellType.WALL)
						{
							if (diagonalWallType == DiagonalWallType.NORTHEAST)
							{
								m_NewImage.SetPixel(X, Y, PinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.NORTHWEST)
							{
								m_NewImage.SetPixel(X, Y, YellowPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHEAST)
							{
								m_NewImage.SetPixel(X, Y, HalfPinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHWEST)
							{
								m_NewImage.SetPixel(X, Y, HalfYellowPixel);
							}
							else
							{
								m_NewImage.SetPixel(X, Y, WhitePixel);
							}
						}
						else if (cellData.Value == CellType.PIT)
						{
							m_NewImage.SetPixel(X, Y, RedPixel);
						}
					}
					else
					{
						m_NewImage.SetPixel(X, Y, BlackPixel);
					}
				}
			}

			m_NewImage.Apply();

			string basePath = "DumpedDungeonLayouts/";

			string fileName = (basePath + m_NewImage.name);
			if (string.IsNullOrEmpty(m_NewImage.name)) { fileName += ("DungeonLayout_" + Guid.NewGuid().ToString()); }

			fileName += "_Layout";

			string path = Path.Combine(ETGMod.ResourcesDirectory, fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar) + ".png");

			if (!File.Exists(path)) { Directory.GetParent(path).Create(); }

			File.WriteAllBytes(path, ImageConversion.EncodeToPNG(m_NewImage));
		}

		public static Texture2D DumpRoomAreaToTexture2D(RoomHandler room)
		{
			int width = room.area.dimensions.x;
			int height = room.area.dimensions.y;
			IntVector2 basePosition = room.area.basePosition;

			Texture2D m_NewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
			if (!string.IsNullOrEmpty(room.GetRoomName())) { m_NewImage.name = room.GetRoomName(); }

			Color WhitePixel = new Color32(255, 255, 255, 255); // Wall Cell
			Color PinkPixel = new Color32(255, 0, 255, 255); // Diagonal Wall Cell (North East)
			Color YellowPixel = new Color32(255, 255, 0, 255); // Diagonal Wall Cell (North West)
			Color HalfPinkPixel = new Color32(127, 0, 127, 255); // Diagonal Wall Cell (South East)
			Color HalfYellowPixel = new Color32(127, 127, 0, 255); // Diagonal Wall Cell (South West)

			Color BluePixel = new Color32(0, 0, 255, 255); // Floor Cell

			Color BlueHalfGreenPixel = new Color32(0, 127, 255, 255); // Floor Cell (Ice Override)
			Color HalfBluePixel = new Color32(0, 0, 127, 255); // Floor Cell (Water Override)
			Color HalfRedPixel = new Color32(0, 0, 127, 255); // Floor Cell (Carpet Override)
			Color GreenHalfRBPixel = new Color32(127, 255, 127, 255); // Floor Cell (Grass Override)
			Color HalfWhitePixel = new Color32(127, 127, 127, 255); // Floor Cell (Bone Override)
			Color OrangePixel = new Color32(255, 127, 0, 255); // Floor Cell (Flesh Override)
			Color RedHalfGBPixel = new Color32(255, 127, 127, 255); // Floor Cell (ThickGoop Override)

			Color GreenPixel = new Color32(0, 255, 0, 255); // Damage Floor Cell

			Color RedPixel = new Color32(255, 0, 0, 255); // Pit Cell

			Color BlackPixel = new Color32(0, 0, 0, 255); // NULL Cell

			for (int X = 0; X < width; X++)
			{
				for (int Y = 0; Y < height; Y++)
				{
					IntVector2 m_CellPosition = (new IntVector2(X, Y) + basePosition);
					if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(m_CellPosition.x, m_CellPosition.y))
					{
						CellType? cellData = GameManager.Instance.Dungeon.data[m_CellPosition].type;
						CellData localDungeonData = GameManager.Instance.Dungeon.data[m_CellPosition];
						bool DamageCell = false;
						DiagonalWallType diagonalWallType = DiagonalWallType.NONE;
						if (localDungeonData != null)
						{
							DamageCell = localDungeonData.doesDamage;
							diagonalWallType = localDungeonData.diagonalWallType;
						}
						if (localDungeonData == null | !cellData.HasValue)
						{
							m_NewImage.SetPixel(X, Y, BlackPixel);
						}
						else if (cellData.Value == CellType.FLOOR)
						{
							if (DamageCell)
							{
								m_NewImage.SetPixel(X, Y, GreenPixel);
							}
							else
							{
								CellVisualData.CellFloorType overrideFloorType = localDungeonData.cellVisualData.floorType;
								if (overrideFloorType == CellVisualData.CellFloorType.Stone)
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Ice)
								{
									m_NewImage.SetPixel(X, Y, BlueHalfGreenPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Water)
								{
									m_NewImage.SetPixel(X, Y, HalfBluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Carpet)
								{
									m_NewImage.SetPixel(X, Y, HalfRedPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Grass)
								{
									m_NewImage.SetPixel(X, Y, GreenHalfRBPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Bone)
								{
									m_NewImage.SetPixel(X, Y, HalfWhitePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Flesh)
								{
									m_NewImage.SetPixel(X, Y, OrangePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.ThickGoop)
								{
									m_NewImage.SetPixel(X, Y, RedHalfGBPixel);
								}
								else
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
							}
						}
						else if (cellData.Value == CellType.WALL)
						{
							if (diagonalWallType == DiagonalWallType.NORTHEAST)
							{
								m_NewImage.SetPixel(X, Y, PinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.NORTHWEST)
							{
								m_NewImage.SetPixel(X, Y, YellowPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHEAST)
							{
								m_NewImage.SetPixel(X, Y, HalfPinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHWEST)
							{
								m_NewImage.SetPixel(X, Y, HalfYellowPixel);
							}
							else
							{
								m_NewImage.SetPixel(X, Y, WhitePixel);
							}
						}
						else if (cellData.Value == CellType.PIT)
						{
							m_NewImage.SetPixel(X, Y, RedPixel);
						}
					}
					else
					{
						m_NewImage.SetPixel(X, Y, BlackPixel);
					}
				}
			}

			m_NewImage.Apply();

			return m_NewImage;
		}

		public static Texture2D LogDungeonToTexture2D()
		{
			int width = GameManager.Instance.Dungeon.data.Height;
			int height = GameManager.Instance.Dungeon.data.Width;
			// int width = 1000;
			// int height = 1000;

			Texture2D m_NewImage = new Texture2D(width, height, TextureFormat.RGBA32, false);
			m_NewImage.name = GameManager.Instance.Dungeon.gameObject.name;

			Color WhitePixel = new Color32(255, 255, 255, 255); // Wall Cell
			Color PinkPixel = new Color32(255, 0, 255, 255); // Diagonal Wall Cell (North East)
			Color YellowPixel = new Color32(255, 255, 0, 255); // Diagonal Wall Cell (North West)
			Color HalfPinkPixel = new Color32(127, 0, 127, 255); // Diagonal Wall Cell (South East)
			Color HalfYellowPixel = new Color32(127, 127, 0, 255); // Diagonal Wall Cell (South West)

			Color BluePixel = new Color32(0, 0, 255, 255); // Floor Cell

			Color BlueHalfGreenPixel = new Color32(0, 127, 255, 255); // Floor Cell (Ice Override)
			Color HalfBluePixel = new Color32(0, 0, 127, 255); // Floor Cell (Water Override)
			Color HalfRedPixel = new Color32(0, 0, 127, 255); // Floor Cell (Carpet Override)
			Color GreenHalfRBPixel = new Color32(127, 255, 127, 255); // Floor Cell (Grass Override)
			Color HalfWhitePixel = new Color32(127, 127, 127, 255); // Floor Cell (Bone Override)
			Color OrangePixel = new Color32(255, 127, 0, 255); // Floor Cell (Flesh Override)
			Color RedHalfGBPixel = new Color32(255, 127, 127, 255); // Floor Cell (ThickGoop Override)

			Color GreenPixel = new Color32(0, 255, 0, 255); // Damage Floor Cell

			Color RedPixel = new Color32(255, 0, 0, 255); // Pit Cell

			Color BlackPixel = new Color32(0, 0, 0, 255); // NULL Cell

			for (int X = 0; X < width; X++)
			{
				for (int Y = 0; Y < height; Y++)
				{
					if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(X, Y))
					{
						IntVector2 m_CellPosition = new IntVector2(X, Y);
						CellType? cellData = GameManager.Instance.Dungeon.data[m_CellPosition].type;
						CellData localDungeonData = GameManager.Instance.Dungeon.data[m_CellPosition];
						bool DamageCell = false;
						DiagonalWallType diagonalWallType = DiagonalWallType.NONE;
						if (localDungeonData != null)
						{
							DamageCell = localDungeonData.doesDamage;
							diagonalWallType = localDungeonData.diagonalWallType;
						}
						if (localDungeonData == null | !cellData.HasValue)
						{
							m_NewImage.SetPixel(X, Y, BlackPixel);
						}
						else if (cellData.Value == CellType.FLOOR)
						{
							if (DamageCell)
							{
								m_NewImage.SetPixel(X, Y, GreenPixel);
							}
							else
							{
								CellVisualData.CellFloorType overrideFloorType = localDungeonData.cellVisualData.floorType;
								if (overrideFloorType == CellVisualData.CellFloorType.Stone)
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Ice)
								{
									m_NewImage.SetPixel(X, Y, BlueHalfGreenPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Water)
								{
									m_NewImage.SetPixel(X, Y, HalfBluePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Carpet)
								{
									m_NewImage.SetPixel(X, Y, HalfRedPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Grass)
								{
									m_NewImage.SetPixel(X, Y, GreenHalfRBPixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Bone)
								{
									m_NewImage.SetPixel(X, Y, HalfWhitePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.Flesh)
								{
									m_NewImage.SetPixel(X, Y, OrangePixel);
								}
								else if (overrideFloorType == CellVisualData.CellFloorType.ThickGoop)
								{
									m_NewImage.SetPixel(X, Y, RedHalfGBPixel);
								}
								else
								{
									m_NewImage.SetPixel(X, Y, BluePixel);
								}
							}
						}
						else if (cellData.Value == CellType.WALL)
						{
							if (diagonalWallType == DiagonalWallType.NORTHEAST)
							{
								m_NewImage.SetPixel(X, Y, PinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.NORTHWEST)
							{
								m_NewImage.SetPixel(X, Y, YellowPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHEAST)
							{
								m_NewImage.SetPixel(X, Y, HalfPinkPixel);
							}
							else if (diagonalWallType == DiagonalWallType.SOUTHWEST)
							{
								m_NewImage.SetPixel(X, Y, HalfYellowPixel);
							}
							else
							{
								m_NewImage.SetPixel(X, Y, WhitePixel);
							}
						}
						else if (cellData.Value == CellType.PIT)
						{
							m_NewImage.SetPixel(X, Y, RedPixel);
						}
					}
					else
					{
						m_NewImage.SetPixel(X, Y, BlackPixel);
					}
				}
			}

			m_NewImage.Apply();

			return m_NewImage;
		}

		/*
        public static void LogPropertiesAndFields<T>(this T obj, string header = "")
        {
            Log(header);
            Log("=======================");
            if (obj == null) { Log("LogPropertiesAndFields: Null object"); return; }
            Type type = obj.GetType();
            Log($"Type: {type}");
            PropertyInfo[] pinfos = type.GetProperties();
            Log($"{typeof(T)} Properties: ");
            foreach (var pinfo in pinfos)
            {
                try
                {
                    var value = pinfo.GetValue(obj, null);
                    string valueString = value.ToString();
                    bool isList = obj?.GetType().GetGenericTypeDefinition() == typeof(List<>);
                    if (isList)
                    {
                        var list = value as List<object>;
                        valueString = $"List[{list.Count}]";
                        foreach (var subval in list)
                        {
                            valueString += "\n\t\t" + subval.ToString();
                        }
                    }
                    Log($"\t{pinfo.Name}: {valueString}");
                }
                catch { }
            }
            Log($"{typeof(T)} Fields: ");
            FieldInfo[] finfos = type.GetFields();
            foreach (var finfo in finfos)
            {
                Log($"\t{finfo.Name}: {finfo.GetValue(obj)}");
            }
        }
        */
	}
}