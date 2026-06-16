using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class MagicLevelsPerProgressionPerLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        public string ValidatorId => "MAGIC_LEVELS_PER_PROGRESSION_PER_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var entry in context.EndOfLevelSnapshot.MagicLevels)
            {
                var before = context.StartOfLevelSnapshot.GetMagicLevel(entry.Key);
                var after = entry.Value;
                var boughtThisLevel = after - before;

                if (boughtThisLevel <= context.EndOfLevelSnapshot.MaxMagicLevelsBoughtPerLevel)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => (x.Purchase is BuyMagicLevelsPurchase || x.Purchase is BuyMagicLevelsWithLimitationsPurchase) &&
                                        x.PurchaseDescription.IndexOf(entry.Key.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Warning,
                    "MAGIC_LEVELS_PER_LEVEL_EXCEEDED",
                    $"Magic progression '{entry.Key}' bought {boughtThisLevel} magic level(s) at level {context.Level}. Eclipse normally allows buying no more than {context.EndOfLevelSnapshot.MaxMagicLevelsBoughtPerLevel} magic levels in one progression per level.",
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
