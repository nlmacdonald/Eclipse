using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class FinalBuildValidationContext
    {
        public FinalBuildValidationContext(
            CharacterBuild build,
            Character finalCharacterSnapshot,
            IReadOnlyList<AppliedPurchaseRecord> appliedPurchases)
        {
            Build = build ?? throw new ArgumentNullException(nameof(build));
            FinalCharacterSnapshot = finalCharacterSnapshot ?? throw new ArgumentNullException(nameof(finalCharacterSnapshot));
            AppliedPurchases = appliedPurchases ?? throw new ArgumentNullException(nameof(appliedPurchases));
        }

        public CharacterBuild Build { get; }
        public Character FinalCharacterSnapshot { get; }
        public IReadOnlyList<AppliedPurchaseRecord> AppliedPurchases { get; }
    }
}

