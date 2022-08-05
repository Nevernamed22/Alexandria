using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomCharacters
{
    class SaveFileHelper
    {
        public static Dictionary<PlayableCharacters, GameStats> customCharacterStats;
    }

	public class CustomPlayableCharactersComparer : IEqualityComparer<CustomPlayableCharacters>
	{
		public bool Equals(CustomPlayableCharacters x, CustomPlayableCharacters y)
		{
			return x == y;
		}
		public int GetHashCode(CustomPlayableCharacters obj)
		{
			return (int)obj;
		}
	}
}
