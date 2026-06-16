using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eclipse_Library
{
    public static class MagicProgressionCatalog
    {
        private static readonly Dictionary<MagicProgressionType, MagicProgressionDefinition> Definitions =
            new()
            {
                [MagicProgressionType.Paladin] = new(MagicProgressionType.Paladin, SpellSource.Divine, 2, false),
                [MagicProgressionType.Ranger] = new(MagicProgressionType.Ranger, SpellSource.Divine, 2, false),
                [MagicProgressionType.PsychicWarrior] = new(MagicProgressionType.PsychicWarrior, SpellSource.Psionic, 6, true),
                [MagicProgressionType.Wilder] = new(MagicProgressionType.Wilder, SpellSource.Psionic, 6, true),
                [MagicProgressionType.Adept] = new(MagicProgressionType.Adept, SpellSource.Divine, 6, true),
                [MagicProgressionType.Bard] = new(MagicProgressionType.Bard, SpellSource.Arcane, 8, true),
                [MagicProgressionType.Cleric] = new(MagicProgressionType.Cleric, SpellSource.Divine, 10, true),
                [MagicProgressionType.Druid] = new(MagicProgressionType.Druid, SpellSource.Divine, 8, true),
                [MagicProgressionType.Psion] = new(MagicProgressionType.Psion, SpellSource.Psionic, 12, true),
                [MagicProgressionType.Wizard] = new(MagicProgressionType.Wizard, SpellSource.Arcane, 14, true),
                [MagicProgressionType.Sorcerer] = new(MagicProgressionType.Sorcerer, SpellSource.Arcane, 16, true),
            };

        public static IReadOnlyCollection<MagicProgressionDefinition> All => Definitions.Values.ToList().AsReadOnly();

        public static MagicProgressionDefinition Get(MagicProgressionType type)
        {
            return Definitions[type];
        }
    }
}
