using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class AbilityScoreBreakdown
    {
        public AbilityScoreBreakdown(string ability, int baseScore, IEnumerable<AbilityScoreContribution> contributions)
        {
            Ability = string.IsNullOrWhiteSpace(ability) ? throw new ArgumentException("Ability is required.", nameof(ability)) : ability.Trim();
            BaseScore = baseScore;
            Contributions = (contributions ?? Enumerable.Empty<AbilityScoreContribution>()).ToList().AsReadOnly();
        }

        public string Ability { get; }
        public int BaseScore { get; }
        public IReadOnlyList<AbilityScoreContribution> Contributions { get; }
        public int Total => BaseScore + Contributions.Sum(x => x.Amount);
        public int Modifier => AbilityScores.GetModifier(Total);
    }
}
