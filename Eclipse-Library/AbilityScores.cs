using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eclipse_Library
{
    public sealed class AbilityScores
    {
        public AbilityScores(int strength, int dexterity, int constitution, int intelligence, int wisdom, int charisma)
        {
            Strength = strength;
            Dexterity = dexterity;
            Constitution = constitution;
            Intelligence = intelligence;
            Wisdom = wisdom;
            Charisma = charisma;
        }

        public int Strength { get; }
        public int Dexterity { get; }
        public int Constitution { get; }
        public int Intelligence { get; }
        public int Wisdom { get; }
        public int Charisma { get; }

        public AbilityScores WithModifier(string ability, int modifier)
        {
            switch ((ability ?? "").Trim().ToLowerInvariant())
            {
                case "strength":
                case "str":
                    return new AbilityScores(Strength + modifier, Dexterity, Constitution, Intelligence, Wisdom, Charisma);
                case "dexterity":
                case "dex":
                    return new AbilityScores(Strength, Dexterity + modifier, Constitution, Intelligence, Wisdom, Charisma);
                case "constitution":
                case "con":
                    return new AbilityScores(Strength, Dexterity, Constitution + modifier, Intelligence, Wisdom, Charisma);
                case "intelligence":
                case "int":
                    return new AbilityScores(Strength, Dexterity, Constitution, Intelligence + modifier, Wisdom, Charisma);
                case "wisdom":
                case "wis":
                    return new AbilityScores(Strength, Dexterity, Constitution, Intelligence, Wisdom + modifier, Charisma);
                case "charisma":
                case "cha":
                    return new AbilityScores(Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma + modifier);
                default:
                    throw new ArgumentException($"Unknown ability score '{ability}'.", nameof(ability));
            }
        }

        public AbilityScores WithModifiers(IEnumerable<RaceAbilityScoreModifierDocument>? modifiers)
        {
            var scores = this;
            foreach (var modifier in modifiers ?? Enumerable.Empty<RaceAbilityScoreModifierDocument>())
            {
                if (modifier is null || modifier.Modifier is null || modifier.Choose)
                {
                    continue;
                }

                scores = scores.WithModifier(modifier.Ability, modifier.Modifier.Value);
            }

            return scores;
        }

        public static int GetModifier(int score)
        {
            // D20-style modifier: floor((score - 10) / 2).
            // Examples: 10-11 => 0, 12-13 => +1, 8-9 => -1.
            return (int)Math.Floor((score - 10) / 2.0);
        }
    }
}
