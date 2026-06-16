using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuySaveBonusPurchase : IPurchase
    {
        private readonly SaveType _saveType;
        private readonly int _bonus;

        public BuySaveBonusPurchase(SaveType saveType, int bonus)
        {
            _saveType = saveType;
            _bonus = bonus;
        }

        public string Description => $"Buy +{_bonus} {_saveType} save";

        public void Apply(Character character)
        {
            character.SpendCp(_bonus * 3);
            character.AddSaveBonus(_saveType, _bonus);
        }
    }
}
