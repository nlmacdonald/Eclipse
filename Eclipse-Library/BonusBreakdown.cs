using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BonusBreakdown
    {
        public BonusBreakdown(string target, IEnumerable<BonusContribution> contributions)
        {
            Target = target;
            Contributions = contributions.ToList().AsReadOnly();
            Total = BonusStackingPolicy.CalculateTotal(Contributions);
        }

        public string Target { get; }
        public IReadOnlyList<BonusContribution> Contributions { get; }
        public int Total { get; }

        public IReadOnlyList<BonusContribution> AppliedContributions =>
            BonusStackingPolicy.GetAppliedContributions(Contributions);
    }
}
