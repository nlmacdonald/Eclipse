using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public static class AbilityCatalog
    {
        public static readonly AbilityDefinition Berserker = new("Berserker", 6);
        public static readonly AbilityDefinition Odinpower = new("Odinpower", 3, "Berserker");
        public static readonly AbilityDefinition Odinmight = new("Odinmight", 3, "Odinpower");
    }
}
