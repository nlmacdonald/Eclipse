using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class SelectedFeatAllowanceValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        private readonly Func<int, int> _featsGrantedByLevel;

        public SelectedFeatAllowanceValidator()
            : this(GetDefaultFeatsGrantedByLevel)
        {
        }

        public SelectedFeatAllowanceValidator(Func<int, int> featsGrantedByLevel)
        {
            _featsGrantedByLevel = featsGrantedByLevel ?? throw new ArgumentNullException(nameof(featsGrantedByLevel));
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
            var regularFeats = Math.Max(0, _featsGrantedByLevel(character.Level));
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

        private static int GetDefaultFeatsGrantedByLevel(int level)
        {
            level = Math.Max(1, level);
            return 1 + ((level - 1) / 3);
        }
    }
}
