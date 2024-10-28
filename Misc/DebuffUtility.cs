using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Alexandria.Misc
{
    public static class DebuffUtility
    {
        public static EffectResistanceType CreateDebuffResistance(string prefix, string effectName)
        {
            if (!string.IsNullOrEmpty(effectName))
                return ETGModCompatibility.ExtendEnum<EffectResistanceType>(prefix, effectName);
            return EffectResistanceType.None;
        }
    }
}
