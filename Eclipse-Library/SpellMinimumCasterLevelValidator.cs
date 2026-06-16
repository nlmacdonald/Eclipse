using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class SpellMinimumCasterLevelValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        private readonly SpellRulesConfig _rules;

        public SpellMinimumCasterLevelValidator(SpellRulesConfig rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string ValidatorId => "SPELL_MINIMUM_CASTER_LEVEL";

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_rules.EnforceMinimumCasterLevelToCast)
            {
                yield break;
            }

            var character = context.FinalCharacterSnapshot;

            foreach (var spell in character.KnownSpells)
            {
                var required = (spell.Level * 2) - 1;
                var available = character.GetHighestCasterLevelAvailable(spell.Source);

                if (available >= required)
                {
                    continue;
                }

                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Warning,
                    "SPELL_CASTER_LEVEL_TOO_LOW",
                    $"Spell '{spell.Name}' requires caster level {required}, but the character only has caster level {available} available for {spell.Source}.",
                    new BuildDiagnosticContext(BuildDiagnosticStage.Final, context.Build.TargetLevel));
            }
        }
    }
}

