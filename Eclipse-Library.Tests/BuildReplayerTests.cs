using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class BuildReplayerTests
{
    [TestMethod]
    public void AppliesPurchasesAndBuilderChoices()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Replay Test", new AbilityScores(10, 10, 10, 14, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new BuyWarcraftPurchase(1));
        build.AddPurchase(1, new SelectFeatPurchase("Combat Casting"));
        build.AddPurchase(1, new AllocateSkillRanksPurchase("Spellcraft", 4, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(1, result.Character.Warcraft);
        Assert.AreEqual(1, result.Character.SelectedFeats["Combat Casting"]);
        Assert.AreEqual(4m, result.Character.GetSkillRanks("Spellcraft"));
        Assert.AreEqual(4, result.Character.Skills["Spellcraft"].CpInvested);
    }

    [TestMethod]
    public void ReportsCpOverspendDiagnostics()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Overspend Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new BuyBonusFeatPurchase(count: 5));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build, cpProgression: new FixedCpProgression(totalCp: 6));

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "CP_OVERSPENT"));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.Stage == BuildDiagnosticStage.AtLevel));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.Stage == BuildDiagnosticStage.Final));
    }

    [TestMethod]
    public void AdvancesLevelsWithCpProgression()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Level Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 2);

        build.AddPurchase(2, new BuyWarcraftPurchase(1));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build, cpProgression: new TableCpProgression(new Dictionary<int, int>
        {
            [1] = 24,
            [2] = 48,
        }));

        Assert.IsFalse(result.HasErrors);
        Assert.AreEqual(2, result.Character.Level);
        Assert.AreEqual(48, result.Character.TotalCp);
        Assert.AreEqual(1, result.Character.Warcraft);
        Assert.IsTrue(result.AppliedPurchases.Any(x => x.SourceLevel == 2 && x.Success));
    }

    [TestMethod]
    public void ReportsSkillRankCapDiagnostics()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Skill Cap Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new AllocateSkillRanksPurchase("Perception", 5, isRelevantSkill: true, rankMultiplier: 1m));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "SKILL_CAP_EXCEEDED"));
        Assert.IsTrue(result.Diagnostics.Any(x => x.Context?.ValidatorId == "SKILL_RANK_CAP_BY_LEVEL"));
    }

    [TestMethod]
    public void RecordsFailedPurchasesAndContinues()
    {
        var build = new CharacterBuild(
            new CharacterSeed("Failure Test", new AbilityScores(10, 10, 10, 10, 10, 10)),
            targetLevel: 1);

        build.AddPurchase(1, new FailingPurchase("broken purchase"));
        build.AddPurchase(1, new SelectFeatPurchase("Dodge"));

        var result = TestReplayerFactory.CreateStandardReplayer().Replay(build);

        Assert.IsTrue(result.HasErrors);
        Assert.AreEqual(2, result.AppliedPurchases.Count);
        Assert.IsFalse(result.AppliedPurchases[0].Success);
        Assert.IsTrue(result.AppliedPurchases[1].Success);
        Assert.IsTrue(result.Diagnostics.Any(x => x.Code == "ENGINE_APPLY_FAILED"));
        Assert.AreEqual(1, result.Character.SelectedFeats["Dodge"]);
    }
}
