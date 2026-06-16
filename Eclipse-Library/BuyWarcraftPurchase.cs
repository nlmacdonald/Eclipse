using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuyWarcraftPurchase : IPurchase
    {
        private readonly int _levels;

        public BuyWarcraftPurchase(int levels)
        {
            _levels = levels;
        }

        public string Description => $"Buy {_levels} Warcraft";

        public void Apply(Character character)
        {
            character.SpendCp(_levels * 6);
            character.AddWarcraft(_levels);
        }
    }
}
