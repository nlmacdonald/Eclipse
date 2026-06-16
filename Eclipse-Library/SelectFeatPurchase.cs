using System;

namespace Eclipse_Library
{
    public sealed class SelectFeatPurchase : IPurchase
    {
        private readonly string _featName;
        private readonly int _count;

        public SelectFeatPurchase(string featName, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(featName))
            {
                throw new ArgumentException("Feat name is required.", nameof(featName));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Feat count must be >= 1.");
            }

            _featName = featName.Trim();
            _count = count;
        }

        public string Description => _count == 1
            ? $"Select feat: {_featName}"
            : $"Select feat: {_featName} x{_count}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            character.SelectFeat(_featName, _count);
        }
    }
}
