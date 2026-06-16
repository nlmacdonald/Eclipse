using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class BuildContext
    {
        public BuildContext(Character character, int targetLevel)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));

            if (targetLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLevel), targetLevel, "Target level must be >= 1.");
            }

            TargetLevel = targetLevel;
        }

        public Character Character { get; }
        public int TargetLevel { get; }
        public int CurrentLevel { get; internal set; } = 1;

        public List<AppliedPurchaseRecord> AppliedPurchases { get; } = new();
        public List<BuildDiagnostic> Diagnostics { get; } = new();
    }
}

