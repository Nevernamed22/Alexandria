using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.TranslationAPI
{
    [Obsolete("TranslationAPI is deprecated, use ETGMod.Databases.Strings for translations instead.", true)]
    public class CustomStringDBTable
    {
        public Dictionary<string, StringTableManager.StringCollection> Table
        {
            get
            {
                if (mtgTable != null)
                    return mtgTable.Table;

                return new();
            }
        }

        public StringTableManager.StringCollection this[string key]
        {
            get
            {
                if (mtgTable != null)
                    return mtgTable[key];

                return null;
            }
        }

        public void SetValue(string key, StringTableManager.StringCollection value)
        {
            if (mtgTable != null)
                mtgTable[key] = value;
        }

        public CustomStringDBTable(Func<Dictionary<string, StringTableManager.StringCollection>> getTable)
        {
        }

        internal CustomStringDBTable(StringDBTable table)
        {
            mtgTable = table;
        }

        internal CustomStringDBTable()
        {
        }

        public bool ContainsKey(string key)
        {
            if(mtgTable != null)
                return mtgTable.ContainsKey(key);

            return false;
        }

        public void Set(string key, string value)
        {
            if(mtgTable != null)
                mtgTable.Set(key, value);
        }

        public string Get(string key)
        {
            return StringTableManager.GetString(key);
        }

        public void LanguageChanged()
        {
        }

        private StringDBTable mtgTable;
    }
}
