using Dungeonator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class DebugUtility
    {
		public static void Init()
		{
			bool flag = File.Exists(DebugUtility.defaultLog);
			if (flag)
			{
				File.Delete(DebugUtility.defaultLog);
			}
		}


		public static void Print<T>(T obj, string color = "FFFFFF", bool force = false)
		{
			bool flag = DebugUtility.verbose || force;
			if (flag)
			{
				string[] array = obj.ToString().Split(new char[]
				{
					'\n'
				});
				foreach (string text in array)
				{
					DebugUtility.LogToConsole(string.Concat(new string[]
					{
						"<color=#",
						color,
						">[",
						DebugUtility.modID,
						"] ",
						text,
						"</color>"
					}));
				}
			}
			DebugUtility.Log<string>(obj.ToString());
		}


		public static void PrintRaw<T>(T obj, bool force = false)
		{
			bool flag = DebugUtility.verbose || force;
			if (flag)
			{
				DebugUtility.LogToConsole(obj.ToString());
			}
			DebugUtility.Log<string>(obj.ToString());
		}


		public static void PrintError<T>(T obj, string color = "FF0000")
		{
			string[] array = obj.ToString().Split(new char[]
			{
				'\n'
			});
			foreach (string text in array)
			{
				DebugUtility.LogToConsole(string.Concat(new string[]
				{
					"<color=#",
					color,
					">[",
					DebugUtility.modID,
					"] ",
					text,
					"</color>"
				}));
			}
			DebugUtility.Log<string>(obj.ToString());
		}


		public static void PrintException(Exception e, string color = "FF0000")
		{
			string text = e.Message + "\n" + e.StackTrace;
			string[] array = text.Split(new char[]
			{
				'\n'
			});
			foreach (string text2 in array)
			{
				DebugUtility.LogToConsole(string.Concat(new string[]
				{
					"<color=#",
					color,
					">[",
					DebugUtility.modID,
					"] ",
					text2,
					"</color>"
				}));
			}
			DebugUtility.Log<string>(e.Message);
			DebugUtility.Log<string>("\t" + e.StackTrace);
		}


		public static void Log<T>(T obj)
		{
			using (StreamWriter streamWriter = new StreamWriter(Path.Combine(ETGMod.ResourcesDirectory, DebugUtility.defaultLog), true))
			{
				streamWriter.WriteLine(obj.ToString());
			}
		}


		public static void Log<T>(T obj, string fileName)
		{
			bool flag = !DebugUtility.verbose;
			if (!flag)
			{
				using (StreamWriter streamWriter = new StreamWriter(Path.Combine(ETGMod.ResourcesDirectory, fileName), true))
				{
					streamWriter.WriteLine(obj.ToString());
				}
			}
		}


		public static void LogToConsole(string message)
		{
			message.Replace("\t", "    ");
			ETGModConsole.Log(message, false);
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


		private static void BreakdownComponentsInternal(this GameObject obj, int lvl = 0)
		{
			string text = "";
			for (int i = 0; i < lvl; i++)
			{
				text += "\t";
			}
			DebugUtility.Log<string>(text + obj.name + "...");
			foreach (Component component in obj.GetComponents<Component>())
			{
				string str = text;
				string str2 = "    -";
				Type type = component.GetType();
				DebugUtility.Log<string>(str + str2 + ((type != null) ? type.ToString() : null));
			}
			foreach (Transform transform in obj.GetComponentsInChildren<Transform>())
			{
				bool flag = transform != obj.transform;
				if (flag)
				{
					transform.gameObject.BreakdownComponentsInternal(lvl + 1);
				}
			}
		}


		public static void BreakdownComponents(this GameObject obj)
		{
			obj.BreakdownComponentsInternal(0);
		}


		public static void ExportTexture(Texture texture, string folder = "")
		{
			string text = Path.Combine(ETGMod.ResourcesDirectory, folder);
			bool flag = !Directory.Exists(text);
			if (flag)
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllBytes(Path.Combine(text, texture.name + DateTime.Now.Ticks.ToString() + ".png"), ((Texture2D)texture).EncodeToPNG());
		}


		public static T GetEnumValue<T>(string val) where T : Enum
		{
			return (T)((object)Enum.Parse(typeof(T), val.ToUpper()));
		}


		public static void LogPropertiesAndFields<T>(T obj, string header = "")
		{
			DebugUtility.Log<string>(header);
			DebugUtility.Log<string>("=======================");
			bool flag = obj == null;
			if (flag)
			{
				DebugUtility.Log<string>("LogPropertiesAndFields: Null object");
			}
			else
			{
				Type type = obj.GetType();
				DebugUtility.Log<string>(string.Format("Type: {0}", type));
				PropertyInfo[] properties = type.GetProperties();
				DebugUtility.Log<string>(string.Format("{0} Properties: ", typeof(T)));
				foreach (PropertyInfo propertyInfo in properties)
				{
					try
					{
						object value = propertyInfo.GetValue(obj, null);
						string text = value.ToString();
						bool flag2 = ((obj != null) ? obj.GetType().GetGenericTypeDefinition() : null) == typeof(List<>);
						bool flag3 = flag2;
						if (flag3)
						{
							List<object> list = value as List<object>;
							text = string.Format("List[{0}]", list.Count);
							foreach (object obj2 in list)
							{
								text = text + "\n\t\t" + obj2.ToString();
							}
						}
						DebugUtility.Log<string>("\t" + propertyInfo.Name + ": " + text);
					}
					catch
					{
					}
				}
				DebugUtility.Log<string>(string.Format("{0} Fields: ", typeof(T)));
				FieldInfo[] fields = type.GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					DebugUtility.Log<string>(string.Format("\t{0}: {1}", fieldInfo.Name, fieldInfo.GetValue(obj)));
				}
			}
		}


		public static void StartTimer(string name)
		{
			string key = name.ToLower();
			bool flag = DebugUtility.timers.ContainsKey(key);
			if (flag)
			{
				DebugUtility.PrintError<string>("Timer " + name + " already exists.", "FF0000");
			}
			else
			{
				DebugUtility.timers.Add(key, Time.realtimeSinceStartup);
			}
		}


		public static void StopTimerAndReport(string name)
		{
			string key = name.ToLower();
			bool flag = !DebugUtility.timers.ContainsKey(key);
			if (flag)
			{
				DebugUtility.PrintError<string>("Could not stop timer " + name + ", no such timer exists", "FF0000");
			}
			else
			{
				float num = DebugUtility.timers[key];
				int num2 = (int)((Time.realtimeSinceStartup - num) * 1000f);
				DebugUtility.timers.Remove(key);
				DebugUtility.Print<string>(name + " finished in " + num2.ToString() + "ms", "FFFFFF", false);
			}
		}

		// Token: 0x0400003F RID: 63
		public static bool verbose = false;

		// Token: 0x04000040 RID: 64
		private static string defaultLog = Path.Combine(ETGMod.ResourcesDirectory, "defaultLog.txt");

		// Token: 0x04000041 RID: 65
		public static string modID = "Alexandria";

		// Token: 0x04000042 RID: 66
		private static Dictionary<string, float> timers = new Dictionary<string, float>();
	}
}

