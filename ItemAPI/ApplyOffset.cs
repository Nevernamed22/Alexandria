using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
    public static class ApplyOffsetStuff
    {
        public static void ApplyOffset(this tk2dSpriteDefinition def, Vector2 offset)
        {
            Shared.MakeOffset(def, offset);
        }
    }
}
