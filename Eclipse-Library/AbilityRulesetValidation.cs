using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class AbilityRulesetValidationResult
    {
        public AbilityRulesetValidationResult(IEnumerable<string> errors)
        {
            Errors = (errors ?? throw new ArgumentNullException(nameof(errors))).ToList().AsReadOnly();
        }

        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public sealed class AbilityRulesetValidationException : Exception
    {
        public AbilityRulesetValidationException(IEnumerable<string> errors)
            : base("Ability ruleset is invalid: " + string.Join("; ", errors ?? Array.Empty<string>()))
        {
            Errors = (errors ?? Array.Empty<string>()).ToList().AsReadOnly();
        }

        public IReadOnlyList<string> Errors { get; }
    }

    public static class AbilityRulesetValidator
    {
        public static AbilityRulesetValidationResult Validate(AbilityRulesetDocument document)
        {
            var errors = new List<string>();
            if (document is null)
            {
                errors.Add("Document is required.");
                return new AbilityRulesetValidationResult(errors);
            }

            if (document.SchemaVersion < 1)
            {
                errors.Add("schemaVersion must be >= 1.");
            }

            ValidateAbilities(document, errors);
            ValidateAbilityModifiers(document, errors);
            ValidateBonusUses(document, errors);

            return new AbilityRulesetValidationResult(errors);
        }

        private static void ValidateAbilities(AbilityRulesetDocument document, List<string> errors)
        {
            if (document.Abilities is null)
            {
                errors.Add("abilities is required.");
                return;
            }

            var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < document.Abilities.Count; index++)
            {
                var ability = document.Abilities[index];
                if (ability is null)
                {
                    errors.Add($"abilities[{index}] must not be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(ability.Id))
                {
                    errors.Add($"abilities[{index}].id is required.");
                }
                else if (ids.TryGetValue(ability.Id, out var priorIndex))
                {
                    errors.Add($"abilities[{index}].id duplicates abilities[{priorIndex}].id ('{ability.Id}').");
                }
                else
                {
                    ids[ability.Id] = index;
                }

                if (string.IsNullOrWhiteSpace(ability.Name))
                {
                    errors.Add($"abilities[{index}].name is required.");
                }

                if (ability.Description is not null && string.IsNullOrWhiteSpace(ability.Description))
                {
                    errors.Add($"abilities[{index}].description, if present, must not be blank.");
                }

                if (ability.CostCp < 0)
                {
                    errors.Add($"abilities[{index}].costCp must be >= 0.");
                }

                ValidateAbilityOptions(ability, index, errors);
            }

            // Prerequisites reference validation + cycle detection.
            var prerequisitesById = document.Abilities
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(
                    x => x.Id,
                    x => (IReadOnlyList<string>)(x.PrerequisiteAbilityIds ?? new List<string>()),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var ability in document.Abilities.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id)))
            {
                foreach (var prereq in (ability.PrerequisiteAbilityIds ?? new List<string>()))
                {
                    if (string.IsNullOrWhiteSpace(prereq))
                    {
                        errors.Add($"abilities[{ids[ability.Id]}].prerequisiteAbilityIds contains a blank id.");
                        continue;
                    }

                    if (!prerequisitesById.ContainsKey(prereq))
                    {
                        errors.Add($"abilities[{ids[ability.Id]}].prerequisiteAbilityIds references unknown id '{prereq}'.");
                    }

                    if (string.Equals(prereq, ability.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"abilities[{ids[ability.Id]}] cannot list itself as a prerequisite.");
                    }
                }
            }

            foreach (var cycle in FindCycles(prerequisitesById))
            {
                errors.Add($"abilities prerequisites contain a cycle: {cycle}");
            }
        }

        private static void ValidateAbilityOptions(AbilityEntryDocument ability, int abilityIndex, List<string> errors)
        {
            if (ability.Options is null)
            {
                return;
            }

            var ids = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < ability.Options.Count; index++)
            {
                var option = ability.Options[index];
                if (option is null)
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}] must not be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(option.Id))
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].id is required.");
                }
                else if (ids.TryGetValue(option.Id, out var priorIndex))
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].id duplicates abilities[{abilityIndex}].options[{priorIndex}].id ('{option.Id}').");
                }
                else
                {
                    ids[option.Id] = index;
                }

                if (string.IsNullOrWhiteSpace(option.Name))
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].name is required.");
                }

                if (option.Description is not null && string.IsNullOrWhiteSpace(option.Description))
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].description, if present, must not be blank.");
                }

                if (option.CostCp < 0)
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].costCp must be >= 0.");
                }

                if (option.CostExpression is not null && string.IsNullOrWhiteSpace(option.CostExpression))
                {
                    errors.Add($"abilities[{abilityIndex}].options[{index}].costExpression, if present, must not be blank.");
                }
            }

            var prerequisitesById = ability.Options
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(
                    x => x.Id,
                    x => (IReadOnlyList<string>)(x.PrerequisiteOptionIds ?? new List<string>()),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var option in ability.Options.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id)))
            {
                foreach (var prereq in (option.PrerequisiteOptionIds ?? new List<string>()))
                {
                    if (string.IsNullOrWhiteSpace(prereq))
                    {
                        errors.Add($"abilities[{abilityIndex}].options[{ids[option.Id]}].prerequisiteOptionIds contains a blank id.");
                        continue;
                    }

                    if (!prerequisitesById.ContainsKey(prereq))
                    {
                        errors.Add($"abilities[{abilityIndex}].options[{ids[option.Id]}].prerequisiteOptionIds references unknown id '{prereq}'.");
                    }

                    if (string.Equals(prereq, option.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"abilities[{abilityIndex}].options[{ids[option.Id]}] cannot list itself as a prerequisite.");
                    }
                }
            }

            foreach (var cycle in FindCycles(prerequisitesById))
            {
                errors.Add($"abilities[{abilityIndex}].options prerequisites contain a cycle: {cycle}");
            }
        }

        private static IEnumerable<string> FindCycles(IReadOnlyDictionary<string, IReadOnlyList<string>> prerequisitesById)
        {
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stack = new Stack<string>();

            foreach (var start in prerequisitesById.Keys)
            {
                foreach (var cycle in Dfs(start))
                {
                    yield return cycle;
                }
            }

            IEnumerable<string> Dfs(string node)
            {
                if (visited.Contains(node))
                {
                    yield break;
                }

                if (!visiting.Add(node))
                {
                    // Already in current stack => cycle; emit using current stack.
                    var path = stack.Reverse().Concat(new[] { node }).ToList();
                    yield return string.Join(" -> ", path);
                    yield break;
                }

                stack.Push(node);
                if (prerequisitesById.TryGetValue(node, out var prereqs))
                {
                    foreach (var prereq in prereqs)
                    {
                        if (string.IsNullOrWhiteSpace(prereq) || !prerequisitesById.ContainsKey(prereq))
                        {
                            continue;
                        }

                        foreach (var cycle in Dfs(prereq))
                        {
                            yield return cycle;
                        }
                    }
                }

                stack.Pop();
                visiting.Remove(node);
                visited.Add(node);
            }
        }

        private static void ValidateAbilityModifiers(AbilityRulesetDocument document, List<string> errors)
        {
            if (document.AbilityModifiers is null)
            {
                errors.Add("abilityModifiers is required.");
                return;
            }

            var seenTypes = new HashSet<AbilityModifierType>();
            for (var index = 0; index < document.AbilityModifiers.Count; index++)
            {
                var modifier = document.AbilityModifiers[index];
                if (modifier is null)
                {
                    errors.Add($"abilityModifiers[{index}] must not be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(modifier.Type))
                {
                    errors.Add($"abilityModifiers[{index}].type is required.");
                    continue;
                }

                if (!TryParseModifierType(modifier.Type, out var modifierType))
                {
                    errors.Add($"abilityModifiers[{index}].type must be one of: Specialized, Corrupted.");
                    continue;
                }

                if (!seenTypes.Add(modifierType))
                {
                    errors.Add($"abilityModifiers[{index}].type duplicates a previously-defined rule ('{modifier.Type}').");
                }

                if (modifier.Adjustments is null || modifier.Adjustments.Count == 0)
                {
                    errors.Add($"abilityModifiers[{index}].adjustments must contain at least 1 item.");
                    continue;
                }

                for (var adjIndex = 0; adjIndex < modifier.Adjustments.Count; adjIndex++)
                {
                    var adj = modifier.Adjustments[adjIndex];
                    if (adj is null)
                    {
                        errors.Add($"abilityModifiers[{index}].adjustments[{adjIndex}] must not be null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(adj.Kind))
                    {
                        errors.Add($"abilityModifiers[{index}].adjustments[{adjIndex}].kind is required.");
                        continue;
                    }

                    if (!TryParseAdjustmentKind(adj.Kind, out _))
                    {
                        errors.Add($"abilityModifiers[{index}].adjustments[{adjIndex}].kind must be one of: CostMultiplier, EffectMultiplier.");
                    }

                    if (adj.Value <= 0)
                    {
                        errors.Add($"abilityModifiers[{index}].adjustments[{adjIndex}].value must be > 0.");
                    }
                }
            }
        }

        private static void ValidateBonusUses(AbilityRulesetDocument document, List<string> errors)
        {
            if (document.BonusUses is null)
            {
                return;
            }

            if (document.BonusUses.BaseCostCp <= 0)
            {
                errors.Add("bonusUses.baseCostCp must be > 0.");
            }

            if (document.BonusUses.BaseAdditionalUses <= 0)
            {
                errors.Add("bonusUses.baseAdditionalUses must be > 0.");
            }

            if (document.BonusUses.AdditionalOptions is null)
            {
                return;
            }

            for (var index = 0; index < document.BonusUses.AdditionalOptions.Count; index++)
            {
                var option = document.BonusUses.AdditionalOptions[index];
                if (option is null)
                {
                    errors.Add($"bonusUses.additionalOptions[{index}] must not be null.");
                    continue;
                }

                if (option.AdditionalUses <= 0)
                {
                    errors.Add($"bonusUses.additionalOptions[{index}].additionalUses must be > 0.");
                }

                if (option.CostCp <= 0)
                {
                    errors.Add($"bonusUses.additionalOptions[{index}].costCp must be > 0.");
                }
            }
        }

        internal static bool TryParseModifierType(string value, out AbilityModifierType modifierType)
        {
            if (string.Equals(value, "Specialized", StringComparison.OrdinalIgnoreCase))
            {
                modifierType = AbilityModifierType.Specialized;
                return true;
            }

            if (string.Equals(value, "Corrupted", StringComparison.OrdinalIgnoreCase))
            {
                modifierType = AbilityModifierType.Corrupted;
                return true;
            }

            modifierType = default;
            return false;
        }

        internal static bool TryParseAdjustmentKind(string value, out AbilityModifierAdjustmentKind kind)
        {
            if (string.Equals(value, "CostMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                kind = AbilityModifierAdjustmentKind.CostMultiplier;
                return true;
            }

            if (string.Equals(value, "EffectMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                kind = AbilityModifierAdjustmentKind.EffectMultiplier;
                return true;
            }

            kind = default;
            return false;
        }
    }
}
