using System.Text.Json;
using Eclipse_Library;

var tests = new (string Name, Action Run)[]
{
    ("CharacterBuildDocument round-trips character builder state", CharacterBuildDocumentRoundTrips),
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failures.Add($"{test.Name}: {ex.Message}");
        Console.Error.WriteLine($"FAIL {test.Name}");
        Console.Error.WriteLine(ex);
    }
}

if (failures.Count > 0)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Failures:");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine(failure);
    }

    Environment.ExitCode = 1;
}

static void CharacterBuildDocumentRoundTrips()
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
    var roundTripped = JsonSerializer.Deserialize<CharacterBuildDocument>(json)
        ?? throw new InvalidOperationException("Document deserialized as null.");

    AssertEqual(2, roundTripped.SchemaVersion, "SchemaVersion");
    AssertEqual("Asha", roundTripped.HeroName, "HeroName");
    AssertEqual(RulesetId.Pathfinder1E, roundTripped.RulesetId, "RulesetId");
    AssertEqual(PathfinderXpProgression.Fast, roundTripped.PathfinderProgression, "PathfinderProgression");
    AssertEqual(1, roundTripped.LevelAbilityScoreAdjustments["Intelligence"], "LevelAbilityScoreAdjustments");
    AssertEqual(2, roundTripped.Languages.Count, "Languages.Count");
    AssertEqual(templateId, roundTripped.ClassLevels[0].TemplateId, "ClassLevels[0].TemplateId");
    AssertEqual(1, roundTripped.ClassLevels[0].CharacterLevel, "ClassLevels[0].CharacterLevel");
    AssertEqual("+1 Skill Point", roundTripped.ClassLevels[0].FavoredBonus, "ClassLevels[0].FavoredBonus");
    AssertEqual("Spell Focus", roundTripped.Feats[1].Name, "Feats[1].Name");
    AssertEqual(2, roundTripped.Feats[1].Count, "Feats[1].Count");
    AssertEqual("Knowledge (arcana)", roundTripped.SkillAllocations[0].DisplayName, "SkillAllocations[0].DisplayName");
    AssertEqual(3, roundTripped.SkillAllocations[0].SkillPointsSpent, "SkillAllocations[0].SkillPointsSpent");
}

static void AssertEqual<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label}: expected '{expected}', got '{actual}'.");
    }
}
