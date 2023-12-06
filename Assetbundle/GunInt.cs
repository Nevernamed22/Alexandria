using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        /// <returns></returns>
        public static void SetupSprite(this Gun gun, tk2dSpriteCollectionData collection = null, string defaultSprite = null, int fps = 0)
        {
            if ((object)collection == null)
            {
                collection = ETGMod.Databases.Items.WeaponCollection;
            }
            if (defaultSprite != null)
            {
                GunSpriteDefs.Add(collection.GetSpriteDefinition(defaultSprite));

                //AddSpriteToCollection(collection.GetSpriteDefinition(defaultSprite), ammonomiconCollection);
                gun.encounterTrackable.journalData.AmmonomiconSprite = defaultSprite;
            }
            gun.UpdateAnimations(collection);
            tk2dBaseSprite sprite = gun.GetSprite();
            tk2dSpriteCollectionData newCollection = collection;
            int newSpriteId = (gun.DefaultSpriteID = collection.GetSpriteIdByName(gun.encounterTrackable.journalData.AmmonomiconSprite));
            sprite.SetSprite(newCollection, newSpriteId);
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

        /// <returns></returns>
        public static void SetupSpritePrebaked(this Gun gun, tk2dSpriteCollectionData collection = null, string defaultSprite = null)
        {
            if ((object)collection == null)
            {
                collection = ETGMod.Databases.Items.WeaponCollection;
            }
            if (defaultSprite != null)// && !GunSpriteDefs.Contains(gun))
            {
                GunSpriteDefs.Add(collection.GetSpriteDefinition(defaultSprite));

                //AddSpriteToCollection(collection.GetSpriteDefinition(defaultSprite), ammonomiconCollection);
                gun.encounterTrackable.journalData.AmmonomiconSprite = defaultSprite;
            }
            gun.emptyAnimation = null;

            tk2dBaseSprite sprite = gun.GetSprite();
            tk2dSpriteCollectionData newCollection = collection;
            int newSpriteId = (gun.DefaultSpriteID = collection.GetSpriteIdByName(gun.encounterTrackable.journalData.AmmonomiconSprite));
            sprite.SetSprite(newCollection, newSpriteId);
        }
        private static List<tk2dSpriteDefinition> GunSpriteDefs = new List<tk2dSpriteDefinition>();



        public static void FinalizeSprites()
        {
            if (GunSpriteDefs.Count() == 0) { return; }
            tk2dSpriteDefinition[] spriteDefinitions = ammonomiconCollection.spriteDefinitions;
            tk2dSpriteDefinition[] array = spriteDefinitions.Concat(GunSpriteDefs.ToArray()).ToArray<tk2dSpriteDefinition>();
            ammonomiconCollection.spriteDefinitions = array;
            ammonomiconCollection.spriteNameLookupDict = ammonomiconCollection.spriteNameLookupDict ?? new Dictionary<string, int>();
            for (int i = 0; i < ammonomiconCollection.spriteDefinitions.Length; i++)
            {
                if (ammonomiconCollection.spriteDefinitions[i] != null && ammonomiconCollection.spriteDefinitions[i].name != null)
                {
                    ammonomiconCollection.spriteNameLookupDict[ammonomiconCollection.spriteDefinitions[i].name] = i;
                }
            }
        }

        public static tk2dSpriteCollectionData ammonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
    }
}
