using System;

namespace Eclipse_Library
{
    public sealed class BuyProficiencyPackagePurchase : IPurchase
    {
        private readonly string _description;
        private readonly int _costCp;

        public BuyProficiencyPackagePurchase(string description, int costCp)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.", nameof(description));
            }

            if (costCp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costCp), costCp, "Cost CP must be >= 0.");
            }

            _description = description.Trim();
            _costCp = costCp;
        }

        public string Description => $"Buy proficiencies: {_description}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            character.SpendCp(_costCp);
            character.AddProficiencyPackage(_description);
        }
    }
}

