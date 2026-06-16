using Eclipse_Library;

namespace Eclipse_Library.Tests;

internal static class TestReplayerFactory
{
    public static BuildReplayer CreateStandardReplayer(BuildRulesConfig? rulesConfig = null)
    {
        rulesConfig ??= new BuildRulesConfig();
        var stepValidators = new IBuildStepValidator[]
        {
            new WarcraftCapByLevelValidator(),
            new SkillRankCapByLevelValidator(rulesConfig.Skills),
            new CpOverspendByLevelValidator(),
        };

        var finalValidators = new IFinalBuildValidator[]
        {
            new CpOverspendFinalValidator(),
            new SelectedFeatAllowanceValidator(rulesConfig.LevelProgression),
            new SkillPointAllowanceValidator(rulesConfig.Skills),
        };

        return new BuildReplayer(stepValidators, finalValidators);
    }
}

internal sealed class FixedCpProgression : ICpProgression
{
    private readonly int _totalCp;

    public FixedCpProgression(int totalCp)
    {
        _totalCp = totalCp;
    }

    public int GetTotalCpAtLevel(int level)
    {
        return _totalCp;
    }
}

internal sealed class TableCpProgression : ICpProgression
{
    private readonly IReadOnlyDictionary<int, int> _totalsByLevel;

    public TableCpProgression(IReadOnlyDictionary<int, int> totalsByLevel)
    {
        _totalsByLevel = totalsByLevel;
    }

    public int GetTotalCpAtLevel(int level)
    {
        return _totalsByLevel.TryGetValue(level, out var total)
            ? total
            : _totalsByLevel.Values.LastOrDefault();
    }
}

internal sealed class FailingPurchase : IPurchase
{
    public FailingPurchase(string description)
    {
        Description = description;
    }

    public string Description { get; }

    public void Apply(Character character)
    {
        throw new InvalidOperationException("Intentional test failure.");
    }
}
