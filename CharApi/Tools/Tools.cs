using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using UnityEngine;

using Dungeonator;
using MonoMod.RuntimeDetour;

namespace Alexandria.CharacterAPI
{
    //Utility methods
    public static class ToolsCharApi
    {
        public static bool verbose = false;
        private static string defaultLog = Path.Combine(ETGMod.ResourcesDirectory, "customCharacterLog.txt");
        public static string modID = "CharApi";

        private static Dictionary<string, float> timers = new Dictionary<string, float>();
        private static string[] BundlePrereqs;

        public static Material[] SetOverrideMaterial (this PlayerController player, Material overrideMaterial)
        {
            FieldInfo _cachedOverrideMaterials = typeof(PlayerController).GetField("m_cachedOverrideMaterials", BindingFlags.NonPublic | BindingFlags.Instance);


            if ((_cachedOverrideMaterials.GetValue(player) as Material[]) == null)
            {
                _cachedOverrideMaterials.SetValue(player, new Material[3]);
            }
            for (int i = 0; i < (_cachedOverrideMaterials.GetValue(player) as Material[]).Length; i++)
            {
                (_cachedOverrideMaterials.GetValue(player) as Material[])[i] = null;
            }
            player.sprite.renderer.material = overrideMaterial;
            (_cachedOverrideMaterials.GetValue(player) as Material[])[0] = player.sprite.renderer.material;
            if (player.primaryHand && player.primaryHand.sprite)
            {
                (_cachedOverrideMaterials.GetValue(player) as Material[])[1] = player.primaryHand.SetOverrideShader(overrideMaterial.shader);
            }
            if (player.secondaryHand && player.secondaryHand.sprite)
            {
                (_cachedOverrideMaterials.GetValue(player) as Material[])[2] = player.secondaryHand.SetOverrideShader(overrideMaterial.shader);
            }
            return (_cachedOverrideMaterials.GetValue(player) as Material[]);
        }

        public static Texture2D Rotated(this Texture2D texture, bool clockwise = false)
        {
            Color32[] original = texture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = texture.width;
            int h = texture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        public static Texture2D Flipped(this Texture2D texture, bool horizontal = true)
        {
            int w = texture.width;
            int h = texture.height;

            Texture2D output = new Texture2D(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    output.SetPixel(i, j, texture.GetPixel(w - i - 1, j));
                }
            }
            output.Apply();
            return output;
        }
        

        public static T LoadAssetFromAnywhere<T>(string path) where T : UnityEngine.Object
        {
            if (BundlePrereqs == null)
            {
                Init();
            }
            T obj = null;
            foreach (string name in BundlePrereqs)
            {
                try
                {
                    obj = ResourceManager.LoadAssetBundle(name).LoadAsset<T>(path);
                }
                catch
                {
                }
                if (obj != null)
                {
                    break;
                }
            }
            return obj;
        }


        /// <summary>
        /// Builds and adds a new <see cref="dfAtlas.ItemInfo"/> to <paramref name="atlas"/> with the texture of <paramref name="tex"/> and the name of <paramref name="name"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to add the new <see cref="dfAtlas.ItemInfo"/> to.</param>
        /// <param name="tex">The texture of the new <see cref="dfAtlas.ItemInfo"/>.</param>
        /// <param name="name">The name of the new <see cref="dfAtlas.ItemInfo"/>. If <see langword="null"/>, it will default to <paramref name="tex"/>'s name.</param>
        /// <returns>The built <see cref="dfAtlas.ItemInfo"/>.</returns>
        public static dfAtlas.ItemInfo AddNewItemToAtlas(this dfAtlas atlas, Texture2D tex, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = tex.name;
            }
            if (atlas[name] != null)
            {
                return atlas[name];
            }
            dfAtlas.ItemInfo item = new dfAtlas.ItemInfo
            {
                border = new RectOffset(),
                deleted = false,
                name = name,
                region = atlas.FindFirstValidEmptySpace(new IntVector2(tex.width, tex.height)),
                rotated = false,
                sizeInPixels = new Vector2(tex.width, tex.height),
                texture = tex,
                textureGUID = name
            };
            int startPointX = Mathf.RoundToInt(item.region.x * atlas.Texture.width);
            int startPointY = Mathf.RoundToInt(item.region.y * atlas.Texture.height);
            for (int x = startPointX; x < Mathf.RoundToInt(item.region.xMax * atlas.Texture.width); x++)
            {
                for (int y = startPointY; y < Mathf.RoundToInt(item.region.yMax * atlas.Texture.height); y++)
                {
                    atlas.Texture.SetPixel(x, y, tex.GetPixel(x - startPointX, y - startPointY));
                }
            }
            atlas.Texture.Apply();
            atlas.AddItem(item);
            return item;
        }

        /// <summary>
        /// Gets the pixel regions of <paramref name="atlas"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to get the pixel regions from.</param>
        /// <returns>A list with all pixel regions in <paramref name="atlas"/></returns>
        public static List<RectInt> GetPixelRegions(this dfAtlas atlas)
        {
            return atlas.Items.Convert(delegate (dfAtlas.ItemInfo item)
            {
                return new RectInt(
                    Mathf.RoundToInt(item.region.x * atlas.Texture.width),
                    Mathf.RoundToInt(item.region.y * atlas.Texture.height),
                    Mathf.RoundToInt(item.region.width * atlas.Texture.width),
                    Mathf.RoundToInt(item.region.height * atlas.Texture.height));
            });
        }

        /// <summary>
        /// Converts a list of the type <typeparamref name="T"/> to a list of the type <typeparamref name="T2"/> using <paramref name="convertor"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="self"/> list.</typeparam>
        /// <typeparam name="T2">The type to convert the <paramref name="self"/> list to.</typeparam>
        /// <param name="self">The original list.</param>
        /// <param name="convertor">A delegate that converts an element of type <typeparamref name="T"/> to an element of a type <typeparamref name="T2"/>.</param>
        /// <returns>The converted list of type <typeparamref name="T2"/></returns>
        public static List<T2> Convert<T, T2>(this List<T> self, Func<T, T2> convertor)
        {
            List<T2> result = new List<T2>();
            foreach (T element in self)
            {
                result.Add(convertor(element));
            }
            return result;
        }




        /// <summary>
        /// Gets the first empty space in <paramref name="atlas"/> that has at least the size of <paramref name="pixelScale"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to find the empty space in.</param>
        /// <param name="pixelScale">The required size of the empty space.</param>
        /// <returns>The rect of the empty space divided by the atlas texture's size.</returns>
        public static Rect FindFirstValidEmptySpace(this dfAtlas atlas, IntVector2 pixelScale)
        {




            if (atlas == null || atlas.Texture == null || !atlas.Texture.IsReadable())
            {
                return new Rect(0f, 0f, 0f, 0f);
            }
            Vector2Int point = new Vector2Int(0, 0);
            int pointIndex = -1;
            List<RectInt> rects = atlas.GetPixelRegions();


            while (true)
            {
                bool shouldContinue = false;
                foreach (RectInt rint in rects)
                {

                    if (rint.DoseOverlap(new RectInt(point, pixelScale.ToVector2Int())))
                    {
                        shouldContinue = true;
                        pointIndex++;
                        if (pointIndex >= rects.Count)
                        {
                            return new Rect(0f, 0f, 0f, 0f);
                        }
                        point = rects[pointIndex].max + Vector2Int.one;
                        if (point.x > atlas.Texture.width || point.y > atlas.Texture.height)
                        {
                            atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                        }
                        break;
                    }
                    bool shouldBreak = false;
                    foreach (RectInt rint2 in rects)
                    {
                        RectInt currentRect = new RectInt(point, pixelScale.ToVector2Int());
                        if (rint2.x < currentRect.x || rint2.y < currentRect.y)
                        {
                            continue;
                        }
                        else
                        {
                            if (currentRect.DoseOverlap(rint2))
                            {
                                shouldContinue = true;
                                shouldBreak = true;
                                pointIndex++;
                                if (pointIndex >= rects.Count)
                                {
                                    return new Rect(0f, 0f, 0f, 0f);
                                }
                                point = rects[pointIndex].max + Vector2Int.one;
                                if (point.x > atlas.Texture.width || point.y > atlas.Texture.height)
                                {
                                    atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                                }
                                break;
                            }
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                }
                if (shouldContinue)
                {
                    continue;
                }
                RectInt currentRect2 = new RectInt(point, pixelScale.ToVector2Int());
                if (currentRect2.xMax > atlas.Texture.width || currentRect2.yMax > atlas.Texture.height)
                {
                    atlas.ResizeAtlas(new IntVector2(atlas.Texture.width * 2, atlas.Texture.height * 2));
                }
                break;
            }
            RectInt currentRect3 = new RectInt(point, pixelScale.ToVector2Int());
            Rect rect = new Rect((float)currentRect3.x / atlas.Texture.width, (float)currentRect3.y / atlas.Texture.height, (float)currentRect3.width / atlas.Texture.width, (float)currentRect3.height / atlas.Texture.height);
            return rect;
        }

        /// <summary>
		/// Resizes <paramref name="atlas"/> and all of it's <see cref="dfAtlas.ItemInfo"/>s.
		/// </summary>
		/// <param name="atlas">The <see cref="dfAtlas"/> to resize/</param>
		/// <param name="newDimensions"><paramref name="atlas"/>'s new size.</param>
		public static void ResizeAtlas(this dfAtlas atlas, IntVector2 newDimensions)
        {
            Texture2D tex = atlas.Texture;
            if (!tex.IsReadable())
            {
                return;
            }
            if (tex.width == newDimensions.x && tex.height == newDimensions.y)
            {
                return;
            }
            foreach (dfAtlas.ItemInfo item in atlas.Items)
            {
                if (item.region != null)
                {
                    item.region.x = (item.region.x * tex.width) / newDimensions.x;
                    item.region.y = (item.region.y * tex.height) / newDimensions.y;
                    item.region.width = (item.region.width * tex.width) / newDimensions.x;
                    item.region.height = (item.region.height * tex.height) / newDimensions.y;
                }
            }
            tex.ResizeBetter(newDimensions.x, newDimensions.y);
            atlas.Material.SetTexture("_MainTex", tex);
        }


        /// <summary>
		/// Resizes <paramref name="tex"/> without it losing it's pixel information.
		/// </summary>
		/// <param name="tex">The <see cref="Texture2D"/> to resize.</param>
		/// <param name="width">The <paramref name="tex"/>'s new width.</param>
		/// <param name="height">The <paramref name="tex"/>'s new height.</param>
		/// <returns></returns>
		public static bool ResizeBetter(this Texture2D tex, int width, int height, bool center = false)
        {
            if (tex.IsReadable())
            {
                Color[][] pixels = new Color[Math.Min(tex.width, width)][];


                for (int x = 0; x < Math.Min(tex.width, width); x++)
                {
                    for (int y = 0; y < Math.Min(tex.height, height); y++)
                    {
                        if (pixels[x] == null)
                        {
                            pixels[x] = new Color[Math.Min(tex.height, height)];
                        }
                        pixels[x][y] = tex.GetPixel(x, y);
                    }
                }

                int value = 2;
                if (center)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }

                bool result = tex.Resize(width, height);
                for (int x = value; x < tex.width - value; x++)
                {
                    for (int y = value; y < tex.height - value; y++)
                    {
                        bool isInOrigTex = false;
                        if (x - value < pixels.Length)
                        {
                            if (y - value < pixels[x - value].Length)
                            {
                                isInOrigTex = true;
                                tex.SetPixel(x, y, pixels[x - value][y - value]);
                            }
                        }
                        if (!isInOrigTex)
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }

                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {

                        if (tex.GetPixel(x, y) == new Color32(205, 205, 205, 205))
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }

                    }
                }

                tex.Apply();
                return result;
            }
            return tex.Resize(width, height);
        }

        /// <summary>
		/// Converts <paramref name="vector"/> to a <see cref="Vector2Int"/>.
		/// </summary>
		/// <param name="vector">The <see cref="IntVector2"/> to convert.</param>
		/// <returns><paramref name="vector"/> converted to <see cref="Vector2Int"/>.</returns>
		public static Vector2Int ToVector2Int(this IntVector2 vector)
        {
            return new Vector2Int(vector.x, vector.y);
        }

        public static void Init()
        {
            if (BundlePrereqs == null) 
            {
                BundlePrereqs = new string[]
                {
                    "brave_resources_001",
                    "dungeon_scene_001",
                    "encounters_base_001",
                    "enemies_base_001",
                    "flows_base_001",
                    "foyer_001",
                    "foyer_002",
                    "foyer_003",
                    "shared_auto_001",
                    "shared_auto_002",
                    "shared_base_001",
                    "dungeons/base_bullethell",
                    "dungeons/base_castle",
                    "dungeons/base_catacombs",
                    "dungeons/base_cathedral",
                    "dungeons/base_forge",
                    "dungeons/base_foyer",
                    "dungeons/base_gungeon",
                    "dungeons/base_mines",
                    "dungeons/base_nakatomi",
                    "dungeons/base_resourcefulrat",
                    "dungeons/base_sewer",
                    "dungeons/base_tutorial",
                    "dungeons/finalscenario_bullet",
                    "dungeons/finalscenario_convict",
                    "dungeons/finalscenario_coop",
                    "dungeons/finalscenario_guide",
                    "dungeons/finalscenario_pilot",
                    "dungeons/finalscenario_robot",
                    "dungeons/finalscenario_soldier"
                };  
            }

            if (File.Exists(defaultLog)) File.Delete(defaultLog);
        }

        public static void Print<T>(T obj, string color = "FFFFFF", bool force = false)
        {
            if (verbose || force)
            {
                string[] lines = obj.ToString().Split('\n');
                foreach (var line in lines)
                    LogToConsole($"<color=#{color}>[{modID}] {line}</color>");
            }

            Log(obj.ToString());
        }


        public static bool DoseOverlap(this RectInt rect1, RectInt rect2)
        {
            return rect2.xMax > rect1.xMin && rect2.xMin < rect1.xMax && rect2.yMax > rect1.yMin && rect2.yMin < rect1.yMax;
        }


        public static void PrintRaw<T>(T obj, bool force = false)
        {
            if (verbose || force)
                LogToConsole(obj.ToString());

            Log(obj.ToString());
        }

        public static void PrintError<T>(T obj, string color = "FF0000")
        {
            string[] lines = obj.ToString().Split('\n');
            foreach (var line in lines)
                LogToConsole($"<color=#{color}>[{modID}] {line}</color>");

            Log(obj.ToString());
        }

        public static void PrintException(Exception e, string color = "FF0000")
        {
            string message = e.Message + "\n" + e.StackTrace;
            {
                string[] lines = message.Split('\n');
                foreach (var line in lines)
                    LogToConsole($"<color=#{color}>[{modID}] {line}</color>");
            }

            Log(e.Message);
            Log("\t" + e.StackTrace);
        }

        public static void Log<T>(T obj)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(ETGMod.ResourcesDirectory, defaultLog), true))
            {
                writer.WriteLine(obj.ToString());
            }
        }

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

            Log(space + obj.name + "...");
            foreach (var comp in obj.GetComponents<Component>())
            {
                Log(space + "    -" + comp.GetType());
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

        public static void ExportTexture(Texture texture, string folder = "", string name = "")
        {
            string path = Path.Combine(ETGMod.ResourcesDirectory, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (string.IsNullOrEmpty(name))
            {
                File.WriteAllBytes(Path.Combine(path, texture.name + ".png"), ((Texture2D)texture).EncodeToPNG());
            }
            else
            {
                File.WriteAllBytes(Path.Combine(path, name + ".png"), ((Texture2D)texture).EncodeToPNG());
            }
           
        }

        public static T GetEnumValue<T>(string val) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), val.ToUpper());
        }

        public static void LogPropertiesAndFields<T>(T obj, string header = "")
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

        public static void StartTimer(string name)
        {
            string key = name.ToLower();
            if (timers.ContainsKey(key))
            {
                PrintError($"Timer {name} already exists.");
                return;
            }
            timers.Add(key, Time.realtimeSinceStartup);
        }

        public static void StopTimerAndReport(string name)
        {
            string key = name.ToLower();
            if (!timers.ContainsKey(key))
            {
                ToolsCharApi.PrintError($"Could not stop timer {name}, no such timer exists");
                return;
            }
            float timerStart = timers[key];
            int elapsed = (int)((Time.realtimeSinceStartup - timerStart) * 1000);
            timers.Remove(key);
            ToolsCharApi.Print($"{name} finished in " + elapsed + "ms");
        }
    }
}
