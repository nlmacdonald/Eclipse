using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class CharacterBuildReplayFactory
    {
        public BuildRulesConfig CreateRulesConfig(HeroConfigurationSettings heroSettings)
        {
            var config = RulesetProfile.Get(heroSettings.RulesetId).CreateBuildRulesConfig();

            config.Skills.EnableIrrelevantToRelevantPromotion =
                !heroSettings.SuppressIrrelevantSkillPromotion
                && config.Skills.EnableIrrelevantToRelevantPromotion;
            config.Skills.UseFirstCharacterLevelSkillPointMultiplier = heroSettings.UseFirstCharacterLevelSkillPointMultiplier;

            switch (heroSettings.SkillRankCapMode)
            {
                case "Character level +2":
                    config.Skills.SkillRankCapBonus = 2;
                    break;
                case "Character level +5":
                    config.Skills.SkillRankCapBonus = 5;
                    break;
                case "Character level +10":
                    config.Skills.SkillRankCapBonus = 10;
                    break;
                case "Unlimited":
                    config.Skills.UnlimitedSkillRankCap = true;
                    break;
            }

            return config;
        }

        public CharacterBuildResult Replay(CharacterBuild build, BuildRulesConfig rulesConfig)
        {
            return CreateReplayer(rulesConfig).Replay(
                build,
                new EclipseCpProgression(),
                new BuildReplayOptions { RunIncrementalValidation = true, RunFinalValidation = true },
                rulesConfig);
        }

        private static BuildReplayer CreateReplayer(BuildRulesConfig rulesConfig)
        {
            var stepValidators = new IBuildStepValidator[]
            {
                new WarcraftCapByLevelValidator(),
                new SkillRankCapByLevelValidator(rulesConfig.Skills),
                new BaseCasterLevelCapByLevelValidator(),
                new MagicLevelsPerProgressionPerLevelValidator(),
                new MagicLevelCapByCharacterLevelValidator(),
                new CpOverspendByLevelValidator(),
                new SkillSpecialtyValidator(rulesConfig.Skills),
                new RuleValidatorBuildStepAdapter(new AbilityPrerequisiteValidator()),
            };

            var finalValidators = new IFinalBuildValidator[]
            {
                new CpOverspendFinalValidator(),
                new SelectedFeatAllowanceValidator(),
                new SkillPointAllowanceValidator(rulesConfig.Skills),
                new RuleValidatorFinalAdapter(new AbilityPrerequisiteValidator()),
            };

            return new BuildReplayer(stepValidators, finalValidators);
        }
    }
}
