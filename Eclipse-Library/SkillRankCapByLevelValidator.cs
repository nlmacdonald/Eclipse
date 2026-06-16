using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SkillRankCapByLevelValidator : IBuildStepValidator, IIdentifiedValidator
    {
        private readonly SkillRulesConfig _rules;

        public SkillRankCapByLevelValidator()
            : this(new SkillRulesConfig())
        {
        }

        public SkillRankCapByLevelValidator(SkillRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "SKILL_RANK_CAP_BY_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cap = _rules.GetSkillRankCap(context.Level);
            if (cap is null)
            {
                yield break;
            }

            foreach (var skill in context.EndOfLevelSnapshot.Skills.Values)
            {
                if (string.Equals(skill.Name, "Unassigned Skill Points", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (skill.Ranks <= cap.Value)
                {
                    continue;
                }

                var before = context.StartOfLevelSnapshot.GetSkillRanks(skill.Name);
                if (before > cap.Value)
                {
                    continue;
                }

                var purchase = context.AppliedPurchasesThisLevel
                    .LastOrDefault(x => x.PurchaseDescription.IndexOf(skill.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? context.LastAppliedPurchaseThisLevel;

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "SKILL_CAP_EXCEEDED",
                    $"Skill '{skill.Name}' has {skill.Ranks} rank(s), but the cap at level {context.Level} is {cap.Value}.",
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
