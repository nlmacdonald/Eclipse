using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class SkillRulesConfig
    {
        public int IrrelevantPromotionCpThreshold { get; set; } = 6;
        public bool EnableIrrelevantToRelevantPromotion { get; set; } = true;
        public bool EnforceSkillRankCap { get; set; } = true;
        public int SkillRankCapBonus { get; set; }
        public bool UnlimitedSkillRankCap { get; set; }
        public decimal IrrelevantSkillRankMultiplier { get; set; } = 0.5m;
        public bool UseFirstCharacterLevelSkillPointMultiplier { get; set; } = true;

        public bool RestrictedSkillsBlockPromotion { get; set; } = true;
        public string RestrictedSkillPromotionAbilityName { get; set; } = "Occult Skill (Improved)";

        public bool EnableSkillSpecialties { get; set; } = true;

        public HashSet<string> InitialRelevantSkills { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> RestrictedSkills { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Validate()
        {
            if (IrrelevantPromotionCpThreshold < 0)
            {
                throw new InvalidOperationException($"{nameof(IrrelevantPromotionCpThreshold)} must be >= 0.");
            }

            if (SkillRankCapBonus < 0)
            {
                throw new InvalidOperationException($"{nameof(SkillRankCapBonus)} must be >= 0.");
            }

            if (IrrelevantSkillRankMultiplier < 0)
            {
                throw new InvalidOperationException($"{nameof(IrrelevantSkillRankMultiplier)} must be >= 0.");
            }
        }

        public int? GetSkillRankCap(int characterLevel)
        {
            Validate();

            if (!EnforceSkillRankCap || UnlimitedSkillRankCap)
            {
                return null;
            }

            return Math.Max(1, characterLevel) + 3 + SkillRankCapBonus;
        }
    }
}
