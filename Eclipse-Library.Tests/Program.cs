using System.Text.Json;
using Eclipse_Library;

var tests = new (string Name, Action Run)[]
{
    ("CharacterBuildDocument round-trips character builder state", CharacterBuildDocumentRoundTrips),
    ("CharacterBuildDocument loads old saves without new state", CharacterBuildDocumentLoadsOldSaveShape),
    ("CharacterBuildDocument tolerates null optional collections", CharacterBuildDocumentToleratesNullCollections),
    ("Builder choice purchases update character state without CP spend", BuilderChoicePurchasesUpdateCharacterState),
    ("BuildReplayer applies purchases and builder choices", BuildReplayerAppliesPurchasesAndBuilderChoices),
    ("BuildReplayer reports CP overspend diagnostics", BuildReplayerReportsCpOverspendDiagnostics),
    ("BuildReplayer advances levels with CP progression", BuildReplayerAdvancesLevelsWithCpProgression),
    ("BuildReplayer reports skill rank cap diagnostics", BuildReplayerReportsSkillRankCapDiagnostics),
    ("BuildReplayer records failed purchases and continues", BuildReplayerRecordsFailedPurchasesAndContinues),
    ("BuildReplayer reports selected feat allowance diagnostics", BuildReplayerReportsSelectedFeatAllowanceDiagnostics),
    ("BuildReplayer reports skill point allowance diagnostics", BuildReplayerReportsSkillPointAllowanceDiagnostics),
    ("BuildReplayer accepts skill point allowance from template and Intelligence", BuildReplayerAcceptsSkillPointAllowanceFromTemplateAndIntelligence),
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

static void CharacterBuildDocumentLoadsOldSaveShape()
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

    var document = JsonSerializer.Deserialize<CharacterBuildDocument>(json)
        ?? throw new InvalidOperationException("Document deserialized as null.");

    AssertEqual("Old Hero", document.HeroName, "HeroName");
    AssertEqual(2, document.TargetLevel, "TargetLevel");
    AssertEqual(1, document.Languages.Count, "Languages.Count");
    AssertNotNull(document.Feats, "Feats");
    AssertEqual(0, document.Feats.Count, "Feats.Count");
    AssertNotNull(document.SkillAllocations, "SkillAllocations");
    AssertEqual(0, document.SkillAllocations.Count, "SkillAllocations.Count");
    AssertEqual(0, document.ClassLevels[0].CharacterLevel, "ClassLevels[0].CharacterLevel default");
}

static void CharacterBuildDocumentToleratesNullCollections()
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

    var document = JsonSerializer.Deserialize<CharacterBuildDocument>(json)
        ?? throw new InvalidOperationException("Document deserialized as null.");

    document.NormalizeCollections();

    AssertNotNull(document.Languages, "Languages");
    AssertNotNull(document.ClassLevels, "ClassLevels");
    AssertNotNull(document.Feats, "Feats");
    AssertNotNull(document.SkillAllocations, "SkillAllocations");
}

static void BuilderChoicePurchasesUpdateCharacterState()
{
    var character = Character.CreateLevelOne("Choice Test", new AbilityScores(10, 10, 10, 10, 10, 10), totalCp: 24);

    new SelectFeatPurchase("Power Attack", count: 2).Apply(character);
    new AllocateSkillRanksPurchase("Knowledge (arcana)", skillPointsSpent: 3, isRelevantSkill: false, rankMultiplier: 0.5m).Apply(character);

    AssertEqual(0, character.SpentCp, "SpentCp");
    AssertEqual(2, character.SelectedFeats["Power Attack"], "SelectedFeats[Power Attack]");
    AssertEqual(1.5m, character.GetSkillRanks("Knowledge (arcana)"), "Knowledge (arcana) ranks");
    AssertEqual(3, character.Skills["Knowledge (arcana)"].CpInvested, "Knowledge (arcana) skill points invested");
}

static void BuildReplayerAppliesPurchasesAndBuilderChoices()
{
    var build = new CharacterBuild(
        new CharacterSeed("Replay Test", new AbilityScores(10, 10, 10, 14, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new BuyWarcraftPurchase(1));
    build.AddPurchase(1, new SelectFeatPurchase("Combat Casting"));
    build.AddPurchase(1, new AllocateSkillRanksPurchase("Spellcraft", 4, isRelevantSkill: true, rankMultiplier: 1m));

    var result = CreateStandardReplayer().Replay(build);

    AssertEqual(false, result.HasErrors, "HasErrors");
    AssertEqual(1, result.Character.Warcraft, "Warcraft");
    AssertEqual(1, result.Character.SelectedFeats["Combat Casting"], "SelectedFeats[Combat Casting]");
    AssertEqual(4m, result.Character.GetSkillRanks("Spellcraft"), "Spellcraft ranks");
    AssertEqual(4, result.Character.Skills["Spellcraft"].CpInvested, "Spellcraft invested");
}

static void BuildReplayerReportsCpOverspendDiagnostics()
{
    var build = new CharacterBuild(
        new CharacterSeed("Overspend Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new BuyBonusFeatPurchase(count: 5));

    var result = CreateStandardReplayer().Replay(build, cpProgression: new FixedCpProgression(totalCp: 6));

    AssertEqual(true, result.HasErrors, "HasErrors");
    AssertEqual(true, result.Diagnostics.Any(x => x.Code == "CP_OVERSPENT"), "CP_OVERSPENT diagnostic present");
    AssertEqual(true, result.Diagnostics.Any(x => x.Context?.Stage == BuildDiagnosticStage.AtLevel), "AtLevel diagnostic present");
    AssertEqual(true, result.Diagnostics.Any(x => x.Context?.Stage == BuildDiagnosticStage.Final), "Final diagnostic present");
}

static void BuildReplayerAdvancesLevelsWithCpProgression()
{
    var build = new CharacterBuild(
        new CharacterSeed("Level Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 2);

    build.AddPurchase(2, new BuyWarcraftPurchase(1));

    var result = CreateStandardReplayer().Replay(build, cpProgression: new TableCpProgression(new Dictionary<int, int>
    {
        [1] = 24,
        [2] = 48,
    }));

    AssertEqual(false, result.HasErrors, "HasErrors");
    AssertEqual(2, result.Character.Level, "Level");
    AssertEqual(48, result.Character.TotalCp, "TotalCp");
    AssertEqual(1, result.Character.Warcraft, "Warcraft");
    AssertEqual(true, result.AppliedPurchases.Any(x => x.SourceLevel == 2 && x.Success), "Level 2 purchase applied");
}

static void BuildReplayerReportsSkillRankCapDiagnostics()
{
    var build = new CharacterBuild(
        new CharacterSeed("Skill Cap Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new AllocateSkillRanksPurchase("Perception", 5, isRelevantSkill: true, rankMultiplier: 1m));

    var result = CreateStandardReplayer().Replay(build);

    AssertEqual(true, result.HasErrors, "HasErrors");
    AssertEqual(true, result.Diagnostics.Any(x => x.Code == "SKILL_CAP_EXCEEDED"), "SKILL_CAP_EXCEEDED diagnostic present");
    AssertEqual(true, result.Diagnostics.Any(x => x.Context?.ValidatorId == "SKILL_RANK_CAP_BY_LEVEL"), "Skill cap validator id present");
}

static void BuildReplayerRecordsFailedPurchasesAndContinues()
{
    var build = new CharacterBuild(
        new CharacterSeed("Failure Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new FailingPurchase("broken purchase"));
    build.AddPurchase(1, new SelectFeatPurchase("Dodge"));

    var result = CreateStandardReplayer().Replay(build);

    AssertEqual(true, result.HasErrors, "HasErrors");
    AssertEqual(2, result.AppliedPurchases.Count, "AppliedPurchases.Count");
    AssertEqual(false, result.AppliedPurchases[0].Success, "First purchase success");
    AssertEqual(true, result.AppliedPurchases[1].Success, "Second purchase success");
    AssertEqual(true, result.Diagnostics.Any(x => x.Code == "ENGINE_APPLY_FAILED"), "ENGINE_APPLY_FAILED diagnostic present");
    AssertEqual(1, result.Character.SelectedFeats["Dodge"], "SelectedFeats[Dodge]");
}

static void BuildReplayerReportsSelectedFeatAllowanceDiagnostics()
{
    var build = new CharacterBuild(
        new CharacterSeed("Feat Allowance Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new SelectFeatPurchase("Dodge"));
    build.AddPurchase(1, new SelectFeatPurchase("Power Attack"));

    var result = CreateStandardReplayer().Replay(build);

    AssertEqual(true, result.HasErrors, "HasErrors");
    AssertEqual(true, result.Diagnostics.Any(x => x.Code == "FEAT_SELECTION_OVERSPENT"), "FEAT_SELECTION_OVERSPENT diagnostic present");
    AssertEqual(true, result.Diagnostics.Any(x => x.Context?.ValidatorId == "SELECTED_FEAT_ALLOWANCE"), "Feat allowance validator id present");
}

static void BuildReplayerReportsSkillPointAllowanceDiagnostics()
{
    var build = new CharacterBuild(
        new CharacterSeed("Skill Allowance Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new AllocateSkillRanksPurchase("Perception", 1, isRelevantSkill: true, rankMultiplier: 1m));

    var result = CreateStandardReplayer().Replay(build);

    AssertEqual(true, result.HasErrors, "HasErrors");
    AssertEqual(true, result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"), "SKILL_POINTS_OVERSPENT diagnostic present");
    AssertEqual(true, result.Diagnostics.Any(x => x.Context?.ValidatorId == "SKILL_POINT_ALLOWANCE"), "Skill point allowance validator id present");
}

static void BuildReplayerAcceptsSkillPointAllowanceFromTemplateAndIntelligence()
{
    var build = new CharacterBuild(
        new CharacterSeed("Skill Allowance Pass Test", new AbilityScores(10, 10, 10, 14, 10, 10)),
        targetLevel: 1);

    build.AddPurchase(1, new BuySkillRanksPurchase("Unassigned Skill Points", 4, isRelevantSkill: true));
    build.AddPurchase(1, new AllocateSkillRanksPurchase("Spellcraft", 12, isRelevantSkill: true, rankMultiplier: 1m));

    var result = CreateStandardReplayer().Replay(build, cpProgression: new FixedCpProgression(totalCp: 24));

    AssertEqual(false, result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"), "SKILL_POINTS_OVERSPENT diagnostic absent");
}

static BuildReplayer CreateStandardReplayer()
{
    var rulesConfig = new BuildRulesConfig();
    var stepValidators = new IBuildStepValidator[]
    {
        new WarcraftCapByLevelValidator(),
        new SkillRankCapByLevelValidator(rulesConfig.Skills),
        new CpOverspendByLevelValidator(),
    };

    var finalValidators = new IFinalBuildValidator[]
    {
        new CpOverspendFinalValidator(),
        new SelectedFeatAllowanceValidator(),
        new SkillPointAllowanceValidator(rulesConfig.Skills),
    };

    return new BuildReplayer(stepValidators, finalValidators);
}

static void AssertEqual<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label}: expected '{expected}', got '{actual}'.");
    }
}

static void AssertNotNull(object? value, string label)
{
    if (value is null)
    {
        throw new InvalidOperationException($"{label}: expected non-null value.");
    }
}

sealed class FixedCpProgression : ICpProgression
{
    private readonly int _totalCp;

    public FixedCpProgression(int totalCp)
    {
        _totalCp = totalCp;
    }

    public int GetTotalCpAtLevel(int level)
    {
        return _totalCp;
    }
}

sealed class TableCpProgression : ICpProgression
{
    private readonly IReadOnlyDictionary<int, int> _totalsByLevel;

    public TableCpProgression(IReadOnlyDictionary<int, int> totalsByLevel)
    {
        _totalsByLevel = totalsByLevel;
    }

    public int GetTotalCpAtLevel(int level)
    {
        return _totalsByLevel.TryGetValue(level, out var total)
            ? total
            : _totalsByLevel.Values.LastOrDefault();
    }
}

sealed class FailingPurchase : IPurchase
{
    public FailingPurchase(string description)
    {
        Description = description;
    }

    public string Description { get; }

    public void Apply(Character character)
    {
        throw new InvalidOperationException("Intentional test failure.");
    }
}
