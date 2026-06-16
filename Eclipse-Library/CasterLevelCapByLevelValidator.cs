using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class CasterLevelCapByLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "CASTER_LEVEL_CAP_BY_SOURCE";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cap = context.Level + 3;

            foreach (var entry in context.EndOfLevelSnapshot.CasterLevelsBySource)
            {
                if (entry.Value <= cap)
                {
                    continue;
                }

                var before = context.StartOfLevelSnapshot.GetCasterLevel(entry.Key);
                if (before > cap)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => x.Purchase is BuyCasterLevelPurchase &&
                                        x.PurchaseDescription.IndexOf(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "CASTER_LEVEL_CAP_EXCEEDED",
                    $"Caster level for {entry.Key} is {entry.Value}, but the cap at level {context.Level} is {cap}.",
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

