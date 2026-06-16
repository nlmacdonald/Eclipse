using System;

namespace Eclipse_Library
{
    public sealed class BuildPurchase
    {
        public BuildPurchase(Guid id, int level, int index, IPurchase purchase)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be >= 0.");
            }

            Id = id;
            Level = level;
            Index = index;
            Purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            Description = purchase.Description;
        }

        public Guid Id { get; }
        public int Level { get; }
        public int Index { get; }
        public IPurchase Purchase { get; }
        public string Description { get; }
    }
}

