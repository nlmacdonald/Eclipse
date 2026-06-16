using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class MagicLevelCapByCharacterLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "MAGIC_LEVEL_CAP_BY_CHARACTER_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cap = context.Level + 3;

            foreach (var entry in context.EndOfLevelSnapshot.MagicLevels)
            {
                if (entry.Value <= cap)
                {
                    continue;
                }

                var before = context.StartOfLevelSnapshot.GetMagicLevel(entry.Key);
                if (before > cap)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => (x.Purchase is BuyMagicLevelsPurchase || x.Purchase is BuyMagicLevelsWithLimitationsPurchase) &&
                                        x.PurchaseDescription.IndexOf(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "MAGIC_LEVEL_CAP_EXCEEDED",
                    $"Magic progression '{entry.Key}' has {entry.Value} magic level(s), but the cap at level {context.Level} is {cap}.",
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
