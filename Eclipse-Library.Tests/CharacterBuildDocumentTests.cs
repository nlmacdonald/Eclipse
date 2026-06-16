using System.Text.Json;
using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class CharacterBuildDocumentTests
{
    [TestMethod]
    public void RoundTripsCharacterBuilderState()
    {
        var templateId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var document = new CharacterBuildDocument
        {
            HeroName = "Asha",
            PlayerName = "N",
            RulesetId = RulesetId.Pathfinder1E,
            TargetLevel = 3,
            HitPoints = 24,
            StartingGold = 150,
            Experience = 3300,
            PathfinderProgression = PathfinderXpProgression.Fast,
            Strength = 12,
            Dexterity = 14,
            Constitution = 13,
            Intelligence = 16,
            Wisdom = 10,
            Charisma = 8,
            LevelAbilityScoreAdjustments = new Dictionary<string, int>
            {
                ["Intelligence"] = 1,
            },
            Race = "Human",
            Size = CharacterSize.Medium,
            Alignment = "Neutral Good",
            Deity = "None",
            ReplaceCommon = false,
            Languages = new List<string> { "Common", "Draconic" },
            ClassLevels = new List<CharacterClassLevelSaveDocument>
            {
                new()
                {
                    CharacterLevel = 1,
                    TemplateId = templateId,
                    TemplateName = "Wizard",
                    TemplateLevel = 1,
                    Source = "players-handbook",
                    HpNote = "4",
                    FavoredBonus = "+1 Skill Point",
                },
            },
            Feats = new List<CharacterFeatSaveDocument>
            {
                new() { Name = "Scribe Scroll", Count = 1 },
                new() { Name = "Spell Focus", Count = 2 },
            },
            SkillAllocations = new List<CharacterSkillAllocationSaveDocument>
            {
                new() { Name = "Knowledge", Specialization = "arcana", SkillPointsSpent = 3 },
                new() { Name = "Spellcraft", SkillPointsSpent = 2 },
            },
        };

        var json = JsonSerializer.Serialize(document);
        var roundTripped = JsonSerializer.Deserialize<CharacterBuildDocument>(json);

        Assert.IsNotNull(roundTripped);
        Assert.AreEqual(2, roundTripped.SchemaVersion);
        Assert.AreEqual("Asha", roundTripped.HeroName);
        Assert.AreEqual(RulesetId.Pathfinder1E, roundTripped.RulesetId);
        Assert.AreEqual(PathfinderXpProgression.Fast, roundTripped.PathfinderProgression);
        Assert.AreEqual(1, roundTripped.LevelAbilityScoreAdjustments["Intelligence"]);
        Assert.AreEqual(2, roundTripped.Languages.Count);
        Assert.AreEqual(templateId, roundTripped.ClassLevels[0].TemplateId);
        Assert.AreEqual(1, roundTripped.ClassLevels[0].CharacterLevel);
        Assert.AreEqual("+1 Skill Point", roundTripped.ClassLevels[0].FavoredBonus);
        Assert.AreEqual("Spell Focus", roundTripped.Feats[1].Name);
        Assert.AreEqual(2, roundTripped.Feats[1].Count);
        Assert.AreEqual("Knowledge (arcana)", roundTripped.SkillAllocations[0].DisplayName);
        Assert.AreEqual(3, roundTripped.SkillAllocations[0].SkillPointsSpent);
    }

    [TestMethod]
    public void LoadsOldSaveShape()
    {
        const string json = """
            {
              "HeroName": "Old Hero",
              "PlayerName": "Original Player",
              "RulesetId": 1,
              "TargetLevel": 2,
              "Strength": 11,
              "Dexterity": 12,
              "Constitution": 13,
              "Intelligence": 14,
              "Wisdom": 15,
              "Charisma": 16,
              "Languages": ["Common"],
              "ClassLevels": [
                {
                  "TemplateId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
                  "HpNote": "8",
                  "FavoredBonus": "+1 Hit Point"
                }
              ]
            }
            """;

        var document = JsonSerializer.Deserialize<CharacterBuildDocument>(json);

        Assert.IsNotNull(document);
        Assert.AreEqual("Old Hero", document.HeroName);
        Assert.AreEqual(2, document.TargetLevel);
        Assert.AreEqual(1, document.Languages.Count);
        Assert.IsNotNull(document.Feats);
        Assert.AreEqual(0, document.Feats.Count);
        Assert.IsNotNull(document.SkillAllocations);
        Assert.AreEqual(0, document.SkillAllocations.Count);
        Assert.AreEqual(0, document.ClassLevels[0].CharacterLevel);
    }

    [TestMethod]
    public void ToleratesNullOptionalCollections()
    {
        const string json = """
            {
              "HeroName": "Hand Edited",
              "Languages": null,
              "ClassLevels": null,
              "Feats": null,
              "SkillAllocations": null
            }
            """;

        var document = JsonSerializer.Deserialize<CharacterBuildDocument>(json);

        Assert.IsNotNull(document);
        document.NormalizeCollections();
        Assert.IsNotNull(document.Languages);
        Assert.IsNotNull(document.ClassLevels);
        Assert.IsNotNull(document.Feats);
        Assert.IsNotNull(document.SkillAllocations);
    }
}
