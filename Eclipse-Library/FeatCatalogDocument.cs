using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class FeatCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "rulesets", IsRequired = true)]
        public List<FeatRulesetDocument> Rulesets { get; set; } = new();
    }

    [DataContract]
    public sealed class FeatRulesetDocument
    {
        [DataMember(Name = "rulesetId", IsRequired = true)]
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;

        [DataMember(Name = "feats", IsRequired = true)]
        public List<FeatDefinitionDocument> Feats { get; set; } = new();
    }

    [DataContract]
    public sealed class FeatDefinitionDocument
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        // True when feat can be taken multiple times (does not stack, but applies to different choice).
        [DataMember(Name = "repeatable", IsRequired = false, EmitDefaultValue = false)]
        public bool Repeatable { get; set; }

        // True when multiple takes stack (e.g., Toughness in 3.0 list, Extra Turning in 3.0 list).
        [DataMember(Name = "stacks", IsRequired = false, EmitDefaultValue = false)]
        public bool Stacks { get; set; }

        // True when explicitly eligible as a Fighter bonus feat (3.5 list notes, PF1e combat feats).
        [DataMember(Name = "fighterBonusFeat", IsRequired = false, EmitDefaultValue = false)]
        public bool FighterBonusFeat { get; set; }

        // Freeform notes (e.g., “choice-based: weapon”, “combat feat”).
        [DataMember(Name = "notes", IsRequired = false, EmitDefaultValue = false)]
        public string? Notes { get; set; }
    }
}

