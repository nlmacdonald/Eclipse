using System;

namespace Eclipse_Library
{
    public sealed class LimitedSaveBonus
    {
        public LimitedSaveBonus(SaveType saveType, int bonus, string limitation)
        {
            if (bonus <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bonus), bonus, "Bonus must be > 0.");
            }

            SaveType = saveType;
            Bonus = bonus;
            Limitation = limitation ?? throw new ArgumentNullException(nameof(limitation));
        }

        public SaveType SaveType { get; }
        public int Bonus { get; }
        public string Limitation { get; }
    }
}

