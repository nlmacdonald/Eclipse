using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class BuyAbilityPurchase : IPurchase
    {
        private readonly AbilityDefinition _definition;
        private readonly IReadOnlyList<AbilityOptionDefinition> _selectedOptions;

        public BuyAbilityPurchase(
            AbilityDefinition definition,
            IEnumerable<AbilityOptionDefinition>? selectedOptions = null)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _selectedOptions = (selectedOptions ?? Array.Empty<AbilityOptionDefinition>())
                .Select(option => _definition.GetOptionById(
                    (option ?? throw new ArgumentNullException(nameof(selectedOptions), "Selected options must not contain null.")).Id))
                .ToList()
                .AsReadOnly();
        }

        public string Description
        {
            get
            {
                if (_selectedOptions.Count == 0)
                {
                    return $"Buy ability: {_definition.Name}";
                }

                return $"Buy ability: {_definition.Name} ({string.Join(", ", _selectedOptions.Select(x => x.Name))})";
            }
        }

        public void Apply(Character character)
        {
            if (character is null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            ValidateSelectedOptions();

            var costCp = _definition.CostCp + _selectedOptions.Sum(x => x.CostCp);
            character.SpendCp(costCp);
            character.AddPurchasedAbility(new PurchasedAbility(_definition, costCp, selectedOptions: _selectedOptions));
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
