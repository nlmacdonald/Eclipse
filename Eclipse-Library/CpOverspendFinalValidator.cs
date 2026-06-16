using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class CpOverspendFinalValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        public string ValidatorId => "CP_OVERSPEND_FINAL";

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var character = context.FinalCharacterSnapshot;

            if (character.SpentCp <= character.TotalCp)
            {
                yield break;
            }

            yield return new BuildDiagnostic(
                BuildDiagnosticSeverity.Error,
                "CP_OVERSPENT",
                $"Character has overspent CP overall. Spent {character.SpentCp}, total available {character.TotalCp}, overspent by {character.SpentCp - character.TotalCp}.",
                new BuildDiagnosticContext(BuildDiagnosticStage.Final, context.Build.TargetLevel));
        }
    }
}
