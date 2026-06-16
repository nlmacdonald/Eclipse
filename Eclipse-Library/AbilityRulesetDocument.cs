using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class AbilityRulesetDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "abilityModifiers", IsRequired = true)]
        public List<AbilityModifierRuleDocument> AbilityModifiers { get; set; } = new();

        [DataMember(Name = "bonusUses", IsRequired = false, EmitDefaultValue = false)]
        public BonusUsesRuleDocument? BonusUses { get; set; }

        [DataMember(Name = "abilities", IsRequired = true)]
        public List<AbilityEntryDocument> Abilities { get; set; } = new();
    }

    [DataContract]
    public sealed class AbilityEntryDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = "";

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "costCp", IsRequired = true)]
        public int CostCp { get; set; }

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }

        [DataMember(Name = "prerequisiteAbilityIds", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? PrerequisiteAbilityIds { get; set; }

        [DataMember(Name = "options", IsRequired = false, EmitDefaultValue = false)]
        public List<AbilityOptionDocument>? Options { get; set; }
    }

    [DataContract]
    public sealed class AbilityOptionDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = "";

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "costCp", IsRequired = true)]
        public int CostCp { get; set; }

        [DataMember(Name = "costExpression", IsRequired = false, EmitDefaultValue = false)]
        public string? CostExpression { get; set; }

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }

        [DataMember(Name = "prerequisiteOptionIds", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? PrerequisiteOptionIds { get; set; }
    }

    [DataContract]
    public sealed class AbilityModifierRuleDocument
    {
        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; set; } = "";

        [DataMember(Name = "requiresDetails", IsRequired = false, EmitDefaultValue = false)]
        public bool RequiresDetails { get; set; } = true;

        [DataMember(Name = "requiresGmApproval", IsRequired = false, EmitDefaultValue = false)]
        public bool RequiresGmApproval { get; set; } = true;

        [DataMember(Name = "adjustments", IsRequired = true)]
        public List<AbilityModifierAdjustmentDocument> Adjustments { get; set; } = new();
    }

    [DataContract]
    public sealed class AbilityModifierAdjustmentDocument
    {
        [DataMember(Name = "kind", IsRequired = true)]
        public string Kind { get; set; } = "";

        [DataMember(Name = "value", IsRequired = true)]
        public decimal Value { get; set; }
    }

    [DataContract]
    public sealed class BonusUsesRuleDocument
    {
        [DataMember(Name = "baseCostCp", IsRequired = true)]
        public int BaseCostCp { get; set; }

        [DataMember(Name = "baseAdditionalUses", IsRequired = true)]
        public int BaseAdditionalUses { get; set; }

        [DataMember(Name = "gmoAllowsAttributeInsteadOfUses", IsRequired = false, EmitDefaultValue = false)]
        public bool GmoAllowsAttributeInsteadOfUses { get; set; }

        [DataMember(Name = "additionalOptions", IsRequired = false, EmitDefaultValue = false)]
        public List<BonusUsesOptionDocument>? AdditionalOptions { get; set; }
    }

    [DataContract]
    public sealed class BonusUsesOptionDocument
    {
        [DataMember(Name = "additionalUses", IsRequired = true)]
        public int AdditionalUses { get; set; }

        [DataMember(Name = "costCp", IsRequired = true)]
        public int CostCp { get; set; }
    }
}
