using System;

namespace Eclipse_Library
{
    public sealed class EclipseCpProgression : ICpProgression
    {
        private readonly int _cpAtLevelOne;
        private readonly int _cpPerAdditionalLevel;

        public EclipseCpProgression(int cpAtLevelOne = 48, int cpPerAdditionalLevel = 24)
        {
            if (cpAtLevelOne < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cpAtLevelOne), cpAtLevelOne, "CP at level 1 must be >= 0.");
            }

            if (cpPerAdditionalLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cpPerAdditionalLevel), cpPerAdditionalLevel, "CP per additional level must be >= 0.");
            }

            _cpAtLevelOne = cpAtLevelOne;
            _cpPerAdditionalLevel = cpPerAdditionalLevel;
        }

        public int GetTotalCpAtLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            return _cpAtLevelOne + ((level - 1) * _cpPerAdditionalLevel);
        }
    }
}

