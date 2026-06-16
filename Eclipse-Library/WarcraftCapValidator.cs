using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class WarcraftCapValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            if (character.Warcraft > character.MaxWarcraft)
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "WARCRAFT_CAP_EXCEEDED",
                    $"Warcraft is {character.Warcraft}, but the cap is {character.MaxWarcraft}.");
            }
        }
    }
}
