using System;

namespace Eclipse_Library
{
    public sealed class BuySpecializedCasterLevelPurchase : IPurchase
    {
        private readonly MagicProgressionType _progressionType;
        private readonly int _levels;

        public BuySpecializedCasterLevelPurchase(MagicProgressionType progressionType, int levels)
        {
            _progressionType = progressionType;
            _levels = levels;
        }

        public string Description => $"Buy {_levels} specialized caster level(s) in {_progressionType}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_levels < 0)
            {
                throw new InvalidOperationException("Cannot buy a negative number of specialized caster levels.");
            }

            character.SpendCp(_levels * 3);
            character.AddSpecializedCasterLevel(_progressionType, _levels);
        }
    }
}

