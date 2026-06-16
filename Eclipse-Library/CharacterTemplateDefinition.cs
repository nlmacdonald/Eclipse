using System;

namespace Eclipse_Library
{
    public sealed class CharacterTemplateDefinition
    {
        public CharacterTemplateDefinition()
            : this(Guid.NewGuid(), "New Template", RulesetId.Dnd35, TemplateTag.Custom, maxClassLevels: null)
        {
        }

        public CharacterTemplateDefinition(Guid id, string name, RulesetId rulesetId, TemplateTag tag, int? maxClassLevels)
        {
            Id = id == Guid.Empty ? throw new ArgumentException("Template id must not be empty.", nameof(id)) : id;
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Template name is required.", nameof(name))
                : name;
            RulesetId = rulesetId;
            Tag = tag;
            MaxClassLevels = maxClassLevels;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public RulesetId RulesetId { get; set; }
        public TemplateTag Tag { get; set; } = TemplateTag.Custom;
        public int? MaxClassLevels { get; set; }
        public string Alignment { get; set; } = "Any";
        public string StartingGold { get; set; } = "";
        public string Notes { get; set; } = "";

        public RulesetProfile Ruleset => RulesetProfile.Get(RulesetId);
    }
}
