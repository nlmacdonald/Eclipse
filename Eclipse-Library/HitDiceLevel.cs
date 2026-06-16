using System;

namespace Eclipse_Library
{
    public sealed class HitDiceLevel
    {
        public HitDiceLevel(HitDieType? primary, HitDieType? secondary)
        {
            Primary = primary;
            Secondary = secondary;
        }

        public HitDieType? Primary { get; }
        public HitDieType? Secondary { get; }

        public HitDiceLevel WithPrimary(HitDieType? primary) => new(primary, Secondary);
        public HitDiceLevel WithSecondary(HitDieType? secondary) => new(Primary, secondary);
    }
}

