using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SkillPointAllowanceValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        private const string UnassignedSkillPointsName = "Unassigned Skill Points";
        private readonly SkillRulesConfig _rules;

        public SkillPointAllowanceValidator()
            : this(new SkillRulesConfig())
        {
        }

        public SkillPointAllowanceValidator(SkillRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "SKILL_POINT_ALLOWANCE";

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var character = context.FinalCharacterSnapshot;
            var spent = context.AppliedPurchases
                .Where(x => x.Success)
                .Select(x => x.Purchase)
                .OfType<AllocateSkillRanksPurchase>()
                .Sum(x => x.SkillPointsSpent);

            var templateSkillPoints = GetTemplateSkillPointPool(character);
            var intelligenceBonus = GetIntelligenceSkillPointBonus(character);
            var available = templateSkillPoints + character.BonusSkillPoints + intelligenceBonus;

            if (spent <= available)
            {
                yield break;
            }

            yield return new BuildDiagnostic(
                BuildDiagnosticSeverity.Error,
                "SKILL_POINTS_OVERSPENT",
                $"Character has allocated {spent} skill point(s), but only {available} skill point(s) are available ({templateSkillPoints} template, {character.BonusSkillPoints} bonus, {intelligenceBonus} Intelligence).",
                new BuildDiagnosticContext(BuildDiagnosticStage.Final, context.Build.TargetLevel));
        }

        private static int GetTemplateSkillPointPool(Character character)
        {
            return character.Skills.TryGetValue(UnassignedSkillPointsName, out var entry)
                ? entry.CpInvested
                : 0;
        }

        private int GetIntelligenceSkillPointBonus(Character character)
        {
            var modifier = AbilityScores.GetModifier(character.AbilityScores.Intelligence);
            var level = Math.Max(1, character.Level);

            if (!_rules.UseFirstCharacterLevelSkillPointMultiplier)
            {
                return modifier * level;
            }

            return (modifier * 4) + (modifier * (level - 1));
        }
    }
}
