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
}
