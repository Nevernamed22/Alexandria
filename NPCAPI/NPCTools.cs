using System;
using System.Collections.Generic;
using UnityEngine;
using Alexandria.CharacterAPI;

namespace Alexandria.NPCAPI
{
    public static class NpcTools
    {
        [Obsolete("This method has been obsoleted since NPCHooks.Init() is called directly in Module.cs; it exists for backwards compatability only.", false)]
        public static void Init()
        {
            // NPCHooks.Init();
        }

        public static void AddComplex(this StringDBTable stringdb, string key, string value)
        {
            StringTableManager.ComplexStringCollection stringCollection = (!stringdb.ContainsKey(key))
                ? new StringTableManager.ComplexStringCollection()
                : stringCollection = (StringTableManager.ComplexStringCollection)stringdb[key];
            stringCollection.AddString(value, 1f);
            stringdb[key] = stringCollection;
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static dfAtlas.ItemInfo AddNewItemToAtlas(this dfAtlas atlas, Texture2D tex, string name = null)
        {
            return ToolsCharApi.AddNewItemToAtlas(atlas, tex, name);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static List<RectInt> GetPixelRegions(this dfAtlas atlas)
        {
            return ToolsCharApi.GetPixelRegions(atlas);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static List<T2> Convert<T, T2>(this List<T> self, Func<T, T2> convertor)
        {
            return ToolsCharApi.Convert(self, convertor);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static Rect FindFirstValidEmptySpace(this dfAtlas atlas, IntVector2 pixelScale)
        {
            return ToolsCharApi.FindFirstValidEmptySpace(atlas, pixelScale);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
		public static void ResizeAtlas(this dfAtlas atlas, IntVector2 newDimensions)
        {
            ToolsCharApi.ResizeAtlas(atlas, newDimensions);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static bool ResizeBetter(this Texture2D tex, int width, int height, bool center = false)
        {
            return ToolsCharApi.ResizeBetter(tex, width, height, center);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
		public static Vector2Int ToVector2Int(this IntVector2 vector)
        {
            return new Vector2Int(vector.x, vector.y);
        }

        [Obsolete("This method has been obsoleted by the equivalent in ToolsCharApi; it exists for backwards compatability only.", false)]
        public static bool DoseOverlap(this RectInt rect1, RectInt rect2)
        {
            return rect2.xMax > rect1.xMin && rect2.xMin < rect1.xMax && rect2.yMax > rect1.yMin && rect2.yMin < rect1.yMax;
        }
    }
}
