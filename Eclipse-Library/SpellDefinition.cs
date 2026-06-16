using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class SpellDefinition
    {
        public SpellDefinition(string name, int level, SpellSource source)
        {
            Name = name;
            Level = level;
            Source = source;
        }

        public string Name { get; }
        public int Level { get; }
        public SpellSource Source { get; }
    }
}
