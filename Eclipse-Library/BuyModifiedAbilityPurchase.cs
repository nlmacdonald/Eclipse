using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuyModifiedAbilityPurchase : IPurchase
    {
        private readonly AbilityDefinition _definition;
        private readonly int _costCp;
        private readonly IReadOnlyList<AbilityModifier> _modifiers;
        private readonly IReadOnlyList<AbilityOptionDefinition> _selectedOptions;
        private readonly bool _gmApproved;

        public BuyModifiedAbilityPurchase(
            AbilityDefinition definition,
            int costCp,
            IEnumerable<AbilityModifier> modifiers,
            bool gmApproved = false,
            IEnumerable<AbilityOptionDefinition>? selectedOptions = null)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _costCp = costCp;
            _modifiers = (modifiers ?? throw new ArgumentNullException(nameof(modifiers))).ToList().AsReadOnly();
            _selectedOptions = (selectedOptions ?? Array.Empty<AbilityOptionDefinition>())
                .Select(option => _definition.GetOptionById(
                    (option ?? throw new ArgumentNullException(nameof(selectedOptions), "Selected options must not contain null.")).Id))
                .ToList()
                .AsReadOnly();
            _gmApproved = gmApproved;
        }

        public string Description
        {
            get
            {
                var optionSuffix = _selectedOptions.Count == 0
                    ? ""
                    : $" [{string.Join(", ", _selectedOptions.Select(x => x.Name))}]";

                if (_modifiers.Count == 0)
                {
                    return $"Buy ability: {_definition.Name}{optionSuffix} (modified)";
                }

                var mods = string.Join(", ", _modifiers.Select(x => $"{x.Type}: {x.Details}"));
                return $"Buy ability: {_definition.Name}{optionSuffix} ({mods})";
            }
        }

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (_costCp < 0)
            {
                throw new InvalidOperationException("Ability CP cost must be >= 0.");
            }

            ValidateSelectedOptions();
            character.SpendCp(_costCp);
            character.AddPurchasedAbility(new PurchasedAbility(_definition, _costCp, _modifiers, _gmApproved, _selectedOptions));
        }

        private void ValidateSelectedOptions()
        {
            var selectedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var option in _selectedOptions)
            {
                if (!selectedIds.Add(option.Id))
                {
                    throw new InvalidOperationException(
                        $"Option '{option.Name}' has already been selected for ability '{_definition.Name}'.");
                }
            }

            foreach (var option in _selectedOptions)
            {
                foreach (var prerequisiteId in option.PrerequisiteOptionIds)
                {
                    if (!selectedIds.Contains(prerequisiteId))
                    {
                        throw new InvalidOperationException(
                            $"Option '{option.Name}' requires option id '{prerequisiteId}' on ability '{_definition.Name}'.");
                    }
                }
            }
        }
    }
}
