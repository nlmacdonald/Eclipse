using System;
using System.Linq;

namespace Eclipse_Library
{
    public static class SkillId
    {
        public static string FromName(string name)
        {
            var value = name ?? "";
            var chars = value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray();

            var normalized = new string(chars);
            while (normalized.Contains("--", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
            }

            return normalized.Trim('-');
        }

        public static string Get(SkillDefinitionDocument skill)
        {
            if (skill is null)
            {
                throw new ArgumentNullException(nameof(skill));
            }

            return string.IsNullOrWhiteSpace(skill.Id)
                ? FromName(skill.Name)
                : skill.Id.Trim();
        }
    }
}
