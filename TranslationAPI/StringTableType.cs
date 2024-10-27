using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.TranslationAPI
{
    [Obsolete("TranslationAPI is deprecated, use ETGMod.Databases.Strings for translations instead.", true)]
    public enum StringTableType
    {
        Core,
        Items,
        Enemies,
        UI,
        Intro,
        Synergy
    }
}
