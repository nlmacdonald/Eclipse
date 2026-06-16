using System;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class RulesCatalogStore
    {
        private readonly CatalogPathResolver _pathResolver;

        public RulesCatalogStore()
            : this(new CatalogPathResolver())
        {
        }

        public RulesCatalogStore(CatalogPathResolver pathResolver)
        {
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public SkillCatalogDocument LoadSkillCatalog()
        {
            return SkillCatalogJson.LoadFromFile(_pathResolver.FindDocsFile("skills.catalog.json"));
        }

        public FeatCatalogDocument LoadFeatCatalog()
        {
            return FeatCatalogJson.LoadFromFile(_pathResolver.FindDocsFile("feats.catalog.json"));
        }

        public RaceCatalogDocument LoadRaceCatalog(RulesetId rulesetId)
        {
            return RaceCatalogJson.LoadFromFile(_pathResolver.FindDocsFile(GetRaceCatalogFileName(rulesetId)));
        }

        public RaceAbilityCatalogDocument LoadRaceAbilityCatalog()
        {
            return RaceAbilityCatalogJson.LoadFromFile(_pathResolver.FindDocsFile("race-abilities.catalog.json"));
        }

        public LanguageCatalogDocument LoadLanguageCatalog()
        {
            return LanguageCatalogJson.LoadFromFile(_pathResolver.FindDocsFile("languages.catalog.json"));
        }

        public AlignmentCatalogDocument LoadAlignmentCatalog()
        {
            return AlignmentCatalogJson.LoadFromFile(_pathResolver.FindDocsFile("alignments.catalog.json"));
        }

        public (LevelProgressionCatalogDocument Catalog, string Path) LoadLevelProgressionCatalog()
        {
            var path = _pathResolver.FindDocsFile("leveling.config.json");
            return (LevelProgressionJson.LoadFromFile(path), path);
        }

        private static string GetRaceCatalogFileName(RulesetId rulesetId)
        {
            return rulesetId switch
            {
                RulesetId.Dnd30 => "races.dnd30.catalog.json",
                RulesetId.Dnd35 => "races.dnd35.catalog.json",
                RulesetId.Pathfinder1E => "races.pathfinder1e.catalog.json",
                _ => "races.dnd35.catalog.json",
            };
        }
    }
}
