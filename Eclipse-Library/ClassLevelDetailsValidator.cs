using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class ClassLevelDetailsValidator : IFinalBuildValidator, IIdentifiedValidator
    {
        private readonly FavoredClassBonusRulesConfig _favoredClassBonuses;

        public ClassLevelDetailsValidator()
            : this(new FavoredClassBonusRulesConfig())
        {
        }

        public ClassLevelDetailsValidator(FavoredClassBonusRulesConfig favoredClassBonuses)
        {
            _favoredClassBonuses = favoredClassBonuses ?? throw new ArgumentNullException(nameof(favoredClassBonuses));
        }

        public string ValidatorId => "CLASS_LEVEL_DETAILS";

        public IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var record in context.AppliedPurchases.Where(x => x.Success))
            {
                if (record.Purchase is not SetClassLevelDetailsPurchase details)
                {
                    continue;
                }

                foreach (var diagnostic in ValidateDetails(context, record, details, _favoredClassBonuses))
                {
                    yield return diagnostic;
                }
            }
        }

        private static IEnumerable<BuildDiagnostic> ValidateDetails(
            FinalBuildValidationContext context,
            AppliedPurchaseRecord record,
            SetClassLevelDetailsPurchase details,
            FavoredClassBonusRulesConfig favoredClassBonuses)
        {
            var diagnosticContext = new BuildDiagnosticContext(
                BuildDiagnosticStage.Final,
                context.Build.TargetLevel,
                purchaseId: record.PurchaseId,
                purchaseDescription: record.PurchaseDescription,
                purchaseIndex: record.PurchaseIndex);

            if (string.IsNullOrWhiteSpace(details.HpNote))
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "CLASS_LEVEL_HP_MISSING",
                    $"Character level {details.CharacterLevel} ({details.TemplateName}) is missing hit points.",
                    diagnosticContext);
            }
            else if (int.TryParse(details.HpNote.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var hitPoints)
                && details.MaxHitPoints is > 0
                && (hitPoints < 1 || hitPoints > details.MaxHitPoints.Value))
            {
                yield return new BuildDiagnostic(
                    BuildDiagnosticSeverity.Error,
                    "CLASS_LEVEL_HP_OUT_OF_RANGE",
                    $"Character level {details.CharacterLevel} ({details.TemplateName}) has {hitPoints} hit point(s), but the template hit die allows 1 to {details.MaxHitPoints.Value}.",
                    diagnosticContext);
            }

            if (!favoredClassBonuses.IsAllowed(details.TemplateName, details.FavoredBonus))
            {
                if (favoredClassBonuses.RulesetId == RulesetId.Pathfinder1E)
                {
                    var allowed = favoredClassBonuses.GetAllowedBonuses(details.TemplateName);
                    var allowedText = allowed.Count == 0 ? "a configured Pathfinder favored class bonus" : string.Join(", ", allowed);
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Error,
                        "FAVORED_CLASS_BONUS_INVALID",
                        $"Character level {details.CharacterLevel} ({details.TemplateName}) has an unsupported favored class bonus for {favoredClassBonuses.RaceName}. Use {allowedText}, or Custom: <note>.",
                        diagnosticContext);
                }
                else
                {
                    yield return new BuildDiagnostic(
                        BuildDiagnosticSeverity.Warning,
                        "FAVORED_CLASS_BONUS_NOT_SUPPORTED",
                        $"Character level {details.CharacterLevel} ({details.TemplateName}) has a favored class bonus, but favored class bonuses are only enabled for Pathfinder 1e.",
                        diagnosticContext);
                }
            }
        }
    }
}
