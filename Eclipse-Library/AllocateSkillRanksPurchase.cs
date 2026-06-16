using System;

namespace Eclipse_Library
{
    public sealed class AllocateSkillRanksPurchase : IPurchase
    {
        private readonly string _skillName;
        private readonly int _skillPointsSpent;
        private readonly bool _isRelevantSkill;
        private readonly decimal _rankMultiplier;

        public AllocateSkillRanksPurchase(
            string skillName,
            int skillPointsSpent,
            bool isRelevantSkill,
            decimal rankMultiplier)
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                throw new ArgumentException("Skill name is required.", nameof(skillName));
            }

            if (skillPointsSpent < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skillPointsSpent), skillPointsSpent, "Skill points spent must be >= 0.");
            }

            if (rankMultiplier < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rankMultiplier), rankMultiplier, "Rank multiplier must be >= 0.");
            }

            _skillName = skillName.Trim();
            _skillPointsSpent = skillPointsSpent;
            _isRelevantSkill = isRelevantSkill;
            _rankMultiplier = rankMultiplier;
        }

        public string Description => $"Allocate {_skillPointsSpent} skill point(s) to {_skillName}";

        public string SkillName => _skillName;

        public int SkillPointsSpent => _skillPointsSpent;

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_skillPointsSpent == 0)
            {
                return;
            }

            character.AddSkillRanks(_skillName, _skillPointsSpent, _isRelevantSkill, _rankMultiplier);
        }
    }
}
