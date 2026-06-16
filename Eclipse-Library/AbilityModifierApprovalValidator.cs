using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class AbilityModifierApprovalValidator : IBuildStepValidator, IIdentifiedValidator
    {
        private readonly AbilityRulesConfig _rules;

        public AbilityModifierApprovalValidator(AbilityRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "ABILITY_MODIFIER_APPROVAL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modifiedAbilityPurchases = context.AppliedPurchasesThisLevel
                .Where(x => x.Purchase is BuyModifiedAbilityPurchase)
                .ToList();

            if (!_rules.EnableAbilityModifiers)
            {
                foreach (var record in modifiedAbilityPurchases)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Warning,
                        "ABILITY_MODIFIERS_DISABLED",
                        "Ability modifiers (Specialized/Corrupted/etc.) are disabled by build configuration.",
                        new BuildDiagnosticContext(
                            BuildDiagnosticStage.AtLevel,
                            context.Level,
                            purchaseId: record.PurchaseId,
                            purchaseDescription: record.PurchaseDescription,
                            purchaseIndex: record.PurchaseIndex));
                }

                yield break;
            }

            if (!_rules.RequireGmApprovalForSpecializedOrCorrupted)
            {
                yield break;
            }

            // We inspect the end-of-level snapshot for the purchased ability details (since purchases already applied).
            foreach (var record in modifiedAbilityPurchases)
            {
                var matching = context.EndOfLevelSnapshot.PurchasedAbilities
                    .LastOrDefault(a =>
                        record.PurchaseDescription.IndexOf(a.Definition.Name, StringComparison.OrdinalIgnoreCase) >= 0);

                if (matching is null || !matching.HasSpecializedOrCorrupted)
                {
                    continue;
                }

                if (matching.GmApproved)
                {
                    continue;
                }

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Warning,
                    "GM_APPROVAL_REQUIRED",
                    "This ability uses Specialized/Corrupted modifiers and should be approved by the GM.",
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

