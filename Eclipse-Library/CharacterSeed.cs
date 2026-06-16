using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class CharacterSeed
    {
        public CharacterSeed(string name, AbilityScores abilityScores)
            : this(name, abilityScores, CharacterSize.Medium)
        {
        }

        public CharacterSeed(string name, AbilityScores abilityScores, CharacterSize size)
            : this(name, abilityScores, size, race: null, raceAbilityCatalog: null)
        {
        }

        public CharacterSeed(
            string name,
            AbilityScores abilityScores,
            CharacterSize size,
            RaceDefinitionDocument? race,
            RaceAbilityCatalogDocument? raceAbilityCatalog)
            : this(name, abilityScores, size, race, raceAbilityCatalog, null)
        {
        }

        public CharacterSeed(
            string name,
            AbilityScores abilityScores,
            CharacterSize size,
            RaceDefinitionDocument? race,
            RaceAbilityCatalogDocument? raceAbilityCatalog,
            IReadOnlyDictionary<string, int>? levelAbilityScoreAdjustments)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AbilityScores = abilityScores ?? throw new ArgumentNullException(nameof(abilityScores));
            Size = size;
            Race = race;
            RaceAbilityCatalog = raceAbilityCatalog;
            LevelAbilityScoreAdjustments = new Dictionary<string, int>(
                levelAbilityScoreAdjustments ?? new Dictionary<string, int>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; }
        public AbilityScores AbilityScores { get; }
        public CharacterSize Size { get; }
        public RaceDefinitionDocument? Race { get; }
        public RaceAbilityCatalogDocument? RaceAbilityCatalog { get; }
        public IReadOnlyDictionary<string, int> LevelAbilityScoreAdjustments { get; }
    }
}
