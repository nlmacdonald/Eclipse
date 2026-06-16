using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuildStepValidationContext
    {
        public BuildStepValidationContext(
            int level,
            Character startOfLevelSnapshot,
            Character endOfLevelSnapshot,
            LevelBuildEntry? levelEntry,
            IReadOnlyList<AppliedPurchaseRecord> appliedPurchasesThisLevel)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            Level = level;
            StartOfLevelSnapshot = startOfLevelSnapshot ?? throw new ArgumentNullException(nameof(startOfLevelSnapshot));
            EndOfLevelSnapshot = endOfLevelSnapshot ?? throw new ArgumentNullException(nameof(endOfLevelSnapshot));
            LevelEntry = levelEntry;
            AppliedPurchasesThisLevel = appliedPurchasesThisLevel ?? throw new ArgumentNullException(nameof(appliedPurchasesThisLevel));
        }

        public int Level { get; }
        public Character StartOfLevelSnapshot { get; }
        public Character EndOfLevelSnapshot { get; }
        public LevelBuildEntry? LevelEntry { get; }
        public IReadOnlyList<AppliedPurchaseRecord> AppliedPurchasesThisLevel { get; }

        public AppliedPurchaseRecord? LastAppliedPurchaseThisLevel =>
            AppliedPurchasesThisLevel.Count == 0 ? null : AppliedPurchasesThisLevel[AppliedPurchasesThisLevel.Count - 1];
    }
}
