using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class Character
    {
        private readonly Dictionary<string, SkillEntry> _skills = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<SaveType, int> _saveBonuses = Enum
            .GetValues(typeof(SaveType))
            .Cast<SaveType>()
            .ToDictionary(x => x, _ => 0);
        private readonly List<AbilityDefinition> _abilities = new();
        private readonly List<PurchasedAbility> _purchasedAbilities = new();
        private readonly Dictionary<string, int> _selectedFeats = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<MagicProgressionType, int> _magicLevels = Enum
            .GetValues(typeof(MagicProgressionType))
            .Cast<MagicProgressionType>()
            .ToDictionary(x => x, _ => 0);
        private readonly Dictionary<SpellSource, int> _casterLevelsBySource = Enum
            .GetValues(typeof(SpellSource))
            .Cast<SpellSource>()
            .ToDictionary(x => x, _ => 0);
        private readonly Dictionary<MagicProgressionType, MagicProgressionConfig> _magicProgressionConfigs = new();
        private readonly Dictionary<MagicProgressionType, int> _specializedCasterLevels = Enum
            .GetValues(typeof(MagicProgressionType))
            .Cast<MagicProgressionType>()
            .ToDictionary(x => x, _ => 0);
        private readonly List<SpellDefinition> _knownSpells = new();
        private readonly Dictionary<int, HitDiceLevel> _hitDiceByLevel = new();
        private readonly Dictionary<string, List<SkillSpecialty>> _skillSpecialties = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<BonusContribution> _bonusContributions = new();
        private readonly List<LimitedSaveBonus> _limitedSaveBonuses = new();
        private readonly Dictionary<string, List<AbilityScoreContribution>> _abilityScoreContributions = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _movementSpeeds = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _senseRanges = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _statBlockNotes = new();
        private int _bonusFeats;
        private int _bonusSkillPoints;
        private int _bonusSkillPointsPerAdditionalLevel;
        private readonly List<string> _proficiencyPackages = new();
        private readonly List<CharacterLevelRecord> _levelRecords = new();
        private int _experience;

        private Character(
            string name,
            int level,
            int totalCp,
            AbilityScores abilityScores,
            CharacterSize size)
        {
            Name = name;
            Level = level;
            TotalCp = totalCp;
            BaseAbilityScores = abilityScores;
            Size = size;
            HitDie = HitDieType.D4;
        }

        public string Name { get; }
        public int Level { get; private set; }
        public int TotalCp { get; private set; }
        public AbilityScores BaseAbilityScores { get; }
        public AbilityScores AbilityScores => new AbilityScores(
            GetAbilityScoreBreakdown("Strength").Total,
            GetAbilityScoreBreakdown("Dexterity").Total,
            GetAbilityScoreBreakdown("Constitution").Total,
            GetAbilityScoreBreakdown("Intelligence").Total,
            GetAbilityScoreBreakdown("Wisdom").Total,
            GetAbilityScoreBreakdown("Charisma").Total);
        public string? RaceName { get; private set; }
        public CharacterSize Size { get; private set; }
        public CharacterSizeProfile SizeProfile => CharacterSizeProfile.Get(Size);
        public HitDieType HitDie { get; private set; }
        public IReadOnlyDictionary<int, HitDiceLevel> HitDiceByLevel => _hitDiceByLevel;
        public int Warcraft { get; private set; }
        public int BaseCasterLevel { get; private set; }
        public int SpentCp { get; private set; }
        public int RemainingCp => TotalCp - SpentCp;
        public IReadOnlyDictionary<SaveType, int> SaveBonuses => _saveBonuses;
        public IReadOnlyList<LimitedSaveBonus> LimitedSaveBonuses => _limitedSaveBonuses.AsReadOnly();
        public IReadOnlyCollection<AbilityDefinition> Abilities => _abilities.AsReadOnly();
        public IReadOnlyList<PurchasedAbility> PurchasedAbilities => _purchasedAbilities.AsReadOnly();
        public IReadOnlyDictionary<string, int> SelectedFeats => _selectedFeats;
        public IReadOnlyDictionary<string, SkillEntry> Skills => _skills;
        public IReadOnlyDictionary<string, IReadOnlyList<SkillSpecialty>> SkillSpecialties =>
            _skillSpecialties.ToDictionary(x => x.Key, x => (IReadOnlyList<SkillSpecialty>)x.Value.AsReadOnly(), StringComparer.OrdinalIgnoreCase);
        public IReadOnlyList<BonusContribution> BonusContributions => _bonusContributions.AsReadOnly();
        public IReadOnlyDictionary<MagicProgressionType, int> MagicLevels => _magicLevels;
        public IReadOnlyDictionary<MagicProgressionType, MagicProgressionConfig> MagicProgressionConfigs => _magicProgressionConfigs;
        public IReadOnlyDictionary<SpellSource, int> CasterLevelsBySource => _casterLevelsBySource;
        public IReadOnlyDictionary<MagicProgressionType, int> SpecializedCasterLevels => _specializedCasterLevels;
        public IReadOnlyList<SpellDefinition> KnownSpells => _knownSpells;
        public int BonusFeats => _bonusFeats;
        public int BonusSkillPoints => _bonusSkillPoints;
        public IReadOnlyList<string> ProficiencyPackages => _proficiencyPackages.AsReadOnly();
        public IReadOnlyDictionary<string, int> MovementSpeeds => _movementSpeeds;
        public IReadOnlyDictionary<string, int> SenseRanges => _senseRanges;
        public IReadOnlyList<string> StatBlockNotes => _statBlockNotes.AsReadOnly();
        public int Experience => _experience;
        public IReadOnlyList<CharacterLevelRecord> LevelRecords => _levelRecords.AsReadOnly();

        public int MaxSkillRanks => Level + 3;
        public int MaxWarcraft => Level + 3;
        public int MaxBaseCasterLevel => Level + 3;
        public int MaxMagicLevelsPerProgression => Level + 3;
        public int MaxMagicLevelsBoughtPerLevel => 2;

        public static Character CreateLevelOne(
            string name,
            int strength,
            int dexterity,
            int constitution,
            int intelligence,
            int wisdom,
            int charisma)
        {
            return new Character(
                name,
                level: 1,
                totalCp: 24,
                abilityScores: new AbilityScores(strength, dexterity, constitution, intelligence, wisdom, charisma),
                size: CharacterSize.Medium);
        }

        public static Character CreateLevelOne(string name, AbilityScores abilityScores, int totalCp)
            => CreateLevelOne(name, abilityScores, totalCp, CharacterSize.Medium);

        public static Character CreateLevelOne(string name, AbilityScores abilityScores, int totalCp, CharacterSize size)
            => CreateLevelOne(name, abilityScores, totalCp, size, race: null, raceAbilityCatalog: null);

        public static Character CreateLevelOne(
            string name,
            AbilityScores abilityScores,
            int totalCp,
            CharacterSize size,
            RaceDefinitionDocument? race,
            RaceAbilityCatalogDocument? raceAbilityCatalog)
        {
            if (abilityScores is null)
            {
                throw new ArgumentNullException(nameof(abilityScores));
            }

            if (totalCp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCp), totalCp, "Total CP must be >= 0.");
            }

            var effectiveSize = race is null ? size : GetRaceSizeOrDefault(race, size);

            var character = new Character(name, level: 1, totalCp: totalCp, abilityScores: abilityScores, size: effectiveSize);
            character.ApplyRaceAbilityScoreModifiers(race);
            RaceAbilityApplicator.Apply(character, race, raceAbilityCatalog);
            character.EnsureLevelRecords(1);
            return character;
        }

        private static CharacterSize GetRaceSizeOrDefault(RaceDefinitionDocument race, CharacterSize defaultSize)
        {
            return CharacterSizeProfile.TryParse(race?.Rules?.Size, out var raceSize)
                ? raceSize
                : defaultSize;
        }

        public void SetSize(CharacterSize size)
        {
            Size = size;
        }

        public void SetRace(string? raceName)
        {
            RaceName = string.IsNullOrWhiteSpace(raceName) ? null : raceName.Trim();
        }

        public void AddAbilityScoreContribution(string ability, int amount, BonusType type, string source)
        {
            var normalized = NormalizeAbilityName(ability);
            if (!_abilityScoreContributions.TryGetValue(normalized, out var contributions))
            {
                contributions = new List<AbilityScoreContribution>();
                _abilityScoreContributions[normalized] = contributions;
            }

            contributions.Add(new AbilityScoreContribution(source, amount, type));
        }

        public AbilityScoreBreakdown GetAbilityScoreBreakdown(string ability)
        {
            var normalized = NormalizeAbilityName(ability);
            var baseScore = GetBaseAbilityScore(normalized);
            return new AbilityScoreBreakdown(
                normalized,
                baseScore,
                _abilityScoreContributions.TryGetValue(normalized, out var contributions)
                    ? contributions
                    : Enumerable.Empty<AbilityScoreContribution>());
        }

        private void ApplyRaceAbilityScoreModifiers(RaceDefinitionDocument? race)
        {
            foreach (var modifier in race?.Rules?.AbilityScoreModifiers ?? Enumerable.Empty<RaceAbilityScoreModifierDocument>())
            {
                if (modifier is null || modifier.Modifier is null || modifier.Choose)
                {
                    continue;
                }

                AddAbilityScoreContribution(
                    modifier.Ability,
                    modifier.Modifier.Value,
                    BonusType.Racial,
                    $"{race!.Name} racial modifier");
            }
        }

        public void SetMovementSpeed(string movementKind, int feet)
        {
            if (string.IsNullOrWhiteSpace(movementKind))
            {
                throw new ArgumentException("Movement kind is required.", nameof(movementKind));
            }

            _movementSpeeds[movementKind.Trim()] = Math.Max(0, feet);
        }

        public void AddMovementSpeedBonus(string movementKind, int feet)
        {
            if (string.IsNullOrWhiteSpace(movementKind))
            {
                throw new ArgumentException("Movement kind is required.", nameof(movementKind));
            }

            var key = movementKind.Trim();
            _movementSpeeds[key] = Math.Max(0, GetMovementSpeed(key) + feet);
        }

        public int GetMovementSpeed(string movementKind)
        {
            return _movementSpeeds.TryGetValue(movementKind ?? "", out var feet) ? feet : 0;
        }

        public void SetSenseRange(string senseKind, int feet)
        {
            if (string.IsNullOrWhiteSpace(senseKind))
            {
                throw new ArgumentException("Sense kind is required.", nameof(senseKind));
            }

            _senseRanges[senseKind.Trim()] = Math.Max(0, feet);
        }

        public void AddSenseRangeBonus(string senseKind, int feet)
        {
            if (string.IsNullOrWhiteSpace(senseKind))
            {
                throw new ArgumentException("Sense kind is required.", nameof(senseKind));
            }

            var key = senseKind.Trim();
            _senseRanges[key] = Math.Max(0, GetSenseRange(key) + feet);
        }

        public int GetSenseRange(string senseKind)
        {
            return _senseRanges.TryGetValue(senseKind ?? "", out var feet) ? feet : 0;
        }

        public HitDiceLevel GetHitDiceForLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            return _hitDiceByLevel.TryGetValue(level, out var entry)
                ? entry
                : new HitDiceLevel(primary: HitDieType.D4, secondary: null);
        }

        public void SetPrimaryHitDieForCurrentLevel(HitDieType hitDie)
        {
            var current = GetHitDiceForLevel(Level);
            _hitDiceByLevel[Level] = current.WithPrimary(hitDie);
        }

        public void BuySecondHitDieForCurrentLevel(HitDieType hitDie)
        {
            var current = GetHitDiceForLevel(Level);
            if (current.Secondary is not null)
            {
                throw new InvalidOperationException($"Second hit die already purchased at level {Level}.");
            }

            _hitDiceByLevel[Level] = current.WithSecondary(hitDie);
        }

        public void AdvanceLevel(int cpGained)
        {
            if (cpGained < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cpGained), cpGained, "CP gained must be >= 0.");
            }

            Level += 1;
            TotalCp += cpGained;
            _bonusSkillPoints += _bonusSkillPointsPerAdditionalLevel;
        }

        public void SetExperience(int totalExperience, IReadOnlyList<LevelProgressionLevelDocument> levelTable)
        {
            if (totalExperience < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalExperience), totalExperience, "Experience must be >= 0.");
            }

            _experience = totalExperience;
            UpdateLevelFromExperience(levelTable);
        }

        public void AddExperience(int amount, IReadOnlyList<LevelProgressionLevelDocument> levelTable)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Experience amount must be >= 0.");
            }

            _experience += amount;
            UpdateLevelFromExperience(levelTable);
        }

        private void UpdateLevelFromExperience(IReadOnlyList<LevelProgressionLevelDocument> levelTable)
        {
            if (levelTable is null)
            {
                throw new ArgumentNullException(nameof(levelTable));
            }

            var targetLevel = 1;
            foreach (var row in levelTable.Where(x => x != null).OrderBy(x => x.Level))
            {
                var xpRequired = row.XpTotal.GetValueOrDefault(row.Level == 1 ? 0 : int.MaxValue);
                if (xpRequired <= _experience)
                {
                    targetLevel = Math.Max(targetLevel, row.Level);
                }
            }

            EnsureLevelRecords(targetLevel);
            Level = targetLevel;
        }

        private void EnsureLevelRecords(int targetLevel)
        {
            if (_levelRecords.Count == 0)
            {
                _levelRecords.Add(new CharacterLevelRecord(level: 1, templateName: "Unassigned"));
            }

            while (_levelRecords.Count < targetLevel)
            {
                _levelRecords.Add(new CharacterLevelRecord(level: _levelRecords.Count + 1, templateName: "Unassigned"));
            }
        }

        public void SetTemplateNameForLevel(int level, string templateName)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name is required.", nameof(templateName));
            }

            EnsureLevelRecords(level);
            _levelRecords[level - 1] = _levelRecords[level - 1].WithTemplateName(templateName.Trim());
        }

        public void SetHpNoteForLevel(int level, string hpNote)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            EnsureLevelRecords(level);
            _levelRecords[level - 1] = _levelRecords[level - 1].WithHpNote(hpNote ?? "");
        }

        public void SetFavoredBonusForLevel(int level, string favoredBonus)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be >= 1.");
            }

            EnsureLevelRecords(level);
            _levelRecords[level - 1] = _levelRecords[level - 1].WithFavoredBonus(favoredBonus ?? "");
        }

        public void SpendCp(int amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot spend a negative CP amount.");
            }

            SpentCp += amount;
        }

        public void UpgradeHitDie(HitDieType newHitDie)
        {
            if (newHitDie <= HitDie)
            {
                throw new InvalidOperationException("New hit die must be higher than current hit die.");
            }

            HitDie = newHitDie;
        }

        public void AddWarcraft(int amount)
        {
            Warcraft += amount;
        }

        public void AddBaseCasterLevel(int amount)
        {
            BaseCasterLevel += amount;
        }

        public void AddCasterLevel(SpellSource source, int amount)
        {
            _casterLevelsBySource[source] += amount;
        }

        public void AddSpecializedCasterLevel(MagicProgressionType progressionType, int amount)
        {
            _specializedCasterLevels[progressionType] += amount;
        }

        public void AddSaveBonus(SaveType saveType, int amount)
        {
            _saveBonuses[saveType] += amount;
        }

        public void AddLimitedSaveBonus(SaveType saveType, int amount, string limitation)
        {
            _limitedSaveBonuses.Add(new LimitedSaveBonus(saveType, amount, limitation));
        }

        public void AddBonusFeats(int amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Bonus feat amount must be >= 1.");
            }

            _bonusFeats += amount;
        }

        public void SelectFeat(string featName, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(featName))
            {
                throw new ArgumentException("Feat name is required.", nameof(featName));
            }

            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Feat count must be >= 1.");
            }

            var normalized = featName.Trim();
            _selectedFeats.TryGetValue(normalized, out var current);
            _selectedFeats[normalized] = current + count;
        }

        public void AddBonusSkillPoints(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Bonus skill points must be >= 0.");
            }

            _bonusSkillPoints += amount;
        }

        public void AddBonusSkillPointsPerAdditionalLevel(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Bonus skill points per additional level must be >= 0.");
            }

            _bonusSkillPointsPerAdditionalLevel += amount;
        }

        public void AddStatBlockNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                _statBlockNotes.Add(note.Trim());
            }
        }

        public void AddProficiencyPackage(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Proficiency description is required.", nameof(description));
            }

            _proficiencyPackages.Add(description.Trim());
        }

        public void AddSkillRanks(string skillName, int ranks, bool isRelevantSkill)
            => AddSkillRanks(skillName, ranks, isRelevantSkill, irrelevantRankMultiplier: 0.5m);

        public void AddSkillRanks(string skillName, int ranks, bool isRelevantSkill, decimal irrelevantRankMultiplier)
        {
            if (!_skills.TryGetValue(skillName, out var existing))
            {
                existing = new SkillEntry(skillName, 0, isRelevantSkill);
                _skills[skillName] = existing;
            }

            existing.AddRanks(ranks, cpSpent: ranks, irrelevantRankMultiplier);
        }

        public void AddSkillSpecialty(string skillName, string specialtyName)
        {
            if (!_skillSpecialties.TryGetValue(skillName, out var existing))
            {
                existing = new List<SkillSpecialty>();
                _skillSpecialties[skillName] = existing;
            }

            var specialty = new SkillSpecialty(skillName, specialtyName);
            existing.Add(specialty);

            AddBonus(
                GetSkillSpecialtyBonusTarget(skillName, specialtyName),
                3,
                BonusType.Competence,
                "Skill Specialty",
                condition: specialtyName);
        }

        public void AddBonus(string target, int amount, BonusType type, string source, string? condition = null)
        {
            _bonusContributions.Add(new BonusContribution(target, amount, type, source, condition));
        }

        public BonusBreakdown GetBonusBreakdown(string target)
        {
            return new BonusBreakdown(
                target,
                _bonusContributions.Where(x => string.Equals(x.Target, target, StringComparison.OrdinalIgnoreCase)));
        }

        public int GetBonusTotal(string target)
        {
            return GetBonusBreakdown(target).Total;
        }

        public int GetSkillSpecialtyBonus(string skillName, string specialtyName)
        {
            return GetBonusTotal(GetSkillSpecialtyBonusTarget(skillName, specialtyName));
        }

        public static string GetSkillBonusTarget(string skillName)
        {
            return $"Skill:{(skillName ?? "").Trim()}";
        }

        public static string GetSkillSpecialtyBonusTarget(string skillName, string specialtyName)
        {
            return $"{GetSkillBonusTarget(skillName)}:{(specialtyName ?? "").Trim()}";
        }

        public void AddAbility(AbilityDefinition definition)
        {
            _abilities.Add(definition);
        }

        public void AddPurchasedAbility(PurchasedAbility purchasedAbility)
        {
            if (purchasedAbility is null)
            {
                throw new ArgumentNullException(nameof(purchasedAbility));
            }

            _purchasedAbilities.Add(purchasedAbility);
            _abilities.Add(purchasedAbility.Definition);
        }

        public void AddMagicLevels(MagicProgressionType progressionType, int amount)
        {
            _magicLevels[progressionType] += amount;
        }

        public void ConfigureMagicProgression(MagicProgressionConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _magicProgressionConfigs[config.Type] = config;
        }

        public void LearnSpell(SpellDefinition spell)
        {
            _knownSpells.Add(spell);
        }

        public decimal GetSkillRanks(string skillName)
        {
            return _skills.TryGetValue(skillName, out var skill) ? skill.Ranks : 0;
        }

        public bool HasAbility(string abilityName)
        {
            return _abilities.Any(x => string.Equals(x.Name, abilityName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasAbilityId(string abilityId)
        {
            return _abilities.Any(x => string.Equals(x.Id, abilityId, StringComparison.OrdinalIgnoreCase));
        }

        public int GetMagicLevel(MagicProgressionType progressionType)
        {
            return _magicLevels[progressionType];
        }

        public int GetSpecializedCasterLevel(MagicProgressionType progressionType)
        {
            return _specializedCasterLevels[progressionType];
        }

        public int GetCasterLevel(SpellSource source)
        {
            return _casterLevelsBySource[source];
        }

        public bool KnowsSpell(string spellName)
        {
            return _knownSpells.Any(x => string.Equals(x.Name, spellName, StringComparison.OrdinalIgnoreCase));
        }

        public int GetHighestSpellLevelAvailable(SpellSource source)
        {
            var matchingProgressions = MagicProgressionCatalog.All
                .Where(x => GetMagicProgressionSource(x.Type) == source && GetMagicLevel(x.Type) > 0)
                .ToList();

            if (matchingProgressions.Count == 0)
            {
                return 0;
            }

            var highestMagicLevel = matchingProgressions.Max(x => GetMagicLevel(x.Type));
            return Math.Min(9, Math.Max(0, (highestMagicLevel + 1) / 2));
        }

        public SpellSource GetMagicProgressionSource(MagicProgressionType type)
        {
            return _magicProgressionConfigs.TryGetValue(type, out var config)
                ? config.Source
                : MagicProgressionCatalog.Get(type).Source;
        }

        public int GetHighestCasterLevelAvailable(SpellSource source)
        {
            var matchingProgressions = MagicProgressionCatalog.All
                .Where(x => GetMagicProgressionSource(x.Type) == source && GetMagicLevel(x.Type) > 0)
                .ToList();

            var specialized = matchingProgressions.Count == 0
                ? 0
                : matchingProgressions.Max(x => GetSpecializedCasterLevel(x.Type));

            // Backward-compatible: legacy BaseCasterLevel is treated as universally applicable.
            // New builds should prefer per-source caster levels.
            return Math.Max(GetCasterLevel(source), BaseCasterLevel) + specialized;
        }

        public int GetCasterLevelForProgression(MagicProgressionType progressionType)
        {
            var source = GetMagicProgressionSource(progressionType);
            return Math.Max(GetCasterLevel(source), BaseCasterLevel) + GetSpecializedCasterLevel(progressionType);
        }

        public string GetSummary()
        {
            var saveSummary = string.Join(", ", _saveBonuses.Select(x => $"{x.Key}: +{x.Value}"));

            var skillsSummary = _skills.Count == 0
                ? "None"
                : string.Join(", ", _skills.Values.OrderBy(x => x.Name).Select(x => $"{x.Name} {x.Ranks}"));

            var specialtiesSummary = _skillSpecialties.Count == 0
                ? "None"
                : string.Join(", ", _skillSpecialties
                    .OrderBy(x => x.Key)
                    .SelectMany(x => x.Value.OrderBy(s => s.SpecialtyName)
                        .Select(s => $"{s.SkillName} ({s.SpecialtyName}) {FormatSigned(GetSkillSpecialtyBonus(s.SkillName, s.SpecialtyName))}")));

            var abilitiesSummary = _abilities.Count == 0
                ? "None"
                : string.Join(", ", _abilities.Select(x => x.Name));

            var selectedFeatsSummary = _selectedFeats.Count == 0
                ? "None"
                : string.Join(", ", _selectedFeats
                    .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.Value == 1 ? x.Key : $"{x.Key} x{x.Value}"));

            var limitedSaveSummary = _limitedSaveBonuses.Count == 0
                ? "None"
                : string.Join(", ", _limitedSaveBonuses.Select(x => $"{x.SaveType} {FormatSigned(x.Bonus)} ({x.Limitation})"));

            var bonusSummary = _bonusContributions.Count == 0
                ? "None"
                : string.Join(", ", _bonusContributions.Select(x =>
                    $"{x.Target} {FormatSigned(x.Amount)} {x.Type}{(string.IsNullOrWhiteSpace(x.Condition) ? "" : $" ({x.Condition})")} from {x.Source}"));

            var magicSummary = _magicLevels.Values.All(x => x == 0)
                ? "None"
                : string.Join(", ", _magicLevels.Where(x => x.Value > 0).Select(x => $"{x.Key} {x.Value}"));

            var specializedCasterSummary = _specializedCasterLevels.Values.All(x => x == 0)
                ? "None"
                : string.Join(", ", _specializedCasterLevels.Where(x => x.Value > 0).Select(x => $"{x.Key} {x.Value}"));

            var spellSummary = _knownSpells.Count == 0
                ? "None"
                : string.Join(", ", _knownSpells.Select(x => $"{x.Name} (L{x.Level})"));

            var casterLevelSummary = _casterLevelsBySource.Values.All(x => x == 0)
                ? "None"
                : string.Join(", ", _casterLevelsBySource.Where(x => x.Value > 0).Select(x => $"{x.Key} {x.Value}"));

            return string.Join(Environment.NewLine, new[]
            {
            $"Name: {Name}",
            $"Level: {Level}",
            $"Race: {RaceName ?? "None"}",
            $"Size: {Size}",
            $"Movement: {FormatDistanceMap(_movementSpeeds)}",
            $"Senses: {FormatDistanceMap(_senseRanges)}",
            $"CP: {RemainingCp}/{TotalCp} remaining ({SpentCp} spent)",
            $"Hit Die: {HitDie}",
            $"Warcraft (BAB): +{Warcraft}",
            $"Base Caster Level: +{BaseCasterLevel}",
            $"Caster Levels: {casterLevelSummary}",
            $"Saves: {saveSummary}",
            $"Skills: {skillsSummary}",
            $"Skill Specialties: {specialtiesSummary}",
            $"Abilities: {abilitiesSummary}",
            $"Selected Feats: {selectedFeatsSummary}",
            $"Bonus Feats: {BonusFeats}",
            $"Bonus Skill Points: {BonusSkillPoints}",
            $"Limited Saves: {limitedSaveSummary}",
            $"Bonuses: {bonusSummary}",
            $"Magic Levels: {magicSummary}",
            $"Specialized Caster Levels: {specializedCasterSummary}",
            $"Known Spells: {spellSummary}"
        });
        }

        public Character Clone()
        {
            var copy = new Character(Name, Level, TotalCp, BaseAbilityScores, Size)
            {
                HitDie = HitDie,
                Warcraft = Warcraft,
                BaseCasterLevel = BaseCasterLevel,
                SpentCp = SpentCp,
                RaceName = RaceName,
                _bonusFeats = _bonusFeats,
                _bonusSkillPoints = _bonusSkillPoints,
                _bonusSkillPointsPerAdditionalLevel = _bonusSkillPointsPerAdditionalLevel,
            };

            foreach (var entry in _saveBonuses)
            {
                copy._saveBonuses[entry.Key] = entry.Value;
            }

            foreach (var skill in _skills.Values)
            {
                copy._skills[skill.Name] = new SkillEntry(
                    skill.Name,
                    skill.Ranks,
                    skill.IsRelevantSkill,
                    skill.IsRestrictedSkill,
                    skill.CpInvested,
                    skill.IrrelevantRankMultiplier);
            }

            copy._abilities.AddRange(_abilities);
            copy._purchasedAbilities.AddRange(_purchasedAbilities.Select(x =>
                new PurchasedAbility(
                    x.Definition,
                    x.CostCp,
                    x.Modifiers.Select(m => new AbilityModifier(m.Type, m.Details)),
                    x.GmApproved,
                    x.SelectedOptions.Select(o => o.Definition))));

            foreach (var entry in _selectedFeats)
            {
                copy._selectedFeats[entry.Key] = entry.Value;
            }

            foreach (var entry in _magicLevels)
            {
                copy._magicLevels[entry.Key] = entry.Value;
            }

            foreach (var entry in _casterLevelsBySource)
            {
                copy._casterLevelsBySource[entry.Key] = entry.Value;
            }

            foreach (var entry in _magicProgressionConfigs)
            {
                copy._magicProgressionConfigs[entry.Key] = new MagicProgressionConfig(
                    entry.Value.Type,
                    entry.Value.Source,
                    entry.Value.Limitations);
            }

            foreach (var entry in _specializedCasterLevels)
            {
                copy._specializedCasterLevels[entry.Key] = entry.Value;
            }

            copy._knownSpells.AddRange(_knownSpells);
            copy._limitedSaveBonuses.AddRange(_limitedSaveBonuses.Select(x => new LimitedSaveBonus(x.SaveType, x.Bonus, x.Limitation)));
            copy._statBlockNotes.AddRange(_statBlockNotes);

            copy._levelRecords.Clear();
            copy._levelRecords.AddRange(_levelRecords);

            foreach (var entry in _abilityScoreContributions)
            {
                copy._abilityScoreContributions[entry.Key] = entry.Value
                    .Select(x => new AbilityScoreContribution(x.Source, x.Amount, x.Type))
                    .ToList();
            }

            foreach (var entry in _movementSpeeds)
            {
                copy._movementSpeeds[entry.Key] = entry.Value;
            }

            foreach (var entry in _senseRanges)
            {
                copy._senseRanges[entry.Key] = entry.Value;
            }

            foreach (var entry in _skillSpecialties)
            {
                copy._skillSpecialties[entry.Key] = entry.Value.Select(x => new SkillSpecialty(x.SkillName, x.SpecialtyName)).ToList();
            }

            copy._bonusContributions.AddRange(_bonusContributions.Select(x =>
                new BonusContribution(x.Target, x.Amount, x.Type, x.Source, x.Condition)));

            foreach (var entry in _hitDiceByLevel)
            {
                copy._hitDiceByLevel[entry.Key] = new HitDiceLevel(entry.Value.Primary, entry.Value.Secondary);
            }

            return copy;
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatDistanceMap(IReadOnlyDictionary<string, int> distances)
        {
            return distances.Count == 0
                ? "None"
                : string.Join(", ", distances.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.Value == 0 ? x.Key : $"{x.Key} {x.Value} ft."));
        }

        private int GetBaseAbilityScore(string ability)
        {
            switch (NormalizeAbilityName(ability))
            {
                case "Strength":
                    return BaseAbilityScores.Strength;
                case "Dexterity":
                    return BaseAbilityScores.Dexterity;
                case "Constitution":
                    return BaseAbilityScores.Constitution;
                case "Intelligence":
                    return BaseAbilityScores.Intelligence;
                case "Wisdom":
                    return BaseAbilityScores.Wisdom;
                case "Charisma":
                    return BaseAbilityScores.Charisma;
                default:
                    throw new ArgumentException($"Unknown ability score '{ability}'.", nameof(ability));
            }
        }

        private static string NormalizeAbilityName(string ability)
        {
            switch ((ability ?? "").Trim().ToLowerInvariant())
            {
                case "strength":
                case "str":
                    return "Strength";
                case "dexterity":
                case "dex":
                    return "Dexterity";
                case "constitution":
                case "con":
                    return "Constitution";
                case "intelligence":
                case "int":
                    return "Intelligence";
                case "wisdom":
                case "wis":
                    return "Wisdom";
                case "charisma":
                case "cha":
                    return "Charisma";
                default:
                    throw new ArgumentException($"Unknown ability score '{ability}'.", nameof(ability));
            }
        }
    }
}
