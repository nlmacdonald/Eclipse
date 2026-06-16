using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class CharacterBuildDocument
    {
        public int SchemaVersion { get; set; } = 2;
        public string HeroName { get; set; } = "New Character";
        public string PlayerName { get; set; } = "";
        public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;
        public int TargetLevel { get; set; } = 1;
        public int HitPoints { get; set; }
        public int StartingGold { get; set; }
        public int Experience { get; set; }
        public PathfinderXpProgression PathfinderProgression { get; set; } = PathfinderXpProgression.Medium;
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Constitution { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Wisdom { get; set; } = 10;
        public int Charisma { get; set; } = 10;
        public Dictionary<string, int> LevelAbilityScoreAdjustments { get; set; } = new();
        public string Race { get; set; } = "Choose Race";
        public CharacterSize? Size { get; set; }
        public string Alignment { get; set; } = "Choose Alignment";
        public string Deity { get; set; } = "Choose Deity";
        public bool ReplaceCommon { get; set; }
        public List<string> Languages { get; set; } = new();
        public List<CharacterClassLevelSaveDocument> ClassLevels { get; set; } = new();
        public List<CharacterFeatSaveDocument> Feats { get; set; } = new();
        public List<CharacterSkillAllocationSaveDocument> SkillAllocations { get; set; } = new();
    }

    public sealed class CharacterClassLevelSaveDocument
    {
        public int CharacterLevel { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = "";
        public int TemplateLevel { get; set; }
        public string Source { get; set; } = "";
        public string HpNote { get; set; } = "";
        public string FavoredBonus { get; set; } = "";
    }

    public sealed class CharacterFeatSaveDocument
    {
        public string Name { get; set; } = "";
        public int Count { get; set; } = 1;
    }

    public sealed class CharacterSkillAllocationSaveDocument
    {
        public string Name { get; set; } = "";
        public string? Specialization { get; set; }
        public int SkillPointsSpent { get; set; }

        public string DisplayName => string.IsNullOrWhiteSpace(Specialization)
            ? Name
            : $"{Name} ({Specialization})";
    }
}
