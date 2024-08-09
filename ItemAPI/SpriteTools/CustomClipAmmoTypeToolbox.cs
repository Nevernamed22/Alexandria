using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace Alexandria.ItemAPI
{
    public static class CustomClipAmmoTypeToolbox
    {
        public static List<GameUIAmmoType> addedAmmoTypes = new List<GameUIAmmoType>();
        public static string AddCustomAmmoType(string name, string ammoTypeSpritePath, string ammoBackgroundSpritePath)
        {
            Texture2D fgTexture = ResourceExtractor.GetTextureFromResource(ammoTypeSpritePath + ".png", Assembly.GetCallingAssembly());
            Texture2D bgTexture = ResourceExtractor.GetTextureFromResource(ammoBackgroundSpritePath + ".png", Assembly.GetCallingAssembly());

            GameObject fgSpriteObject = new GameObject("sprite fg");
            fgSpriteObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(fgSpriteObject);
            UnityEngine.Object.DontDestroyOnLoad(fgSpriteObject);
            GameObject bgSpriteObject = new GameObject("sprite bg");
            bgSpriteObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(bgSpriteObject);
            UnityEngine.Object.DontDestroyOnLoad(bgSpriteObject);

            dfTiledSprite fgSprite = fgSpriteObject.SetupDfSpriteFromTexture<dfTiledSprite>(fgTexture, ShaderCache.Acquire("Daikon Forge/Default UI Shader"));
            dfTiledSprite bgSprite = bgSpriteObject.SetupDfSpriteFromTexture<dfTiledSprite>(bgTexture, ShaderCache.Acquire("Daikon Forge/Default UI Shader"));
            GameUIAmmoType uiammotype = new GameUIAmmoType
            {
                ammoBarBG = bgSprite,
                ammoBarFG = fgSprite,
                ammoType = GameUIAmmoType.AmmoType.CUSTOM,
                customAmmoType = name
            };
            CustomClipAmmoTypeToolbox.addedAmmoTypes.Add(uiammotype);
            foreach (GameUIAmmoController uiammocontroller in GameUIRoot.Instance.ammoControllers)
            {
                Add(ref uiammocontroller.ammoTypes, uiammotype);
            }
            return name;
        }

        public static T SetupDfSpriteFromTexture<T>(this GameObject obj, Texture2D texture, Shader shader) where T : dfSprite
        {
            T sprite = obj.GetOrAddComponent<T>();
            dfAtlas atlas = obj.GetOrAddComponent<dfAtlas>();
            atlas.Material = new Material(shader);
            atlas.Material.mainTexture = texture;
            atlas.Items.Clear();
            dfAtlas.ItemInfo info = new dfAtlas.ItemInfo
            {
                border = new RectOffset(),
                deleted = false,
                name = "main_sprite",
                region = new Rect(Vector2.zero, new Vector2(1, 1)),
                rotated = false,
                sizeInPixels = new Vector2(texture.width, texture.height),
                texture = null,
                textureGUID = "main_sprite"
            };
            atlas.AddItem(info);
            sprite.Atlas = atlas;
            sprite.SpriteName = "main_sprite";
            return sprite;
        }
        public static void Add<T>(ref T[] array, T toAdd)
        {
            List<T> list = array.ToList();
            list.Add(toAdd);
            array = list.ToArray<T>();
        }
    }

    [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.Initialize))]
    internal static class GameUIAmmoControllerInitializePatch
    {
        private static void Postfix(GameUIAmmoController __instance)
        {
            int newCount = CustomClipAmmoTypeToolbox.addedAmmoTypes.Count;
            if (newCount < 1)
                return;
            int curLength = __instance.ammoTypes.Length;
            Array.Resize(ref __instance.ammoTypes, curLength + newCount);
            for (int i = 0; i < newCount; ++i)
                __instance.ammoTypes[curLength++] = CustomClipAmmoTypeToolbox.addedAmmoTypes[i];
        }
    }
}
