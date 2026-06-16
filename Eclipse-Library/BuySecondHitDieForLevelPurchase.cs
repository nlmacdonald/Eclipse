using System;

namespace Eclipse_Library
{
    public sealed class BuySecondHitDieForLevelPurchase : IPurchase
    {
        private readonly HitDieType _hitDie;

        public BuySecondHitDieForLevelPurchase(HitDieType hitDie)
        {
            _hitDie = hitDie;
        }

        public string Description => $"Buy second hit die: {_hitDie}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            var cost = 8 + Math.Max(0, ((int)_hitDie) - 4);
            character.SpendCp(cost);
            character.BuySecondHitDieForCurrentLevel(_hitDie);
        }
    }
}
