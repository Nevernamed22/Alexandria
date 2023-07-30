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
            if (asmb != null)
            {
                path = path.Replace("/", ".").Replace("\\", ".");
                if (!path.EndsWith("."))
                {
                    path += ".";
                }

                tk2dSpriteCollectionData tk2dSpriteCollectionData = data;
                List<string> list5 = new List<string>();
                string[] manifestResourceNames = asmb.GetManifestResourceNames();

                foreach (string text in manifestResourceNames)
                {
                    if (text.StartsWith(path) && text.Length > path.Length)
                    {

                        string[] array2 = text.Substring(path.LastIndexOf(".") + 1).Split(new char[]
                        {
                            '.'
                        });


                        string text2 = array2.Last();
                        if (text2.ToLowerInvariant() == "json" || text2.ToLowerInvariant() == "jtk2d")
                        {
                            list5.Add(text);
                        }
                    }
                }
                foreach (string text5 in list5)
                {
                    string[] array5 = text5.Substring(path.LastIndexOf(".") + 1).Split(new char[]
                    {
                        '.'
                    });


                    string collection = array5[array5.Count() - 2];


                    if (collection != null)
                    {
                        if (((tk2dSpriteCollectionData != null) ? tk2dSpriteCollectionData.spriteDefinitions : null) != null && tk2dSpriteCollectionData.Count > 0)
                        {
                            int spriteIdByName = tk2dSpriteCollectionData.GetSpriteIdByName(collection, -1);

                            if (spriteIdByName > -1)
                            {
                                using (Stream manifestResourceStream2 = asmb.GetManifestResourceStream(text5))
                                {
                                    AssetSpriteData assetSpriteData = default(AssetSpriteData);
                                    try
                                    {
                                        assetSpriteData = JSONHelper.ReadJSON<AssetSpriteData>(manifestResourceStream2);
                                    }
                                    catch
                                    {
                                        ETGModConsole.Log("Error: invalid json at project path " + text5, false);
                                        continue;
                                    }
                                    tk2dSpriteCollectionData.SetAttachPoints(spriteIdByName, assetSpriteData.attachPoints);
                                    //tk2dSpriteCollectionData.inst.SetAttachPoints(spriteIdByName, assetSpriteData.attachPoints);
                                }
                            }
                        }
                    }

                }
            }
        }
    }

}
