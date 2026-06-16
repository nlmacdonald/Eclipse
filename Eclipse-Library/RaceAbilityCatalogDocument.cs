using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class RaceAbilityCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "sourceDocument", IsRequired = false, EmitDefaultValue = false)]
        public string? SourceDocument { get; set; }

        [DataMember(Name = "abilities", IsRequired = true)]
        public List<RaceAbilityDefinitionDocument> Abilities { get; set; } = new();
    }

    [DataContract]
    public sealed class RaceAbilityDefinitionDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = "";

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }

        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public List<RaceAbilityParameterDocument>? Parameters { get; set; }

        [DataMember(Name = "effects", IsRequired = false, EmitDefaultValue = false)]
        public List<RaceAbilityEffectDocument>? Effects { get; set; }
    }

    [DataContract]
    public sealed class RaceAbilityParameterDocument
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "kind", IsRequired = true)]
        public string Kind { get; set; } = "";

        [DataMember(Name = "defaultValue", IsRequired = false, EmitDefaultValue = false)]
        public string? DefaultValue { get; set; }
    }

    [DataContract]
    public sealed class RaceAbilityEffectDocument
    {
        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; set; } = "";

        [DataMember(Name = "movementKind", IsRequired = false, EmitDefaultValue = false)]
        public string? MovementKind { get; set; }

        [DataMember(Name = "movementKindParameter", IsRequired = false, EmitDefaultValue = false)]
        public string? MovementKindParameter { get; set; }

        [DataMember(Name = "senseKind", IsRequired = false, EmitDefaultValue = false)]
        public string? SenseKind { get; set; }

        [DataMember(Name = "feetParameter", IsRequired = false, EmitDefaultValue = false)]
        public string? FeetParameter { get; set; }

        [DataMember(Name = "feet", IsRequired = false, EmitDefaultValue = false)]
        public int? Feet { get; set; }

        [DataMember(Name = "amount", IsRequired = false, EmitDefaultValue = false)]
        public int? Amount { get; set; }

        [DataMember(Name = "firstLevelAmount", IsRequired = false, EmitDefaultValue = false)]
        public int? FirstLevelAmount { get; set; }

        [DataMember(Name = "perLevelAmount", IsRequired = false, EmitDefaultValue = false)]
        public int? PerLevelAmount { get; set; }

        [DataMember(Name = "target", IsRequired = false, EmitDefaultValue = false)]
        public string? Target { get; set; }

        [DataMember(Name = "bonusType", IsRequired = false, EmitDefaultValue = false)]
        public BonusType? BonusType { get; set; }

        [DataMember(Name = "condition", IsRequired = false, EmitDefaultValue = false)]
        public string? Condition { get; set; }

        [DataMember(Name = "note", IsRequired = false, EmitDefaultValue = false)]
        public string? Note { get; set; }
    }
}
