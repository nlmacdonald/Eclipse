using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SpecializedCasterLevelProgressionValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "SPECIALIZED_CASTER_LEVEL_PROGRESSION";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var entry in context.EndOfLevelSnapshot.SpecializedCasterLevels)
            {
                if (entry.Value <= 0 || context.EndOfLevelSnapshot.GetMagicLevel(entry.Key) > 0)
                {
                    continue;
                }

                var before = context.StartOfLevelSnapshot.GetSpecializedCasterLevel(entry.Key);
                if (before > 0)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => x.PurchaseDescription.IndexOf(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "SPECIALIZED_CASTER_LEVEL_WITHOUT_PROGRESSION",
                    $"Specialized caster level for '{entry.Key}' requires at least one magic level in that progression.",
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
