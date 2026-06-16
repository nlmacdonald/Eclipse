using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BaseCasterLevelCapValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            if (character.BaseCasterLevel > character.MaxBaseCasterLevel)
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "CASTER_LEVEL_CAP_EXCEEDED",
                    $"Base Caster Level is {character.BaseCasterLevel}, but the cap is {character.MaxBaseCasterLevel}.");
            }
        }
    }
}
