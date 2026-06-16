using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class AbilityPrerequisiteValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            foreach (var ability in character.Abilities)
            {
                foreach (var prerequisite in ability.PrerequisiteAbilityIds)
                {
                    if (!character.HasAbilityId(prerequisite))
                    {
                        yield return new BuildDiagnostic(
                            BuildDiagnosticSeverity.Error,
                            "ABILITY_PREREQUISITE_MISSING",
                            $"Ability '{ability.Name}' requires ability id '{prerequisite}'.");
                    }
                }
            }
        }
    }
}
