using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public class TextHelper
    {
        public static void RegisterCustomTokenInsert(string assetPath, string name)
        {
            tk2dSpriteCollectionData collection = ((GameObject)ResourceCache.Acquire("ControllerButtonSprite")).GetComponent<tk2dBaseSprite>().Collection;
            int spriteId = SpriteBuilder.AddSpriteToCollection(assetPath, collection, name: name, assembly: Assembly.GetCallingAssembly());
        }
    }
}
