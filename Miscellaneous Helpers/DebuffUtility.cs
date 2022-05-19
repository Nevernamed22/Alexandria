using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Alexandria.Miscellaneous_Helpers
{
    public static class DebuffUtility
    {

        public static EffectResistanceType CreateDebuffResistance(string prefix, string effectName)
        {
            if (!string.IsNullOrEmpty(effectName))
            {
                return EnumUtility.GetEnumValue<EffectResistanceType>(prefix, effectName);
            }
            return EffectResistanceType.None;
        }

    }
}
