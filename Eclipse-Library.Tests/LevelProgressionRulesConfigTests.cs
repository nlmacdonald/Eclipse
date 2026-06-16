using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class LevelProgressionRulesConfigTests
{
    [TestMethod]
    public void UsesConfiguredFeatTable()
    {
        var catalog = new LevelProgressionCatalogDocument
        {
            Rulesets =
            {
                new RulesetLevelProgressionDocument
                {
                    RulesetId = RulesetId.Dnd35,
                    Tables =
                    {
                        new LevelProgressionTableDocument
                        {
                            Levels =
                            {
                                new LevelProgressionLevelDocument { Level = 1, GrantsFeat = true },
                                new LevelProgressionLevelDocument { Level = 2, GrantsFeat = true },
                                new LevelProgressionLevelDocument { Level = 3 },
                            },
                        },
                    },
                },
            },
        };

        var rules = LevelProgressionRulesConfig.FromCatalog(catalog, RulesetId.Dnd35);

        Assert.AreEqual(2, rules.GetFeatsGrantedByLevel(3));
    }

    [TestMethod]
    public void UsesRulesetFallbackFeatCadence()
    {
        var dndRules = LevelProgressionRulesConfig.FromCatalog(null, RulesetId.Dnd35);
        var pathfinderRules = LevelProgressionRulesConfig.FromCatalog(null, RulesetId.Pathfinder1E);

        Assert.AreEqual(2, dndRules.GetFeatsGrantedByLevel(5));
        Assert.AreEqual(3, pathfinderRules.GetFeatsGrantedByLevel(5));
    }
}
