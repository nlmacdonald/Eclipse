using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class CharacterBuild
    {
        private readonly SortedDictionary<int, LevelBuildEntry> _levels = new();

        public CharacterBuild(CharacterSeed seed, int targetLevel)
        {
            Seed = seed ?? throw new ArgumentNullException(nameof(seed));

            if (targetLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLevel), targetLevel, "Target level must be >= 1.");
            }

            TargetLevel = targetLevel;
        }

        public CharacterSeed Seed { get; }
        public int TargetLevel { get; private set; }

        public IReadOnlyList<LevelBuildEntry> Levels => _levels.Values.ToList().AsReadOnly();

        public void SetTargetLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Target level must be >= 1.");
            }

            TargetLevel = level;
        }

        public LevelBuildEntry GetLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (!_levels.TryGetValue(level, out var entry))
            {
                entry = new LevelBuildEntry(level);
                _levels.Add(level, entry);
            }

            return entry;
        }

        public bool TryGetLevel(int level, out LevelBuildEntry? entry)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            return _levels.TryGetValue(level, out entry);
        }

        public void AddPurchase(int level, IPurchase purchase)
        {
            if (purchase is null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (level > TargetLevel)
            {
                SetTargetLevel(level);
            }

            GetLevel(level).AddPurchase(purchase);
        }

        public BuildPurchase AddPurchaseWithResult(int level, IPurchase purchase)
        {
            if (purchase is null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (level > TargetLevel)
            {
                SetTargetLevel(level);
            }

            return GetLevel(level).AddPurchase(purchase);
        }
    }
}
