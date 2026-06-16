using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class FavoredClassBonusRulesConfig
    {
        private static readonly string[] PathfinderCommonBonuses =
        {
            "+1 Hit Point",
            "+1 Skill Point",
        };

        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;
        public string RaceName { get; set; } = "";
        public List<FavoredClassBonusOptionDocument> Options { get; } = new();

        public static FavoredClassBonusRulesConfig FromRace(RulesetId rulesetId, RaceDefinitionDocument? race)
        {
            var config = new FavoredClassBonusRulesConfig
            {
                RulesetId = rulesetId,
                RaceName = race?.Name ?? "",
            };

            foreach (var option in race?.Rules?.FavoredClassBonuses ?? Enumerable.Empty<FavoredClassBonusOptionDocument>())
            {
                if (option is null || string.IsNullOrWhiteSpace(option.Bonus))
                {
                    continue;
                }

                config.Options.Add(new FavoredClassBonusOptionDocument
                {
                    ClassName = option.ClassName,
                    Bonus = option.Bonus.Trim(),
                    Description = option.Description,
                });
            }

            return config;
        }

        public IReadOnlyList<string> GetAllowedBonuses(string className)
        {
            if (RulesetId != RulesetId.Pathfinder1E)
            {
                return Array.Empty<string>();
            }

            var bonuses = new List<string>();
            bonuses.AddRange(PathfinderCommonBonuses);

            foreach (var option in Options.Where(x => AppliesToClass(x, className)))
            {
                AddDistinct(bonuses, option.Bonus);
            }

            return bonuses;
        }

        public bool IsAllowed(string className, string favoredBonus)
        {
            var normalized = Normalize(favoredBonus);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return true;
            }

            if (RulesetId != RulesetId.Pathfinder1E)
            {
                return false;
            }

            if (SetClassLevelDetailsPurchase.IsCustomFavoredBonus(normalized))
            {
                return true;
            }

            return GetAllowedBonuses(className).Any(x => string.Equals(Normalize(x), normalized, StringComparison.OrdinalIgnoreCase));
        }

        private static bool AppliesToClass(FavoredClassBonusOptionDocument option, string className)
        {
            return string.IsNullOrWhiteSpace(option.ClassName)
                || string.Equals(Normalize(option.ClassName), Normalize(className), StringComparison.OrdinalIgnoreCase);
        }

        private static void AddDistinct(List<string> values, string value)
        {
            var normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized)
                || values.Any(x => string.Equals(Normalize(x), normalized, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            values.Add(normalized);
        }

        private static string Normalize(string? value) => (value ?? "").Trim();
    }
}
