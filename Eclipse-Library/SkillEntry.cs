using System;

namespace Eclipse_Library
{
    public sealed class SkillEntry
    {
        public SkillEntry(string name, decimal ranks, bool isRelevantSkill)
        {
            Name = name;
            Ranks = ranks;
            IsRelevantSkill = isRelevantSkill;
            IrrelevantRankMultiplier = 0.5m;
        }

        internal SkillEntry(string name, decimal ranks, bool isRelevantSkill, bool isRestrictedSkill, int cpInvested, decimal irrelevantRankMultiplier)
            : this(name, ranks, isRelevantSkill)
        {
            IsRestrictedSkill = isRestrictedSkill;
            CpInvested = cpInvested;
            IrrelevantRankMultiplier = irrelevantRankMultiplier;
        }

        public string Name { get; }
        public decimal Ranks { get; private set; }
        public bool IsRelevantSkill { get; private set; }
        public bool IsRestrictedSkill { get; private set; }
        public int CpInvested { get; private set; }
        public decimal IrrelevantRankMultiplier { get; private set; }

        public void AddRanks(int amount, int cpSpent, decimal irrelevantRankMultiplier)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot add a negative number of ranks.");
            }

            if (cpSpent < 0)
            {
                throw new InvalidOperationException("Cannot spend a negative CP amount on a skill.");
            }

            if (irrelevantRankMultiplier < 0)
            {
                throw new InvalidOperationException("Irrelevant skill rank multiplier must be >= 0.");
            }

            IrrelevantRankMultiplier = irrelevantRankMultiplier;
            CpInvested += cpSpent;
            Ranks += IsRelevantSkill ? amount : amount * irrelevantRankMultiplier;
        }

        public void SetRelevant(bool isRelevant)
        {
            IsRelevantSkill = isRelevant;
            Ranks = isRelevant ? CpInvested : CpInvested * IrrelevantRankMultiplier;
        }

        public void SetRestricted(bool isRestricted)
        {
            IsRestrictedSkill = isRestricted;
        }
    }
}
