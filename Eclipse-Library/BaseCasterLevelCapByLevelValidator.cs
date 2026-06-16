using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BaseCasterLevelCapByLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "BASE_CASTER_LEVEL_CAP_BY_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cap = context.Level + 3;
            var value = context.EndOfLevelSnapshot.BaseCasterLevel;

            if (value <= cap)
            {
                yield break;
            }

            if (context.StartOfLevelSnapshot.BaseCasterLevel > cap)
            {
                yield break;
            }

            var purchase = context.AppliedPurchasesThisLevel
                .LastOrDefault(x => x.PurchaseDescription.IndexOf("Base Caster Level", StringComparison.OrdinalIgnoreCase) >= 0)
                ?? context.LastAppliedPurchaseThisLevel;

            yield return new BuildDiagnostic(
                BuildDiagnosticSeverity.Error,
                "CASTER_LEVEL_CAP_EXCEEDED",
                $"Base Caster Level is {value}, but the cap at level {context.Level} is {cap}.",
                new BuildDiagnosticContext(
                    BuildDiagnosticStage.AtLevel,
                    context.Level,
                    purchaseId: purchase?.PurchaseId,
                    purchaseDescription: purchase?.PurchaseDescription,
                    purchaseIndex: purchase?.PurchaseIndex));
        }
    }
}
