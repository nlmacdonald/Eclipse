using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuySkillRanksPurchase : IPurchase
    {
        private readonly string _skillName;
        private readonly int _ranks;
        private readonly bool _isRelevantSkill;
        private readonly decimal _irrelevantRankMultiplier;

        public BuySkillRanksPurchase(string skillName, int ranks, bool isRelevantSkill)
            : this(skillName, ranks, isRelevantSkill, irrelevantRankMultiplier: 0.5m)
        {
        }

        public BuySkillRanksPurchase(string skillName, int ranks, bool isRelevantSkill, decimal irrelevantRankMultiplier)
        {
            _skillName = skillName;
            _ranks = ranks;
            _isRelevantSkill = isRelevantSkill;
            _irrelevantRankMultiplier = irrelevantRankMultiplier;
        }

        public string Description => $"Buy {_ranks} rank(s) in {_skillName}";

        public void Apply(Character character)
        {
            character.SpendCp(_ranks);
            character.AddSkillRanks(_skillName, _ranks, _isRelevantSkill, _irrelevantRankMultiplier);
        }
    }
}
