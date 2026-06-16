using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class UpgradeHitDiePurchase : IPurchase
    {
        private readonly HitDieType _targetHitDie;

        public UpgradeHitDiePurchase(HitDieType targetHitDie)
        {
            _targetHitDie = targetHitDie;
        }

        public string Description => $"Upgrade hit die to {_targetHitDie}";

        public void Apply(Character character)
        {
            var cost = ((int)_targetHitDie) - 4;
            character.SpendCp(cost);
            character.UpgradeHitDie(_targetHitDie);
        }
    }
}
