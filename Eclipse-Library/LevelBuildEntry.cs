using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class LevelBuildEntry
    {
        private readonly List<BuildPurchase> _purchases = new();

        public LevelBuildEntry(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            Level = level;
        }

        public int Level { get; }

        public IReadOnlyList<BuildPurchase> Purchases => _purchases.AsReadOnly();

        public BuildPurchase AddPurchase(IPurchase purchase)
        {
            if (purchase is null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            var entry = new BuildPurchase(Guid.NewGuid(), Level, _purchases.Count, purchase);
            _purchases.Add(entry);
            return entry;
        }
    }
}
