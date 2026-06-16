using System;

namespace Eclipse_Library
{
    public sealed class BuyHitDieForLevelPurchase : IPurchase
    {
        private readonly HitDieType _hitDie;

        public BuyHitDieForLevelPurchase(HitDieType hitDie)
        {
            _hitDie = hitDie;
        }

        public string Description => $"Buy hit die: {_hitDie}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            var cost = ((int)_hitDie) - 4;
            if (cost < 0)
            {
                cost = 0;
            }

            character.SpendCp(cost);
            character.SetPrimaryHitDieForCurrentLevel(_hitDie);
        }
    }
}
