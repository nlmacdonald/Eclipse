using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public static class BonusStackingPolicy
    {
        public static bool Stacks(BonusType type)
        {
            return type == BonusType.Dodge
                || type == BonusType.Circumstance
                || type == BonusType.Untyped;
        }

        public static int CalculateTotal(IEnumerable<BonusContribution> contributions)
        {
            return GetAppliedContributions(contributions).Sum(x => x.Amount);
        }

        public static IReadOnlyList<BonusContribution> GetAppliedContributions(IEnumerable<BonusContribution> contributions)
        {
            var all = (contributions ?? Enumerable.Empty<BonusContribution>())
                .Where(x => x != null)
                .ToList();

            var applied = all
                .Where(x => x.IsPenalty || Stacks(x.Type))
                .ToList();

            applied.AddRange(all
                .Where(x => !x.IsPenalty && !Stacks(x.Type))
                .GroupBy(x => x.Type)
                .Select(x => x.OrderByDescending(b => b.Amount).First()));

            return applied.AsReadOnly();
        }
    }
}
