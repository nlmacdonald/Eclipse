using System;

namespace Eclipse_Library
{
    public sealed class BuyLimitedSaveBonusPurchase : IPurchase
    {
        private readonly SaveType _saveType;
        private readonly int _bonus;
        private readonly int _costCp;
        private readonly string _limitation;

        public BuyLimitedSaveBonusPurchase(SaveType saveType, int bonus, int costCp, string limitation)
        {
            _saveType = saveType;
            _bonus = bonus;
            _costCp = costCp;
            _limitation = limitation ?? throw new ArgumentNullException(nameof(limitation));
        }

        public string Description => $"Buy limited +{_bonus} {_saveType} save ({_limitation})";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_bonus <= 0)
            {
                throw new InvalidOperationException("Limited save bonus must be > 0.");
            }

            if (_costCp < 0)
            {
                throw new InvalidOperationException("Limited save CP cost must be >= 0.");
            }

            character.SpendCp(_costCp);
            character.AddLimitedSaveBonus(_saveType, _bonus, _limitation);
        }
    }
}

