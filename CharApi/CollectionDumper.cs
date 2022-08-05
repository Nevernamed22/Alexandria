using UnityEngine;
using System.Collections;
using System;



namespace CustomCharacters
{
    public static class CollectionDumper
    {
        //For debugging
        public static void DumpCollection(tk2dSpriteCollectionData collection)
        {
            string collectionName = string.IsNullOrEmpty(collection.name) ? collection.gameObject.name + "_Collection" : collection.name;

            tk2dSpriteDefinition def;
            string defName;
            Material material;
            Texture2D texture, output;
            int width, height, minX, minY, maxX, maxY, w, h;
            Vector2[] uvs;
            Color[] pixels;


            for (int i = 0; i < collection.spriteDefinitions.Length; i++)
            {
                def = collection.spriteDefinitions[i];
                if (def == null) continue;


                defName = string.IsNullOrEmpty(def.name) ? collectionName + "_" + i : def.name;
                material = def.material == null ? def.materialInst : def.material;
                if (material == null || material.mainTexture == null)
                {
                    ToolsCharApi.PrintError($"Failed to dump {defName} in {collectionName}: No valid material");
                    continue;
                }

                texture = (Texture2D)material.mainTexture.GetReadable();
                width = texture.width;
                height = texture.height;

                uvs = def.uvs;
                if (def.uvs == null || def.uvs.Length < 4)
                {
                    ToolsCharApi.PrintError($"Failed to dump {defName} in {collectionName}: Invalid UV's");
                    continue;
                }

                minX = Mathf.RoundToInt(uvs[0].x * width);
                minY = Mathf.RoundToInt(uvs[0].y * height);
                maxX = Mathf.RoundToInt(uvs[3].x * width);
                maxY = Mathf.RoundToInt(uvs[3].y * height);

                w = maxX - minX;
                h = maxY - minY;
                if (w <= 0 || h <= 0)
                {
                    ToolsCharApi.ExportTexture(new Texture2D(1, 1) { name = defName });
                    continue;
                };

                pixels = texture.GetPixels(minX, minY, w, h);

                output = new Texture2D(w, h);
                output.SetPixels(pixels);
                output.Apply();
                if (def.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
                {
                    output = output.Rotated().Flipped();
                }
                output.name = def.name;
                ToolsCharApi.ExportTexture(output, "SpriteDump/" + collectionName.Replace("/", "-").Replace("\\", "-"));



            }
        }

        public static void DumpAnimation(tk2dSpriteAnimation animation)
        {
            string collectionName = string.IsNullOrEmpty(animation.name) ? animation.gameObject.name + "_Animation" : animation.name;

            tk2dSpriteDefinition def;
            string defName;
            Material material;
            Texture2D texture, output;
            int width, height, minX, minY, maxX, maxY, w, h;
            Vector2[] uvs;
            Color[] pixels;


            foreach (var clip in animation.clips)
            {
                foreach (var frame in clip.frames)
                {
                    def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
                    if (def == null) continue;


                    defName = string.IsNullOrEmpty(def.name) ? collectionName + "_" + frame.spriteId : def.name;
                    material = def.material == null ? def.materialInst : def.material;
                    if (material == null || material.mainTexture == null)
                    {
                        ToolsCharApi.PrintError($"Failed to dump {defName} in {collectionName}: No valid material");
                        continue;
                    }

                    texture = (Texture2D)material.mainTexture.GetReadable();
                    width = texture.width;
                    height = texture.height;

                    uvs = def.uvs;
                    if (def.uvs == null || def.uvs.Length < 4)
                    {
                        ToolsCharApi.PrintError($"Failed to dump {defName} in {collectionName}: Invalid UV's");
                        continue;
                    }

                    minX = Mathf.RoundToInt(uvs[0].x * width);
                    minY = Mathf.RoundToInt(uvs[0].y * height);
                    maxX = Mathf.RoundToInt(uvs[3].x * width);
                    maxY = Mathf.RoundToInt(uvs[3].y * height);

                    w = maxX - minX;
                    h = maxY - minY;
                    if (w <= 0 || h <= 0)
                    {
                        ToolsCharApi.ExportTexture(new Texture2D(1, 1) { name = defName });
                        continue;
                    };

                    pixels = texture.GetPixels(minX, minY, w, h);

                    output = new Texture2D(w, h);
                    output.SetPixels(pixels);
                    output.Apply();
                    if (def.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
                    {
                        output = output.Rotated().Flipped();
                    }
                    output.name = def.name;

                    ToolsCharApi.ExportTexture(output, $"AnimationDump/{collectionName.Replace("/", "-").Replace("\\", "-")}/{clip.name}", $"{output.name}_{clip.frames.IndexOf(frame)}");
                }
            }
            
        }

        public static void DumpdfAtlas(dfAtlas atlas)
        {
            string collectionName = atlas.name;

            string defName;
            Texture2D texture, output;
            int width, height, minX, minY, maxX, maxY, w, h;
            Color[] pixels;

            
            var itemSizes = atlas.GetPixelRegions();

            for (int i = 0; i < itemSizes.Count; i++)
            {
                var def = atlas.Items[i];
                if (def == null) continue;


                defName = string.IsNullOrEmpty(def.name) ? collectionName + "_" + i : def.name;
                
               
                texture = (Texture2D)atlas.Texture.GetReadable();
                width = texture.width;
                height = texture.height;

               
               
                minX = itemSizes[i].xMin;
                minY = itemSizes[i].yMin;
                maxX = itemSizes[i].xMax;
                maxY = itemSizes[i].yMax;

                w = maxX - minX;
                h = maxY - minY;

               
                if (w <= 0 || h <= 0)
                {
                    ToolsCharApi.PrintError<string>($"[{defName}]: is to small. minX: {minX}, minY: {minY}, maxX: {maxX}, maxY: {maxY}");
                    //ToolsCharApi.ExportTexture(new Texture2D(1, 1) { name = defName });
                    continue;
                };

                pixels = texture.GetPixels(minX, minY, w, h);

                output = new Texture2D(w, h);
                output.SetPixels(pixels);
                output.Apply();
                output.name = def.name;
                //BotsModule.Log(output.name, BotsModule.TEXT_COLOR);
                ToolsCharApi.ExportTexture(output, "SpriteDump/df/" + collectionName);


            }
        }
    }

}
