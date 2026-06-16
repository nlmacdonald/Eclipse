using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public enum AbilityModifierAdjustmentKind
    {
        CostMultiplier = 0,
        EffectMultiplier = 1,
    }

    public sealed class AbilityModifierRule
    {
        public AbilityModifierRule(
            AbilityModifierType type,
            IReadOnlyList<AbilityModifierAdjustmentOption> adjustments,
            bool requiresDetails,
            bool requiresGmApproval)
        {
            Type = type;
            Adjustments = adjustments ?? throw new ArgumentNullException(nameof(adjustments));
            RequiresDetails = requiresDetails;
            RequiresGmApproval = requiresGmApproval;
        }

        public AbilityModifierType Type { get; }
        public IReadOnlyList<AbilityModifierAdjustmentOption> Adjustments { get; }
        public bool RequiresDetails { get; }
        public bool RequiresGmApproval { get; }
    }

    public sealed class AbilityModifierAdjustmentOption
    {
        public AbilityModifierAdjustmentOption(AbilityModifierAdjustmentKind kind, decimal value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be > 0.");
            }

            Kind = kind;
            Value = value;
        }

        public AbilityModifierAdjustmentKind Kind { get; }
        public decimal Value { get; }
    }

    public sealed class BonusUsesRule
    {
        public BonusUsesRule(
            int baseCostCp,
            int baseAdditionalUses,
            bool gmoAllowsAttributeInsteadOfUses,
            IReadOnlyList<BonusUsesOption> additionalOptions)
        {
            if (baseCostCp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCostCp), baseCostCp, "BaseCostCp must be > 0.");
            }

            if (baseAdditionalUses <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAdditionalUses), baseAdditionalUses, "BaseAdditionalUses must be > 0.");
            }

            BaseCostCp = baseCostCp;
            BaseAdditionalUses = baseAdditionalUses;
            GmoAllowsAttributeInsteadOfUses = gmoAllowsAttributeInsteadOfUses;
            AdditionalOptions = additionalOptions ?? throw new ArgumentNullException(nameof(additionalOptions));
        }

        public int BaseCostCp { get; }
        public int BaseAdditionalUses { get; }
        public bool GmoAllowsAttributeInsteadOfUses { get; }
        public IReadOnlyList<BonusUsesOption> AdditionalOptions { get; }
    }

    public sealed class BonusUsesOption
    {
        public BonusUsesOption(int additionalUses, int costCp)
        {
            if (additionalUses <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(additionalUses), additionalUses, "AdditionalUses must be > 0.");
            }

            if (costCp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costCp), costCp, "CostCp must be > 0.");
            }

            AdditionalUses = additionalUses;
            CostCp = costCp;
        }

        public int AdditionalUses { get; }
        public int CostCp { get; }
    }

    public sealed class AbilityLookup
    {
        private readonly IReadOnlyDictionary<string, AbilityDefinition> _byId;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<AbilityDefinition>> _byName;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _idsByName;

        internal AbilityLookup(
            IReadOnlyDictionary<string, AbilityDefinition> byId,
            IReadOnlyDictionary<string, IReadOnlyList<AbilityDefinition>> byName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> idsByName)
        {
            _byId = byId ?? throw new ArgumentNullException(nameof(byId));
            _byName = byName ?? throw new ArgumentNullException(nameof(byName));
            _idsByName = idsByName ?? throw new ArgumentNullException(nameof(idsByName));
        }

        public IEnumerable<string> AbilityIds => _byId.Keys;
        public IEnumerable<AbilityDefinition> All => _byId.Values;

        public bool TryGetById(string id, out AbilityDefinition ability)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return _byId.TryGetValue(id, out ability!);
        }

        public AbilityDefinition GetById(string id)
        {
            if (!TryGetById(id, out var ability))
            {
                throw new KeyNotFoundException($"Unknown ability id '{id}'.");
            }

            return ability;
        }

        public bool TryGetByName(string name, out AbilityDefinition ability)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_byName.TryGetValue(name, out var list) || list.Count == 0)
            {
                ability = default!;
                return false;
            }

            if (list.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Ability name '{name}' is ambiguous ({list.Count} matches). Use ids instead.");
            }

            ability = list[0];
            return true;
        }

        public AbilityDefinition GetByName(string name)
        {
            if (!TryGetByName(name, out var ability))
            {
                throw new KeyNotFoundException($"Unknown ability name '{name}'.");
            }

            return ability;
        }

        public IReadOnlyList<AbilityDefinition> GetAllByName(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _byName.TryGetValue(name, out var list) ? list : Array.Empty<AbilityDefinition>();
        }

        public IReadOnlyList<string> GetIdsByName(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _idsByName.TryGetValue(name, out var list) ? list : Array.Empty<string>();
        }
    }

    public sealed class AbilityRuleset
    {
        public AbilityRuleset(AbilityRulesetDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));

            var validation = AbilityRulesetValidator.Validate(document);
            if (!validation.IsValid)
            {
                throw new AbilityRulesetValidationException(validation.Errors);
            }

            BonusUses = document.BonusUses is null
                ? null
                : new BonusUsesRule(
                    document.BonusUses.BaseCostCp,
                    document.BonusUses.BaseAdditionalUses,
                    document.BonusUses.GmoAllowsAttributeInsteadOfUses,
                    (document.BonusUses.AdditionalOptions ?? new List<BonusUsesOptionDocument>())
                        .Select(x => new BonusUsesOption(x.AdditionalUses, x.CostCp))
                        .ToList()
                        .AsReadOnly());

            ModifierRules = document.AbilityModifiers
                .Select(CompileModifierRule)
                .ToDictionary(x => x.Type);

            var (byId, byName, idsByName) = CompileAbilityIndexes(document.Abilities);
            Abilities = new AbilityLookup(byId, byName, idsByName);
        }

        public AbilityRulesetDocument Document { get; }
        public AbilityLookup Abilities { get; }
        public IReadOnlyDictionary<AbilityModifierType, AbilityModifierRule> ModifierRules { get; }
        public BonusUsesRule? BonusUses { get; }

        public static AbilityRuleset LoadFromFile(string path)
        {
            return new AbilityRuleset(AbilityRulesetJson.LoadFromFile(path));
        }

        public static AbilityRuleset LoadFromJson(string json)
        {
            return new AbilityRuleset(AbilityRulesetJson.Deserialize(json));
        }

        private static AbilityModifierRule CompileModifierRule(AbilityModifierRuleDocument document)
        {
            if (!AbilityRulesetValidator.TryParseModifierType(document.Type, out var modifierType))
            {
                throw new InvalidOperationException($"Invalid ability modifier type '{document.Type}'.");
            }

            var adjustments = document.Adjustments.Select(adj =>
            {
                if (!AbilityRulesetValidator.TryParseAdjustmentKind(adj.Kind, out var kind))
                {
                    throw new InvalidOperationException($"Invalid ability modifier adjustment kind '{adj.Kind}'.");
                }

                return new AbilityModifierAdjustmentOption(kind, adj.Value);
            }).ToList().AsReadOnly();

            return new AbilityModifierRule(
                modifierType,
                adjustments,
                requiresDetails: document.RequiresDetails,
                requiresGmApproval: document.RequiresGmApproval);
        }

        private static (
            IReadOnlyDictionary<string, AbilityDefinition> byId,
            IReadOnlyDictionary<string, IReadOnlyList<AbilityDefinition>> byName,
            IReadOnlyDictionary<string, IReadOnlyList<string>> idsByName)
            CompileAbilityIndexes(IReadOnlyList<AbilityEntryDocument> abilities)
        {
            var byId = new Dictionary<string, AbilityDefinition>(StringComparer.OrdinalIgnoreCase);
            var byName = new Dictionary<string, List<AbilityDefinition>>(StringComparer.OrdinalIgnoreCase);
            var idsByName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in abilities)
            {
                var prerequisiteIds = (entry.PrerequisiteAbilityIds ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                var options = CompileOptions(entry.Options ?? new List<AbilityOptionDocument>());
                var definition = new AbilityDefinition(entry.Id, entry.Name, entry.Description, entry.CostCp, prerequisiteIds, options);
                byId[entry.Id] = definition;

                if (!byName.TryGetValue(entry.Name, out var list))
                {
                    list = new List<AbilityDefinition>();
                    byName[entry.Name] = list;
                }

                list.Add(definition);

                if (!idsByName.TryGetValue(entry.Name, out var idList))
                {
                    idList = new List<string>();
                    idsByName[entry.Name] = idList;
                }

                idList.Add(entry.Id);
            }

            return (
                byId,
                byName.ToDictionary(x => x.Key, x => (IReadOnlyList<AbilityDefinition>)x.Value.AsReadOnly(), StringComparer.OrdinalIgnoreCase),
                idsByName.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value.AsReadOnly(), StringComparer.OrdinalIgnoreCase));
        }

        private static IReadOnlyList<AbilityOptionDefinition> CompileOptions(IReadOnlyList<AbilityOptionDocument> options)
        {
            return options.Select(option =>
            {
                var prerequisiteIds = (option.PrerequisiteOptionIds ?? new List<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                return new AbilityOptionDefinition(
                    option.Id,
                    option.Name,
                    option.Description,
                    option.CostCp,
                    prerequisiteIds,
                    option.CostExpression);
            }).ToList().AsReadOnly();
        }
    }
}
