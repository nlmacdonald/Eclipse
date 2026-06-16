using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eclipse_Library
{
    public sealed class RulesEngine
    {
        private readonly IReadOnlyCollection<IRuleValidator> _validators;

        public RulesEngine(IEnumerable<IRuleValidator> validators)
        {
            _validators = validators.ToList().AsReadOnly();
        }

        public CharacterBuildResult Build(Character character, IEnumerable<IPurchase> purchases)
        {
            var result = new CharacterBuildResult(character);

            foreach (var purchase in purchases)
            {
                try
                {
                    purchase.Apply(character);
                    result.AppliedPurchaseDescriptions.Add(purchase.Description);
                }
                catch (Exception ex)
                {
                    result.Diagnostics.Add(new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "ENGINE_APPLY_FAILED",
                        $"{purchase.Description} could not be applied: {ex.Message}"));
                }
            }

            foreach (var validator in _validators)
            {
                result.Diagnostics.AddRange(validator.Validate(character));
            }

            return result;
        }

        public JournalBuildResult Build(
            BuildJournal journal,
            ICpProgression? cpProgression = null,
            BuildJournalBuildOptions? options = null)
        {
            if (journal is null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            cpProgression ??= new EclipseCpProgression();
            options ??= new BuildJournalBuildOptions();

            var character = Character.CreateLevelOne(
                journal.Seed.Name,
                journal.Seed.AbilityScores,
                totalCp: cpProgression.GetTotalCpAtLevel(1),
                size: journal.Seed.Size,
                race: journal.Seed.Race,
                raceAbilityCatalog: journal.Seed.RaceAbilityCatalog);
            ApplyLevelAbilityScoreAdjustments(character, journal.Seed, journal.CurrentLevel);

            var result = new JournalBuildResult(character);

            for (var level = 1; level <= journal.CurrentLevel; level++)
            {
                if (level > 1)
                {
                    var newTotalCp = cpProgression.GetTotalCpAtLevel(level);
                    var cpGained = newTotalCp - character.TotalCp;
                    character.AdvanceLevel(cpGained);
                }

                var levelResult = new LevelBuildResult(level);
                var purchases = journal.GetPurchasesForLevel(level);

                for (var purchaseIndex = 0; purchaseIndex < purchases.Count; purchaseIndex++)
                {
                    var entry = purchases[purchaseIndex];
                    try
                    {
                        entry.Purchase.Apply(character);
                        levelResult.AppliedPurchaseDescriptions.Add(entry.Description);
                    }
                    catch (Exception ex)
                    {
                        var diagnostic = new BuildDiagnostic(
                            BuildDiagnosticSeverity.Error,
                            "ENGINE_APPLY_FAILED",
                            $"{entry.Description} could not be applied: {ex.Message}",
                            new BuildDiagnosticContext(
                                BuildDiagnosticStage.AtLevel,
                                level,
                                purchaseId: entry.Id,
                                purchaseDescription: entry.Description,
                                purchaseIndex: purchaseIndex));

                        levelResult.Diagnostics.Add(diagnostic);
                        result.Diagnostics.Add(diagnostic);
                    }

                    if (options.ValidateAfterEachPurchase)
                    {
                        AddValidatorDiagnostics(
                            result,
                            levelResult,
                            character,
                            BuildDiagnosticStage.AtLevel,
                            level,
                            purchaseId: entry.Id,
                            purchaseDescription: entry.Description,
                            purchaseIndex: purchaseIndex);
                    }
                }

                if (options.ValidateAtEachLevel && !options.ValidateAfterEachPurchase)
                {
                    AddValidatorDiagnostics(result, levelResult, character, BuildDiagnosticStage.AtLevel, level);
                }

                if (options.CaptureLevelSnapshots)
                {
                    levelResult.Snapshot = character.Clone();
                }

                result.Levels.Add(levelResult);
            }

            if (options.ValidateAtFinal)
            {
                var finalLevel = journal.CurrentLevel;
                foreach (var validator in _validators)
                {
                    foreach (var diagnostic in validator.Validate(character))
                    {
                        result.Diagnostics.Add(diagnostic.WithContext(new BuildDiagnosticContext(BuildDiagnosticStage.Final, finalLevel)));
                    }
                }
            }

            return result;
        }

        private void AddValidatorDiagnostics(
            JournalBuildResult result,
            LevelBuildResult levelResult,
            Character character,
            BuildDiagnosticStage stage,
            int level,
            Guid? purchaseId = null,
            string? purchaseDescription = null,
            int? purchaseIndex = null)
        {
            foreach (var validator in _validators)
            {
                foreach (var diagnostic in validator.Validate(character))
                {
                    var stamped = diagnostic.WithContext(new BuildDiagnosticContext(
                        stage,
                        level,
                        purchaseId: purchaseId,
                        purchaseDescription: purchaseDescription,
                        purchaseIndex: purchaseIndex));
                    levelResult.Diagnostics.Add(stamped);
                    result.Diagnostics.Add(stamped);
                }
            }
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
