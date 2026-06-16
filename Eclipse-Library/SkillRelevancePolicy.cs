using System;

namespace Eclipse_Library
{
    public static class SkillRelevancePolicy
    {
        public static void Apply(Character character, SkillRulesConfig rules)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (rules is null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            rules.Validate();

            foreach (var skill in character.Skills.Values)
            {
                var isRestricted = rules.RestrictedSkills.Contains(skill.Name);
                skill.SetRestricted(isRestricted);

                if (rules.InitialRelevantSkills.Contains(skill.Name))
                {
                    skill.SetRelevant(true);
                    continue;
                }

                if (!rules.EnableIrrelevantToRelevantPromotion)
                {
                    continue;
                }

                if (skill.IsRelevantSkill)
                {
                    continue;
                }

                if (skill.CpInvested < rules.IrrelevantPromotionCpThreshold)
                {
                    continue;
                }

                if (isRestricted && rules.RestrictedSkillsBlockPromotion)
                {
                    var abilityName = rules.RestrictedSkillPromotionAbilityName;
                    if (!string.IsNullOrWhiteSpace(abilityName) && !character.HasAbility(abilityName))
                    {
                        continue;
                    }
                }

                skill.SetRelevant(true);
            }
        }
    }
}

