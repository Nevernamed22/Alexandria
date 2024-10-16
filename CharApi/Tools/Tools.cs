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
        internal const float PIXELS_PER_UNIT = 16f;

        public static bool verbose = false;
        private static string defaultLog = Path.Combine(ETGMod.ResourcesDirectory, "customCharacterLog.txt");
        public static string modID = "CharAPI";
        public static bool EnableDebugLogging = false;

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

        private static dfAtlas.ItemInfo AddNewItemToAtlasInternal(this dfAtlas atlas, Texture2D tex, string name, int texX, int texY, int texW, int texH, int offX, int offY, int regW, int regH)
        {
            if (string.IsNullOrEmpty(name))
                name = tex.name;
            if (atlas[name] != null)
                return atlas[name];
            dfAtlas.ItemInfo item = new dfAtlas.ItemInfo
            {
                border = new RectOffset(),
                name = name,
                region = atlas.FindFirstValidEmptySpace(new IntVector2(regW, regH)),
                sizeInPixels = new Vector2(regW, regH),
                texture = tex,
                textureGUID = name
            };
            int startPointX = offX + Mathf.RoundToInt(item.region.x * atlas.Texture.width);
            int startPointY = offY + Mathf.RoundToInt(item.region.y * atlas.Texture.height);
            atlas.Texture.SetPixels(startPointX, startPointY, texW, texH, tex.GetPixels(texX, texY, texW, texH));

            atlas.Texture.Apply();
            //NOTE: circumventing a hopefully unnecessary call to atlas.RebuildIndexes(); change back to atlas.AddItem() if problems arise
            atlas.items.Add(item);
            atlas.map[item.name] = item;
            return item;
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
            return atlas.AddNewItemToAtlasInternal(tex, name, 0, 0, tex.width, tex.height, 0, 0, tex.width, tex.height);
        }

        /// <summary>
        /// Builds and adds a new <see cref="dfAtlas.ItemInfo"/> to <paramref name="atlas"/> with the sprite of <paramref name="def"/> and the name of <paramref name="name"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to add the new <see cref="dfAtlas.ItemInfo"/> to.</param>
        /// <param name="def">The sprite of the new <see cref="dfAtlas.ItemInfo"/>.</param>
        /// <param name="name">The name of the new <see cref="dfAtlas.ItemInfo"/>. If <see langword="null"/>, it will default to <paramref name="def"/>'s name.</param>
        /// <returns>The built <see cref="dfAtlas.ItemInfo"/>.</returns>
        public static dfAtlas.ItemInfo AddNewItemToAtlas(this dfAtlas atlas, tk2dSpriteDefinition def, string name = null)
        {
            if (string.IsNullOrEmpty(name))
                name = def.name;
            if (atlas[name] != null)
              return atlas[name];

            Texture2D tex = def.material.mainTexture as Texture2D;
            IntVector2 origin  = (new Vector2(tex.width * def.uvs[0].x, tex.height * def.uvs[0].y)).ToIntVector2();
            IntVector2 dims = (new Vector2(tex.width * def.uvs[3].x, tex.height * def.uvs[3].y)).ToIntVector2() - origin;
            IntVector2 offset = (PIXELS_PER_UNIT * def.position0.XY()).ToIntVector2();
            IntVector2 region = (PIXELS_PER_UNIT * def.untrimmedBoundsDataExtents.XY()).ToIntVector2();
            return atlas.AddNewItemToAtlasInternal(tex, name, origin.x, origin.y, dims.x, dims.y, offset.x, offset.y, region.x, region.y);
        }

        /// <summary>
        /// Builds and adds a new <see cref="dfAtlas.ItemInfo"/> to <paramref name="atlas"/> with the specified <paramref name="resourcePath"/> and the name of <paramref name="name"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to add the new <see cref="dfAtlas.ItemInfo"/> to.</param>
        /// <param name="resourcePath">The resource path for the image of the new <see cref="dfAtlas.ItemInfo"/>.</param>
        /// <param name="name">The name of the new <see cref="dfAtlas.ItemInfo"/>. If <see langword="null"/>, it will default to <paramref name="tex"/>'s name.</param>
        /// <returns>The built <see cref="dfAtlas.ItemInfo"/>.</returns>
        public static dfAtlas.ItemInfo AddNewItemToAtlas(this dfAtlas atlas, string resourcePath, string name = null, Assembly assembly = null)
        {
            Texture2D tex = ItemAPI.ResourceExtractor.GetTextureFromResource(resourcePath, assembly ?? Assembly.GetCallingAssembly());
            return atlas.AddNewItemToAtlasInternal(tex, name, 0, 0, tex.width, tex.height, 0, 0, tex.width, tex.height);
        }

        /// <summary>Convenience function for adding a texture to the UI atlas.</summary>
        public static dfAtlas.ItemInfo AddUISprite(Texture2D tex, string name = null)
            => GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas.AddNewItemToAtlas(tex, name);

        /// <summary>Convenience function for adding a sprite to the UI atlas.</summary>
        public static dfAtlas.ItemInfo AddUISprite(tk2dSpriteDefinition def, string name = null)
            => GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas.AddNewItemToAtlas(def, name);

        /// <summary>Convenience function for adding an image resource to the UI atlas.</summary>
        public static dfAtlas.ItemInfo AddUISprite(string resourcePath, string name = null)
            => GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas.AddNewItemToAtlas(resourcePath, name, Assembly.GetCallingAssembly());

        private static readonly Dictionary<dfAtlas, List<RectInt>> _CachedPixelRegions = new();
        /// <summary>
        /// Gets the pixel regions of <paramref name="atlas"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to get the pixel regions from.</param>
        /// <returns>A list with all pixel regions in <paramref name="atlas"/></returns>
        public static List<RectInt> GetPixelRegions(this dfAtlas atlas)
        {
            if (!_CachedPixelRegions.TryGetValue(atlas, out List<RectInt> rects))
                rects = _CachedPixelRegions[atlas] = new List<RectInt>(atlas.Items.Count);
            for (int i = rects.Count; i < atlas.Items.Count; ++i)
            {
                dfAtlas.ItemInfo item = atlas.Items[i];
                rects.Add(new RectInt(
                    Mathf.RoundToInt(item.region.x * atlas.Texture.width),
                    Mathf.RoundToInt(item.region.y * atlas.Texture.height),
                    Mathf.RoundToInt(item.region.width * atlas.Texture.width),
                    Mathf.RoundToInt(item.region.height * atlas.Texture.height)));
            }
            return rects;
        }

        private static readonly Dictionary<dfAtlas, LinkedList<RectInt>> _CachedFreeRegions = new();
        /// <summary>
        /// Gets the free regions of <paramref name="atlas"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to get the free regions from.</param>
        /// <returns>A list with all free regions in <paramref name="atlas"/></returns>
        private static LinkedList<RectInt> GetFreeRegions(this dfAtlas atlas)
        {
            if (_CachedFreeRegions.TryGetValue(atlas, out LinkedList<RectInt> rects))
                return rects;
            int texW = atlas.Texture.width;
            int texH = atlas.Texture.height;
            int maxY = 0;
            for (int i = 0; i < atlas.Items.Count; ++i)
            {
                dfAtlas.ItemInfo item = atlas.Items[i];
                maxY = Mathf.Max(maxY, Mathf.RoundToInt(texH * (item.region.height + item.region.y)));
            }
            rects = _CachedFreeRegions[atlas] = new LinkedList<RectInt>();
            rects.AddLast(new RectInt(0, maxY + 1, texW, texH - (maxY + 1)));
            return rects;
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

        private static readonly Rect _NullRect = new Rect(0f, 0f, 0f, 0f);

        //WARNING: there might be off-by-one errors littered throughout this function, so keep an eye on it
        /// <summary>
        /// Gets the first empty space in <paramref name="atlas"/> that has at least the size of <paramref name="pixelScale"/>.
        /// </summary>
        /// <param name="atlas">The <see cref="dfAtlas"/> to find the empty space in.</param>
        /// <param name="pixelScale">The required size of the empty space.</param>
        /// <returns>The rect of the empty space divided by the atlas texture's size.</returns>
        public static Rect FindFirstValidEmptySpace(this dfAtlas atlas, IntVector2 pixelScale)
        {
            int tw = atlas.Texture.width;
            int th = atlas.Texture.height;
            int neededWidth = pixelScale.x;
            int neededHeight = pixelScale.y;
            LinkedList<RectInt> freeRects = atlas.GetFreeRegions();
            LinkedListNode<RectInt> bestNode = null;
            int smallestWidth = tw;
            // iterate starting from the last node added since we might be adding a lot of similarly sized sprites
            for (LinkedListNode<RectInt> node = freeRects.Last; node != null; node = node.Previous)
            {
                RectInt freeRect = node.Value;
                if (freeRect.width < neededWidth || freeRect.height < neededHeight)
                    continue;
                if (freeRect.width > smallestWidth)
                    continue;
                smallestWidth = freeRect.width;
                bestNode = node;
                if (smallestWidth == neededWidth)
                    break; // literally cannot do better, so stop looking now
            }
            if (bestNode == null) // resize the atlas, recompute free rectangles, and retry finding an empty space
            {
                atlas.ResizeAtlas(new IntVector2(tw * 2, th * 2));
                freeRects.Clear();
                freeRects.AddLast(new RectInt(tw, 0, tw, th * 2)); // tall vertical rectangle on the right half
                freeRects.AddLast(new RectInt(0, th, tw, th)); // square in the bottom left corner
                return atlas.FindFirstValidEmptySpace(pixelScale);
            }
            // split the free rectangle into two, preferring tall rectangles to wide rectangles
            RectInt bestFreeRect = bestNode.Value;
            freeRects.Remove(bestNode);
            RectInt currentRect = new RectInt(bestFreeRect.x, bestFreeRect.y, neededWidth, neededHeight);
            if (bestFreeRect.width > neededWidth)
                freeRects.AddLast(new RectInt(currentRect.xMax + 1, bestFreeRect.yMin, bestFreeRect.width - neededWidth, bestFreeRect.height));
            if (bestFreeRect.height > neededHeight)
                freeRects.AddLast(new RectInt(bestFreeRect.xMin, currentRect.yMax + 1, neededWidth, bestFreeRect.height - neededHeight));
            return new Rect((float)currentRect.x / tw, (float)currentRect.y / th, (float)currentRect.width / tw, (float)currentRect.height / th);
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
            if (!tex.IsReadable())
              return tex.Resize(width, height);

            int value = center ? 1 : 0;
            Texture2D tempTex = new Texture2D(width, height);
            tempTex.SetPixels(value, value, tex.width - 2 * value, tex.height - 2 * value, tex.GetPixels());
            bool result = tex.Resize(width, height);
            tex.SetPixels(tempTex.GetPixels());
            tex.Apply();
            return result;
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
            Print(header, "FFFFFF", true);
            Print("=======================");
            if (obj == null) { Print("LogPropertiesAndFields: Null object", "FFFFFF", true); return; }
            Type type = obj.GetType();
            Print($"Type: {type}", "FFFFFF", true);
            PropertyInfo[] pinfos = type.GetProperties();
            Print($"{typeof(T)} Properties: ", "FFFFFF", true);
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
                    Print($"\t{pinfo.Name}: {valueString}", "FFFFFF", true);
                }
                catch { }
            }
            Print($"{typeof(T)} Fields: ", "FFFFFF", true);
            FieldInfo[] finfos = type.GetFields();
            foreach (var finfo in finfos)
            {
                Print($"\t{finfo.Name}: {finfo.GetValue(obj)}", "FFFFFF", true);
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
