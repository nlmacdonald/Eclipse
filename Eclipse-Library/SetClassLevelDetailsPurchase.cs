using System;

namespace Eclipse_Library
{
    public sealed class SetClassLevelDetailsPurchase : IPurchase
    {
        public SetClassLevelDetailsPurchase(
            int characterLevel,
            string templateName,
            string hpNote,
            string favoredBonus,
            int? maxHitPoints)
        {
            if (characterLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(characterLevel), characterLevel, "Character level must be >= 1.");
            }

            CharacterLevel = characterLevel;
            TemplateName = string.IsNullOrWhiteSpace(templateName) ? "Unassigned" : templateName.Trim();
            HpNote = hpNote ?? "";
            FavoredBonus = favoredBonus ?? "";
            MaxHitPoints = maxHitPoints;
        }

        public int CharacterLevel { get; }
        public string TemplateName { get; }
        public string HpNote { get; }
        public string FavoredBonus { get; }
        public int? MaxHitPoints { get; }

        public string Description => $"Set level {CharacterLevel} details for {TemplateName}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            character.SetTemplateNameForLevel(CharacterLevel, TemplateName);
            character.SetHpNoteForLevel(CharacterLevel, HpNote);
            character.SetFavoredBonusForLevel(CharacterLevel, FavoredBonus);

            if (IsSkillPointFavoredBonus(FavoredBonus))
            {
                character.AddBonusSkillPoints(1);
            }
        }

        public static bool IsHitPointFavoredBonus(string favoredBonus) =>
            string.Equals(Normalize(favoredBonus), "+1 hit point", StringComparison.OrdinalIgnoreCase);

        public static bool IsSkillPointFavoredBonus(string favoredBonus) =>
            string.Equals(Normalize(favoredBonus), "+1 skill point", StringComparison.OrdinalIgnoreCase);

        public static bool IsCustomFavoredBonus(string favoredBonus) =>
            Normalize(favoredBonus).StartsWith("custom:", StringComparison.OrdinalIgnoreCase);

        private static string Normalize(string value) => (value ?? "").Trim();
    }
}
