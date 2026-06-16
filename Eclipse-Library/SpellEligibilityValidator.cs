using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class SpellEligibilityValidator : IRuleValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(Character character)
        {
            foreach (var spell in character.KnownSpells)
            {
                var highestAvailable = character.GetHighestSpellLevelAvailable(spell.Source);
                if (highestAvailable == 0)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "SPELL_SOURCE_UNAVAILABLE",
                        $"Spell '{spell.Name}' cannot be learned because the character has no matching {spell.Source} magic progression.");
                    continue;
                }

                if (spell.Level > highestAvailable)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "SPELL_LEVEL_TOO_HIGH",
                        $"Spell '{spell.Name}' is level {spell.Level}, but the character currently only supports up to level {highestAvailable} {spell.Source} spells.");
                }
            }
        }
    }
}
