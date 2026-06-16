namespace Eclipse_Library
{
    public sealed class BuildJournalBuildOptions
    {
        public bool ValidateAfterEachPurchase { get; set; }
        public bool ValidateAtEachLevel { get; set; } = true;
        public bool ValidateAtFinal { get; set; } = true;
        public bool CaptureLevelSnapshots { get; set; }
    }
}

