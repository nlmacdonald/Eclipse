using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class MagicProgressionDefinition
    {
        public MagicProgressionDefinition(
            MagicProgressionType type,
            SpellSource source,
            int costPerLevelCp,
            bool includesSpecializedCasterLevel)
        {
            Type = type;
            Source = source;
            ChartCostPerLevelCp = costPerLevelCp;
            IncludesSpecializedCasterLevel = includesSpecializedCasterLevel;
        }

        public MagicProgressionType Type { get; }
        public SpellSource Source { get; }
        public int ChartCostPerLevelCp { get; }
        public int CostPerLevelCp => ChartCostPerLevelCp;
        public int SpellProgressionCostPerLevelCp =>
            IncludesSpecializedCasterLevel
                ? Math.Max(0, ChartCostPerLevelCp - SpecializedCasterLevelCostCp)
                : ChartCostPerLevelCp;
        public int SpecializedCasterLevelCostCp => IncludesSpecializedCasterLevel ? 3 : 0;
        public bool IncludesSpecializedCasterLevel { get; }
    }
}
