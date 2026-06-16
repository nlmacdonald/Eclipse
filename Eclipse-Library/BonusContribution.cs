using System;

namespace Eclipse_Library
{
    public sealed class BonusContribution
    {
        public BonusContribution(string target, int amount, BonusType type, string source, string? condition = null)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("Bonus target is required.", nameof(target));
            }

            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Bonus amount cannot be 0.");
            }

            Target = target.Trim();
            Amount = amount;
            Type = type;
            Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source.Trim();
            Condition = string.IsNullOrWhiteSpace(condition) ? null : condition.Trim();
        }

        public string Target { get; }
        public int Amount { get; }
        public BonusType Type { get; }
        public string Source { get; }
        public string? Condition { get; }

        public bool IsPenalty => Amount < 0;
        public bool Stacks => IsPenalty || BonusStackingPolicy.Stacks(Type);
    }
}
