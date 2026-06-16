using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class TemplateCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "templates", IsRequired = true)]
        public List<ClassTemplateDocument> Templates { get; set; } = new();
    }

    [DataContract]
    public sealed class ClassTemplateDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public Guid Id { get; set; }

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "rulesetId", IsRequired = true)]
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;

        [DataMember(Name = "tag", IsRequired = true)]
        public TemplateTag Tag { get; set; } = TemplateTag.Custom;

        [DataMember(Name = "source", IsRequired = false, EmitDefaultValue = false)]
        public ResourceSourceDocument? Source { get; set; }

        [DataMember(Name = "maxClassLevels", IsRequired = false, EmitDefaultValue = false)]
        public int? MaxClassLevels { get; set; }

        [DataMember(Name = "alignment", IsRequired = false, EmitDefaultValue = false)]
        public string? Alignment { get; set; }

        [DataMember(Name = "startingGold", IsRequired = false, EmitDefaultValue = false)]
        public string? StartingGold { get; set; }

        // Freeform notes (e.g., class skills) per ruleset.
        [DataMember(Name = "notesByRuleset", IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<RulesetId, string>? NotesByRuleset { get; set; }

        [DataMember(Name = "classSkillIds", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? ClassSkillIds { get; set; }

        [DataMember(Name = "levels", IsRequired = true)]
        public List<ClassTemplateLevelDocument> Levels { get; set; } = new();
    }

    [DataContract]
    public sealed class ClassTemplateLevelDocument
    {
        [DataMember(Name = "classLevel", IsRequired = true)]
        public int ClassLevel { get; set; }

        [DataMember(Name = "purchases", IsRequired = true)]
        public List<TemplatePurchaseDocument> Purchases { get; set; } = new();
    }

    [DataContract]
    public sealed class TemplatePurchaseDocument
    {
        [DataMember(Name = "kind", IsRequired = true)]
        public TemplatePurchaseKind Kind { get; set; }

        // Hit Die
        [DataMember(Name = "hitDie", IsRequired = false, EmitDefaultValue = false)]
        public HitDieType? HitDie { get; set; }

        // Warcraft
        [DataMember(Name = "warcraft", IsRequired = false, EmitDefaultValue = false)]
        public int? Warcraft { get; set; }

        // Save
        [DataMember(Name = "saveType", IsRequired = false, EmitDefaultValue = false)]
        public SaveType? SaveType { get; set; }

        [DataMember(Name = "saveBonus", IsRequired = false, EmitDefaultValue = false)]
        public int? SaveBonus { get; set; }

        // Skill ranks
        [DataMember(Name = "skillName", IsRequired = false, EmitDefaultValue = false)]
        public string? SkillName { get; set; }

        [DataMember(Name = "skillId", IsRequired = false, EmitDefaultValue = false)]
        public string? SkillId { get; set; }

        [DataMember(Name = "skillRanks", IsRequired = false, EmitDefaultValue = false)]
        public int? SkillRanks { get; set; }

        [DataMember(Name = "skillRanksAtFirstCharacterLevel", IsRequired = false, EmitDefaultValue = false)]
        public int? SkillRanksAtFirstCharacterLevel { get; set; }

        [DataMember(Name = "skillRanksAtClassLevel", IsRequired = false, EmitDefaultValue = false)]
        public int? SkillRanksAtClassLevel { get; set; }

        [DataMember(Name = "skillIsRelevant", IsRequired = false, EmitDefaultValue = false)]
        public bool? SkillIsRelevant { get; set; }

        // Bonus feat (no feat system yet; tracks count)
        [DataMember(Name = "bonusFeats", IsRequired = false, EmitDefaultValue = false)]
        public int? BonusFeats { get; set; }

        // Proficiency package
        [DataMember(Name = "proficiencyText", IsRequired = false, EmitDefaultValue = false)]
        public string? ProficiencyText { get; set; }

        [DataMember(Name = "proficiencyCostCp", IsRequired = false, EmitDefaultValue = false)]
        public int? ProficiencyCostCp { get; set; }

        // Base caster level
        [DataMember(Name = "baseCasterLevels", IsRequired = false, EmitDefaultValue = false)]
        public int? BaseCasterLevels { get; set; }

        // Magic levels
        [DataMember(Name = "magicProgression", IsRequired = false, EmitDefaultValue = false)]
        public MagicProgressionType? MagicProgression { get; set; }

        [DataMember(Name = "magicLevels", IsRequired = false, EmitDefaultValue = false)]
        public int? MagicLevels { get; set; }

        // Ability purchase (ruleset-driven)
        [DataMember(Name = "abilityId", IsRequired = false, EmitDefaultValue = false)]
        public string? AbilityId { get; set; }

        [DataMember(Name = "abilityOptionIds", IsRequired = false, EmitDefaultValue = false)]
        public List<string>? AbilityOptionIds { get; set; }
    }

    public enum TemplatePurchaseKind
    {
        HitDie = 0,
        Warcraft = 1,
        SaveBonus = 2,
        SkillRanks = 3,
        BonusFeat = 4,
        BaseCasterLevel = 5,
        MagicLevels = 6,
        Ability = 7,
        Proficiencies = 8,
    }
}
