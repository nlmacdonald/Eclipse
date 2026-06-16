using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class CpOverspendByLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "CP_OVERSPEND_BY_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var before = context.StartOfLevelSnapshot;
            var after = context.EndOfLevelSnapshot;

            if (after.SpentCp <= after.TotalCp)
            {
                yield break;
            }

            if (before.SpentCp > before.TotalCp)
            {
                yield break;
            }

            var purchase = context.LastAppliedPurchaseThisLevel;
            yield return new BuildDiagnostic(
                BuildDiagnosticSeverity.Error,
                "CP_OVERSPENT",
                $"Character has overspent CP at level {context.Level}. Spent {after.SpentCp}, total available {after.TotalCp}, overspent by {after.SpentCp - after.TotalCp}.",
                new BuildDiagnosticContext(
                    BuildDiagnosticStage.AtLevel,
                    context.Level,
                    purchaseId: purchase?.PurchaseId,
                    purchaseDescription: purchase?.PurchaseDescription,
                    purchaseIndex: purchase?.PurchaseIndex));
        }
    }
}
