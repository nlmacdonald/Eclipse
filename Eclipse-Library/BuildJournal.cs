using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuildJournal
    {
        private readonly SortedDictionary<int, BuildJournalLevel> _levels = new();

        public BuildJournal(CharacterSeed seed, int currentLevel)
        {
            Seed = seed ?? throw new ArgumentNullException(nameof(seed));

            if (currentLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLevel), currentLevel, "Current level must be >= 1.");
            }

            CurrentLevel = currentLevel;
        }

        public CharacterSeed Seed { get; }
        public int CurrentLevel { get; private set; }

        public IReadOnlyList<BuildJournalLevel> Levels => _levels.Values.ToList().AsReadOnly();

        public void SetCurrentLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            CurrentLevel = level;
        }

        public BuildJournalLevel GetLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (!_levels.TryGetValue(level, out var existing))
            {
                existing = new BuildJournalLevel(level);
                _levels.Add(level, existing);
            }

            return existing;
        }

        public BuildJournalPurchase AddPurchase(int level, IPurchase purchase)
        {
            if (purchase is null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (level > CurrentLevel)
            {
                SetCurrentLevel(level);
            }

            return GetLevel(level).AddPurchase(purchase);
        }

        public IReadOnlyList<BuildJournalPurchase> GetPurchasesForLevel(int level)
        {
            if (!_levels.TryGetValue(level, out var existing))
            {
                return Array.Empty<BuildJournalPurchase>();
            }

            return existing.Purchases;
        }
    }
}

