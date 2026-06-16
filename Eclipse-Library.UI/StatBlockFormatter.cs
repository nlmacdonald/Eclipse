using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class StatBlockFormatter
    {
        public string BuildEmpty(
            CharacterInputs inputs,
            Character character,
            string playerName,
            string rulesetName,
            IEnumerable<SelectedFeatRow>? feats = null,
            IEnumerable<SkillAllocationRow>? skills = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(inputs.Name);
            sb.AppendLine($"Player: {playerName}");
            sb.AppendLine($"Ruleset: {rulesetName}");
            sb.AppendLine($"Target Level: {inputs.TargetLevel}");
            sb.AppendLine($"Race: {inputs.Race}");
            sb.AppendLine($"Size: {character.Size}");
            sb.AppendLine();
            sb.AppendLine("Ability Scores");
            AppendAbilityScores(sb, character);
            AppendFeatLines(sb, feats);
            AppendSkillLines(sb, skills);
            sb.AppendLine();
            sb.AppendLine("Add a class level to begin building this hero.");
            return sb.ToString();
        }

        public string Build(
            CharacterBuildResult result,
            CharacterInputs inputs,
            CharacterTemplateDefinition template,
            IEnumerable<CharacterClassLevelRow>? classLevels = null,
            IEnumerable<SelectedFeatRow>? feats = null,
            IEnumerable<SkillAllocationRow>? skills = null)
        {
            var character = result.Character;
            var levels = classLevels?.ToList() ?? new List<CharacterClassLevelRow>();
            var sb = new StringBuilder();
            sb.AppendLine(character.Name);
            sb.AppendLine($"Level {character.Level}");
            if (levels.Count > 0)
            {
                sb.AppendLine("Classes: " + string.Join(", ", levels.GroupBy(x => x.TemplateName).Select(x => $"{x.Key} {x.Count()}")));
            }
            else
            {
                sb.AppendLine($"Template: {template.Name} ({template.Id})");
            }

            sb.AppendLine($"Ruleset: {template.Ruleset.DisplayName}");
            sb.AppendLine($"Race: {inputs.Race}");
            sb.AppendLine($"Size: {inputs.Size}");
            sb.AppendLine($"Alignment: {inputs.Alignment}");
            if (!string.IsNullOrWhiteSpace(inputs.Deity))
            {
                sb.AppendLine($"Deity: {inputs.Deity}");
            }

            sb.AppendLine($"HP {inputs.HitPoints}; Starting Gold {inputs.StartingGold}");
            sb.AppendLine($"Languages: {(inputs.Languages.Count == 0 ? "None" : string.Join(", ", inputs.Languages))}");
            sb.AppendLine();
            sb.AppendLine("Ability Scores");
            var abilityScores = character.AbilityScores;
            AppendAbilityScores(sb, character);

            if (character.MovementSpeeds.Count > 0)
            {
                sb.AppendLine("Speed: " + string.Join(", ", character.MovementSpeeds.OrderBy(x => x.Key).Select(x => $"{x.Key} {x.Value} ft.")));
            }

            if (character.SenseRanges.Count > 0)
            {
                sb.AppendLine("Senses: " + string.Join(", ", character.SenseRanges.OrderBy(x => x.Key).Select(x => x.Value == 0 ? x.Key : $"{x.Key} {x.Value} ft.")));
            }

            foreach (var note in character.StatBlockNotes)
            {
                sb.AppendLine(note);
            }

            sb.AppendLine();
            sb.AppendLine("Combat");
            var size = character.SizeProfile;
            var strengthMod = AbilityScores.GetModifier(abilityScores.Strength);
            var dexterityMod = AbilityScores.GetModifier(abilityScores.Dexterity);
            sb.AppendLine($"BAB (Warcraft): {FormatSigned(character.Warcraft + size.AcAttackModifier)}");
            sb.AppendLine($"Melee BAB: {FormatSigned(character.Warcraft + strengthMod + size.AcAttackModifier)}");
            sb.AppendLine($"Ranged BAB: {FormatSigned(character.Warcraft + dexterityMod + size.AcAttackModifier)}");
            sb.AppendLine($"AC: {10 + dexterityMod + size.AcAttackModifier}");
            sb.AppendLine($"Grapple: {FormatSigned(character.Warcraft + strengthMod + size.GrappleModifier)}");
            sb.AppendLine($"Space/Reach: {size.Space}/{size.Reach}");
            sb.AppendLine($"Size Modifiers: AC/Attack {FormatSigned(size.AcAttackModifier)}, Grapple {FormatSigned(size.GrappleModifier)}, Hide/Stealth {FormatSigned(size.HideModifier)}");
            sb.AppendLine();
            sb.AppendLine("Saves");
            sb.AppendLine($"Fortitude: {FormatSigned(GetSaveTotal(character, SaveType.Fortitude, AbilityScores.GetModifier(abilityScores.Constitution)))}");
            sb.AppendLine($"Reflex: {FormatSigned(GetSaveTotal(character, SaveType.Reflex, AbilityScores.GetModifier(abilityScores.Dexterity)))}");
            sb.AppendLine($"Will: {FormatSigned(GetSaveTotal(character, SaveType.Will, AbilityScores.GetModifier(abilityScores.Wisdom)))}");
            AppendFeatLines(sb, feats);
            AppendSkillLines(sb, skills);
            sb.AppendLine();
            sb.AppendLine(character.GetSummary());
            sb.AppendLine();
            sb.AppendLine("Hit Dice By Level");
            for (var level = 1; level <= character.Level; level++)
            {
                var hitDice = character.GetHitDiceForLevel(level);
                var secondary = hitDice.Secondary is null ? "" : $", second {hitDice.Secondary}";
                var classLevel = levels.FirstOrDefault(x => x.CharacterLevel == level);
                var classText = classLevel is null ? "" : $" ({classLevel.DisplayName})";
                sb.AppendLine($"L{level}: {hitDice.Primary}{secondary}{classText}");
            }

            sb.AppendLine();
            sb.AppendLine("Purchased Abilities");
            if (character.PurchasedAbilities.Count == 0)
            {
                sb.AppendLine("None");
            }
            else
            {
                foreach (var ability in character.PurchasedAbilities)
                {
                    var options = ability.SelectedOptions.Count == 0
                        ? ""
                        : $" [{string.Join(", ", ability.SelectedOptions.Select(x => x.Name))}]";
                    sb.AppendLine($"{ability.Definition.Name}{options}: {ability.CostCp} CP");
                }
            }

            return sb.ToString();
        }

        private static void AppendAbilityScores(StringBuilder sb, Character character)
        {
            sb.AppendLine(FormatAbilityScoreLine(character, "Strength", "Str"));
            sb.AppendLine(FormatAbilityScoreLine(character, "Dexterity", "Dex"));
            sb.AppendLine(FormatAbilityScoreLine(character, "Constitution", "Con"));
            sb.AppendLine(FormatAbilityScoreLine(character, "Intelligence", "Int"));
            sb.AppendLine(FormatAbilityScoreLine(character, "Wisdom", "Wis"));
            sb.AppendLine(FormatAbilityScoreLine(character, "Charisma", "Cha"));
        }

        private static void AppendFeatLines(StringBuilder sb, IEnumerable<SelectedFeatRow>? feats)
        {
            var selectedFeats = (feats ?? Enumerable.Empty<SelectedFeatRow>())
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Count > 0)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedFeats.Count == 0)
            {
                return;
            }

            sb.AppendLine();
            sb.AppendLine("Feats");
            foreach (var feat in selectedFeats)
            {
                var count = feat.Count > 1 ? $" x{feat.Count}" : "";
                sb.AppendLine($"{feat.Name}{count}");
            }
        }

        private static void AppendSkillLines(StringBuilder sb, IEnumerable<SkillAllocationRow>? skills)
        {
            var allocatedSkills = (skills ?? Enumerable.Empty<SkillAllocationRow>())
                .Where(x => x.SkillPointsSpent > 0)
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (allocatedSkills.Count == 0)
            {
                return;
            }

            sb.AppendLine();
            sb.AppendLine("Skills");
            foreach (var skill in allocatedSkills)
            {
                sb.AppendLine($"{skill.DisplayName} {skill.TotalModifierText} ({skill.RanksText} rank{(skill.Ranks == 1m ? "" : "s")}, {skill.AttributeShort}, {skill.SkillPointsSpent} SP)");
            }
        }

        private static int GetSaveTotal(Character character, SaveType saveType, int abilityModifier)
        {
            return character.SaveBonuses.TryGetValue(saveType, out var baseSave)
                ? baseSave + abilityModifier
                : abilityModifier;
        }

        private static string FormatAbilityScoreLine(Character character, string ability, string abbreviation)
        {
            var breakdown = character.GetAbilityScoreBreakdown(ability);
            var pieces = new List<string> { $"base {breakdown.BaseScore}" };
            pieces.AddRange(breakdown.Contributions.Select(FormatAbilityScoreContributionInline));
            return $"{abbreviation} {breakdown.Total} ({string.Join(", ", pieces)}; mod {FormatSigned(breakdown.Modifier)})";
        }

        private static string FormatAbilityScoreContributionInline(AbilityScoreContribution contribution)
        {
            return $"{FormatSigned(contribution.Amount)} {FormatContributionKind(contribution)} from {contribution.Source}";
        }

        private static string FormatContributionKind(AbilityScoreContribution contribution)
        {
            if (contribution.Amount < 0)
            {
                return contribution.Type == BonusType.Untyped
                    ? "untyped penalty"
                    : $"{contribution.Type.ToString().ToLowerInvariant()} penalty";
            }

            return contribution.Type == BonusType.Untyped
                ? "untyped bonus"
                : $"{contribution.Type.ToString().ToLowerInvariant()} bonus";
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }
    }
}
