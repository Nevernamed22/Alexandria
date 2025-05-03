namespace Alexandria.CustomDodgeRollAPI
{
    public class CustomDodgeRollItem : PassiveItem
    {
        /// <summary>The CustomDodgeRoll, if any, this item grants while held</summary>
        public virtual CustomDodgeRoll CustomDodgeRoll() => null;
    }

    public class CustomDodgeRollActiveItem : PlayerItem
    {
        /// <summary>The CustomDodgeRoll, if any, this item grants while held</summary>
        public virtual CustomDodgeRoll CustomDodgeRoll() => null;
    }
}

