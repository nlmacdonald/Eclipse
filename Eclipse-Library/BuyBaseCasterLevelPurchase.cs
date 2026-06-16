using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuyBaseCasterLevelPurchase : IPurchase
    {
        private readonly int _levels;

        public BuyBaseCasterLevelPurchase(int levels)
        {
            _levels = levels;
        }

        public string Description => $"Buy {_levels} Base Caster Level";

        public void Apply(Character character)
        {
            character.SpendCp(_levels * 6);
            character.AddBaseCasterLevel(_levels);
        }
    }
}
