using System;

namespace Eclipse_Library
{
    public static class MagicLevelCostCalculator
    {
        public static int GetCostPerLevelCp(int chartCostPerLevelCp, int appliedLimitationsCount, MagicRulesConfig rules)
        {
            if (rules is null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (chartCostPerLevelCp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chartCostPerLevelCp), chartCostPerLevelCp, "Chart cost must be >= 0.");
            }

            if (appliedLimitationsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(appliedLimitationsCount), appliedLimitationsCount, "Applied limitations must be >= 0.");
            }

            var cost = chartCostPerLevelCp;
            var baseline = rules.DefaultIncludedLimitationsCount;

            if (appliedLimitationsCount < baseline)
            {
                var removed = baseline - appliedLimitationsCount;
                cost += removed * rules.AddedCostPerRemovedLimitation;
            }
            else if (appliedLimitationsCount > baseline)
            {
                var extra = appliedLimitationsCount - baseline;
                var reductionPerExtra = (int)Math.Ceiling(chartCostPerLevelCp / 6.0);
                cost -= extra * reductionPerExtra;
            }

            if (cost < rules.MinMagicCostPerLevelCp)
            {
                cost = rules.MinMagicCostPerLevelCp;
            }

            return cost;
        }
    }
}

