using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuildReplayer
    {
        private readonly IReadOnlyCollection<IBuildStepValidator> _stepValidators;
        private readonly IReadOnlyCollection<IFinalBuildValidator> _finalValidators;

        public BuildReplayer(
            IEnumerable<IBuildStepValidator> stepValidators,
            IEnumerable<IFinalBuildValidator> finalValidators)
        {
            _stepValidators = (stepValidators ?? throw new ArgumentNullException(nameof(stepValidators))).ToList().AsReadOnly();
            _finalValidators = (finalValidators ?? throw new ArgumentNullException(nameof(finalValidators))).ToList().AsReadOnly();
        }

        public CharacterBuildResult Replay(
            CharacterBuild build,
            ICpProgression? cpProgression = null,
            BuildReplayOptions? options = null,
            BuildRulesConfig? rulesConfig = null)
        {
            if (build is null)
            {
                throw new ArgumentNullException(nameof(build));
            }

            cpProgression ??= new EclipseCpProgression();
            options ??= new BuildReplayOptions();
            rulesConfig ??= new BuildRulesConfig();

            var character = Character.CreateLevelOne(
                build.Seed.Name,
                build.Seed.AbilityScores,
                totalCp: cpProgression.GetTotalCpAtLevel(1),
                size: build.Seed.Size,
                race: build.Seed.Race,
                raceAbilityCatalog: build.Seed.RaceAbilityCatalog);
            ApplyLevelAbilityScoreAdjustments(character, build.Seed, build.TargetLevel);

            var context = new BuildContext(character, build.TargetLevel);

            for (var level = 1; level <= build.TargetLevel; level++)
            {
                context.CurrentLevel = level;

                if (level > 1)
                {
                    var newTotalCp = cpProgression.GetTotalCpAtLevel(level);
                    var cpGained = newTotalCp - character.TotalCp;
                    character.AdvanceLevel(cpGained);
                }

                var appliedCountBeforeLevel = context.AppliedPurchases.Count;
                var startSnapshot = character.Clone();

                var purchases = build.TryGetLevel(level, out var levelEntry)
                    ? levelEntry!.Purchases
                    : Array.Empty<BuildPurchase>();

                for (var purchaseIndex = 0; purchaseIndex < purchases.Count; purchaseIndex++)
                {
                    var purchaseEntry = purchases[purchaseIndex];
                    var purchase = purchaseEntry.Purchase;
                    try
                    {
                        purchase.Apply(character);
                        context.AppliedPurchases.Add(new AppliedPurchaseRecord(
                            purchaseId: purchaseEntry.Id,
                            sourceLevel: level,
                            purchaseIndex: purchaseEntry.Index,
                            purchaseDescription: purchaseEntry.Description,
                            success: true,
                            purchase: purchase));
                    }
                    catch (Exception ex)
                    {
                        context.AppliedPurchases.Add(new AppliedPurchaseRecord(
                            purchaseId: purchaseEntry.Id,
                            sourceLevel: level,
                            purchaseIndex: purchaseEntry.Index,
                            purchaseDescription: purchaseEntry.Description,
                            success: false,
                            purchase: purchase,
                            failureMessage: ex.Message));

                        context.Diagnostics.Add(new BuildDiagnostic(
                            BuildDiagnosticSeverity.Error,
                            "ENGINE_APPLY_FAILED",
                            $"{purchaseEntry.Description} could not be applied: {ex.Message}",
                            new BuildDiagnosticContext(
                                BuildDiagnosticStage.AtLevel,
                                level,
                                purchaseId: purchaseEntry.Id,
                                purchaseDescription: purchaseEntry.Description,
                                purchaseIndex: purchaseEntry.Index,
                                validatorId: "ENGINE_APPLY")));
                    }
                }

                SkillRelevancePolicy.Apply(character, rulesConfig.Skills);

                if (options.RunIncrementalValidation)
                {
                    var endSnapshot = character.Clone();
                    var appliedThisLevel = context.AppliedPurchases
                        .Skip(appliedCountBeforeLevel)
                        .ToList()
                        .AsReadOnly();

                    var stepContext = new BuildStepValidationContext(
                        level,
                        startSnapshot,
                        endSnapshot,
                        levelEntry,
                        appliedThisLevel);

                    foreach (var validator in _stepValidators)
                    {
                        var validatorId = GetValidatorId(validator);
                        foreach (var diagnostic in validator.Validate(stepContext))
                        {
                            context.Diagnostics.Add(EnsureStamped(diagnostic, BuildDiagnosticStage.AtLevel, level, validatorId));
                        }
                    }
                }
            }

            if (options.RunFinalValidation)
            {
                var finalSnapshotForValidation = character.Clone();
                var finalContext = new FinalBuildValidationContext(
                    build,
                    finalSnapshotForValidation,
                    context.AppliedPurchases.AsReadOnly());

                foreach (var validator in _finalValidators)
                {
                    var validatorId = GetValidatorId(validator);
                    foreach (var diagnostic in validator.Validate(finalContext))
                    {
                        context.Diagnostics.Add(EnsureStamped(diagnostic, BuildDiagnosticStage.Final, build.TargetLevel, validatorId));
                    }
                }
            }

            var finalCharacterSnapshot = character.Clone();
            var result = new CharacterBuildResult(finalCharacterSnapshot)
            {
                AppliedPurchases = context.AppliedPurchases,
                Diagnostics = context.Diagnostics,
            };

            foreach (var record in context.AppliedPurchases.Where(x => x.Success))
            {
                result.AppliedPurchaseDescriptions.Add(record.PurchaseDescription);
            }

            return result;
        }

        private static BuildDiagnostic EnsureStamped(
            BuildDiagnostic diagnostic,
            BuildDiagnosticStage stage,
            int level,
            string validatorId)
        {
            if (diagnostic is null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            if (string.IsNullOrWhiteSpace(validatorId))
            {
                validatorId = "UNKNOWN_VALIDATOR";
            }

            if (diagnostic.Context is null)
            {
                return diagnostic.WithContext(new BuildDiagnosticContext(stage, level, validatorId: validatorId));
            }

            if (!string.IsNullOrWhiteSpace(diagnostic.Context.ValidatorId))
            {
                return diagnostic;
            }

            return diagnostic.WithContext(new BuildDiagnosticContext(
                diagnostic.Context.Stage,
                diagnostic.Context.Level,
                purchaseId: diagnostic.Context.PurchaseId,
                purchaseDescription: diagnostic.Context.PurchaseDescription,
                purchaseIndex: diagnostic.Context.PurchaseIndex,
                validatorId: validatorId));
        }

        private static string GetValidatorId(object validator)
        {
            if (validator is IIdentifiedValidator identified && !string.IsNullOrWhiteSpace(identified.ValidatorId))
            {
                return identified.ValidatorId;
            }

            return validator.GetType().Name;
        }

        private static void ApplyLevelAbilityScoreAdjustments(Character character, CharacterSeed seed, int targetLevel)
        {
            var maxAdjustments = targetLevel / 4;
            var usedAdjustments = 0;
            foreach (var entry in seed.LevelAbilityScoreAdjustments.Where(x => x.Value > 0))
            {
                var grants = Math.Min(entry.Value, Math.Max(0, maxAdjustments - usedAdjustments));
                if (grants <= 0)
                {
                    break;
                }

                character.AddAbilityScoreContribution(
                    entry.Key,
                    grants * 2,
                    BonusType.Untyped,
                    grants == 1
                        ? "Level 4 ability score adjustment"
                        : $"{grants} level-based ability score adjustments");
                usedAdjustments += grants;
            }
        }
    }
}
