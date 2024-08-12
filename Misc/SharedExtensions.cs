using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class SharedExtensions
    {
        public static void MakeOffset(this tk2dSpriteDefinition def, Vector3 offset, bool changesCollider = false)
        {
            def.position0 += offset;
            def.position1 += offset;
            def.position2 += offset;
            def.position3 += offset;
            def.boundsDataCenter += offset;
            def.untrimmedBoundsDataCenter += offset;
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
                def.colliderVertices[0] += offset;
        }
    }
}
