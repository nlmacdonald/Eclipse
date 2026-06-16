using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class LevelBuildResult
    {
        public LevelBuildResult(int level)
        {
            Level = level;
        }

        public int Level { get; }
        public Character? Snapshot { get; set; }
        public List<string> AppliedPurchaseDescriptions { get; } = new();
        public List<BuildDiagnostic> Diagnostics { get; } = new();
    }
}

