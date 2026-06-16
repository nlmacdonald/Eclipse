using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Eclipse_Library
{
    public static class SpecialAbilitiesTextImporter
    {
        private static readonly Regex AbilityLineRegex = new(
            @"^(?<name>.+?)\.?\s*\((?<cost>\+?\d+)\s*CP\)(?:\.|\s|$)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex BulletAbilityLineRegex = new(
            @"^\*\s+(?<name>.+?)\s*\((?<cost>\+?\d+)\s*CP\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex BulletNamedOptionWithNonCpCostRegex = new(
            @"^\*\s+(?<name>[A-Z][A-Za-z0-9' -]{0,40}?)\s*\((?<cost>[^)]*?(?:SL|spell level|Power|Mana)[^)]*)\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex BulletNamedOptionLineRegex = new(
            @"^\*\s+(?<name>[A-Z][A-Za-z0-9' -]{0,40}?)(?=\s+(?:allows|lets|keeps|temporarily|provides|grants|can|is)\b)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static AbilityRulesetDocument ImportFromFile(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ImportFromText(File.ReadAllText(path, Encoding.UTF8));
        }

        public static AbilityRulesetDocument ImportFromText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var lines = SplitLines(text).ToList();

            var startIndex = FindLineIndex(lines, "Basic Abilities");
            if (startIndex < 0)
            {
                throw new InvalidOperationException("Could not find the 'Basic Abilities' section in the source text.");
            }

            var document = new AbilityRulesetDocument
            {
                SchemaVersion = 1,
                AbilityModifiers = new List<AbilityModifierRuleDocument>
                {
                    new AbilityModifierRuleDocument
                    {
                        Type = "Corrupted",
                        RequiresDetails = true,
                        RequiresGmApproval = true,
                        Adjustments = new List<AbilityModifierAdjustmentDocument>
                        {
                            new AbilityModifierAdjustmentDocument { Kind = "EffectMultiplier", Value = 1.5m },
                            new AbilityModifierAdjustmentDocument { Kind = "CostMultiplier", Value = 0.6666667m },
                        },
                    },
                    new AbilityModifierRuleDocument
                    {
                        Type = "Specialized",
                        RequiresDetails = true,
                        RequiresGmApproval = true,
                        Adjustments = new List<AbilityModifierAdjustmentDocument>
                        {
                            new AbilityModifierAdjustmentDocument { Kind = "EffectMultiplier", Value = 2.0m },
                            new AbilityModifierAdjustmentDocument { Kind = "CostMultiplier", Value = 0.5m },
                        },
                    },
                },
                BonusUses = new BonusUsesRuleDocument
                {
                    BaseCostCp = 6,
                    BaseAdditionalUses = 4,
                    GmoAllowsAttributeInsteadOfUses = true,
                    AdditionalOptions = new List<BonusUsesOptionDocument>
                    {
                        new BonusUsesOptionDocument { AdditionalUses = 1, CostCp = 2 },
                        new BonusUsesOptionDocument { AdditionalUses = 2, CostCp = 3 },
                        new BonusUsesOptionDocument { AdditionalUses = 3, CostCp = 5 },
                        new BonusUsesOptionDocument { AdditionalUses = 4, CostCp = 6 },
                    },
                },
            };

            var abilities = new List<AbilityEntryDocument>();
            var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AbilityEntryDocument? currentAbility = null;
            AbilityEntryDocument? currentEntry = null;
            AbilityOptionDocument? currentOption = null;
            var currentDescription = new List<string>();

            for (var i = startIndex + 1; i < lines.Count; i++)
            {
                var rawLine = lines[i].TrimEnd();
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Bullet cost entries are options on the current parent ability, not standalone abilities.
                var bulletMatch = BulletAbilityLineRegex.Match(line);
                if (bulletMatch.Success && currentAbility is not null)
                {
                    FinalizeCurrentEntry();

                    var name = bulletMatch.Groups["name"].Value.Trim();
                    if (!TryParseCp(bulletMatch.Groups["cost"].Value, out var costCp))
                    {
                        continue;
                    }

                    currentOption = new AbilityOptionDocument
                    {
                        Id = MakeUniqueOptionId(Slugify(name), currentAbility),
                        Name = name,
                        CostCp = costCp,
                    };
                    currentEntry = currentAbility;
                    currentAbility.Options ??= new List<AbilityOptionDocument>();
                    currentAbility.Options.Add(currentOption);
                    currentDescription.Clear();
                    currentDescription.Add(line);

                    continue;
                }

                var nonCpOptionMatch = BulletNamedOptionWithNonCpCostRegex.Match(line);
                if (nonCpOptionMatch.Success && currentAbility is not null && IsInNamedOptionList(currentAbility, currentDescription))
                {
                    FinalizeCurrentEntry();

                    var name = nonCpOptionMatch.Groups["name"].Value.Trim();
                    if (LooksLikeInstructionBullet(name))
                    {
                        currentDescription.Add(line);
                        continue;
                    }

                    currentOption = new AbilityOptionDocument
                    {
                        Id = MakeUniqueOptionId(Slugify(name), currentAbility),
                        Name = name,
                        CostCp = 0,
                        CostExpression = nonCpOptionMatch.Groups["cost"].Value.Trim(),
                    };
                    currentEntry = currentAbility;
                    currentAbility.Options ??= new List<AbilityOptionDocument>();
                    currentAbility.Options.Add(currentOption);
                    currentDescription.Clear();
                    currentDescription.Add(line);

                    continue;
                }

                var namedOptionMatch = BulletNamedOptionLineRegex.Match(line);
                if (namedOptionMatch.Success && currentAbility is not null && IsInNamedOptionList(currentAbility, currentDescription))
                {
                    FinalizeCurrentEntry();

                    var name = namedOptionMatch.Groups["name"].Value.Trim();
                    if (LooksLikeInstructionBullet(name))
                    {
                        currentDescription.Add(line);
                        continue;
                    }

                    currentOption = new AbilityOptionDocument
                    {
                        Id = MakeUniqueOptionId(Slugify(name), currentAbility),
                        Name = name,
                        CostCp = 0,
                    };
                    currentEntry = currentAbility;
                    currentAbility.Options ??= new List<AbilityOptionDocument>();
                    currentAbility.Options.Add(currentOption);
                    currentDescription.Clear();
                    currentDescription.Add(line);

                    continue;
                }

                // Parent ability entry.
                var match = AbilityLineRegex.Match(line);
                if (match.Success)
                {
                    FinalizeCurrentEntry();

                    var name = match.Groups["name"].Value.Trim();
                    if (!TryParseCp(match.Groups["cost"].Value, out var costCp))
                    {
                        continue;
                    }

                    var id = MakeUniqueId(Slugify(name), usedIds);
                    currentEntry = new AbilityEntryDocument
                    {
                        Id = id,
                        Name = name,
                        CostCp = costCp,
                    };
                    currentAbility = currentEntry;
                    currentOption = null;
                    abilities.Add(currentEntry);
                    currentDescription.Clear();
                    currentDescription.Add(line);

                    continue;
                }

                if (currentEntry is not null)
                {
                    // Continuation prose for the current ability/sub-ability.
                    currentDescription.Add(line);
                }
            }

            FinalizeCurrentEntry();
            document.Abilities = abilities;
            InferOptionPrerequisites(document.Abilities);
            var validation = AbilityRulesetValidator.Validate(document);
            if (!validation.IsValid)
            {
                throw new AbilityRulesetValidationException(validation.Errors);
            }

            return document;

            void FinalizeCurrentEntry()
            {
                if (currentEntry is null)
                {
                    return;
                }

                var desc = string.Join("\n", currentDescription)
                    .Trim();

                if (currentOption is not null)
                {
                    currentOption.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                    currentOption = null;
                }
                else
                {
                    currentEntry.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;
                }

                currentEntry = null;
                currentDescription.Clear();
            }
        }

        private static bool IsInNamedOptionList(AbilityEntryDocument currentAbility, IReadOnlyList<string> currentDescription)
        {
            var parentText = currentAbility.Description;
            if (string.IsNullOrWhiteSpace(parentText))
            {
                parentText = string.Join("\n", currentDescription);
            }

            if (string.IsNullOrWhiteSpace(parentText))
            {
                return false;
            }

            return parentText.IndexOf("option", StringComparison.OrdinalIgnoreCase) >= 0
                || parentText.IndexOf("select one", StringComparison.OrdinalIgnoreCase) >= 0
                || parentText.IndexOf("choose one", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool LooksLikeInstructionBullet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            var wordCount = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount > 4)
            {
                return true;
            }

            return string.Equals(name, "You", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "For", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Characters", StringComparison.OrdinalIgnoreCase);
        }

        private static void InferOptionPrerequisites(IReadOnlyList<AbilityEntryDocument> abilities)
        {
            foreach (var ability in abilities)
            {
                if (ability.Options is null || ability.Options.Count < 2)
                {
                    continue;
                }

                foreach (var option in ability.Options)
                {
                    var description = option.Description ?? "";
                    foreach (var possiblePrereq in ability.Options)
                    {
                        if (string.Equals(possiblePrereq.Id, option.Id, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!ContainsRequiresPhrase(description, possiblePrereq.Name))
                        {
                            continue;
                        }

                        option.PrerequisiteOptionIds ??= new List<string>();
                        if (!option.PrerequisiteOptionIds.Any(x => string.Equals(x, possiblePrereq.Id, StringComparison.OrdinalIgnoreCase)))
                        {
                            option.PrerequisiteOptionIds.Add(possiblePrereq.Id);
                        }
                    }
                }
            }
        }

        private static bool ContainsRequiresPhrase(string description, string prerequisiteName)
        {
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(prerequisiteName))
            {
                return false;
            }

            var escaped = Regex.Escape(prerequisiteName);
            return Regex.IsMatch(
                description,
                $@"\brequires\s+(?:both\s+)?(?:the\s+)?{escaped}\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static IEnumerable<string> SplitLines(string text)
        {
            using var reader = new StringReader(text);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        private static int FindLineIndex(IReadOnlyList<string> lines, string exactTrimmed)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (string.Equals(lines[i].Trim(), exactTrimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool TryParseCp(string value, out int cp)
        {
            // Accept "+6" as well as "6".
            value = value.Trim();
            if (value.StartsWith("+", StringComparison.Ordinal))
            {
                value = value.Substring(1);
            }

            return int.TryParse(value, out cp);
        }

        private static string Slugify(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "ability";
            }

            var sb = new StringBuilder();
            var lastWasDash = false;
            foreach (var ch in name.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    lastWasDash = false;
                    continue;
                }

                if (!lastWasDash)
                {
                    sb.Append('-');
                    lastWasDash = true;
                }
            }

            var slug = sb.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(slug) ? "ability" : slug;
        }

        private static string MakeUniqueId(string baseId, HashSet<string> usedIds)
        {
            var candidate = baseId;
            var suffix = 2;
            while (!usedIds.Add(candidate))
            {
                candidate = $"{baseId}-{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string MakeUniqueOptionId(string baseId, AbilityEntryDocument ability)
        {
            var usedIds = new HashSet<string>(
                (ability.Options ?? new List<AbilityOptionDocument>())
                    .Select(x => x.Id),
                StringComparer.OrdinalIgnoreCase);

            var candidate = baseId;
            var suffix = 2;
            while (!usedIds.Add(candidate))
            {
                candidate = $"{baseId}-{suffix}";
                suffix++;
            }

            return candidate;
        }
    }
}
