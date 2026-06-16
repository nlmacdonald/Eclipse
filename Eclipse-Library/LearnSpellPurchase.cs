using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class LearnSpellPurchase : IPurchase
    {
        private readonly SpellDefinition _spell;

        public LearnSpellPurchase(SpellDefinition spell)
        {
            _spell = spell;
        }

        public string Description => $"Learn spell: {_spell.Name} (L{_spell.Level})";

        public void Apply(Character character)
        {
            character.SpendCp(1);
            character.LearnSpell(_spell);
        }
    }
}
