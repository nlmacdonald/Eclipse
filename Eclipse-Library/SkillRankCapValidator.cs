using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class SkillRankCapValidator : IRuleValidator
    {
        private readonly SkillRulesConfig _rules;

        public SkillRankCapValidator()
            : this(new SkillRulesConfig())
        {
        }

        public SkillRankCapValidator(SkillRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            var cap = _rules.GetSkillRankCap(character.Level);
            if (cap is null)
            {
                yield break;
            }

            foreach (var skill in character.Skills.Values)
            {
                if (string.Equals(skill.Name, "Unassigned Skill Points", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (skill.Ranks > cap.Value)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "SKILL_CAP_EXCEEDED",
                        $"Skill '{skill.Name}' has {skill.Ranks} rank(s), but the cap is {cap.Value}.");
                }
            }
        }
    }
}
