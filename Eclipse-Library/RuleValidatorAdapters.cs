using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class RuleValidatorBuildStepAdapter : IBuildStepValidator, IIdentifiedValidator
    {
        private readonly IRuleValidator _inner;

        public RuleValidatorBuildStepAdapter(IRuleValidator inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public string ValidatorId => $"FINAL_STATE_ADAPTER::{_inner.GetType().Name}";

        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var stamp = new BuildDiagnosticContext(BuildDiagnosticStage.AtLevel, context.Level);
            foreach (var diagnostic in _inner.Validate(context.EndOfLevelSnapshot))
            {
                yield return diagnostic.WithContext(stamp);
            }
        }
    }

    public sealed class RuleValidatorFinalAdapter : IFinalBuildValidator, IIdentifiedValidator
    {
        private readonly IRuleValidator _inner;

        public RuleValidatorFinalAdapter(IRuleValidator inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public string ValidatorId => _inner.GetType().Name;

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var stamp = new BuildDiagnosticContext(BuildDiagnosticStage.Final, context.Build.TargetLevel);
            foreach (var diagnostic in _inner.Validate(context.FinalCharacterSnapshot))
            {
                yield return diagnostic.WithContext(stamp);
            }
        }
    }
}
