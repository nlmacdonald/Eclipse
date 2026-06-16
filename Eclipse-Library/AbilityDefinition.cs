using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eclipse_Library
{
    public sealed class AbilityOptionDefinition
    {
        public AbilityOptionDefinition(
            string id,
            string name,
            string? description,
            int costCp,
            IEnumerable<string>? prerequisiteOptionIds = null,
            string? costExpression = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name;
            Description = description;
            CostCp = costCp;
            CostExpression = costExpression;
            PrerequisiteOptionIds = (prerequisiteOptionIds ?? Array.Empty<string>()).ToList().AsReadOnly();
        }

        public string Id { get; }
        public string Name { get; }
        public string? Description { get; }
        public int CostCp { get; }
        public string? CostExpression { get; }
        public IReadOnlyList<string> PrerequisiteOptionIds { get; }

        [Obsolete("Use PrerequisiteOptionIds. Option prerequisites are stored as ids, not names.")]
        public IReadOnlyList<string> PrerequisiteOptionNames => PrerequisiteOptionIds;
    }

    public sealed class AbilityDefinition
    {
        public AbilityDefinition(string name, int costCp, params string[] prerequisiteAbilityNames)
            : this(id: name, name, description: null, costCp, prerequisiteAbilityNames)
        {
        }

        public AbilityDefinition(
            string id,
            string name,
            string? description,
            int costCp,
            params string[] prerequisiteAbilityNames)
            : this(id, name, description, costCp, prerequisiteAbilityNames, Array.Empty<AbilityOptionDefinition>())
        {
        }

        public AbilityDefinition(
            string id,
            string name,
            string? description,
            int costCp,
            IEnumerable<string>? prerequisiteAbilityIds,
            IEnumerable<AbilityOptionDefinition>? options)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name;
            Description = description;
            CostCp = costCp;
            PrerequisiteAbilityIds = (prerequisiteAbilityIds ?? Array.Empty<string>()).ToList().AsReadOnly();
            Options = (options ?? Array.Empty<AbilityOptionDefinition>()).ToList().AsReadOnly();
        }

        public string Id { get; }
        public string Name { get; }
        public string? Description { get; }
        public int CostCp { get; }
        public IReadOnlyList<string> PrerequisiteAbilityIds { get; }

        [Obsolete("Use PrerequisiteAbilityIds. Ability prerequisites are stored as ids, not names.")]
        public IReadOnlyList<string> PrerequisiteAbilityNames => PrerequisiteAbilityIds;
        public IReadOnlyList<AbilityOptionDefinition> Options { get; }

        public bool TryGetOptionById(string id, out AbilityOptionDefinition option)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            option = Options.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))!;
            return option is not null;
        }

        public AbilityOptionDefinition GetOptionById(string id)
        {
            if (!TryGetOptionById(id, out var option))
            {
                throw new KeyNotFoundException($"Unknown option id '{id}' on ability '{Name}'.");
            }

            return option;
        }
    }
}
