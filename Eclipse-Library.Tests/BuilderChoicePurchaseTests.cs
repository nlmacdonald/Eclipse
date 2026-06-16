using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class BuilderChoicePurchaseTests
{
    [TestMethod]
    public void PurchasesUpdateCharacterStateWithoutCpSpend()
    {
        var character = Character.CreateLevelOne("Choice Test", new AbilityScores(10, 10, 10, 10, 10, 10), totalCp: 24);

        new SelectFeatPurchase("Power Attack", count: 2).Apply(character);
        new AllocateSkillRanksPurchase("Knowledge (arcana)", skillPointsSpent: 3, isRelevantSkill: false, rankMultiplier: 0.5m).Apply(character);

        Assert.AreEqual(0, character.SpentCp);
        Assert.AreEqual(2, character.SelectedFeats["Power Attack"]);
        Assert.AreEqual(1.5m, character.GetSkillRanks("Knowledge (arcana)"));
        Assert.AreEqual(3, character.Skills["Knowledge (arcana)"].CpInvested);
    }
}
