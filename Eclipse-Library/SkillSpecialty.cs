using System;

namespace Eclipse_Library
{
    public sealed class SkillSpecialty
    {
        public SkillSpecialty(string skillName, string specialtyName)
        {
            SkillName = skillName ?? throw new ArgumentNullException(nameof(skillName));
            SpecialtyName = specialtyName ?? throw new ArgumentNullException(nameof(specialtyName));
        }

        public string SkillName { get; }
        public string SpecialtyName { get; }
    }
}

