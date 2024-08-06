using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Assetbundle
{
    public static class GunInt
    {
        /// <summary>
        /// Virtually the same as GunExt, except allows for using pre-built sprite collections to use as Ammonomicon sprites.
        /// </summary>
        /// <param name="gun">The prefix of the mod that adds the stat.</param>
        /// <param name="collection">The sprite collection the ammonomicon sprite is from.</param>
        /// <param name="defaultSprite">The name of your sprite you want to use for your ammonomicon entry, thats in your collection.</param>
        /// <param name="fps">Frames Per Second.</param>
        /// <param name="ammonomiconSprite">Specific Ammonomicon Sprite.</param>

        /// <returns></returns>
        public static void SetupSprite(this Gun gun, tk2dSpriteCollectionData collection = null, string defaultSprite = null, int fps = 0, string ammonomiconSprite = null)
        {
            if ((object)collection == null)
            {
                collection = ETGMod.Databases.Items.WeaponCollection;
            }
            if (defaultSprite != null || ammonomiconSprite != null)
            {
                var DefineSprite = (ammonomiconSprite ?? defaultSprite);
                AddSpriteToCollection(collection.GetSpriteDefinition(DefineSprite), ammonomiconCollection);
                gun.encounterTrackable.journalData.AmmonomiconSprite = DefineSprite;
            }
            gun.UpdateAnimations(collection);
            tk2dBaseSprite sprite = gun.GetSprite();
            int newSpriteId = (gun.DefaultSpriteID = collection.GetSpriteIdByName(defaultSprite));
            sprite.SetSprite(collection, newSpriteId);
            if (fps != 0)
            {
                gun.SetAnimationFPS(fps);
            }
        }


        /// <summary>
        /// Virtually the same as GunExt, except allows for using pre-built sprite collections to use as Ammonomicon sprites. Use only if your animations are also pre-built.
        /// </summary>
        /// <param name="gun">The prefix of the mod that adds the stat.</param>
        /// <param name="collection">The sprite collection the ammonomicon sprite is from.</param>
        /// <param name="defaultSprite">The name of your sprite you want to use for your ammonomicon entry, thats in your collection.</param>
        /// <param name="ammonomiconSprite">Specific Ammonomicon Sprite.</param>

        /// <returns></returns>
        public static void SetupSpritePrebaked(this Gun gun, tk2dSpriteCollectionData collection = null, string defaultSprite = null, string ammonomiconSprite = null)
        {
            if ((object)collection == null)
            {
                collection = ETGMod.Databases.Items.WeaponCollection;
            }
            if (defaultSprite != null || ammonomiconSprite != null)
            {
                var DefineSprite = (ammonomiconSprite ?? defaultSprite);
                AddSpriteToCollection(collection.GetSpriteDefinition(DefineSprite), ammonomiconCollection);
                gun.encounterTrackable.journalData.AmmonomiconSprite = DefineSprite;
            }
            gun.emptyAnimation = null;

            tk2dBaseSprite sprite = gun.GetSprite();
            int newSpriteId = (gun.DefaultSpriteID = collection.GetSpriteIdByName(defaultSprite));
            sprite.SetSprite(collection, newSpriteId);
        }
        private static List<tk2dSpriteDefinition> GunSpriteDefs = new List<tk2dSpriteDefinition>();

        public static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
        {
            //Add definition to collection
            var defs = collection.spriteDefinitions;
            var newDefs = defs.Concat(new tk2dSpriteDefinition[] { spriteDefinition }).ToArray();
            collection.spriteDefinitions = newDefs;

            //Reset lookup dictionary
            if (collection.spriteNameLookupDict == null)
                collection.InitDictionary();
            else
                collection.spriteNameLookupDict[spriteDefinition.name] = newDefs.Length - 1;
            return newDefs.Length - 1;
        }

        /*
        public static void FinalizeSprites()
        {
            var collection = ammonomiconCollection;

            var defs = collection.spriteDefinitions;

            foreach (var entry in GunSpriteDefs)
            {
                var newDefs = defs.Concat(new tk2dSpriteDefinition[] {entry}).ToArray();
                collection.spriteDefinitions = newDefs;
            }


            //Reset lookup dictionary
            FieldInfo f = typeof(tk2dSpriteCollectionData).GetField("spriteNameLookupDict", BindingFlags.Instance | BindingFlags.NonPublic);
            f.SetValue(collection, null);  //Set dictionary to null
            collection.InitDictionary(); //InitDictionary only runs if the dictionary is null

        }
        */
        public static tk2dSpriteCollectionData ammonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
    }
}
