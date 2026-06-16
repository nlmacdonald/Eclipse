using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class MagicProgressionConfig
    {
        public MagicProgressionConfig(
            MagicProgressionType type,
            SpellSource source,
            IEnumerable<MagicLimitation> limitations)
        {
            Type = type;
            Source = source;
            Limitations = (limitations ?? Array.Empty<MagicLimitation>()).Distinct().ToList().AsReadOnly();
        }

        public MagicProgressionType Type { get; }
        public SpellSource Source { get; }
        public IReadOnlyList<MagicLimitation> Limitations { get; }
    }
}

