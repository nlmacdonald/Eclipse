using System;

namespace Eclipse_Library
{
    public sealed class BuildDiagnosticContext
    {
        public BuildDiagnosticContext(
            BuildDiagnosticStage stage,
            int level,
            Guid? purchaseId = null,
            string? purchaseDescription = null,
            int? purchaseIndex = null,
            string? validatorId = null)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            Stage = stage;
            Level = level;
            PurchaseId = purchaseId;
            PurchaseDescription = purchaseDescription;
            PurchaseIndex = purchaseIndex;
            ValidatorId = validatorId;
        }

        public BuildDiagnosticStage Stage { get; }
        public int Level { get; }
        public Guid? PurchaseId { get; }
        public string? PurchaseDescription { get; }
        public int? PurchaseIndex { get; }
        public string? ValidatorId { get; }
    }
}
