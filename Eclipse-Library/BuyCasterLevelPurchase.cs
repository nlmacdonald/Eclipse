using System;

namespace Eclipse_Library
{
    public sealed class BuyCasterLevelPurchase : IPurchase
    {
        private readonly SpellSource _source;
        private readonly int _levels;

        public BuyCasterLevelPurchase(SpellSource source, int levels)
        {
            _source = source;
            _levels = levels;
        }

        public string Description => $"Buy {_levels} caster level(s) in {_source}";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_levels < 0)
            {
                throw new InvalidOperationException("Cannot buy a negative number of caster levels.");
            }

            character.SpendCp(_levels * 6);
            character.AddCasterLevel(_source, _levels);
        }
    }
}

