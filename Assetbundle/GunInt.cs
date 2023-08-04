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
                AddSpriteToCollection(collection.GetSpriteDefinition(defaultSprite), ammonomiconCollection);
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
        private static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
        {
            tk2dSpriteDefinition[] spriteDefinitions = collection.spriteDefinitions;
            tk2dSpriteDefinition[] array = spriteDefinitions.Concat(new tk2dSpriteDefinition[]
            {
                spriteDefinition
            }).ToArray<tk2dSpriteDefinition>();
            collection.spriteDefinitions = array;
            FieldInfo field = typeof(tk2dSpriteCollectionData).GetField("spriteNameLookupDict", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(collection, null);
            collection.InitDictionary();
            return array.Length - 1;
        }
        public static tk2dSpriteCollectionData ammonomiconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
    }
}
