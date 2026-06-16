using System;

namespace Eclipse_Library
{
    public sealed class AbilityModifier
    {
        public AbilityModifier(AbilityModifierType type, string details)
        {
            Type = type;
            Details = details ?? throw new ArgumentNullException(nameof(details));
        }

        public AbilityModifierType Type { get; }
        public string Details { get; }
    }
}

