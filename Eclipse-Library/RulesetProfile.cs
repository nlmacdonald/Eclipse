using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class RulesetProfile
    {
        private static readonly IReadOnlyList<RulesetProfile> Profiles = new[]
        {
            new RulesetProfile(RulesetId.Dnd30, "D&D 3.0", "Baseline Eclipse appendix support for D&D 3.0 assumptions."),
            new RulesetProfile(RulesetId.Dnd35, "D&D 3.5", "Baseline Eclipse appendix support for D&D 3.5 assumptions."),
            new RulesetProfile(RulesetId.Pathfinder1E, "Pathfinder 1e", "Pathfinder 1e / D&D 3.75 compatibility profile."),
        };

        public RulesetProfile(RulesetId id, string displayName, string description)
        {
            Id = id;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public RulesetId Id { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public static IReadOnlyList<RulesetProfile> All => Profiles;

        public static RulesetProfile Get(RulesetId id)
        {
            return Profiles.First(x => x.Id == id);
        }

        public BuildRulesConfig CreateBuildRulesConfig()
        {
            var config = new BuildRulesConfig
            {
                RulesetId = Id,
            };

            if (Id == RulesetId.Pathfinder1E)
            {
                config.Skills.IrrelevantSkillRankMultiplier = 1m;
                config.Skills.EnableIrrelevantToRelevantPromotion = false;
                config.Skills.UseFirstCharacterLevelSkillPointMultiplier = false;
            }

            return config;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
