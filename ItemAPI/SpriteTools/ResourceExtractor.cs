﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SGUI;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using Alexandria.ItemAPI;

namespace Alexandria.ItemAPI
{
    public static class ResourceExtractor
    {
        private static string spritesDirectory = Path.Combine(ETGMod.ResourcesDirectory, "sprites");
        /// <summary>
        /// Converts all png's in a folder to a list of Texture2D objects
        /// </summary>
        public static List<Texture2D> GetTexturesFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                ETGModConsole.Log(directoryPath + " not found.");
                return null;
            }

            List<Texture2D> textures = new List<Texture2D>();
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (!filePath.EndsWith(".png")) continue;

                Texture2D texture = BytesToTexture(File.ReadAllBytes(filePath), Path.GetFileName(filePath).Replace(".png", ""));
                textures.Add(texture);
            }
            return textures;
        }

        /// <summary>
        /// Creates a Texture2D from a file in the sprites directory
        /// </summary>
        public static Texture2D GetTextureFromFile(string fileName, string extension = ".png")
        {
            fileName = fileName.Replace(extension, "");
            string filePath = Path.Combine(spritesDirectory, fileName + extension);
            if (!File.Exists(filePath))
            {
                ETGModConsole.Log(filePath + " not found.");
                return null;
            }
            Texture2D texture = BytesToTexture(File.ReadAllBytes(filePath), fileName);
            return texture;
        }

        /// <summary>
        /// Retuns a list of sprite collections in the sprite folder
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCollectionFiles()
        {
            List<string> collectionNames = new List<string>();
            foreach (string filePath in Directory.GetFiles(spritesDirectory))
            {
                if (filePath.EndsWith(".png"))
                {
                    collectionNames.Add(Path.GetFileName(filePath).Replace(".png", ""));
                }
            }
            return collectionNames;
        }

        /// <summary>
        /// Converts a byte array into a Texture2D
        /// </summary>
        public static Texture2D BytesToTexture(byte[] bytes, string resourceName)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, bytes);
            texture.filterMode = FilterMode.Point;
            texture.name = resourceName;
            return texture;
        }

        public static string[] GetLinesFromEmbeddedResource(string filePath)
        {
            string allLines = BytesToString(ExtractEmbeddedResource(filePath, Assembly.GetCallingAssembly()));
            return allLines.Split('\n');
        }

        public static string[] GetLinesFromFile(string filePath)
        {
            string allLines = BytesToString(File.ReadAllBytes(filePath));
            return allLines.Split('\n');
        }

        public static string BytesToString(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Returns a list of folders in the ETG resources directory
        /// </summary>
        public static List<string> GetResourceFolders()
        {
            List<string> dirs = new List<string>();
            string spritesDirectory = Path.Combine(ETGMod.ResourcesDirectory, "sprites");

            if (Directory.Exists(spritesDirectory))
            {
                foreach (string directory in Directory.GetDirectories(spritesDirectory))
                {
                    dirs.Add(Path.GetFileName(directory));
                }
            }
            return dirs;
        }

        /// <summary>
        /// Converts an embedded resource to a byte array
        /// </summary>
        public static byte[] ExtractEmbeddedResource(string filePath, Assembly assembly = null)
        {
            filePath = filePath.Replace("/", ".");
            filePath = filePath.Replace("\\", ".");

            //ETGModConsole.Log($"[{(assembly ?? Assembly.GetCallingAssembly()).GetName().Name}]: {filePath}");

            using (Stream resFilestream = (assembly ?? Assembly.GetCallingAssembly()).GetManifestResourceStream(filePath))
            {
                
                if (resFilestream == null)
                {
                    return null;
                }
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        /// Converts an embedded resource to a Texture2D object
        /// </summary>
        public static Texture2D GetTextureFromResource(string resourceName, Assembly assembly = null)
        {          
            byte[] bytes = ExtractEmbeddedResource(resourceName, assembly ?? Assembly.GetCallingAssembly());
            if (bytes == null)
            {
                ETGModConsole.Log("No bytes found in " + resourceName);
                return null;
            }
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, bytes);
            texture.filterMode = FilterMode.Point;

            string name = resourceName.Substring(0, resourceName.LastIndexOf('.'));
            if (name.LastIndexOf('.') >= 0)
            {
                name = name.Substring(name.LastIndexOf('.') + 1);
            }
            texture.name = name;

            return texture;
        }

        public static List<Texture2D> GetTexturesFromResource(string resourceName, Assembly assembly = null)
        {
            string[] resources = GetResourceNames(assembly ?? Assembly.GetCallingAssembly());
            List<Texture2D> result = new List<Texture2D>();

            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].StartsWith(resourceName.Replace('/', '.') + ".", StringComparison.OrdinalIgnoreCase))
                {
                    ////DebugUtility.PrintError<string>(resourceName, "FF0000");
                    result.Add(GetTextureFromResource(resources[i], assembly ?? Assembly.GetCallingAssembly()));
                }
            }

            if (result.Count == 0)
            {
                ETGModConsole.Log("No bytes found in " + resourceName);
                result = null;
            }

            return result;
        }


        /// <summary>
        /// Returns a list of the names of all embedded resources
        /// </summary>
        public static string[] GetResourceNames(Assembly assembly = null)
        {
            var baseAssembly = assembly ?? Assembly.GetCallingAssembly();
            string[] names = baseAssembly.GetManifestResourceNames();
            if (names == null)
            {
                ETGModConsole.Log("No manifest resources found.");
                return null;
            }
            return names;
        }
    }
}
