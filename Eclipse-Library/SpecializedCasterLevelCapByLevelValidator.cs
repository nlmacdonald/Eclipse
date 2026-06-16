using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SpecializedCasterLevelCapByLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "SPECIALIZED_CASTER_LEVEL_CAP_BY_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cap = context.Level + 3;

            foreach (var entry in context.EndOfLevelSnapshot.SpecializedCasterLevels)
            {
                if (entry.Value <= cap)
                {
                    continue;
                }

                var before = context.StartOfLevelSnapshot.GetSpecializedCasterLevel(entry.Key);
                if (before > cap)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => x.PurchaseDescription.IndexOf(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "SPECIALIZED_CASTER_LEVEL_CAP_EXCEEDED",
                    $"Specialized caster level for '{entry.Key}' is {entry.Value}, but the cap at level {context.Level} is {cap}.",
                    new BuildDiagnosticContext(
                        BuildDiagnosticStage.AtLevel,
                        context.Level,
                        purchaseId: purchase?.PurchaseId,
                        purchaseDescription: purchase?.PurchaseDescription,
                        purchaseIndex: purchase?.PurchaseIndex));
            }
        }
    }
}

