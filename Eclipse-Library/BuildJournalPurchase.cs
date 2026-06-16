using System;

namespace Eclipse_Library
{
    public sealed class BuildJournalPurchase
    {
        public BuildJournalPurchase(int level, IPurchase purchase)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            Level = level;
            Purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            Id = Guid.NewGuid();
            Description = purchase.Description;
        }

        public Guid Id { get; }
        public int Level { get; }
        public IPurchase Purchase { get; }
        public string Description { get; }
    }
}

