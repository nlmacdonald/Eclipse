using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class JournalBuildResult
    {
        public JournalBuildResult(Character character)
        {
            Character = character;
        }

        public Character Character { get; }
        public List<LevelBuildResult> Levels { get; } = new();
        public List<BuildDiagnostic> Diagnostics { get; } = new();
        public bool HasErrors => Diagnostics.Any(x => x.Severity == BuildDiagnosticSeverity.Error);
    }
}

