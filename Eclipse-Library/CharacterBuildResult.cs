using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eclipse_Library
{
    public sealed class CharacterBuildResult
    {
        public CharacterBuildResult(Character character)
        {
            Character = character;
        }

        public Character Character { get; }
        public List<AppliedPurchaseRecord> AppliedPurchases { get; set; } = new();
        public List<string> AppliedPurchaseDescriptions { get; } = new();
        public List<BuildDiagnostic> Diagnostics { get; set; } = new();
        public bool HasErrors => Diagnostics.Any(x => x.Severity == BuildDiagnosticSeverity.Error);
    }
}
