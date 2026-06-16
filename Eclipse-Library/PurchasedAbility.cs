using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class PurchasedAbilityOption
    {
        public PurchasedAbilityOption(AbilityOptionDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public AbilityOptionDefinition Definition { get; }
        public string Id => Definition.Id;
        public string Name => Definition.Name;
        public int CostCp => Definition.CostCp;
        public string? CostExpression => Definition.CostExpression;
    }

    public sealed class PurchasedAbility
    {
        public PurchasedAbility(
            AbilityDefinition definition,
            int costCp,
            IEnumerable<AbilityModifier>? modifiers = null,
            bool gmApproved = false,
            IEnumerable<AbilityOptionDefinition>? selectedOptions = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            if (costCp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costCp), costCp, "Cost CP must be >= 0.");
            }

            CostCp = costCp;
            Modifiers = (modifiers ?? Array.Empty<AbilityModifier>()).ToList().AsReadOnly();
            GmApproved = gmApproved;
            SelectedOptions = (selectedOptions ?? Array.Empty<AbilityOptionDefinition>())
                .Select(x => new PurchasedAbilityOption(x))
                .ToList()
                .AsReadOnly();
        }

        public AbilityDefinition Definition { get; }
        public int CostCp { get; }
        public IReadOnlyList<AbilityModifier> Modifiers { get; }
        public bool GmApproved { get; }
        public IReadOnlyList<PurchasedAbilityOption> SelectedOptions { get; }

        public bool HasSpecializedOrCorrupted =>
            Modifiers.Any(x => x.Type == AbilityModifierType.Specialized || x.Type == AbilityModifierType.Corrupted);
    }
}
