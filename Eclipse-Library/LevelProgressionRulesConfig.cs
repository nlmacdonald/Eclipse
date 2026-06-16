using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class LevelProgressionRulesConfig
    {
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;
        public PathfinderXpProgression? PathfinderProgression { get; set; }
        public List<LevelProgressionLevelDocument> Levels { get; } = new();

        public static LevelProgressionRulesConfig FromCatalog(
            LevelProgressionCatalogDocument? catalog,
            RulesetId rulesetId,
            PathfinderXpProgression? pathfinderProgression = null)
        {
            var config = new LevelProgressionRulesConfig
            {
                RulesetId = rulesetId,
                PathfinderProgression = pathfinderProgression,
            };

            var ruleset = catalog?.Rulesets.FirstOrDefault(x => x.RulesetId == rulesetId);
            var table = ruleset is null
                ? null
                : rulesetId == RulesetId.Pathfinder1E
                    ? ruleset.Tables.FirstOrDefault(x => x.Progression == pathfinderProgression) ?? ruleset.Tables.FirstOrDefault()
                    : ruleset.Tables.FirstOrDefault();

            foreach (var row in table?.Levels.OrderBy(x => x.Level) ?? Enumerable.Empty<LevelProgressionLevelDocument>())
            {
                config.Levels.Add(new LevelProgressionLevelDocument
                {
                    Level = row.Level,
                    XpTotal = row.XpTotal,
                    GrantsFeat = row.GrantsFeat,
                    GrantsAbilityScoreIncrease = row.GrantsAbilityScoreIncrease,
                });
            }

            return config;
        }

        public int GetFeatsGrantedByLevel(int level)
        {
            level = Math.Max(1, level);
            if (Levels.Count > 0)
            {
                return Levels.Count(x => x.Level >= 1 && x.Level <= level && x.GrantsFeat);
            }

            return RulesetId == RulesetId.Pathfinder1E
                ? (level + 1) / 2
                : 1 + ((level - 1) / 3);
        }

        public IReadOnlyList<LevelProgressionLevelDocument> GetLevels()
        {
            return Levels.OrderBy(x => x.Level).ToList();
        }
    }
}
