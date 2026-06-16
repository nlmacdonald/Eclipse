using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuyMagicLevelsWithLimitationsPurchase : IPurchase
    {
        private readonly MagicProgressionType _progressionType;
        private readonly SpellSource _source;
        private readonly int _levels;
        private readonly IReadOnlyList<MagicLimitation> _limitations;
        private readonly MagicRulesConfig _rules;

        public BuyMagicLevelsWithLimitationsPurchase(
            MagicProgressionType progressionType,
            SpellSource source,
            int levels,
            IEnumerable<MagicLimitation> limitations,
            MagicRulesConfig rules)
        {
            _progressionType = progressionType;
            _source = source;
            _levels = levels;
            _limitations = (limitations ?? Array.Empty<MagicLimitation>()).Distinct().ToList().AsReadOnly();
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public string Description
        {
            get
            {
                var lim = _limitations.Count == 0 ? "no limitations" : string.Join(", ", _limitations);
                return $"Buy {_levels} magic level(s) in {_progressionType} [{_source}; {lim}]";
            }
        }

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_levels < 0)
            {
                throw new InvalidOperationException("Cannot buy a negative number of magic levels.");
            }

            var progression = MagicProgressionCatalog.Get(_progressionType);

            var costPerLevel = _rules.EnableMagicLimitationsCosting
                ? MagicLevelCostCalculator.GetCostPerLevelCp(progression.SpellProgressionCostPerLevelCp, _limitations.Count, _rules)
                : progression.SpellProgressionCostPerLevelCp;

            character.ConfigureMagicProgression(new MagicProgressionConfig(_progressionType, _source, _limitations));
            character.SpendCp(costPerLevel * _levels);
            character.AddMagicLevels(_progressionType, _levels);
        }
    }
}
