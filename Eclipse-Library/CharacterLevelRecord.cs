using System;

namespace Eclipse_Library
{
    public readonly struct CharacterLevelRecord
    {
        public CharacterLevelRecord(int level, string templateName, string? hpNote = null)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name is required.", nameof(templateName));
            }

            Level = level;
            TemplateName = templateName.Trim();
            HpNote = hpNote ?? "";
        }

        public int Level { get; }
        public string TemplateName { get; }
        public string HpNote { get; }

        public CharacterLevelRecord WithTemplateName(string templateName) =>
            new(Level, templateName, HpNote);

        public CharacterLevelRecord WithHpNote(string hpNote) =>
            new(Level, TemplateName, hpNote);
    }
}

