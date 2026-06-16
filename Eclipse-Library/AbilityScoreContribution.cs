using System;

namespace Eclipse_Library
{
    public sealed class AbilityScoreContribution
    {
        public AbilityScoreContribution(string source, int amount, BonusType type)
        {
            Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source.Trim();
            Amount = amount;
            Type = type;
        }

        public string Source { get; }
        public int Amount { get; }
        public BonusType Type { get; }
    }
}
