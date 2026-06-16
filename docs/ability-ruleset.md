# Ability ruleset JSON

This repo supports loading a simple ability + modifier ruleset from JSON.

## Files

- `docs/ability-ruleset.sample.json`: example ruleset document

## C# usage

```csharp
using Eclipse_Library;

var ruleset = AbilityRuleset.LoadFromFile("docs/ability-ruleset.sample.json");

var berserker = ruleset.Abilities.GetById("berserker");
var byName = ruleset.Abilities.GetByName("Berserker");
var odinpower = berserker.GetOptionById("odinpower");

var specializedRule = ruleset.ModifierRules[AbilityModifierType.Specialized];

var text = berserker.Description; // raw prose, when present
```

## Document shape

Top-level: `AbilityRulesetDocument` (`Eclipse-Library/AbilityRulesetDocument.cs`)

- `schemaVersion` (int, >= 1)
- `abilityModifiers[]`
  - `type`: `"Specialized"` | `"Corrupted"`
  - `requiresDetails` (bool)
  - `requiresGmApproval` (bool)
  - `adjustments[]`
    - `kind`: `"CostMultiplier"` | `"EffectMultiplier"`
    - `value`: decimal, > 0
- `bonusUses` (optional)
  - `baseCostCp` (int, > 0)
  - `baseAdditionalUses` (int, > 0)
  - `gmoAllowsAttributeInsteadOfUses` (bool)
  - `additionalOptions[]` (optional)
- `abilities[]`
  - `id` (string, unique, required)
  - `name` (string, required; may be non-unique in Chapter 2 text)
  - `costCp` (int, >= 0)
  - `description` (string, optional)
  - `prerequisiteAbilityIds[]` (optional, must reference existing ids; cycles rejected)
  - `options[]` (optional; purchasable options/upgrades that require this base ability)
    - `id` (string, unique within the parent ability, required)
    - `name` (string, required)
    - `costCp` (int, >= 0; usually the +CP option cost; 0 for named modes/choices included in the base purchase)
    - `costExpression` (string, optional; source-text cost when it is not a CP cost, such as `+1 SL`)
    - `description` (string, optional)
    - `prerequisiteOptionIds[]` (optional, must reference sibling option ids; cycles rejected)

## Runtime purchase shape

`AbilityDefinition` exposes `Options`, `TryGetOptionById`, and `GetOptionById`. `BuyAbilityPurchase` accepts selected `AbilityOptionDefinition` instances and spends the base ability cost plus selected option costs. Selected options are preserved on `PurchasedAbility.SelectedOptions`.
