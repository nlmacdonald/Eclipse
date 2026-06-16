using System;

namespace Eclipse_Library
{
    public sealed class BuyBonusFeatPurchase : IPurchase
    {
        private readonly int _count;

        public BuyBonusFeatPurchase(int count = 1)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Bonus feat count must be >= 1.");
            }

            _count = count;
        }

        public string Description => _count == 1 ? "Buy bonus feat" : $"Buy {_count} bonus feats";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            // Eclipse summary assumes +6 CP per bonus feat.
            character.SpendCp(_count * 6);
            character.AddBonusFeats(_count);
        }
    }
}

