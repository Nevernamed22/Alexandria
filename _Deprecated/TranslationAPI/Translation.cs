using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.TranslationAPI
{
    [Obsolete("TranslationAPI is deprecated, use ETGMod.Databases.Strings for translations instead.", true)]
    public class Translation
    {
        public Dictionary<string, StringTableManager.StringCollection> Table => new();

        public bool UpdateLanguage(StringTableManager.GungeonSupportedLanguages? overrideLang = null)
        {
            return false;
        }

        public void AddStringTranslation(string key, string value)
        {
        }

        public StringTableType type;
        public StringTableManager.GungeonSupportedLanguages language;
        public Dictionary<string, StringTableManager.StringCollection> strings;
        public Func<Dictionary<string, StringTableManager.StringCollection>> getTable;
    }
}
