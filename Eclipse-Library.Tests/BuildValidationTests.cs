using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class BuildValidationTests
{
    [TestMethod]
    public void ReportsSelectedFeatAllowanceDiagnostics()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Feat Allowance Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SelectFeatPurchase("Dodge"));
        build.AddPurchase(1, new SelectFeatPurchase("Power Attack"));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "FEAT_SELECTION_OVERSPENT"));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.ValidatorId == "SELECTED_FEAT_ALLOWANCE"));
    }

    [TestMethod]
    public void ReportsSkillPointAllowanceDiagnostics()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Skill Allowance Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new AllocateSkillRanksPurchase("Perception", 1, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.ValidatorId == "SKILL_POINT_ALLOWANCE"));
    }

    [TestMethod]
    public void AcceptsSkillPointAllowanceFromTemplateAndIntelligence()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Skill Allowance Pass Test", new AbilityScores(10, 10, 10, 14, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new BuySkillRanksPurchase("Unassigned Skill Points", 4, isRelevantSkill: true));
        build.AddPurchase(1, new AllocateSkillRanksPurchase("Spellcraft", 12, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build, cpProgression: new FixedCpProgression(totalCp: 24));

        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"));
    }

    [TestMethod]
    public void PathfinderProfileUsesPerLevelIntelligenceSkillPoints()
    {
        var rulesConfig = RulesetProfile.Get(RulesetId.Pathfinder1E).CreateBuildRulesConfig();
        var build = new CharacterBuild(
            new CharacterSeed("Pathfinder Skill Test", new AbilityScores(10, 10, 10, 14, 10, 10)),
            targetLevel: 2);

        build.AddPurchase(1, new AllocateSkillRanksPurchase("Perception", 4, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer(rulesConfig).Replay(
            build,
            cpProgression: new TableCpProgression(new Dictionary<int, int>
            {
                [1] = 24,
                [2] = 48,
            }),
            rulesConfig: rulesConfig);

        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"));
    }

    [TestMethod]
    public void ClassLevelDetailsPurchaseUpdatesLevelRecordsAndFavoredSkillPoints()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Class Detail Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Wizard", "4", "+1 Skill Point", maxHitPoints: 4));
        build.AddPurchase(1, new AllocateSkillRanksPurchase("Spellcraft", 1, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "SKILL_POINTS_OVERSPENT"));
        Assert.AreEqual("Wizard", result.Character.LevelRecords[0].TemplateName);
        Assert.AreEqual("4", result.Character.LevelRecords[0].HpNote);
        Assert.AreEqual("+1 Skill Point", result.Character.LevelRecords[0].FavoredBonus);
        Assert.AreEqual(1, result.Character.BonusSkillPoints);
    }

    [TestMethod]
    public void ReportsMissingClassLevelHitPoints()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Missing HP Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Fighter", "", "+1 Hit Point", maxHitPoints: 10));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "CLASS_LEVEL_HP_MISSING"));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.ValidatorId == "CLASS_LEVEL_DETAILS"));
    }

    [TestMethod]
    public void ReportsOutOfRangeClassLevelHitPoints()
    {
        var build = new CharacterBuild(
            new CharacterSeed("HP Range Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Wizard", "9", "+1 Hit Point", maxHitPoints: 4));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "CLASS_LEVEL_HP_OUT_OF_RANGE"));
    }

    [TestMethod]
    public void ReportsInvalidFavoredClassBonus()
    {
        var rulesConfig = CreatePathfinderFavoredBonusRules();
        var build = new CharacterBuild(
            new CharacterSeed("Favored Bonus Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Rogue", "6", "Sneaky", maxHitPoints: 6));

        var result = TestReplayerFactory.CreateStandardReplayer(rulesConfig).Replay(build, rulesConfig: rulesConfig);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "FAVORED_CLASS_BONUS_INVALID"));
    }

    [TestMethod]
    public void AcceptsCustomFavoredClassBonusNote()
    {
        var rulesConfig = CreatePathfinderFavoredBonusRules();
        var build = new CharacterBuild(
            new CharacterSeed("Custom Favored Bonus Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Rogue", "6", "Custom: alternate racial bonus", maxHitPoints: 6));

        var result = TestReplayerFactory.CreateStandardReplayer(rulesConfig).Replay(build, rulesConfig: rulesConfig);

        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "FAVORED_CLASS_BONUS_INVALID"));
        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "CLASS_LEVEL_HP_OUT_OF_RANGE"));
    }

    [TestMethod]
    public void AcceptsRaceClassSpecificPathfinderFavoredBonus()
    {
        var rulesConfig = CreatePathfinderFavoredBonusRules();
        var build = new CharacterBuild(
            new CharacterSeed("Race Favored Bonus Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Rogue", "6", "+1/2 trap sense bonus", maxHitPoints: 6));

        var result = TestReplayerFactory.CreateStandardReplayer(rulesConfig).Replay(build, rulesConfig: rulesConfig);

        Assert.IsFalse(result.Diagnostics.Any(x => x.Code == "FAVORED_CLASS_BONUS_INVALID"));
    }

    [TestMethod]
    public void ReportsDndFavoredClassBonusAsUnsupportedWarning()
    {
        var build = new CharacterBuild(
            new CharacterSeed("D&D Favored Bonus Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new SetClassLevelDetailsPurchase(1, "Fighter", "10", "+1 Hit Point", maxHitPoints: 10));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsFalse(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "FAVORED_CLASS_BONUS_NOT_SUPPORTED"));
    }

    private static BuildRulesConfig CreatePathfinderFavoredBonusRules()
    {
        var race = new RaceDefinitionDocument
        {
            Name = "Human",
            Rules = new RaceRulesDocument
            {
                FavoredClassBonuses = new List<FavoredClassBonusOptionDocument>
                {
                    new() { ClassName = "Rogue", Bonus = "+1/2 trap sense bonus" },
                },
            },
        };

        var rulesConfig = RulesetProfile.Get(RulesetId.Pathfinder1E).CreateBuildRulesConfig();
        rulesConfig.FavoredClassBonuses = FavoredClassBonusRulesConfig.FromRace(RulesetId.Pathfinder1E, race);
        return rulesConfig;
    }
}
