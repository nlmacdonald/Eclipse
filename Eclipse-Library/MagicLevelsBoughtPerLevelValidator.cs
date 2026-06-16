using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class MagicLevelsBoughtPerLevelValidator : IBuildStepValidator
    {
        public IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var entry in context.EndOfLevelSnapshot.MagicLevels)
            {
                var before = context.StartOfLevelSnapshot.GetMagicLevel(entry.Key);
                var after = entry.Value;
                var boughtThisLevel = after - before;

                if (boughtThisLevel > context.EndOfLevelSnapshot.MaxMagicLevelsBoughtPerLevel)
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Warning,
                        "MAGIC_LEVELS_PER_LEVEL_EXCEEDED",
                        $"Magic progression '{entry.Key}' bought {boughtThisLevel} magic level(s) at level {context.Level}. Eclipse normally allows buying no more than {context.EndOfLevelSnapshot.MaxMagicLevelsBoughtPerLevel} magic levels in one progression per level.",
                        new BuildDiagnosticContext(BuildDiagnosticStage.AtLevel, context.Level));
                }
            }
        }
    }
}
