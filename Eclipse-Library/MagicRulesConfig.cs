namespace Eclipse_Library
{
    public sealed class MagicRulesConfig
    {
        public bool EnableMagicLimitationsCosting { get; set; } = true;
        public int DefaultIncludedLimitationsCount { get; set; } = 2;
        public int AddedCostPerRemovedLimitation { get; set; } = 2;
        public int MinMagicCostPerLevelCp { get; set; } = 2;
    }
}

