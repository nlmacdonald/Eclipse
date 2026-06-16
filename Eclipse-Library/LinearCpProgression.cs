using System;

namespace Eclipse_Library
{
    public sealed class LinearCpProgression : ICpProgression
    {
        private readonly int _cpAtLevelOne;
        private readonly int _cpPerLevel;

        public LinearCpProgression(int cpAtLevelOne, int cpPerLevel)
        {
            if (cpAtLevelOne < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cpAtLevelOne), cpAtLevelOne, "CP at level 1 must be >= 0.");
            }

            if (cpPerLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cpPerLevel), cpPerLevel, "CP per level must be >= 0.");
            }

            _cpAtLevelOne = cpAtLevelOne;
            _cpPerLevel = cpPerLevel;
        }

        public int GetTotalCpAtLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            return _cpAtLevelOne + ((level - 1) * _cpPerLevel);
        }
    }
}

