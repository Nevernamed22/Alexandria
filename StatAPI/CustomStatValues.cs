using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.StatAPI
{
    public class CustomStatValues
    {
        internal CustomStatValues(List<string> k, List<float> v)
        {
            keys = k;
            values = v;
        }

        /// <summary>
        /// Gets or sets the current value of a stat from the mod with the prefix modPrefix and the name statName.
        /// </summary>
		/// <param name="modPrefix">The prefix of the mod that adds the stat.</param>
		/// <param name="statName">The name of the stat.</param>
        /// <returns></returns>
        public float this[string modPrefix, string statName]
        {
            get
            {
                if (!keys.Contains($"{modPrefix}.{statName}"))
                {
                    return 1f;
                }
                return values[keys.IndexOf($"{modPrefix}.{statName}")];
            }
            set
            {
                if (!keys.Contains($"{modPrefix}.{statName}"))
                {
                    keys.Add(statName);
                    values.Add(value);
                }
                else
                {
                    values[keys.IndexOf($"{modPrefix}.{statName}")] = value;
                }
            }
        }

        internal void SetWithoutPrefix(string k, float v)
        {
            if (!keys.Contains(k))
            {
                keys.Add(k);
                values.Add(v);
            }
            else
            {
                values[keys.IndexOf(k)] = v;
            }
        }

        internal float GetWithoutPrefix(string k)
        {
            if (!keys.Contains(k))
            {
                return 1f;
            }
            return values[keys.IndexOf(k)];
        }

        private readonly List<string> keys;
        private readonly List<float> values;
    }
}
