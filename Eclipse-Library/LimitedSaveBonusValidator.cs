using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class LimitedSaveBonusValidator : IBuildStepValidator, IIdentifiedValidator
    {
        private readonly SaveRulesConfig _rules;

        public LimitedSaveBonusValidator(SaveRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "LIMITED_SAVES";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var purchases = context.AppliedPurchasesThisLevel
                .Where(x => x.Purchase is BuyLimitedSaveBonusPurchase)
                .ToList();

            if (_rules.EnableLimitedSaves)
            {
                yield break;
            }

            foreach (var record in purchases)
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Warning,
                    "LIMITED_SAVES_DISABLED",
                    "Limited/modified saves are disabled by build configuration.",
                    new BuildDiagnosticContext(
                        BuildDiagnosticStage.AtLevel,
                        context.Level,
                        purchaseId: record.PurchaseId,
                        purchaseDescription: record.PurchaseDescription,
                        purchaseIndex: record.PurchaseIndex));
            }
        }
    }
}

