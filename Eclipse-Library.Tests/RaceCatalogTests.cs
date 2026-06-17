using Eclipse_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eclipse_Library.Tests;

[TestClass]
public sealed class RaceCatalogTests
{
    [TestMethod]
    public void PathfinderCatalogLoadsFavoredClassBonusEntries()
    {
        var catalog = RaceCatalogJson.LoadFromFile(FindDocsFile("races.pathfinder1e.catalog.json"));
        var human = catalog.Races.First(x => x.Name == "Human");
        var elf = catalog.Races.First(x => x.Name == "Elf");
        var dwarf = catalog.Races.First(x => x.Name == "Dwarf");

        Assert.IsTrue(human.Rules.FavoredClassBonuses?.Any(x => x.ClassName == "Rogue" && x.Bonus == "+1/6 rogue talent") == true);
        Assert.IsTrue(elf.Rules.FavoredClassBonuses?.Any(x => x.ClassName == "Wizard" && x.Bonus == "+1/2 arcane school power use") == true);
        Assert.IsTrue(dwarf.Rules.FavoredClassBonuses?.Any(x => x.ClassName == "Fighter" && x.Bonus == "+1 CMD vs bull rush or trip") == true);
    }

    [TestMethod]
    public void FavoredClassBonusRulesResolveSpecificOptionsAndDescriptions()
    {
        var catalog = RaceCatalogJson.LoadFromFile(FindDocsFile("races.pathfinder1e.catalog.json"));
        var human = catalog.Races.First(x => x.Name == "Human");
        var rules = FavoredClassBonusRulesConfig.FromRace(RulesetId.Pathfinder1E, human);

        var rogueOptions = rules.GetAllowedBonuses("Rogue");
        var fighterOptions = rules.GetAllowedBonuses("Fighter");

        CollectionAssert.Contains(rogueOptions.ToList(), "+1 Hit Point");
        CollectionAssert.Contains(rogueOptions.ToList(), "+1 Skill Point");
        CollectionAssert.Contains(rogueOptions.ToList(), "+1/6 rogue talent");
        CollectionAssert.DoesNotContain(fighterOptions.ToList(), "+1/6 rogue talent");
        Assert.AreEqual("Add 1/6 of a rogue talent.", rules.GetDescription("Rogue", "+1/6 rogue talent"));
        Assert.AreEqual("Add 1 hit point for this class level.", rules.GetDescription("Rogue", "+1 Hit Point"));
    }

    private static string FindDocsFile(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "docs", fileName);
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find docs file '{fileName}'.");
    }
}
