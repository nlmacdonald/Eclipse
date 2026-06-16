using System.Collections.Generic;
using System.Linq;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class CharacterBuildAssembler
    {
        public CharacterBuild CreateBuild(
            CharacterInputs inputs,
            RaceDefinitionDocument? selectedRace,
            RaceAbilityCatalogDocument? raceAbilityCatalog,
            IReadOnlyDictionary<string, int> levelAbilityScoreAdjustments,
            IReadOnlyCollection<CharacterClassLevelRow> classLevels,
            IEnumerable<SelectedFeatRow> selectedFeats,
            IEnumerable<SkillAllocationRow> skillRows)
        {
            var buildLevel = classLevels.Count;
            var build = new CharacterBuild(
                new CharacterSeed(
                    inputs.Name,
                    inputs.AbilityScores,
                    inputs.Size,
                    selectedRace,
                    raceAbilityCatalog,
                    levelAbilityScoreAdjustments),
                buildLevel);

            foreach (var row in classLevels
                .SelectMany(x => x.Purchases)
                .OrderBy(x => x.Level))
            {
                build.AddPurchase(row.Level, row.CreatePurchase());
            }

            foreach (var purchase in CreateBuilderChoicePurchases(selectedFeats, skillRows))
            {
                build.AddPurchase(buildLevel, purchase);
            }

            return build;
        }

        private static IEnumerable<IPurchase> CreateBuilderChoicePurchases(
            IEnumerable<SelectedFeatRow> selectedFeats,
            IEnumerable<SkillAllocationRow> skillRows)
        {
            foreach (var feat in selectedFeats.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Count > 0))
            {
                yield return new SelectFeatPurchase(feat.Name, feat.Count);
            }

            foreach (var skill in skillRows.Where(x => x.SkillPointsSpent > 0))
            {
                yield return new AllocateSkillRanksPurchase(
                    skill.DisplayName,
                    skill.SkillPointsSpent,
                    isRelevantSkill: skill.RankMultiplier >= 1m,
                    rankMultiplier: skill.RankMultiplier);
            }
        }
    }
}
