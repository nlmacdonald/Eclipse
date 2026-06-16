using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuyMagicLevelsPurchase : IPurchase
    {
        private readonly MagicProgressionType _progressionType;
        private readonly int _levels;

        public BuyMagicLevelsPurchase(MagicProgressionType progressionType, int levels)
        {
            _progressionType = progressionType;
            _levels = levels;
        }

        public string Description => $"Buy {_levels} magic level(s) in {_progressionType}";

        public void Apply(Character character)
        {
            var progression = MagicProgressionCatalog.Get(_progressionType);
            character.SpendCp(progression.ChartCostPerLevelCp * _levels);
            character.AddMagicLevels(_progressionType, _levels);
            if (progression.IncludesSpecializedCasterLevel)
            {
                character.AddSpecializedCasterLevel(_progressionType, _levels);
            }
        }
    }
}
