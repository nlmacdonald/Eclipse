using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class LevelProgressionCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "rulesets", IsRequired = true)]
        public List<RulesetLevelProgressionDocument> Rulesets { get; set; } = new();
    }

    [DataContract]
    public sealed class RulesetLevelProgressionDocument
    {
        [DataMember(Name = "rulesetId", IsRequired = true)]
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;

        // For D&D 3.x there is a single XP table, so "tables" will have one entry with progression=null.
        // For Pathfinder this will include Slow/Medium/Fast.
        [DataMember(Name = "tables", IsRequired = true)]
        public List<LevelProgressionTableDocument> Tables { get; set; } = new();
    }

    [DataContract]
    public sealed class LevelProgressionTableDocument
    {
        [DataMember(Name = "progression", IsRequired = false, EmitDefaultValue = false)]
        public PathfinderXpProgression? Progression { get; set; }

        [DataMember(Name = "levels", IsRequired = true)]
        public List<LevelProgressionLevelDocument> Levels { get; set; } = new();
    }

    [DataContract]
    public sealed class LevelProgressionLevelDocument
    {
        [DataMember(Name = "level", IsRequired = true)]
        public int Level { get; set; }

        // Total XP required to reach this level.
        [DataMember(Name = "xpTotal", IsRequired = false, EmitDefaultValue = false)]
        public int? XpTotal { get; set; }

        [DataMember(Name = "grantsFeat", IsRequired = false, EmitDefaultValue = false)]
        public bool GrantsFeat { get; set; }

        [DataMember(Name = "grantsAbilityScoreIncrease", IsRequired = false, EmitDefaultValue = false)]
        public bool GrantsAbilityScoreIncrease { get; set; }
    }
}

