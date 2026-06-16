using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SkillSpecialtyValidator : IBuildStepValidator, IIdentifiedValidator
    {
        private readonly SkillRulesConfig _rules;

        public SkillSpecialtyValidator(SkillRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "SKILL_SPECIALTIES";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var specialtyPurchases = context.AppliedPurchasesThisLevel
                .Where(x => x.Purchase is BuySkillSpecialtyPurchase)
                .ToList();

            if (!_rules.EnableSkillSpecialties)
            {
                foreach (var record in specialtyPurchases)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Warning,
                        "SKILL_SPECIALTIES_DISABLED",
                        "Skill specialties are disabled by build configuration.",
                        new BuildDiagnosticContext(
                            BuildDiagnosticStage.AtLevel,
                            context.Level,
                            purchaseId: record.PurchaseId,
                            purchaseDescription: record.PurchaseDescription,
                            purchaseIndex: record.PurchaseIndex));
                }

                yield break;
            }

            // Simple duplicate check per skill + specialty name.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in specialtyPurchases)
            {
                var key = record.PurchaseDescription;
                if (seen.Add(key))
                {
                    continue;
                }

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Warning,
                    "SKILL_SPECIALTY_DUPLICATE",
                    "This specialty appears to be purchased more than once.",
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

