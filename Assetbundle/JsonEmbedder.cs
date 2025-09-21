using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria.Assetbundle
{
    public static class JsonEmbedder
    {
        /// <summary>
        /// Processes every json in the selected folder and adds the appropriate hand json data to the sprites in your sprite collection.
        /// </summary>
        /// <param name="asmb">your Assembly. Get this via Assembly.GetExecutingAssembly(), I think.</param>
        /// <param name="data">The sprite collection of your guns.</param>
        /// <param name="path">The direct filepath to all of your *embedded gun jsons.*.</param>
        public static void EmbedJsonDataFromAssembly(Assembly asmb, tk2dSpriteCollectionData data, string path)
        {
            if (asmb == null || data == null || data.spriteDefinitions == null || data.Count == 0)
                return;

            path = path.Replace("/", ".").Replace("\\", ".");
            if (!path.EndsWith("."))
                path += ".";

            List<string> list5 = new List<string>();
            string[] manifestResourceNames = asmb.GetManifestResourceNames();

            foreach (string text in manifestResourceNames)
            {
                if (!text.StartsWith(path) || text.Length <= path.Length)
                    continue;

                string[] array2 = text.Substring(path.LastIndexOf(".") + 1).Split(new char[] { '.' });
                string text2 = array2.Last().ToLowerInvariant();
                if (text2 != "json" && text2 != "jtk2d")
                    continue;

                string collection = array2[array2.Count() - 2];
                if (collection == null)
                    continue;

                int spriteIdByName = data.GetSpriteIdByName(collection, -1);
                if (spriteIdByName <= -1)
                    continue;

                using (Stream manifestResourceStream2 = asmb.GetManifestResourceStream(text))
                {
                    AssetSpriteData assetSpriteData = default(AssetSpriteData);
                    try
                    {
                        assetSpriteData = JSONHelper.ReadJSON<AssetSpriteData>(manifestResourceStream2);
                    }
                    catch
                    {
                        ETGModConsole.Log("Error: invalid json at project path " + text, false);
                        continue;
                    }
                    data.SetAttachPoints(spriteIdByName, assetSpriteData.attachPoints);
                }
            }
        }
    }

}
