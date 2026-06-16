using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class SkillCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "rulesets", IsRequired = true)]
        public List<SkillRulesetDocument> Rulesets { get; set; } = new();
    }

    [DataContract]
    public sealed class SkillRulesetDocument
    {
        [DataMember(Name = "rulesetId", IsRequired = true)]
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;

        [DataMember(Name = "skills", IsRequired = true)]
        public List<SkillDefinitionDocument> Skills { get; set; } = new();
    }

    [DataContract]
    public sealed class SkillDefinitionDocument
    {
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string? Id { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "attribute", IsRequired = true)]
        public SkillAttribute Attribute { get; set; } = SkillAttribute.None;

        [DataMember(Name = "requiresSpecialization", IsRequired = false, EmitDefaultValue = false)]
        public bool RequiresSpecialization { get; set; }

        [DataMember(Name = "trainedOnly", IsRequired = false, EmitDefaultValue = false)]
        public bool TrainedOnly { get; set; }

        [DataMember(Name = "armorCheckPenalty", IsRequired = false, EmitDefaultValue = false)]
        public bool ArmorCheckPenalty { get; set; }

        [DataMember(Name = "restrictedToClasses", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? RestrictedToClasses { get; set; }

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }

        [DataMember(Name = "notes", IsRequired = false, EmitDefaultValue = false)]
        public string? Notes { get; set; }
    }
}
