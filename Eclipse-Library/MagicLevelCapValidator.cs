using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class MagicLevelCapValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            foreach (var entry in character.MagicLevels)
            {
                if (entry.Value > character.MaxMagicLevelsPerProgression)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "MAGIC_LEVEL_CAP_EXCEEDED",
                        $"Magic progression '{entry.Key}' has {entry.Value} magic level(s), but the cap is {character.MaxMagicLevelsPerProgression}.");
                }
            }
        }
    }
}
