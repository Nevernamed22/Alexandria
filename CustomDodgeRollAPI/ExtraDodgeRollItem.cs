using UnityEngine;

namespace Alexandria.CustomDodgeRollAPI
{
    public class ExtraDodgeRollItem : MonoBehaviour
    {
        /// <summary>The number of extra midair dodge rolls this item grants (must be >= 0)</summary>
        public virtual int ExtraMidairDodgeRolls() => 0;
    }
}

