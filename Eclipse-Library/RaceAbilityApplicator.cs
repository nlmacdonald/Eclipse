using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public static class RaceAbilityApplicator
    {
        public static void Apply(Character character, RaceDefinitionDocument? race, RaceAbilityCatalogDocument? abilityCatalog)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (race is null)
            {
                return;
            }

            character.SetRace(race.Name);

            var definitions = (abilityCatalog?.Abilities ?? new List<RaceAbilityDefinitionDocument>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            var appliedReferencedAbility = false;
            foreach (var reference in race.Rules.Abilities ?? new List<RaceAbilityReferenceDocument>())
            {
                if (reference is null || string.IsNullOrWhiteSpace(reference.AbilityId))
                {
                    continue;
                }

                if (!definitions.TryGetValue(reference.AbilityId, out var definition))
                {
                    continue;
                }

                appliedReferencedAbility = true;
                ApplyDefinition(character, definition, reference);
            }

            if (!appliedReferencedAbility)
            {
                ApplyLegacyRules(character, race.Rules);
            }
        }

        private static void ApplyDefinition(
            Character character,
            RaceAbilityDefinitionDocument definition,
            RaceAbilityReferenceDocument reference)
        {
            foreach (var effect in definition.Effects ?? Enumerable.Empty<RaceAbilityEffectDocument>())
            {
                var feet = ResolveFeet(effect, reference.Configuration);
                switch ((effect.Type ?? "").Trim().ToLowerInvariant())
                {
                    case "movement":
                        character.SetMovementSpeed(ResolveText(effect.MovementKindParameter, reference.Configuration, effect.MovementKind ?? "ground"), feet);
                        break;
                    case "sense":
                        character.SetSenseRange(effect.SenseKind ?? definition.Name, feet);
                        break;
                    case "bonusfeat":
                    case "bonus-feat":
                        character.AddBonusFeats(Math.Max(1, effect.Amount.GetValueOrDefault(1)));
                        break;
                    case "skillpoints":
                    case "skill-points":
                        character.AddBonusSkillPoints(effect.FirstLevelAmount.GetValueOrDefault());
                        character.AddBonusSkillPointsPerAdditionalLevel(effect.PerLevelAmount.GetValueOrDefault());
                        break;
                    case "bonus":
                        var bonusAmount = effect.Amount.GetValueOrDefault();
                        if (!string.IsNullOrWhiteSpace(effect.Target) && bonusAmount != 0)
                        {
                            character.AddBonus(
                                effect.Target,
                                bonusAmount,
                                effect.BonusType.GetValueOrDefault(BonusType.Untyped),
                                definition.Name,
                                effect.Condition);
                        }

                        break;
                    case "limitedsave":
                    case "limited-save":
                        ApplyLimitedSaveBonus(character, effect);
                        break;
                    case "note":
                        character.AddStatBlockNote(effect.Note ?? definition.Description ?? definition.Name);
                        break;
                }
            }
        }

        private static void ApplyLimitedSaveBonus(Character character, RaceAbilityEffectDocument effect)
        {
            var amount = effect.Amount.GetValueOrDefault();
            if (amount <= 0 || string.IsNullOrWhiteSpace(effect.Condition))
            {
                return;
            }

            foreach (var saveType in Enum.GetValues(typeof(SaveType)).Cast<SaveType>())
            {
                character.AddLimitedSaveBonus(saveType, amount, effect.Condition!);
            }
        }

        private static int ResolveFeet(RaceAbilityEffectDocument effect, Dictionary<string, string>? configuration)
        {
            if (!string.IsNullOrWhiteSpace(effect.FeetParameter)
                && configuration is not null
                && configuration.TryGetValue(effect.FeetParameter, out var configured)
                && int.TryParse(configured, out var configuredFeet))
            {
                return configuredFeet;
            }

            return effect.Feet.GetValueOrDefault();
        }

        private static string ResolveText(string? parameter, Dictionary<string, string>? configuration, string defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(parameter)
                && configuration is not null
                && configuration.TryGetValue(parameter, out var configured)
                && !string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            return defaultValue;
        }

        private static void ApplyLegacyRules(Character character, RaceRulesDocument rules)
        {
            if (rules.SpeedFeet is not null)
            {
                character.SetMovementSpeed("ground", rules.SpeedFeet.Value);
            }

            foreach (var vision in rules.Vision ?? Enumerable.Empty<RaceVisionDocument>())
            {
                character.SetSenseRange(vision.Kind, vision.RangeFeet.GetValueOrDefault());
            }
        }
    }
}
