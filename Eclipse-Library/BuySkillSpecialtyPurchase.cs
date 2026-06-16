using System;

namespace Eclipse_Library
{
    public sealed class BuySkillSpecialtyPurchase : IPurchase
    {
        private readonly string _skillName;
        private readonly string _specialtyName;

        public BuySkillSpecialtyPurchase(string skillName, string specialtyName)
        {
            _skillName = skillName ?? throw new ArgumentNullException(nameof(skillName));
            _specialtyName = specialtyName ?? throw new ArgumentNullException(nameof(specialtyName));
        }

        public string Description => $"Buy specialty: {_skillName} ({_specialtyName})";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            character.SpendCp(1);
            character.AddSkillSpecialty(_skillName, _specialtyName);
        }
    }
}

