using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class BuildJournalLevel
    {
        private readonly List<BuildJournalPurchase> _purchases = new();

        public BuildJournalLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            Level = level;
        }

        public int Level { get; }
        public IReadOnlyList<BuildJournalPurchase> Purchases => _purchases.AsReadOnly();

        public BuildJournalPurchase AddPurchase(IPurchase purchase)
        {
            if (purchase is null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            var entry = new BuildJournalPurchase(level: Level, purchase);
            _purchases.Add(entry);
            return entry;
        }
    }
}

