using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.StatAPI
{
    [EnumExtension(typeof(StatModifier.ModifyMethod))]
    public static class ModifyMethodE
    {
        /// <summary>
        /// A method of stat modification similar to the Multiplicative method, but TrueMultiplicative stat modifiers are applied after additive stat modifiers, unlike Multiplicative stat modifiers.
        /// </summary>
        public static StatModifier.ModifyMethod TrueMultiplicative;
        /// <summary>
        /// A method of stat modification that boosts the value of a stat to a power equal to the stat modifier's amount. Exponent modifiers are the last to be applied.
        /// </summary>
        public static StatModifier.ModifyMethod Exponent;
    }
}
