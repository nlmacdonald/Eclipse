using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class CpOverspendValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            if (character.SpentCp > character.TotalCp)
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "CP_OVERSPENT",
                    $"Character has overspent CP. Spent {character.SpentCp}, total available {character.TotalCp}, overspent by {character.SpentCp - character.TotalCp}.");
            }
        }
    }
}
