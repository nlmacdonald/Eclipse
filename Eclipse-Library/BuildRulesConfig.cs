namespace Eclipse_Library
{
    public sealed class BuildRulesConfig
    {
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;
        public SkillRulesConfig Skills { get; set; } = new SkillRulesConfig();
        public LevelProgressionRulesConfig LevelProgression { get; set; } = new LevelProgressionRulesConfig();
        public SaveRulesConfig Saves { get; set; } = new SaveRulesConfig();
        public AbilityRulesConfig Abilities { get; set; } = new AbilityRulesConfig();
        public SpellRulesConfig Spells { get; set; } = new SpellRulesConfig();
        public MagicRulesConfig Magic { get; set; } = new MagicRulesConfig();
    }
}
