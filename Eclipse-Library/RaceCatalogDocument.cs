using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class RaceCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "rulesetId", IsRequired = true)]
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;

        [DataMember(Name = "sourceDocument", IsRequired = false, EmitDefaultValue = false)]
        public string? SourceDocument { get; set; }

        [DataMember(Name = "races", IsRequired = true)]
        public List<RaceDefinitionDocument> Races { get; set; } = new();
    }

    [DataContract]
    public sealed class RaceDefinitionDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = "";

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "pluralName", IsRequired = false, EmitDefaultValue = false)]
        public string? PluralName { get; set; }

        [DataMember(Name = "sourceText", IsRequired = true)]
        public RaceSourceTextDocument SourceText { get; set; } = new();

        [DataMember(Name = "rules", IsRequired = true)]
        public RaceRulesDocument Rules { get; set; } = new();
    }

    [DataContract]
    public sealed class RaceSourceTextDocument
    {
        [DataMember(Name = "overview", IsRequired = false, EmitDefaultValue = false)]
        public string? Overview { get; set; }

        [DataMember(Name = "sections", IsRequired = true)]
        public Dictionary<string, string> Sections { get; set; } = new();

        [DataMember(Name = "nameExamples", IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, List<string>>? NameExamples { get; set; }
    }

    [DataContract]
    public sealed class RaceRulesDocument
    {
        [DataMember(Name = "abilityScoreModifiers", IsRequired = false, EmitDefaultValue = false)]
        public List<RaceAbilityScoreModifierDocument>? AbilityScoreModifiers { get; set; }

        [DataMember(Name = "size", IsRequired = false, EmitDefaultValue = false)]
        public string? Size { get; set; }

        [DataMember(Name = "speedFeet", IsRequired = false, EmitDefaultValue = false)]
        public int? SpeedFeet { get; set; }

        [DataMember(Name = "vision", IsRequired = false, EmitDefaultValue = false)]
        public List<RaceVisionDocument>? Vision { get; set; }

        [DataMember(Name = "abilities", IsRequired = false, EmitDefaultValue = false)]
        public List<RaceAbilityReferenceDocument>? Abilities { get; set; }

        [DataMember(Name = "automaticLanguages", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? AutomaticLanguages { get; set; }

        [DataMember(Name = "bonusLanguages", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? BonusLanguages { get; set; }

        [DataMember(Name = "favoredClass", IsRequired = false, EmitDefaultValue = false)]
        public string? FavoredClass { get; set; }

        [DataMember(Name = "favoredClassBonuses", IsRequired = false, EmitDefaultValue = false)]
        public List<FavoredClassBonusOptionDocument>? FavoredClassBonuses { get; set; }

        [DataMember(Name = "traits", IsRequired = true)]
        public List<RaceTraitDocument> Traits { get; set; } = new();
    }

    [DataContract]
    public sealed class FavoredClassBonusOptionDocument
    {
        [DataMember(Name = "className", IsRequired = false, EmitDefaultValue = false)]
        public string? ClassName { get; set; }

        [DataMember(Name = "bonus", IsRequired = true)]
        public string Bonus { get; set; } = "";

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }
    }

    [DataContract]
    public sealed class RaceAbilityScoreModifierDocument
    {
        [DataMember(Name = "ability", IsRequired = true)]
        public string Ability { get; set; } = "";

        [DataMember(Name = "modifier", IsRequired = false, EmitDefaultValue = false)]
        public int? Modifier { get; set; }

        [DataMember(Name = "choose", IsRequired = false, EmitDefaultValue = false)]
        public bool Choose { get; set; }
    }

    [DataContract]
    public sealed class RaceVisionDocument
    {
        [DataMember(Name = "kind", IsRequired = true)]
        public string Kind { get; set; } = "";

        [DataMember(Name = "rangeFeet", IsRequired = false, EmitDefaultValue = false)]
        public int? RangeFeet { get; set; }
    }

    [DataContract]
    public sealed class RaceAbilityReferenceDocument
    {
        [DataMember(Name = "abilityId", IsRequired = true)]
        public string AbilityId { get; set; } = "";

        [DataMember(Name = "configuration", IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, string>? Configuration { get; set; }
    }

    [DataContract]
    public sealed class RaceTraitDocument
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "description", IsRequired = true)]
        public string Description { get; set; } = "";

        [DataMember(Name = "rawText", IsRequired = true)]
        public string RawText { get; set; } = "";
    }
}
