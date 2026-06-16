using System;
using System.Collections.Generic;

namespace Eclipse_Library
{
    public sealed class CharacterSizeProfile
    {
        private static readonly IReadOnlyDictionary<CharacterSize, CharacterSizeProfile> Profiles =
            new Dictionary<CharacterSize, CharacterSizeProfile>
            {
                [CharacterSize.Fine] = new(CharacterSize.Fine, 8, -16, 16, "6 in. or less", "Less than 1/8 lb.", "1/2 ft.", "0 ft."),
                [CharacterSize.Diminutive] = new(CharacterSize.Diminutive, 4, -12, 12, "6 in.-1 ft.", "1/8-1 lb.", "1 ft.", "0 ft."),
                [CharacterSize.Tiny] = new(CharacterSize.Tiny, 2, -8, 8, "1-2 ft.", "1-8 lb.", "2-1/2 ft.", "0 ft."),
                [CharacterSize.Small] = new(CharacterSize.Small, 1, -4, 4, "2-4 ft.", "8-60 lb.", "5 ft.", "5 ft."),
                [CharacterSize.Medium] = new(CharacterSize.Medium, 0, 0, 0, "4-8 ft.", "60-500 lb.", "5 ft.", "5 ft."),
                [CharacterSize.Large] = new(CharacterSize.Large, -1, 4, -4, "8-16 ft.", "500-4,000 lb.", "10 ft.", "10 ft."),
                [CharacterSize.Huge] = new(CharacterSize.Huge, -2, 8, -8, "16-32 ft.", "2-16 tons", "15 ft.", "15 ft."),
                [CharacterSize.Gargantuan] = new(CharacterSize.Gargantuan, -4, 12, -12, "32-64 ft.", "16-125 tons", "20 ft.", "20 ft."),
                [CharacterSize.Colossal] = new(CharacterSize.Colossal, -8, 16, -16, "64 ft. or more", "More than 125 tons", "30 ft.", "30 ft."),
            };

        private CharacterSizeProfile(
            CharacterSize size,
            int acAttackModifier,
            int grappleModifier,
            int hideModifier,
            string dimension,
            string weight,
            string space,
            string reach)
        {
            Size = size;
            AcAttackModifier = acAttackModifier;
            GrappleModifier = grappleModifier;
            HideModifier = hideModifier;
            Dimension = dimension;
            Weight = weight;
            Space = space;
            Reach = reach;
        }

        public CharacterSize Size { get; }
        public int AcAttackModifier { get; }
        public int GrappleModifier { get; }
        public int HideModifier { get; }
        public string Dimension { get; }
        public string Weight { get; }
        public string Space { get; }
        public string Reach { get; }

        public static CharacterSizeProfile Get(CharacterSize size)
        {
            return Profiles.TryGetValue(size, out var profile)
                ? profile
                : Profiles[CharacterSize.Medium];
        }

        public static bool TryParse(string? text, out CharacterSize size)
        {
            if (Enum.TryParse((text ?? "").Trim(), ignoreCase: true, out size))
            {
                return true;
            }

            size = CharacterSize.Medium;
            return false;
        }
    }
}
