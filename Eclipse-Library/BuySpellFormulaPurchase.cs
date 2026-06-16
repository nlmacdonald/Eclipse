using System;

namespace Eclipse_Library
{
    public sealed class BuySpellFormulaPurchase : IPurchase
    {
        private readonly SpellDefinition _spell;
        private readonly SpellAcquisitionKind _kind;
        private readonly SpellCasterKind _casterKind;

        public BuySpellFormulaPurchase(SpellDefinition spell, SpellAcquisitionKind kind, SpellCasterKind casterKind)
        {
            _spell = spell ?? throw new ArgumentNullException(nameof(spell));
            _kind = kind;
            _casterKind = casterKind;
        }

        public string Description => $"Buy spell formula: {_spell.Name} (L{_spell.Level})";

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            var baseCost = _kind == SpellAcquisitionKind.CustomSpell ? 2 : 1;
            var multiplier = _casterKind switch
            {
                SpellCasterKind.Prepared => 1,
                SpellCasterKind.Spontaneous => 2,
                SpellCasterKind.PsionicAugmentable => 3,
                _ => 1
            };

            character.SpendCp(baseCost * multiplier);
            character.LearnSpell(_spell);
        }
    }
}

