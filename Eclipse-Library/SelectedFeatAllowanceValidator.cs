using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SelectedFeatAllowanceValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        private readonly LevelProgressionRulesConfig _levelProgression;

        public SelectedFeatAllowanceValidator()
            : this(new LevelProgressionRulesConfig())
        {
        }

        public SelectedFeatAllowanceValidator(LevelProgressionRulesConfig levelProgression)
        {
            _levelProgression = levelProgression ?? throw new ArgumentNullException(nameof(levelProgression));
        }

        public string ValidatorId => "SELECTED_FEAT_ALLOWANCE";

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var character = context.FinalCharacterSnapshot;
            var selectedCount = character.SelectedFeats.Values.Sum();
            var regularFeats = Math.Max(0, _levelProgression.GetFeatsGrantedByLevel(character.Level));
            var availableFeats = regularFeats + character.BonusFeats;

            if (selectedCount <= availableFeats)
            {
                yield break;
            }

            yield return new BuildDiagnostic(
                BuildDiagnosticSeverity.Error,
                "FEAT_SELECTION_OVERSPENT",
                $"Character has selected {selectedCount} feat(s), but only {availableFeats} feat slot(s) are available ({regularFeats} level-granted, {character.BonusFeats} bonus).",
                new BuildDiagnosticContext(BuildDiagnosticStage.Final, context.Build.TargetLevel));
        }

    }
}
