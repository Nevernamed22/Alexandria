using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public class ProjectileExt : MonoBehaviour
    {
        public Action<Projectile, HealthHaver, HealthHaver.ModifyDamageEventArgs> ModifyDealtDamage; // Transpiler to handle this is in HealthHaverExt
    }
}
