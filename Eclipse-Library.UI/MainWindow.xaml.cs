using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Eclipse_Library;
using Microsoft.Win32;

namespace Eclipse_Library.UI
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<TemplatePurchaseRow> _templatePurchases = new();
        private readonly ObservableCollection<TemplateLevelRow> _templateLevels = new();
        private readonly ObservableCollection<ClassTemplateListItem> _availableTemplates = new();
        private readonly ObservableCollection<TemplateSegmentRow> _segmentRows = new();
        private readonly ObservableCollection<CharacterClassLevelRow> _classLevels = new();
        private readonly ObservableCollection<SkillAllocationRow> _skillRows = new();
        private readonly ObservableCollection<FeatListItem> _availableFeats = new();
        private readonly ObservableCollection<SelectedFeatRow> _selectedFeats = new();
        private readonly ObservableCollection<RaceListItem> _availableRaces = new();
        private readonly ObservableCollection<AbilityListItem> _visibleAbilities = new();
        private readonly ObservableCollection<AbilityOptionSelection> _visibleOptions = new();
        private readonly ObservableCollection<AlignmentDefinition> _alignments = new();
        private readonly ObservableCollection<string> _knownLanguages = new();
        private readonly ObservableCollection<RaceAbilityDisplayRow> _racialAbilityRows = new();
        private readonly CharacterBuildDocumentStore _characterBuildDocumentStore = new();
        private readonly StatBlockFormatter _statBlockFormatter = new();
        private readonly CharacterBuildReplayFactory _buildReplayFactory = new();
        private readonly CharacterBuildAssembler _buildAssembler = new();
        private readonly CatalogPathResolver _catalogPathResolver = new();
        private readonly TemplateCatalogStore _templateCatalogStore = new();
        private AbilityRuleset? _ruleset;
        private List<AbilityDefinition> _allAbilities = new();
        private CharacterTemplateDefinition _currentTemplate = new();
        private int _editingLevel = 1;
        private bool _currentLevelDirty;
        private bool _isLoadingLevelSelection;
        private TemplateLevelRow? _selectedTemplateLevel;
        private LevelProgressionCatalogDocument? _levelProgressionCatalog;
        private PathfinderXpProgression _pathfinderProgression = PathfinderXpProgression.Medium;
        private HeroConfigurationSettings _heroSettings = HeroConfigurationSettings.LoadDefaults();
        private HashSet<string> _includedTemplateSources = new(StringComparer.OrdinalIgnoreCase)
        {
            ResourceSourceIds.CoreRules,
            ResourceSourceIds.Eclipse,
            ResourceSourceIds.PlayersHandbook,
            ResourceSourceIds.Custom,
        };
        private SkillCatalogDocument? _skillCatalog;
        private IReadOnlyList<SkillDefinitionDocument> _rulesetSkills = Array.Empty<SkillDefinitionDocument>();
        private HashSet<string> _currentTemplateClassSkillIds = new(StringComparer.OrdinalIgnoreCase);
        private FeatCatalogDocument? _featCatalog;
        private IReadOnlyList<FeatDefinitionDocument> _rulesetFeats = Array.Empty<FeatDefinitionDocument>();
        private RaceCatalogDocument? _raceCatalog;
        private RaceAbilityCatalogDocument? _raceAbilityCatalog;
        private IReadOnlyList<RaceDefinitionDocument> _rulesetRaces = Array.Empty<RaceDefinitionDocument>();
        private LanguageCatalogDocument? _languageCatalog;
        private IReadOnlyList<LanguageDefinitionDocument> _languages = Array.Empty<LanguageDefinitionDocument>();
        private AlignmentCatalogDocument? _alignmentCatalog;
        private Character? _lastCalculatedCharacter;
        private readonly Dictionary<string, int> _levelAbilityScoreAdjustments = new(StringComparer.OrdinalIgnoreCase);
        private bool _updatingAbilityScoreDisplay;
        private Window? _templateBuilderDialog;
        private bool _currentTemplateIsImmutable;
        private string? _currentCharacterFilePath;
        private readonly Dictionary<Guid, TabItem> _classDetailTabs = new();

        public MainWindow()
        {
            InitializeComponent();
            _includedTemplateSources = new HashSet<string>(_heroSettings.IncludedSources, StringComparer.OrdinalIgnoreCase);
            _currentTemplate.RulesetId = _heroSettings.RulesetId;
            InitializeLists();
            ApplyHeroConfiguration(_heroSettings, resetClassLevels: false);
            LoadTemplateLibrary();
            LoadRuleset();
            RefreshAbilityScoreModifiers();
            RefreshDerivedCombatAndSaves();
            LoadLevelProgressionConfig();
            LoadSkillCatalog();
            RefreshSkillRowsForRuleset();
            LoadFeatCatalog();
            RefreshFeatsForRuleset();
            LoadRaceCatalog();
            LoadRaceAbilityCatalog();
            RefreshRacesForRuleset();
            LoadLanguageCatalog();
            LoadAlignmentCatalog();
            EnsureCommonLanguage();
            CharacterBuildTabs.SelectedIndex = 0;
            RefreshCharacterBuild();
            Loaded += MainWindow_Loaded;
        }

        private void InitializeLists()
        {
            HitDieComboBox.ItemsSource = Enum.GetValues(typeof(HitDieType));
            HitDieComboBox.SelectedItem = HitDieType.D8;

            SaveTypeComboBox.ItemsSource = Enum.GetValues(typeof(SaveType));
            SaveTypeComboBox.SelectedItem = SaveType.Fortitude;

            MagicProgressionComboBox.ItemsSource = Enum.GetValues(typeof(MagicProgressionType));
            MagicProgressionComboBox.SelectedItem = MagicProgressionType.Wizard;

            RulesetComboBox.ItemsSource = RulesetProfile.All;
            RulesetComboBox.SelectedItem = RulesetProfile.Get(_currentTemplate.RulesetId);

            TemplateTagComboBox.ItemsSource = Enum.GetValues(typeof(TemplateTag));
            TemplateTagComboBox.SelectedItem = _currentTemplate.Tag;

            PathfinderProgressionComboBox.ItemsSource = Enum.GetValues(typeof(PathfinderXpProgression));
            PathfinderProgressionComboBox.SelectedItem = _pathfinderProgression;

            SizeComboBox.ItemsSource = Enum.GetValues(typeof(CharacterSize));
            SizeComboBox.SelectedItem = CharacterSize.Medium;
            AbilityScoreAdjustmentComboBox.ItemsSource = new[]
            {
                "Strength",
                "Dexterity",
                "Constitution",
                "Intelligence",
                "Wisdom",
                "Charisma",
            };
            AbilityScoreAdjustmentComboBox.SelectedItem = "Strength";

            TemplatePurchasesGrid.ItemsSource = _templatePurchases;
            TemplateLevelsListBox.ItemsSource = _templateLevels;
            ClassLevelsListBox.ItemsSource = _classLevels;
            AvailableTemplatesListBox.ItemsSource = _availableTemplates;
            SegmentRowsGrid.ItemsSource = _segmentRows;
            SkillsGrid.ItemsSource = _skillRows;
            ClassSkillsListBox.ItemsSource = _rulesetSkills;
            SelectedFeatsGrid.ItemsSource = _selectedFeats;
            AbilitiesListBox.ItemsSource = _visibleAbilities;
            AbilityOptionsItemsControl.ItemsSource = _visibleOptions;
            KnownLanguagesListBox.ItemsSource = _knownLanguages;
            RacialAbilitiesItemsControl.ItemsSource = _racialAbilityRows;
            CharacterAlignmentComboBox.ItemsSource = _alignments;
            RefreshTemplateMetadataControls();
            RefreshEditingLevelControls();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            ShowHeroConfigurationDialog(isStartup: true);
        }

        private void NewHero_Click(object sender, RoutedEventArgs e)
        {
            ResetCharacterBuild();
            ShowHeroConfigurationDialog(isStartup: false);
        }

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            NewHero_Click(sender, e);
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            PromptOpenCharacterFile();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            SaveCharacter(saveAs: false);
        }

        private void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveCharacter(saveAs: true);
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenTemplateEditor_Click(object sender, RoutedEventArgs e)
        {
            ShowTemplateBuilderDialog();
        }

        private void HelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "Eclipse Library", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ConfigureHero_Click(object sender, RoutedEventArgs e)
        {
            ShowHeroConfigurationDialog(isStartup: false);
        }

        private void PromptOpenCharacterFile()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Eclipse character files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open Character",
            };

            if (openDialog.ShowDialog(this) == true)
            {
                LoadCharacter(openDialog.FileName);
            }
        }

        private void SaveCharacter(bool saveAs)
        {
            var path = _currentCharacterFilePath;
            if (saveAs || string.IsNullOrWhiteSpace(path))
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Eclipse character files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save Character",
                    FileName = $"{SanitizeFileName(CharacterNameTextBox.Text)}.json",
                };

                if (saveDialog.ShowDialog(this) != true)
                {
                    return;
                }

                path = saveDialog.FileName;
            }

            var document = CreateCharacterBuildDocumentFromUi();

            _characterBuildDocumentStore.Save(path, document);
            _currentCharacterFilePath = path;
            SetStatus($"Saved character: {path}");
        }

        private CharacterBuildDocument CreateCharacterBuildDocumentFromUi()
        {
            return new CharacterBuildDocument
            {
                SchemaVersion = 2,
                HeroName = CharacterNameTextBox.Text,
                PlayerName = PlayerNameTextBox.Text,
                RulesetId = _heroSettings.RulesetId,
                TargetLevel = ReadIntOrNull(TargetLevelTextBox.Text) ?? 1,
                HitPoints = ReadIntOrNull(HitPointsTextBox.Text) ?? 0,
                StartingGold = ReadIntOrNull(StartingGoldTextBox.Text) ?? 0,
                Experience = ReadIntOrNull(ExperienceTextBox.Text) ?? 0,
                PathfinderProgression = _pathfinderProgression,
                Strength = GetBaseAbilityScore(StrengthTextBox),
                Dexterity = GetBaseAbilityScore(DexterityTextBox),
                Constitution = GetBaseAbilityScore(ConstitutionTextBox),
                Intelligence = GetBaseAbilityScore(IntelligenceTextBox),
                Wisdom = GetBaseAbilityScore(WisdomTextBox),
                Charisma = GetBaseAbilityScore(CharismaTextBox),
                LevelAbilityScoreAdjustments = new Dictionary<string, int>(_levelAbilityScoreAdjustments, StringComparer.OrdinalIgnoreCase),
                Race = GetSelectedRaceName(),
                Size = GetSelectedSize(),
                Alignment = GetSelectedAlignmentName(),
                Deity = DeityTextBox.Text,
                ReplaceCommon = ReplaceCommonCheckBox.IsChecked == true,
                Languages = _knownLanguages.ToList(),
                ClassLevels = _classLevels.Select(x => new CharacterClassLevelSaveDocument
                {
                    CharacterLevel = x.CharacterLevel,
                    TemplateId = x.TemplateId,
                    TemplateName = x.TemplateName,
                    TemplateLevel = x.TemplateLevel,
                    Source = x.Source,
                    HpNote = x.HpNote,
                    FavoredBonus = x.FavoredBonus,
                }).ToList(),
                Feats = _selectedFeats.Select(x => new CharacterFeatSaveDocument
                {
                    Name = x.Name,
                    Count = x.Count,
                }).ToList(),
                SkillAllocations = _skillRows
                    .Where(x => x.SkillPointsSpent > 0 || !string.IsNullOrWhiteSpace(x.Specialization))
                    .Select(x => new CharacterSkillAllocationSaveDocument
                    {
                        Name = x.Name,
                        Specialization = x.Specialization,
                        SkillPointsSpent = x.SkillPointsSpent,
                    })
                    .ToList(),
            };
        }

        private void LoadCharacter(string path)
        {
            CharacterBuildDocument? document;
            try
            {
                document = _characterBuildDocumentStore.Load(path);
            }
            catch (Exception ex)
            {
                SetStatus($"Could not open character: {ex.Message}");
                MessageBox.Show(this, ex.Message, "Open Character", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ApplyCharacterBuildDocumentToUi(document);
            _currentCharacterFilePath = path;
            RefreshCharacterBuild();
            SetStatus($"Opened character: {path}");
        }

        private void ApplyCharacterBuildDocumentToUi(CharacterBuildDocument document)
        {
            var settings = new HeroConfigurationSettings
            {
                HeroName = document.HeroName,
                PlayerName = document.PlayerName,
                RulesetId = document.RulesetId,
                AbilityMethod = _heroSettings.AbilityMethod,
                StartingLevel = Math.Max(1, document.TargetLevel),
                IncludedSources = _heroSettings.IncludedSources.ToList(),
                UseGestalt = _heroSettings.UseGestalt,
                MaxHpFirstLevel = _heroSettings.MaxHpFirstLevel,
                UseDefaultStartingCash = _heroSettings.UseDefaultStartingCash,
            };

            ApplyHeroConfiguration(settings, resetClassLevels: true);
            HitPointsTextBox.Text = document.HitPoints.ToString();
            StartingGoldTextBox.Text = document.StartingGold.ToString();
            ExperienceTextBox.Text = document.Experience.ToString();
            PathfinderProgressionComboBox.SelectedItem = document.PathfinderProgression;
            SetAbilityScoreTextAndBase(StrengthTextBox, document.Strength);
            SetAbilityScoreTextAndBase(DexterityTextBox, document.Dexterity);
            SetAbilityScoreTextAndBase(ConstitutionTextBox, document.Constitution);
            SetAbilityScoreTextAndBase(IntelligenceTextBox, document.Intelligence);
            SetAbilityScoreTextAndBase(WisdomTextBox, document.Wisdom);
            SetAbilityScoreTextAndBase(CharismaTextBox, document.Charisma);
            _levelAbilityScoreAdjustments.Clear();
            foreach (var entry in document.LevelAbilityScoreAdjustments.Where(x => x.Value > 0))
            {
                _levelAbilityScoreAdjustments[entry.Key] = entry.Value;
            }

            RefreshAbilityScoreAdjustmentsSummary();
            SetSelectedRaceName(document.Race);
            SizeComboBox.SelectedItem = document.Size ?? GetRaceSizeByName(document.Race);
            SetSelectedAlignment(document.Alignment);
            DeityTextBox.Text = document.Deity;
            ReplaceCommonCheckBox.IsChecked = document.ReplaceCommon;
            _knownLanguages.Clear();
            foreach (var language in document.Languages.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                _knownLanguages.Add(language);
            }

            EnsureCommonLanguage();

            foreach (var savedLevel in document.ClassLevels)
            {
                var template = _availableTemplates.FirstOrDefault(x => x.Document.Id == savedLevel.TemplateId);
                if (template is null)
                {
                    SetStatus($"Skipped missing template {savedLevel.TemplateId}.");
                    continue;
                }

                if (AddClassLevelFromTemplate(template) && _classLevels.LastOrDefault() is { } row)
                {
                    row.HpNote = savedLevel.HpNote;
                    row.FavoredBonus = savedLevel.FavoredBonus;
                }
            }

            RestoreSavedFeats(document.Feats);
            RestoreSavedSkillAllocations(document.SkillAllocations);
        }

        private void RestoreSavedFeats(IEnumerable<CharacterFeatSaveDocument>? feats)
        {
            _selectedFeats.Clear();

            foreach (var feat in feats ?? Enumerable.Empty<CharacterFeatSaveDocument>())
            {
                if (string.IsNullOrWhiteSpace(feat.Name))
                {
                    continue;
                }

                _selectedFeats.Add(new SelectedFeatRow(feat.Name.Trim())
                {
                    Count = Math.Max(1, feat.Count),
                });
            }

            RefreshFeatsSummary();
        }

        private void RestoreSavedSkillAllocations(IEnumerable<CharacterSkillAllocationSaveDocument>? allocations)
        {
            foreach (var allocation in allocations ?? Enumerable.Empty<CharacterSkillAllocationSaveDocument>())
            {
                if (string.IsNullOrWhiteSpace(allocation.Name))
                {
                    continue;
                }

                var row = FindSkillAllocationRow(allocation.Name, allocation.Specialization);
                if (row is null && !string.IsNullOrWhiteSpace(allocation.Specialization))
                {
                    AddSkillSpecializationRow(
                        new SkillSpecializationDefinition(allocation.Name.Trim(), allocation.Specialization.Trim(), baseDescription: null),
                        setStatus: false);
                    row = FindSkillAllocationRow(allocation.Name, allocation.Specialization);
                }

                if (row is not null)
                {
                    row.SkillPointsSpent = Math.Max(0, allocation.SkillPointsSpent);
                }
                else
                {
                    SetStatus($"Skipped missing skill allocation: {allocation.DisplayName}.");
                }
            }

            RefreshSkillDerivedFields();
            RefreshSkillPointsSummary();
        }

        private SkillAllocationRow? FindSkillAllocationRow(string name, string? specialization)
        {
            return _skillRows.FirstOrDefault(x =>
                string.Equals(x.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Specialization ?? "", specialization?.Trim() ?? "", StringComparison.OrdinalIgnoreCase));
        }

        private static string SanitizeFileName(string? name)
        {
            var value = string.IsNullOrWhiteSpace(name) ? "New Character" : name.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '-');
            }

            return value;
        }

        private void ShowHeroConfigurationDialog(bool isStartup)
        {
            var dialog = new HeroConfigurationDialog(_heroSettings)
            {
                Owner = this,
            };

            if (dialog.ShowDialog() != true)
            {
                if (isStartup)
                {
                    RefreshCharacterBuild();
                }

                return;
            }

            ApplyHeroConfiguration(dialog.Settings, resetClassLevels: true);

            if (dialog.OpenFileRequested)
            {
                PromptOpenCharacterFile();
            }
        }

        private void ApplyHeroConfiguration(HeroConfigurationSettings settings, bool resetClassLevels)
        {
            _heroSettings = settings;
            _includedTemplateSources = new HashSet<string>(settings.IncludedSources, StringComparer.OrdinalIgnoreCase);
            _includedTemplateSources.Add(ResourceSourceIds.CoreRules);
            _includedTemplateSources.Add(ResourceSourceIds.Eclipse);
            _includedTemplateSources.Add(ResourceSourceIds.PlayersHandbook);

            _currentTemplate.RulesetId = settings.RulesetId;

            if (CharacterNameTextBox is not null)
            {
                CharacterNameTextBox.Text = settings.HeroName;
            }

            if (PlayerNameTextBox is not null)
            {
                PlayerNameTextBox.Text = settings.PlayerName;
            }

            if (TargetLevelTextBox is not null)
            {
                TargetLevelTextBox.Text = Math.Max(1, settings.StartingLevel).ToString();
            }

            if (RulesetComboBox is not null)
            {
                RulesetComboBox.SelectedItem = RulesetProfile.Get(settings.RulesetId);
            }

            EnsureCommonLanguage();

            if (resetClassLevels)
            {
                ResetCharacterBuild();
            }

            LoadTemplateLibrary();
            RefreshSkillRowsForRuleset();
            RefreshFeatsForRuleset();
            LoadRaceCatalog();
            LoadRaceAbilityCatalog();
            RefreshRacesForRuleset();
            CharacterBuildTabs.SelectedIndex = 0;
            RefreshCharacterBuild();
            SetStatus($"Configured hero for {RulesetProfile.Get(settings.RulesetId).DisplayName}.");
        }

        private void ResetCharacterBuild()
        {
            foreach (var level in _classLevels)
            {
                level.PropertyChanged -= CharacterClassLevel_PropertyChanged;
            }

            _classLevels.Clear();
            _levelAbilityScoreAdjustments.Clear();
            RefreshAbilityScoreAdjustmentsSummary();
            DiagnosticsListBox.ItemsSource = null;
            _lastCalculatedCharacter = null;
            RefreshClassDetailTabs();
        }

        private void LoadSkillCatalog()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile("skills.catalog.json");
                _skillCatalog = SkillCatalogJson.LoadFromFile(path);
            }
            catch
            {
                _skillCatalog = null;
            }
        }

        private void LoadFeatCatalog()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile("feats.catalog.json");
                _featCatalog = FeatCatalogJson.LoadFromFile(path);
            }
            catch
            {
                _featCatalog = null;
            }
        }

        private void LoadRaceCatalog()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile(GetRaceCatalogFileName(_currentTemplate.RulesetId));
                _raceCatalog = RaceCatalogJson.LoadFromFile(path);
            }
            catch
            {
                _raceCatalog = null;
            }
        }

        private void LoadRaceAbilityCatalog()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile("race-abilities.catalog.json");
                _raceAbilityCatalog = RaceAbilityCatalogJson.LoadFromFile(path);
            }
            catch
            {
                _raceAbilityCatalog = null;
            }
        }

        private static string GetRaceCatalogFileName(RulesetId rulesetId)
        {
            return rulesetId switch
            {
                RulesetId.Dnd30 => "races.dnd30.catalog.json",
                RulesetId.Dnd35 => "races.dnd35.catalog.json",
                RulesetId.Pathfinder1E => "races.pathfinder1e.catalog.json",
                _ => "races.dnd35.catalog.json",
            };
        }

        private void LoadLanguageCatalog()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile("languages.catalog.json");
                _languageCatalog = LanguageCatalogJson.LoadFromFile(path);
                _languages = _languageCatalog.Languages;
            }
            catch
            {
                _languageCatalog = null;
                _languages = Array.Empty<LanguageDefinitionDocument>();
            }
        }

        private void LoadAlignmentCatalog()
        {
            _alignments.Clear();

            try
            {
                var path = _catalogPathResolver.FindDocsFile("alignments.catalog.json");
                _alignmentCatalog = AlignmentCatalogJson.LoadFromFile(path);
                foreach (var alignment in _alignmentCatalog.ToDefinitions())
                {
                    _alignments.Add(alignment);
                }
            }
            catch
            {
                _alignmentCatalog = null;
            }

            if (CharacterAlignmentComboBox is not null && CharacterAlignmentComboBox.SelectedItem is null)
            {
                CharacterAlignmentComboBox.SelectedItem = _alignments.FirstOrDefault();
            }
        }

        private void EnsureCommonLanguage()
        {
            if (ReplaceCommonCheckBox?.IsChecked == true)
            {
                return;
            }

            if (!_knownLanguages.Contains("Common"))
            {
                _knownLanguages.Insert(0, "Common");
            }
        }

        private void LoadLevelProgressionConfig()
        {
            try
            {
                var path = _catalogPathResolver.FindDocsFile("leveling.config.json");
                _levelProgressionCatalog = LevelProgressionJson.LoadFromFile(path);
                SetStatus($"Loaded leveling config from {path}.");
            }
            catch (Exception ex)
            {
                _levelProgressionCatalog = null;
                SetStatus($"Could not load leveling config: {ex.Message}");
            }
        }

        private void LoadTemplateLibrary()
        {
            _availableTemplates.Clear();
            _segmentRows.Clear();

            try
            {
                foreach (var item in _templateCatalogStore.LoadTemplates(_currentTemplate.RulesetId, _includedTemplateSources))
                {
                    _availableTemplates.Add(item);
                }

                var rulesetName = RulesetProfile.Get(_currentTemplate.RulesetId).DisplayName;
                SetStatus($"Loaded {_availableTemplates.Count} {rulesetName} templates.");
            }
            catch (Exception ex)
            {
                SetStatus($"Could not load template library: {ex.Message}");
            }
        }

        private void LoadRuleset()
        {
            try
            {
                var path = FindRulesetPath();
                _ruleset = AbilityRuleset.LoadFromFile(path);
                _allAbilities = _ruleset.Abilities.All
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                RefreshAbilityList();
                SetStatus($"Loaded {_allAbilities.Count} abilities from {path}.");
            }
            catch (Exception ex)
            {
                SetStatus($"Could not load special abilities: {ex.Message}");
            }
        }

        private void ReloadTemplateLibrary_Click(object sender, RoutedEventArgs e)
        {
            LoadTemplateLibrary();
        }

        private void LoadSelectedTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableTemplatesListBox.SelectedItem is not ClassTemplateListItem selected)
            {
                SetStatus("Select a template to load.");
                return;
            }

            LoadTemplateIntoBuilder(selected);
        }

        private bool LoadTemplateIntoBuilder(ClassTemplateListItem selected)
        {
            if (!EnsureCurrentLevelCanBeReplaced())
            {
                return false;
            }

            try
            {
                var doc = selected.Document;
                _currentTemplate = new CharacterTemplateDefinition(doc.Id, doc.Name, doc.RulesetId, doc.Tag, doc.MaxClassLevels);
                _currentTemplateClassSkillIds = GetTemplateClassSkillIds(doc, _rulesetSkills);
                _currentTemplate.Alignment = string.IsNullOrWhiteSpace(doc.Alignment) ? "Any" : doc.Alignment.Trim();
                _currentTemplate.StartingGold = doc.StartingGold?.Trim() ?? "";
                // Catalog is intended to have one entry per ruleset; still allow older files with a dictionary.
                _currentTemplate.Notes = (doc.NotesByRuleset ?? new Dictionary<RulesetId, string>())
                    .TryGetValue(doc.RulesetId, out var notes)
                    ? (notes ?? "")
                    : "";
                _templatePurchases.Clear();
                _templateLevels.Clear();
                _editingLevel = 1;
                _currentLevelDirty = false;
                _selectedTemplateLevel = null;

                foreach (var level in (doc.Levels ?? new List<ClassTemplateLevelDocument>()).OrderBy(x => x.ClassLevel))
                {
                    var purchases = new List<TemplatePurchaseRow>();
                    foreach (var purchase in (level.Purchases ?? new List<TemplatePurchaseDocument>()))
                    {
                        var row = TryCreateTemplatePurchaseRow(level.ClassLevel, purchase, level.ClassLevel, doc.RulesetId, _currentTemplateClassSkillIds);
                        if (row is not null)
                        {
                            purchases.Add(row);
                        }
                    }

                    _templateLevels.Add(new TemplateLevelRow(level.ClassLevel, purchases));
                }

                SortTemplateLevels();

                // Load the first level into the draft so totals and editing match what the user sees.
                var firstLevel = _templateLevels.Count == 0 ? 1 : _templateLevels.Min(x => x.Level);
                var firstRow = _templateLevels.FirstOrDefault(x => x.Level == firstLevel);
                LoadLevelDraft(firstLevel, firstRow?.Purchases ?? Array.Empty<TemplatePurchaseRow>(), firstRow);
                RefreshTemplateMetadataControls();
                RefreshEditingLevelControls();
                RefreshTemplateSummary();
                RefreshSkillRowsForRuleset();
                RefreshClassSkillSelection();
                RefreshLevelDependentSummaries();
                _currentTemplateIsImmutable = IsOfficialTemplate(selected);
                RefreshTemplateMutability();
                SetStatus(_currentTemplateIsImmutable
                    ? $"Loaded immutable official template '{doc.Name}' for review."
                    : $"Loaded template '{doc.Name}' ({selected.Source}).");
                return true;
            }
            catch (Exception ex)
            {
                SetStatus($"Could not load template: {ex.Message}");
                return false;
            }
        }

        private void SaveTemplateAsCustom_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentTemplateIsImmutable && !ApplyTemplateMetadataFromControls())
            {
                return;
            }

            var unsupported = _templateLevels
                .SelectMany(level => level.Purchases)
                .Where(p => p.Document is null)
                .Select(p => p.Kind)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (unsupported.Count > 0)
            {
                SetStatus("Template contains purchases that can't be saved yet: " + string.Join(", ", unsupported) + ".");
                return;
            }

            try
            {
                var template = new ClassTemplateDocument
                {
                    Id = _currentTemplateIsImmutable ? Guid.NewGuid() : _currentTemplate.Id,
                    Name = _currentTemplateIsImmutable ? $"{_currentTemplate.Name} Custom Copy" : _currentTemplate.Name,
                    RulesetId = _currentTemplate.RulesetId,
                    Tag = _currentTemplate.Tag == TemplateTag.Official ? TemplateTag.Custom : _currentTemplate.Tag,
                    Source = new ResourceSourceDocument
                    {
                        Id = ResourceSourceIds.Custom,
                        Name = "Custom",
                        IsOptional = true,
                    },
                    MaxClassLevels = _currentTemplate.MaxClassLevels,
                    Alignment = _currentTemplate.Alignment,
                    StartingGold = _currentTemplate.StartingGold,
                    NotesByRuleset = new Dictionary<RulesetId, string> { { _currentTemplate.RulesetId, _currentTemplate.Notes } },
                    ClassSkillIds = _currentTemplateClassSkillIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
                    Levels = _templateLevels
                        .OrderBy(x => x.Level)
                        .Select(level => new ClassTemplateLevelDocument
                        {
                            ClassLevel = level.Level,
                            Purchases = level.Purchases
                                .Select(p => p.Document!)
                                .ToList(),
                        })
                        .ToList(),
                };

                _templateCatalogStore.SaveCustomTemplate(template);
                _currentTemplateIsImmutable = false;
                _currentTemplate = new CharacterTemplateDefinition(template.Id, template.Name, template.RulesetId, template.Tag, template.MaxClassLevels)
                {
                    Alignment = template.Alignment ?? "Any",
                    StartingGold = template.StartingGold ?? "",
                    Notes = template.NotesByRuleset is not null && template.NotesByRuleset.TryGetValue(template.RulesetId, out var savedNotes)
                        ? savedNotes ?? ""
                        : "",
                };
                _currentTemplateClassSkillIds = new HashSet<string>(template.ClassSkillIds ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
                RefreshTemplateMetadataControls();
                RefreshTemplateMutability();
                LoadTemplateLibrary();
                SetStatus($"Saved custom template '{template.Name}'.");
            }
            catch (Exception ex)
            {
                SetStatus($"Could not save custom template: {ex.Message}");
            }
        }

        private void LoadSegmentRows_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableTemplatesListBox.SelectedItem is not ClassTemplateListItem selected)
            {
                SetStatus("Select a template to load a segment from.");
                return;
            }

            if (!int.TryParse(SegmentFromTextBox.Text.Trim(), out var fromLevel) || fromLevel < 1
                || !int.TryParse(SegmentToTextBox.Text.Trim(), out var toLevel) || toLevel < fromLevel
                || !int.TryParse(SegmentStartAtTextBox.Text.Trim(), out var startAt) || startAt < 1)
            {
                SetStatus("Segment levels must be positive numbers (from <= to).");
                return;
            }

            _segmentRows.Clear();

            var templateLevels = (selected.Document.Levels ?? new List<ClassTemplateLevelDocument>())
                .Where(x => x.ClassLevel >= fromLevel && x.ClassLevel <= toLevel)
                .OrderBy(x => x.ClassLevel)
                .ToList();

            for (var i = 0; i < templateLevels.Count; i++)
            {
                var level = templateLevels[i];
                var targetLevel = startAt + i;
                var purchases = new List<TemplatePurchaseRow>();
                foreach (var purchase in (level.Purchases ?? new List<TemplatePurchaseDocument>()))
                {
                    var row = TryCreateTemplatePurchaseRow(targetLevel, purchase);
                    if (row is not null)
                    {
                        purchases.Add(row);
                    }
                }

                _segmentRows.Add(new TemplateSegmentRow(selected.Document.Name, level.ClassLevel, targetLevel, purchases));
            }

            SetStatus($"Loaded {templateLevels.Count} segment row(s) from '{selected.Document.Name}'.");
        }

        private void ApplySegmentRows_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureCurrentTemplateMutable("apply segment rows"))
            {
                return;
            }

            if (_segmentRows.Count == 0)
            {
                SetStatus("No segment rows loaded.");
                return;
            }

            foreach (var row in _segmentRows)
            {
                if (row.TargetLevel < 1)
                {
                    SetStatus("Target levels must be >= 1.");
                    return;
                }
            }

            foreach (var segment in _segmentRows)
            {
                var existing = _templateLevels.FirstOrDefault(x => x.Level == segment.TargetLevel);
                if (existing is null)
                {
                    _templateLevels.Add(new TemplateLevelRow(segment.TargetLevel, segment.Purchases));
                }
                else
                {
                    existing.ReplacePurchases(existing.Purchases.Concat(segment.Purchases).ToList());
                }
            }

            SortTemplateLevels();
            RefreshTemplateSummary();
            SetStatus($"Applied {_segmentRows.Count} segment row(s) into the template.");
        }

        private void ClearSegmentRows_Click(object sender, RoutedEventArgs e)
        {
            _segmentRows.Clear();
            SetStatus("Cleared segment rows.");
        }

        private TemplatePurchaseRow? TryCreateTemplatePurchaseRow(
            int level,
            TemplatePurchaseDocument purchase,
            int? templateLevel = null,
            RulesetId? rulesetId = null,
            ISet<string>? classSkillIds = null)
        {
            if (purchase is null)
            {
                return null;
            }

            switch (purchase.Kind)
            {
                case TemplatePurchaseKind.HitDie:
                    if (purchase.HitDie is null)
                    {
                        return null;
                    }

                    var hitDie = purchase.HitDie.Value;
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Hit Die",
                        description: $"Hit Die {hitDie}",
                        createPurchase: () => new BuyHitDieForLevelPurchase(hitDie),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.Warcraft:
                    if (purchase.Warcraft is null)
                    {
                        return null;
                    }

                    var warcraft = purchase.Warcraft.Value;
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Warcraft",
                        description: $"+{warcraft} Warcraft",
                        createPurchase: () => new BuyWarcraftPurchase(warcraft),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.SaveBonus:
                    if (purchase.SaveType is null || purchase.SaveBonus is null)
                    {
                        return null;
                    }

                    var saveType = purchase.SaveType.Value;
                    var saveBonus = purchase.SaveBonus.Value;
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Save",
                        description: $"+{saveBonus} {saveType}",
                        createPurchase: () => new BuySaveBonusPurchase(saveType, saveBonus),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.SkillRanks:
                    if (string.IsNullOrWhiteSpace(purchase.SkillName) || purchase.SkillRanks is null)
                    {
                        return null;
                    }

                    var skillName = purchase.SkillName.Trim();
                    var skillId = GetTemplatePurchaseSkillId(purchase);
                    var ranks = GetEffectiveTemplateSkillRanks(
                        level,
                        templateLevel,
                        rulesetId,
                        purchase,
                        _heroSettings.UseFirstCharacterLevelSkillPointMultiplier);
                    var relevant = purchase.SkillIsRelevant
                        ?? (classSkillIds is not null && !string.IsNullOrWhiteSpace(skillId) && classSkillIds.Contains(skillId));
                    var relevance = relevant ? "relevant" : "irrelevant";
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Skill",
                        description: $"{skillName} {ranks} rank(s), {relevance}",
                        createPurchase: () => new BuySkillRanksPurchase(skillName, ranks, relevant, GetIrrelevantSkillRankMultiplier(rulesetId)),
                        document: purchase,
                        costCp: ranks);

                case TemplatePurchaseKind.BonusFeat:
                    var count = purchase.BonusFeats.GetValueOrDefault(1);
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Bonus Feat",
                        description: count == 1 ? "Bonus Feat" : $"Bonus Feat x{count}",
                        createPurchase: () => new BuyBonusFeatPurchase(count),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.BaseCasterLevel:
                    if (purchase.BaseCasterLevels is null)
                    {
                        return null;
                    }

                    var baseCaster = purchase.BaseCasterLevels.Value;
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Base Caster Level",
                        description: $"+{baseCaster} Base Caster Level",
                        createPurchase: () => new BuyBaseCasterLevelPurchase(baseCaster),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.MagicLevels:
                    if (purchase.MagicProgression is null || purchase.MagicLevels is null)
                    {
                        return null;
                    }

                    var progression = purchase.MagicProgression.Value;
                    var magicLevels = purchase.MagicLevels.Value;
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Magic Levels",
                        description: $"{progression} +{magicLevels} magic level(s)",
                        createPurchase: () => new BuyMagicLevelsPurchase(progression, magicLevels),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                case TemplatePurchaseKind.Ability:
                    if (_ruleset is null || string.IsNullOrWhiteSpace(purchase.AbilityId))
                    {
                        return null;
                    }

                    var ability = _ruleset.Abilities.GetById(purchase.AbilityId.Trim());
                    var optionIds = (purchase.AbilityOptionIds ?? new List<string>())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                    var selectedOptions = optionIds
                        .Select(id => ability.GetOptionById(id.Trim()))
                        .ToList();

                    var optionText = selectedOptions.Count == 0
                        ? ""
                        : $" ({string.Join(", ", selectedOptions.Select(x => x.Name))})";

                    return new TemplatePurchaseRow(
                        level,
                        kind: "Ability",
                        description: $"{ability.Name}{optionText}",
                        createPurchase: () => new BuyAbilityPurchase(ability, selectedOptions),
                        document: purchase,
                        costCp: ability.CostCp + selectedOptions.Sum(x => x.CostCp));

                case TemplatePurchaseKind.Proficiencies:
                    if (string.IsNullOrWhiteSpace(purchase.ProficiencyText))
                    {
                        return null;
                    }

                    var text = purchase.ProficiencyText.Trim();
                    var profCost = purchase.ProficiencyCostCp.GetValueOrDefault(0);
                    return new TemplatePurchaseRow(
                        level,
                        kind: "Proficiencies",
                        description: text,
                        createPurchase: () => new BuyProficiencyPackagePurchase(text, profCost),
                        document: purchase,
                        costCp: GetPurchaseCostCp(purchase));

                default:
                    return null;
            }
        }

        private static int GetEffectiveTemplateSkillRanks(
            int characterLevel,
            int? templateLevel,
            RulesetId? rulesetId,
            TemplatePurchaseDocument purchase,
            bool useFirstCharacterLevelSkillPointMultiplier)
        {
            if (!IsUnassignedSkillPointPurchase(purchase))
            {
                return purchase.SkillRanks.GetValueOrDefault(0);
            }

            if (IsDnd30Or35(rulesetId)
                && templateLevel == 1
                && useFirstCharacterLevelSkillPointMultiplier)
            {
                if (characterLevel == 1)
                {
                    return purchase.SkillRanksAtFirstCharacterLevel
                        ?? purchase.SkillRanks.GetValueOrDefault(0);
                }

                return purchase.SkillRanksAtClassLevel
                    ?? Math.Max(0, purchase.SkillRanks.GetValueOrDefault(0) / 4);
            }

            var ranks = purchase.SkillRanks.GetValueOrDefault(0);
            if (characterLevel <= 1
                || templateLevel != 1
                || !IsDnd30Or35(rulesetId)
                || !useFirstCharacterLevelSkillPointMultiplier)
            {
                return ranks;
            }

            return ranks / 4;
        }

        private static bool IsDnd30Or35(RulesetId? rulesetId)
        {
            return rulesetId == RulesetId.Dnd30 || rulesetId == RulesetId.Dnd35;
        }

        private static decimal GetIrrelevantSkillRankMultiplier(RulesetId? rulesetId)
        {
            return rulesetId == RulesetId.Pathfinder1E ? 1m : 0.5m;
        }

        private static bool IsUnassignedSkillPointPurchase(TemplatePurchaseDocument purchase)
        {
            return string.Equals(
                purchase.SkillName?.Trim(),
                "Unassigned Skill Points",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetTemplatePurchaseSkillId(TemplatePurchaseDocument purchase)
        {
            return string.IsNullOrWhiteSpace(purchase.SkillId)
                ? SkillId.FromName(purchase.SkillName ?? "")
                : purchase.SkillId.Trim();
        }

        private static HashSet<string> GetTemplateClassSkillIds(
            ClassTemplateDocument template,
            IEnumerable<SkillDefinitionDocument> rulesetSkills)
        {
            var explicitIds = template.ClassSkillIds?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            if (explicitIds is { Count: > 0 })
            {
                return explicitIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var notes = template.NotesByRuleset is not null
                && template.NotesByRuleset.TryGetValue(template.RulesetId, out var value)
                ? value ?? ""
                : "";

            return InferClassSkillIdsFromNotes(notes, rulesetSkills);
        }

        private static HashSet<string> InferClassSkillIdsFromNotes(
            string notes,
            IEnumerable<SkillDefinitionDocument> rulesetSkills)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(notes))
            {
                return ids;
            }

            foreach (var skill in rulesetSkills.Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name)))
            {
                var name = skill.Name.Trim();
                if (notes.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ids.Add(SkillId.Get(skill));
                }
            }

            return ids;
        }

        private static string FindRulesetPath()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "docs", "special-abilities.chapter2.json");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "special-abilities.chapter2.json"));
        }

        private void RefreshTemplateMetadataControls()
        {
            TemplateNameTextBox.Text = _currentTemplate.Name;
            TemplateIdTextBox.Text = _currentTemplate.Id.ToString();
            RulesetComboBox.SelectedItem = RulesetProfile.Get(_currentTemplate.RulesetId);
            TemplateTagComboBox.SelectedItem = _currentTemplate.Tag;
            MaxClassLevelsTextBox.Text = _currentTemplate.MaxClassLevels?.ToString() ?? "";
            AlignmentTextBox.Text = _currentTemplate.Alignment;
            TemplateStartingGoldTextBox.Text = _currentTemplate.StartingGold;
            TemplateNotesTextBox.Text = _currentTemplate.Notes;
            RefreshTemplateSummary();
        }

        private void ShowTemplateBuilderDialog()
        {
            if (_templateBuilderDialog is { IsVisible: true })
            {
                _templateBuilderDialog.Activate();
                return;
            }

            if (TemplateBuildDialogContent.Parent is Panel oldParent)
            {
                oldParent.Children.Remove(TemplateBuildDialogContent);
            }

            TemplateBuildDialogContent.Visibility = Visibility.Visible;
            _templateBuilderDialog = new Window
            {
                Title = _currentTemplateIsImmutable ? "Template Builder - Official Template" : "Template Builder",
                Owner = this,
                Width = 1080,
                Height = 720,
                MinWidth = 980,
                MinHeight = 640,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = TemplateBuildDialogContent,
            };

            _templateBuilderDialog.Closed += (_, _) =>
            {
                if (_templateBuilderDialog is not null)
                {
                    _templateBuilderDialog.Content = null;
                }

                TemplateBuildDialogContent.Visibility = Visibility.Collapsed;
                if (MainTabs.Parent is Panel parent && TemplateBuildDialogContent.Parent is null)
                {
                    var index = parent.Children.IndexOf(MainTabs);
                    parent.Children.Insert(Math.Max(0, index), TemplateBuildDialogContent);
                }

                _templateBuilderDialog = null;
            };

            _templateBuilderDialog.Show();
        }

        private static bool IsOfficialTemplate(ClassTemplateListItem template)
        {
            return !string.Equals(template.Source, ResourceSourceIds.Custom, StringComparison.OrdinalIgnoreCase)
                || template.Document.Tag == TemplateTag.Official;
        }

        private bool EnsureCurrentTemplateMutable(string action)
        {
            if (!_currentTemplateIsImmutable)
            {
                return true;
            }

            SetStatus($"Official templates are immutable. Save a custom copy before you {action}.");
            return false;
        }

        private void RefreshTemplateMutability()
        {
            var canEdit = !_currentTemplateIsImmutable;
            TemplateNameTextBox.IsReadOnly = !canEdit;
            MaxClassLevelsTextBox.IsReadOnly = !canEdit;
            AlignmentTextBox.IsReadOnly = !canEdit;
            TemplateStartingGoldTextBox.IsReadOnly = !canEdit;
            TemplateNotesTextBox.IsReadOnly = !canEdit;
            RulesetComboBox.IsEnabled = canEdit;
            TemplateTagComboBox.IsEnabled = canEdit;
            SegmentRowsGrid.IsReadOnly = !canEdit;
        }

        private void RefreshTemplateSummary()
        {
            if (TemplateSummaryTextBox is null)
            {
                return;
            }

            var ruleset = _currentTemplate.Ruleset;
            var progression = new EclipseCpProgression();
            var maxLevel = Math.Max(
                _editingLevel,
                _templateLevels.Count == 0 ? 1 : _templateLevels.Max(x => x.Level));
            var totalSpent = GetTotalTemplateSpentCp();
            var totalAvailable = progression.GetTotalCpAtLevel(maxLevel);
            var breakdown = GetTemplateCostBreakdown();
            var purchasesByLevel = _templateLevels
                .OrderBy(x => x.Level)
                .Select(level => $"L{level.Level}: {string.Join("; ", level.Purchases.Select(x => x.Description))}");

            var draftSummary = _templatePurchases.Count == 0
                ? "None"
                : string.Join("; ", _templatePurchases.Select(x => x.Description));

            TemplateSummaryTextBox.Text = string.Join(
                Environment.NewLine,
                new[]
            {
                $"Template: {_currentTemplate.Name}",
                $"Id: {_currentTemplate.Id}",
                $"Ruleset: {ruleset.DisplayName}",
                $"Ruleset Notes: {ruleset.Description}",
                $"Tag: {_currentTemplate.Tag}; Max Class Levels: {_currentTemplate.MaxClassLevels?.ToString() ?? "n/a"}",
                $"Alignment: {_currentTemplate.Alignment}; Starting Gold: {_currentTemplate.StartingGold}",
                string.IsNullOrWhiteSpace(_currentTemplate.Notes) ? null : $"Notes: {_currentTemplate.Notes}",
                $"Hit Die: {breakdown.HitDieCp} CP; Saves: {breakdown.SavesCp} CP; Warcraft: {breakdown.WarcraftCp} CP",
                $"Magic Levels: {breakdown.MagicLevelsCp} CP; Base Caster Level: {breakdown.BaseCasterLevelCp} CP",
                $"Abilities: {breakdown.AbilitiesCp} CP; Proficiencies: {breakdown.ProficienciesCp} CP; Skills: {breakdown.SkillsCp} CP",
                $"Total CP: {totalSpent} / {totalAvailable} (through level {maxLevel})",
                "",
                "Saved Levels:",
                _templateLevels.Count == 0 ? "None" : string.Join(Environment.NewLine, purchasesByLevel),
                "",
                $"Editing Level {_editingLevel}{(_currentLevelDirty ? " (unsaved)" : "")}:",
                draftSummary,
            }.Where(x => x is not null));

            RefreshTemplateCpSummary();
        }

        private void NewTemplate_Click(object sender, RoutedEventArgs e)
        {
            _currentTemplate = new CharacterTemplateDefinition();
            _currentTemplateClassSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _currentTemplateIsImmutable = false;
            _templatePurchases.Clear();
            _templateLevels.Clear();
            _editingLevel = 1;
            _currentLevelDirty = false;
            _selectedTemplateLevel = null;
            RefreshEditingLevelControls();
            RefreshTemplateMetadataControls();
            RefreshTemplateMutability();
            RefreshClassSkillSelection();
            RefreshLevelDependentSummaries();
            SetStatus("Created a new template.");
        }

        private void ApplyTemplateMetadata_Click(object sender, RoutedEventArgs e)
        {
            ApplyTemplateMetadataFromControls();
        }

        private void TemplateMetadata_Changed(object sender, TextChangedEventArgs e)
        {
            if (TemplateNameTextBox is null || TemplateSummaryTextBox is null)
            {
                return;
            }

            if (_currentTemplateIsImmutable)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(TemplateNameTextBox.Text))
            {
                _currentTemplate.Name = TemplateNameTextBox.Text.Trim();
                RefreshTemplateSummary();
                RefreshLevelDependentSummaries();
            }
        }

        private void RulesetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentTemplateIsImmutable)
            {
                RefreshTemplateMetadataControls();
                SetStatus("Official templates are immutable. Save a custom copy before changing the ruleset.");
                return;
            }

            if (RulesetComboBox.SelectedItem is RulesetProfile profile)
            {
                _currentTemplate.RulesetId = profile.Id;
                LoadTemplateLibrary();
                RefreshTemplateSummary();
                RefreshSkillRowsForRuleset();
                RefreshFeatsForRuleset();
                LoadRaceCatalog();
                LoadRaceAbilityCatalog();
                RefreshRacesForRuleset();
                SetStatus($"Template ruleset set to {profile.DisplayName}; loaded {_availableTemplates.Count} matching templates.");
            }
        }

        private void RefreshRacesForRuleset()
        {
            _availableRaces.Clear();

            if (_raceCatalog is null || _raceCatalog.RulesetId != _currentTemplate.RulesetId)
            {
                _rulesetRaces = Array.Empty<RaceDefinitionDocument>();
                return;
            }

            _rulesetRaces = (_raceCatalog.Races ?? new List<RaceDefinitionDocument>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var race in _rulesetRaces)
            {
                _availableRaces.Add(new RaceListItem(race));
            }

            var selectedRace = GetSelectedRaceName();
            if (!string.IsNullOrWhiteSpace(selectedRace)
                && !string.Equals(selectedRace, "Choose Race", StringComparison.OrdinalIgnoreCase)
                && !_rulesetRaces.Any(x => string.Equals(x.Name, selectedRace, StringComparison.OrdinalIgnoreCase)))
            {
                SetSelectedRaceName("Choose Race");
            }

            RefreshRacialAbilities();
        }

        private void OpenRaceSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_availableRaces.Count == 0)
            {
                SetStatus("No races are available for the selected ruleset.");
                return;
            }

            var dialog = new RaceSelectionDialog(_availableRaces, GetSelectedRaceName())
            {
                Owner = this,
            };

            if (dialog.ShowDialog() != true || dialog.SelectedRace is null)
            {
                return;
            }

            SetSelectedRaceName(dialog.SelectedRace.Definition.Name);
            SetSelectedSize(GetRaceSize(dialog.SelectedRace.Definition));
            RefreshRacialAbilities();
            RefreshAbilityScoreModifiers();
            RefreshFeatsSummary();
            RefreshSkillPointsSummary();
            RefreshCharacterBuild();
            SetStatus($"Selected race '{dialog.SelectedRace.Definition.Name}'.");
        }

        private string GetSelectedRaceName()
        {
            return RaceButton?.Content?.ToString()?.Trim() ?? "Choose Race";
        }

        private void SetSelectedRaceName(string? raceName)
        {
            if (RaceButton is not null)
            {
                RaceButton.Content = string.IsNullOrWhiteSpace(raceName) ? "Choose Race" : raceName.Trim();
            }

            RefreshRacialAbilities();
        }

        private string GetSelectedAlignmentName()
        {
            return CharacterAlignmentComboBox?.SelectedItem is AlignmentDefinition alignment
                ? alignment.Name
                : "Choose Alignment";
        }

        private void SetSelectedAlignment(string? alignment)
        {
            if (CharacterAlignmentComboBox is null)
            {
                return;
            }

            var normalized = (alignment ?? "").Trim();
            var selected = _alignments.FirstOrDefault(x =>
                string.Equals(x.Id, normalized, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Name, normalized, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Abbreviation, normalized, StringComparison.OrdinalIgnoreCase));

            CharacterAlignmentComboBox.SelectedItem = selected ?? _alignments.FirstOrDefault();
        }

        private CharacterSize GetSelectedSize()
        {
            return SizeComboBox?.SelectedItem is CharacterSize size
                ? size
                : CharacterSize.Medium;
        }

        private void SetSelectedSize(CharacterSize size)
        {
            if (SizeComboBox is not null)
            {
                SizeComboBox.SelectedItem = size;
            }
        }

        private static CharacterSize GetRaceSize(RaceDefinitionDocument race)
        {
            return CharacterSizeProfile.TryParse(race?.Rules?.Size, out var size)
                ? size
                : CharacterSize.Medium;
        }

        private CharacterSize GetRaceSizeByName(string? raceName)
        {
            var race = _rulesetRaces.FirstOrDefault(x => string.Equals(x.Name, raceName, StringComparison.OrdinalIgnoreCase));
            return race is null ? CharacterSize.Medium : GetRaceSize(race);
        }

        private RaceDefinitionDocument? GetSelectedRaceDefinition()
        {
            var raceName = GetSelectedRaceName();
            return _rulesetRaces.FirstOrDefault(x => string.Equals(x.Name, raceName, StringComparison.OrdinalIgnoreCase));
        }

        private void RefreshRacialAbilities()
        {
            _racialAbilityRows.Clear();
            var race = GetSelectedRaceDefinition();
            if (race is null)
            {
                return;
            }

            var definitions = (_raceAbilityCatalog?.Abilities ?? new List<RaceAbilityDefinitionDocument>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var reference in race.Rules.Abilities ?? new List<RaceAbilityReferenceDocument>())
            {
                if (reference is null || string.IsNullOrWhiteSpace(reference.AbilityId))
                {
                    continue;
                }

                if (!definitions.TryGetValue(reference.AbilityId, out var definition))
                {
                    continue;
                }

                _racialAbilityRows.Add(new RaceAbilityDisplayRow(definition.Name, BuildRaceAbilityDescription(definition, reference)));
            }
        }

        private static string BuildRaceAbilityDescription(RaceAbilityDefinitionDocument definition, RaceAbilityReferenceDocument reference)
        {
            var lines = new List<string> { definition.Name };
            if (!string.IsNullOrWhiteSpace(definition.Description))
            {
                lines.Add("");
                lines.Add(definition.Description.Trim());
            }

            if (reference.Configuration is { Count: > 0 })
            {
                lines.Add("");
                lines.Add("Configuration: " + string.Join(", ", reference.Configuration.Select(x => $"{x.Key}: {x.Value}")));
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void RefreshFeatsForRuleset()
        {
            _availableFeats.Clear();
            _selectedFeats.Clear();

            if (_featCatalog is null)
            {
                _rulesetFeats = Array.Empty<FeatDefinitionDocument>();
                RefreshFeatsSummary();
                return;
            }

            var ruleset = _featCatalog.Rulesets.FirstOrDefault(x => x.RulesetId == _currentTemplate.RulesetId);
            if (ruleset is null)
            {
                _rulesetFeats = Array.Empty<FeatDefinitionDocument>();
                RefreshFeatsSummary();
                return;
            }

            _rulesetFeats = (ruleset.Feats ?? new List<FeatDefinitionDocument>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var feat in _rulesetFeats)
            {
                _availableFeats.Add(new FeatListItem(feat));
            }

            RefreshFeatsSummary();
        }

        private void OpenFeatSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_availableFeats.Count == 0)
            {
                SetStatus("No feats are available for the selected ruleset.");
                return;
            }

            var keepAdding = true;
            while (keepAdding)
            {
                var level = ReadIntOrNull(TargetLevelTextBox.Text) ?? Math.Max(1, _classLevels.Count);
                var selectedCount = _selectedFeats.Sum(x => x.Count);
                var availableCount = GetFeatsGrantedByLevel(level);
                var dialog = new FeatSelectionDialog(_availableFeats, selectedCount, availableCount)
                {
                    Owner = this,
                };

                if (dialog.ShowDialog() != true || dialog.SelectedFeat is null)
                {
                    return;
                }

                if (!AddFeat(dialog.SelectedFeat.Definition))
                {
                    return;
                }

                keepAdding = dialog.AddAnother;
            }
        }

        private bool AddFeat(FeatDefinitionDocument feat)
        {
            var existing = _selectedFeats.FirstOrDefault(x => string.Equals(x.Name, feat.Name, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                _selectedFeats.Add(new SelectedFeatRow(feat.Name));
            }
            else
            {
                if (!feat.Repeatable && !feat.Stacks)
                {
                    SetStatus("That feat can't be taken more than once.");
                    return false;
                }

                existing.Count += 1;
            }

            RefreshFeatsSummary();
            SetStatus($"Added feat '{feat.Name}'.");
            RefreshCharacterBuild();
            return true;
        }

        private void RemoveSelectedFeat_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFeatsGrid.SelectedItem is not SelectedFeatRow row)
            {
                return;
            }

            _selectedFeats.Remove(row);
            RefreshFeatsSummary();
            RefreshCharacterBuild();
        }

        private void RemoveOneSelectedFeat_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFeatsGrid.SelectedItem is not SelectedFeatRow row)
            {
                return;
            }

            if (row.Count <= 1)
            {
                _selectedFeats.Remove(row);
            }
            else
            {
                row.Count -= 1;
            }

            RefreshFeatsSummary();
            RefreshCharacterBuild();
        }

        private void ClearFeats_Click(object sender, RoutedEventArgs e)
        {
            _selectedFeats.Clear();
            RefreshFeatsSummary();
            RefreshCharacterBuild();
        }

        private void RefreshSkillRowsForRuleset()
        {
            _skillRows.Clear();

            if (_skillCatalog is null)
            {
                return;
            }

            var ruleset = _skillCatalog.Rulesets.FirstOrDefault(x => x.RulesetId == _currentTemplate.RulesetId);
            if (ruleset is null)
            {
                _rulesetSkills = Array.Empty<SkillDefinitionDocument>();
                return;
            }

            _rulesetSkills = (ruleset.Skills ?? new List<SkillDefinitionDocument>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            ClassSkillsListBox.ItemsSource = _rulesetSkills;
            RefreshClassSkillSelection();

            foreach (var skill in _rulesetSkills
                .Where(x => !x.RequiresSpecialization)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                _skillRows.Add(new SkillAllocationRow(skill.Name.Trim(), specialization: null, skill.Attribute, requiresSpecialization: false));
            }

            RefreshSkillDerivedFields();
            RefreshSkillPointsSummary();
        }

        private void RefreshClassSkillSelection()
        {
            if (ClassSkillsListBox is null)
            {
                return;
            }

            ClassSkillsListBox.SelectedItems.Clear();
            foreach (var skill in _rulesetSkills.Where(skill => _currentTemplateClassSkillIds.Contains(SkillId.Get(skill))))
            {
                ClassSkillsListBox.SelectedItems.Add(skill);
            }
        }

        private void RefreshSkillDerivedFields()
        {
            var str = ReadIntOrNull(StrengthTextBox?.Text) ?? 10;
            var dex = ReadIntOrNull(DexterityTextBox?.Text) ?? 10;
            var con = ReadIntOrNull(ConstitutionTextBox?.Text) ?? 10;
            var intel = ReadIntOrNull(IntelligenceTextBox?.Text) ?? 10;
            var wis = ReadIntOrNull(WisdomTextBox?.Text) ?? 10;
            var cha = ReadIntOrNull(CharismaTextBox?.Text) ?? 10;
            var sizeProfile = CharacterSizeProfile.Get(GetSelectedSize());

            foreach (var row in _skillRows)
            {
                var score = row.Attribute switch
                {
                    SkillAttribute.Strength => str,
                    SkillAttribute.Dexterity => dex,
                    SkillAttribute.Constitution => con,
                    SkillAttribute.Intelligence => intel,
                    SkillAttribute.Wisdom => wis,
                    SkillAttribute.Charisma => cha,
                    _ => 10,
                };

                row.AttributeModifier = AbilityScores.GetModifier(score);
                row.SizeModifier = IsSizeModifiedSkill(row.Name)
                    ? sizeProfile.HideModifier
                    : 0;
                var isClassSkill = IsClassSkillKnown(row.Name);
                row.IsClassSkill = isClassSkill;
                row.RankMultiplier = IsDnd30Or35(_currentTemplate.RulesetId) && !isClassSkill
                    ? 0.5m
                    : 1m;
                row.ClassSkillBonus = _currentTemplate.RulesetId == RulesetId.Pathfinder1E
                    && isClassSkill
                    && row.SkillPointsSpent > 0
                        ? 3
                        : 0;
            }
        }

        private bool IsClassSkillKnown(string skillName)
        {
            var skillId = SkillId.FromName(skillName);
            if (_currentTemplateClassSkillIds.Contains(skillId))
            {
                return true;
            }

            return _classLevels
                .Select(x => _availableTemplates.FirstOrDefault(t => t.Document.Id == x.TemplateId)?.Document)
                .Where(x => x is not null)
                .Select(x => GetTemplateClassSkillIds(x!, _rulesetSkills))
                .Any(ids => ids.Contains(skillId));
        }

        private static bool IsSizeModifiedSkill(string skillName)
        {
            return string.Equals(skillName, "Hide", StringComparison.OrdinalIgnoreCase)
                || string.Equals(skillName, "Stealth", StringComparison.OrdinalIgnoreCase);
        }

        private IReadOnlyList<SkillSpecializationDefinition> GetDefaultSkillSpecializations()
        {
            var descriptions = _rulesetSkills
                .Where(x => x.RequiresSpecialization)
                .ToDictionary(x => x.Name, GetSkillDescription, StringComparer.OrdinalIgnoreCase);

            return SkillSpecializationDefinition.CreateDefaults(descriptions);
        }

        private static string GetSkillDescription(SkillDefinitionDocument skill)
        {
            return !string.IsNullOrWhiteSpace(skill.Description)
                ? skill.Description
                : skill.Notes ?? "";
        }

        private void AddSpecializedSkill_Click(object sender, RoutedEventArgs e)
        {
            var keepAdding = true;
            while (keepAdding)
            {
                var dialog = new SkillSelectionDialog(
                    GetDefaultSkillSpecializations(),
                    _skillRows.Select(x => x.DisplayName))
                {
                    Owner = this,
                };

                if (dialog.ShowDialog() != true || dialog.SelectedSkill is null)
                {
                    return;
                }

                AddSkillSpecializationRow(dialog.SelectedSkill, setStatus: true);
                keepAdding = dialog.AddAnother;
            }
        }

        private bool AddSkillSpecializationRow(SkillSpecializationDefinition option, bool setStatus)
        {
            var definition = _rulesetSkills.FirstOrDefault(x => string.Equals(x.Name, option.BaseSkillName, StringComparison.OrdinalIgnoreCase));
            if (definition is null)
            {
                return false;
            }

            if (_skillRows.Any(x => string.Equals(x.DisplayName, option.DisplayName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var row = new SkillAllocationRow(definition.Name.Trim(), option.Specialization, definition.Attribute, requiresSpecialization: true);
            _skillRows.Add(row);
            SortSkillRows();
            RefreshSkillDerivedFields();
            if (setStatus)
            {
                SetStatus($"Added {row.DisplayName}.");
            }

            return true;
        }

        private void SortSkillRows()
        {
            var sorted = _skillRows
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _skillRows.Clear();
            foreach (var row in sorted)
            {
                _skillRows.Add(row);
            }
        }

        private void Skill_Increment_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not SkillAllocationRow row)
            {
                return;
            }

            if (GetRemainingSkillPoints() <= 0)
            {
                SetStatus("No skill points remaining.");
                return;
            }

            row.SkillPointsSpent += 1;
            RefreshSkillDerivedFields();
            RefreshSkillPointsSummary();
            RefreshCharacterBuild();
        }

        private void Skill_Decrement_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not SkillAllocationRow row)
            {
                return;
            }

            if (row.SkillPointsSpent <= 0)
            {
                return;
            }

            row.SkillPointsSpent -= 1;
            RefreshSkillDerivedFields();
            RefreshSkillPointsSummary();
            RefreshCharacterBuild();
        }

        private bool ApplyTemplateMetadataFromControls()
        {
            if (!EnsureCurrentTemplateMutable("update template metadata"))
            {
                return false;
            }

            if (!TryReadRequiredText(TemplateNameTextBox, "Template name", out var templateName))
            {
                return false;
            }

            if (RulesetComboBox.SelectedItem is not RulesetProfile profile)
            {
                SetStatus("Rule set is required.");
                return false;
            }

            _currentTemplate.Name = templateName;
            _currentTemplate.RulesetId = profile.Id;

            if (TemplateTagComboBox.SelectedItem is TemplateTag tag)
            {
                _currentTemplate.Tag = tag;
            }

            if (string.IsNullOrWhiteSpace(MaxClassLevelsTextBox.Text))
            {
                _currentTemplate.MaxClassLevels = null;
            }
            else if (int.TryParse(MaxClassLevelsTextBox.Text.Trim(), out var maxLevels) && maxLevels > 0)
            {
                _currentTemplate.MaxClassLevels = maxLevels;
            }
            else
            {
                SetStatus("Max class levels must be blank or a positive number.");
                return false;
            }

            _currentTemplate.Alignment = (AlignmentTextBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(_currentTemplate.Alignment))
            {
                _currentTemplate.Alignment = "Any";
            }

            _currentTemplate.StartingGold = (TemplateStartingGoldTextBox.Text ?? "").Trim();
            _currentTemplate.Notes = (TemplateNotesTextBox.Text ?? "").Trim();

            RefreshTemplateSummary();
            SetStatus($"Template metadata updated for {templateName}.");
            return true;
        }

        private void NewLevel_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureCurrentTemplateMutable("create a level"))
            {
                return;
            }

            if (!EnsureCurrentLevelCanBeReplaced())
            {
                return;
            }

            var nextLevel = _templateLevels.Count == 0
                ? Math.Max(1, _editingLevel + 1)
                : Math.Max(_editingLevel + 1, _templateLevels.Max(x => x.Level) + 1);

            LoadLevelDraft(nextLevel, Array.Empty<TemplatePurchaseRow>(), selectedLevel: null);
            SetStatus($"Started editing level {nextLevel}.");
        }

        private void SaveLevel_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentLevel();
        }

        private void TemplateLevelsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoadingLevelSelection)
            {
                return;
            }

            if (TemplateLevelsListBox.SelectedItem is not TemplateLevelRow selected)
            {
                return;
            }

            if (!EnsureCurrentLevelCanBeReplaced())
            {
                _isLoadingLevelSelection = true;
                TemplateLevelsListBox.SelectedItem = _selectedTemplateLevel;
                _isLoadingLevelSelection = false;
                return;
            }

            LoadLevelDraft(selected.Level, selected.Purchases, selected);
            SetStatus($"Loaded level {selected.Level} for editing.");
        }

        private bool EnsureCurrentLevelCanBeReplaced()
        {
            if (!_currentLevelDirty)
            {
                return true;
            }

            var result = MessageBox.Show(
                $"Level {_editingLevel} has unsaved changes. Save it before continuing?",
                "Unsaved Template Level",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
            {
                return false;
            }

            if (result == MessageBoxResult.Yes)
            {
                SaveCurrentLevel();
            }

            return true;
        }

        private void SaveCurrentLevel()
        {
            if (!EnsureCurrentTemplateMutable("save levels"))
            {
                return;
            }

            var purchases = _templatePurchases.ToList();
            var existing = _templateLevels.FirstOrDefault(x => x.Level == _editingLevel);
            if (existing is null)
            {
                existing = new TemplateLevelRow(_editingLevel, purchases);
                _templateLevels.Add(existing);
            }
            else
            {
                existing.ReplacePurchases(purchases);
            }

            SortTemplateLevels();
            _currentLevelDirty = false;
            _selectedTemplateLevel = _templateLevels.First(x => x.Level == _editingLevel);
            _isLoadingLevelSelection = true;
            TemplateLevelsListBox.SelectedItem = _selectedTemplateLevel;
            _isLoadingLevelSelection = false;
            RefreshEditingLevelControls();
            RefreshTemplateSummary();
            RefreshLevelDependentSummaries();
            SetStatus($"Saved template level {_editingLevel}.");
        }

        private void SortTemplateLevels()
        {
            var sorted = _templateLevels.OrderBy(x => x.Level).ToList();
            _templateLevels.Clear();
            foreach (var level in sorted)
            {
                _templateLevels.Add(level);
            }
        }

        private void LoadLevelDraft(int level, IEnumerable<TemplatePurchaseRow> purchases, TemplateLevelRow? selectedLevel)
        {
            _editingLevel = level;
            _templatePurchases.Clear();
            foreach (var purchase in purchases)
            {
                _templatePurchases.Add(purchase);
            }

            _currentLevelDirty = false;
            _selectedTemplateLevel = selectedLevel;
            _isLoadingLevelSelection = true;
            TemplateLevelsListBox.SelectedItem = selectedLevel;
            _isLoadingLevelSelection = false;
            RefreshEditingLevelControls();
            RefreshTemplateSummary();
        }

        private void MarkCurrentLevelDirty()
        {
            _currentLevelDirty = true;
            RefreshEditingLevelControls();
            RefreshTemplateSummary();
        }

        private void RefreshEditingLevelControls()
        {
            if (EditingLevelTextBox is not null)
            {
                EditingLevelTextBox.Text = _currentLevelDirty
                    ? $"{_editingLevel} (unsaved)"
                    : _editingLevel.ToString();
            }
        }

        private void RefreshAbilityList()
        {
            var query = AbilitySearchTextBox.Text?.Trim() ?? "";
            _visibleAbilities.Clear();

            foreach (var ability in _allAbilities.Where(x =>
                string.IsNullOrWhiteSpace(query)
                || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || x.Id.Contains(query, StringComparison.OrdinalIgnoreCase)))
            {
                _visibleAbilities.Add(new AbilityListItem(ability));
            }
        }

        private void AbilitySearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshAbilityList();
        }

        private void AbilitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _visibleOptions.Clear();
            if (AbilitiesListBox.SelectedItem is not AbilityListItem item)
            {
                return;
            }

            foreach (var option in item.Definition.Options.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                _visibleOptions.Add(new AbilityOptionSelection(option));
            }
        }

        private void AddHitDie_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level) || HitDieComboBox.SelectedItem is not HitDieType hitDie)
            {
                return;
            }

            AddTemplatePurchase(
                level,
                "Hit Die",
                $"Hit Die {hitDie}",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.HitDie, HitDie = hitDie },
                () => new BuyHitDieForLevelPurchase(hitDie));
        }

        private void AddWarcraft_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level) || !TryReadPositiveInt(WarcraftTextBox, "Warcraft levels", out var amount))
            {
                return;
            }

            AddTemplatePurchase(
                level,
                "Warcraft",
                $"+{amount} Warcraft",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.Warcraft, Warcraft = amount },
                () => new BuyWarcraftPurchase(amount));
        }

        private void AddSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level)
                || SaveTypeComboBox.SelectedItem is not SaveType saveType
                || !TryReadPositiveInt(SaveBonusTextBox, "Save bonus", out var bonus))
            {
                return;
            }

            AddTemplatePurchase(
                level,
                "Save",
                $"+{bonus} {saveType}",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.SaveBonus, SaveType = saveType, SaveBonus = bonus },
                () => new BuySaveBonusPurchase(saveType, bonus));
        }

        private void AddSkillRanks_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level)
                || !TryReadRequiredText(SkillNameTextBox, "Skill name", out var skillName)
                || !TryReadPositiveInt(SkillRanksTextBox, "Skill ranks", out var ranks))
            {
                return;
            }

            var isRelevant = RelevantSkillCheckBox.IsChecked == true;
            var skillId = SkillId.FromName(skillName);
            var relevance = isRelevant ? "relevant" : "irrelevant";
            AddTemplatePurchase(
                level,
                "Skill",
                $"{skillName} {ranks} rank(s), {relevance}",
                new TemplatePurchaseDocument
                {
                    Kind = TemplatePurchaseKind.SkillRanks,
                    SkillName = skillName,
                    SkillId = skillId,
                    SkillRanks = ranks,
                    SkillIsRelevant = isRelevant,
                },
                () => new BuySkillRanksPurchase(skillName, ranks, isRelevant, GetIrrelevantSkillRankMultiplier(_currentTemplate.RulesetId)));
        }

        private void SaveClassSkills_Click(object sender, RoutedEventArgs e)
        {
            if (_editingLevel != 1)
            {
                SetStatus("Class skills are defined at template level 1.");
                return;
            }

            _currentTemplateClassSkillIds = ClassSkillsListBox.SelectedItems
                .OfType<SkillDefinitionDocument>()
                .Select(SkillId.Get)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            _currentLevelDirty = true;
            SetStatus($"Saved {_currentTemplateClassSkillIds.Count} class skill(s) for this template.");
        }

        private void AddBaseCasterLevel_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level)
                || !TryReadPositiveInt(BaseCasterLevelTextBox, "Base Caster Level", out var levels))
            {
                return;
            }

            AddTemplatePurchase(
                level,
                "Base Caster Level",
                $"+{levels} Base Caster Level",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.BaseCasterLevel, BaseCasterLevels = levels },
                () => new BuyBaseCasterLevelPurchase(levels));
        }

        private void AddMagicLevels_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level)
                || MagicProgressionComboBox.SelectedItem is not MagicProgressionType progression
                || !TryReadPositiveInt(MagicLevelsTextBox, "Magic levels", out var levels))
            {
                return;
            }

            var definition = MagicProgressionCatalog.Get(progression);
            AddTemplatePurchase(
                level,
                "Magic Levels",
                $"{progression} +{levels} magic level(s), {definition.ChartCostPerLevelCp} CP each",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.MagicLevels, MagicProgression = progression, MagicLevels = levels },
                () => new BuyMagicLevelsPurchase(progression, levels));
        }

        private void AddAbility_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level) || AbilitiesListBox.SelectedItem is not AbilityListItem abilityItem)
            {
                return;
            }

            var selectedOptions = _visibleOptions
                .Where(x => x.IsSelected)
                .Select(x => x.Definition)
                .ToList();

            var optionText = selectedOptions.Count == 0
                ? ""
                : $" ({string.Join(", ", selectedOptions.Select(x => x.Name))})";

            AddTemplatePurchase(
                level,
                "Ability",
                $"{abilityItem.Definition.Name}{optionText}",
                new TemplatePurchaseDocument
                {
                    Kind = TemplatePurchaseKind.Ability,
                    AbilityId = abilityItem.Definition.Id,
                    AbilityOptionIds = selectedOptions.Select(x => x.Id).ToList(),
                },
                () => new BuyAbilityPurchase(abilityItem.Definition, selectedOptions));
        }

        private void AddBonusFeats_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetPurchaseLevel(out var level) || !TryReadPositiveInt(BonusFeatsTextBox, "Bonus feats", out var count))
            {
                return;
            }

            AddTemplatePurchase(
                level,
                "Bonus Feat",
                count == 1 ? "Bonus Feat" : $"Bonus Feat x{count}",
                new TemplatePurchaseDocument { Kind = TemplatePurchaseKind.BonusFeat, BonusFeats = count },
                () => new BuyBonusFeatPurchase(count));
        }

        private void AddTemplatePurchase(int level, string kind, string description, TemplatePurchaseDocument? document, Func<IPurchase> createPurchase)
        {
            if (!EnsureCurrentTemplateMutable("add purchases"))
            {
                return;
            }

            var cost = GetPurchaseCostCp(document);
            _templatePurchases.Add(new TemplatePurchaseRow(level, kind, description, createPurchase, document, cost));
            MarkCurrentLevelDirty();
            SetStatus($"Added {description} at level {level}.");
        }

        private void RemoveSelectedPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureCurrentTemplateMutable("remove purchases"))
            {
                return;
            }

            if (TemplatePurchasesGrid.SelectedItem is TemplatePurchaseRow row)
            {
                _templatePurchases.Remove(row);
                MarkCurrentLevelDirty();
                SetStatus("Removed selected template purchase.");
            }
        }

        private void ClearTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureCurrentTemplateMutable("clear a level"))
            {
                return;
            }

            _templatePurchases.Clear();
            MarkCurrentLevelDirty();
            SetStatus($"Cleared level {_editingLevel} draft.");
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            CharacterNameTextBox.Text = "New Character";
            PlayerNameTextBox.Text = "";
            TargetLevelTextBox.Text = "1";
            HitPointsTextBox.Text = "0";
            StartingGoldTextBox.Text = "0";
            SetAbilityScoreTextAndBase(StrengthTextBox, 10);
            SetAbilityScoreTextAndBase(DexterityTextBox, 10);
            SetAbilityScoreTextAndBase(ConstitutionTextBox, 10);
            SetAbilityScoreTextAndBase(IntelligenceTextBox, 10);
            SetAbilityScoreTextAndBase(WisdomTextBox, 10);
            SetAbilityScoreTextAndBase(CharismaTextBox, 10);
            ExperienceTextBox.Text = "0";
            ResetCharacterBuild();
            RefreshAbilityScoreModifiers();
            RefreshLevelDependentSummaries();
            DiagnosticsListBox.ItemsSource = null;
            StatBlockTextBox.Text = "";
            _lastCalculatedCharacter = null;
            RefreshDerivedCombatAndSaves();
            RefreshCharacterBuild();
            SetStatus("Character fields reset.");
        }

        private void AddClassLevel_Click(object sender, RoutedEventArgs e)
        {
            ShowTemplateSelectionDialog();
        }

        private void CreateCustomTemplate_Click(object sender, RoutedEventArgs e)
        {
            NewTemplate_Click(sender, e);
            ShowTemplateBuilderDialog();
            SetStatus("Started a custom template. Define and save levels in the Template Builder dialog.");
        }

        private void RemoveSelectedClassLevel_Click(object sender, RoutedEventArgs e)
        {
            if (ClassLevelsListBox.SelectedItem is not CharacterClassLevelRow selected)
            {
                SetStatus("Select a class level to remove.");
                return;
            }

            RemoveClassLevel(selected);
        }

        private void RemoveClassLevel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: CharacterClassLevelRow selected })
            {
                RemoveClassLevel(selected);
            }
        }

        private void RemoveClassLevel(CharacterClassLevelRow selected)
        {
            selected.PropertyChanged -= CharacterClassLevel_PropertyChanged;
            _classLevels.Remove(selected);
            RenumberClassLevels();
            RefreshCharacterBuild();
            RefreshClassDetailTabs();
            SetStatus($"Removed {selected.DisplayName}.");
        }

        private void ShowTemplateSelectionDialog()
        {
            if (_availableTemplates.Count == 0)
            {
                SetStatus("No templates are available for the selected ruleset and sources.");
                return;
            }

            var keepAdding = true;
            while (keepAdding)
            {
                var targetLevel = _classLevels.Count + 1;
                var configuredTarget = ReadIntOrNull(TargetLevelTextBox.Text) ?? Math.Max(1, targetLevel);
                var levelsRemaining = Math.Max(1, configuredTarget - _classLevels.Count);
                var dialog = new TemplateSelectionDialog(_availableTemplates, levelsRemaining)
                {
                    Owner = this,
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                if (dialog.CreateCustomTemplateRequested)
                {
                    CreateCustomTemplate_Click(this, new RoutedEventArgs());
                    return;
                }

                if (dialog.SelectedTemplate is null)
                {
                    return;
                }

                if (dialog.EditTemplateRequested)
                {
                    if (IsOfficialTemplate(dialog.SelectedTemplate))
                    {
                        SetStatus("Official templates are immutable and cannot be edited. Create a custom template instead.");
                        MessageBox.Show(
                            this,
                            "Official templates are immutable. Create a custom template or save a custom copy before editing.",
                            "Official Template",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }

                    if (LoadTemplateIntoBuilder(dialog.SelectedTemplate))
                    {
                        ShowTemplateBuilderDialog();
                    }

                    return;
                }

                for (var i = 0; i < dialog.LevelsToAdd; i++)
                {
                    if (!AddClassLevelFromTemplate(dialog.SelectedTemplate))
                    {
                        return;
                    }
                }

                keepAdding = dialog.AddAnother;
            }
        }

        private bool AddClassLevelFromTemplate(ClassTemplateListItem template)
        {
            var doc = template.Document;
            var nextTemplateLevel = _classLevels
                .Where(x => x.TemplateId == doc.Id)
                .Select(x => x.TemplateLevel)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var levelDoc = (doc.Levels ?? new List<ClassTemplateLevelDocument>())
                .FirstOrDefault(x => x.ClassLevel == nextTemplateLevel);
            if (levelDoc is null)
            {
                SetStatus($"{doc.Name} does not define template level {nextTemplateLevel}.");
                return false;
            }

            var characterLevel = _classLevels.Count + 1;
            var purchases = new List<TemplatePurchaseRow>();
            foreach (var purchase in levelDoc.Purchases ?? new List<TemplatePurchaseDocument>())
            {
                var classSkillIds = GetTemplateClassSkillIds(doc, _rulesetSkills);
                var row = TryCreateTemplatePurchaseRow(characterLevel, purchase, nextTemplateLevel, doc.RulesetId, classSkillIds);
                if (row is not null)
                {
                    purchases.Add(row);
                }
            }

            var classLevel = new CharacterClassLevelRow(
                characterLevel,
                doc.Id,
                doc.Name,
                nextTemplateLevel,
                template.Source,
                BuildTemplateDescription(template),
                purchases);
            classLevel.PropertyChanged += CharacterClassLevel_PropertyChanged;
            _classLevels.Add(classLevel);

            TargetLevelTextBox.Text = Math.Max(ReadIntOrNull(TargetLevelTextBox.Text) ?? 1, _classLevels.Count).ToString();
            RefreshCharacterBuild();
            RefreshClassDetailTabs();
            SetStatus($"Added {doc.Name} level {nextTemplateLevel} as character level {characterLevel}.");
            return true;
        }

        private static string BuildTemplateDescription(ClassTemplateListItem item)
        {
            var doc = item.Document;
            var lines = new List<string>
            {
                doc.Name,
                $"Source: {item.Source}",
                $"Ruleset: {doc.RulesetId}",
                $"Max levels: {doc.MaxClassLevels?.ToString() ?? "Unspecified"}",
                $"Alignment: {doc.Alignment ?? "Any"}",
                $"Starting gold: {doc.StartingGold ?? "Unspecified"}",
            };

            if (doc.NotesByRuleset is not null && doc.NotesByRuleset.TryGetValue(doc.RulesetId, out var notes) && !string.IsNullOrWhiteSpace(notes))
            {
                lines.Add("");
                lines.Add(notes);
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void RenumberClassLevels()
        {
            for (var i = 0; i < _classLevels.Count; i++)
            {
                _classLevels[i].CharacterLevel = i + 1;
                _classLevels[i].ReplacePurchases(_classLevels[i].Purchases
                    .Select(p => p.Document)
                    .Where(p => p is not null)
                    .Select(p => TryCreateTemplatePurchaseRow(i + 1, p!, _classLevels[i].TemplateLevel, GetRulesetIdForTemplate(_classLevels[i].TemplateId), GetClassSkillIdsForTemplate(_classLevels[i].TemplateId)))
                    .Where(p => p is not null)
                    .Cast<TemplatePurchaseRow>()
                    .ToList());
            }
        }

        private RulesetId? GetRulesetIdForTemplate(Guid templateId)
        {
            return _availableTemplates.FirstOrDefault(x => x.Document.Id == templateId)?.Document.RulesetId;
        }

        private ISet<string>? GetClassSkillIdsForTemplate(Guid templateId)
        {
            var template = _availableTemplates.FirstOrDefault(x => x.Document.Id == templateId)?.Document;
            return template is null ? null : GetTemplateClassSkillIds(template, _rulesetSkills);
        }

        private void CharacterClassLevel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RefreshCharacterBuild();
        }

        private void CharacterInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StatBlockTextBox is null)
            {
                return;
            }

            RefreshAbilityScoreAdjustmentsSummary();
            RefreshAbilityScoreModifiers();
            RefreshCharacterBuild();
        }

        private void BackgroundInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StatBlockTextBox is null)
            {
                return;
            }

            RefreshCharacterBuild();
        }

        private void Alignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatBlockTextBox is null)
            {
                return;
            }

            RefreshCharacterBuild();
        }

        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatBlockTextBox is null)
            {
                return;
            }

            RefreshSkillDerivedFields();
            RefreshCharacterBuild();
        }

        private void BackgroundInput_Checked(object sender, RoutedEventArgs e)
        {
            if (ReplaceCommonCheckBox?.IsChecked == true)
            {
                _knownLanguages.Remove("Common");
            }
            else
            {
                EnsureCommonLanguage();
            }

            RefreshCharacterBuild();
        }

        private void ChooseStartingLanguages_Click(object sender, RoutedEventArgs e)
        {
            if (_languages.Count == 0)
            {
                SetStatus("No language catalog is available.");
                return;
            }

            var keepAdding = true;
            while (keepAdding)
            {
                var dialog = new LanguageSelectionDialog(_languages, _knownLanguages)
                {
                    Owner = this,
                };

                if (dialog.ShowDialog() != true || dialog.SelectedLanguage is null)
                {
                    return;
                }

                var name = dialog.SelectedLanguage.Name;
                if (string.Equals(name, "Custom Language", StringComparison.OrdinalIgnoreCase))
                {
                    name = PromptForCustomLanguageName();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return;
                    }
                }

                AddKnownLanguage(name);
                keepAdding = dialog.AddAnother;
            }
        }

        private string? PromptForCustomLanguageName()
        {
            var dialog = new Window
            {
                Title = "Custom Language",
                Owner = this,
                Width = 360,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
            };

            var input = new TextBox { Margin = new Thickness(12), MinHeight = 28 };
            var ok = new Button { Content = "Add", Width = 86, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            var cancel = new Button { Content = "Cancel", Width = 86, IsCancel = true };
            ok.Click += (_, _) => dialog.DialogResult = true;

            var panel = new DockPanel();
            panel.Children.Add(new TextBlock { Text = "Language name", Margin = new Thickness(12, 12, 12, 0) });
            DockPanel.SetDock(input, Dock.Top);
            panel.Children.Add(input);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(12) };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Bottom);
            panel.Children.Add(buttons);
            dialog.Content = panel;

            return dialog.ShowDialog() == true ? input.Text.Trim() : null;
        }

        private void RemoveSelectedLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (KnownLanguagesListBox.SelectedItem is not string language)
            {
                SetStatus("Select a language to remove.");
                return;
            }

            if (string.Equals(language, "Common", StringComparison.OrdinalIgnoreCase) && ReplaceCommonCheckBox.IsChecked != true)
            {
                SetStatus("Common cannot be removed unless Replace Common is checked.");
                return;
            }

            _knownLanguages.Remove(language);
            RefreshCharacterBuild();
        }

        private void AddKnownLanguage(string language)
        {
            language = language.Trim();
            if (string.IsNullOrWhiteSpace(language)
                || _knownLanguages.Any(x => string.Equals(x, language, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _knownLanguages.Add(language);
            RefreshCharacterBuild();
            SetStatus($"Added language: {language}.");
        }

        private void AbilityScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingAbilityScoreDisplay)
            {
                return;
            }

            if (sender is TextBox textBox && ReadIntOrNull(textBox.Text) is int baseScore)
            {
                textBox.Tag = Math.Max(1, baseScore);
            }

            RefreshAbilityScoreModifiers();
            // CON modifier impacts HP total in the level log.
            RefreshHitPointTotalFromClassLevels();
            RefreshSkillDerivedFields();
            RefreshSkillPointsSummary();
            RefreshDerivedCombatAndSaves();
            RefreshCharacterBuild();
        }

        private void AbilityScoreAdjust_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string tag })
            {
                return;
            }

            var parts = tag.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var delta))
            {
                return;
            }

            var target = parts[0] switch
            {
                "Strength" => StrengthTextBox,
                "Dexterity" => DexterityTextBox,
                "Constitution" => ConstitutionTextBox,
                "Intelligence" => IntelligenceTextBox,
                "Wisdom" => WisdomTextBox,
                "Charisma" => CharismaTextBox,
                _ => null,
            };

            if (target is null)
            {
                return;
            }

            var current = ReadIntOrNull(target.Text) ?? 10;
            var baseScore = GetBaseAbilityScore(target);
            SetBaseAbilityScore(target, Math.Max(1, baseScore + delta));
            RefreshAbilityScoreModifiers();
            RefreshCharacterBuild();
        }

        private void AddAbilityScoreAdjustment_Click(object sender, RoutedEventArgs e)
        {
            if (AbilityScoreAdjustmentComboBox.SelectedItem is not string ability)
            {
                return;
            }

            var maxAdjustments = Math.Max(0, (ReadIntOrNull(TargetLevelTextBox?.Text) ?? 1) / 4);
            var usedAdjustments = _levelAbilityScoreAdjustments.Values.Sum();
            if (usedAdjustments >= maxAdjustments)
            {
                SetStatus($"No level-based ability score adjustments are available at level {ReadIntOrNull(TargetLevelTextBox?.Text) ?? 1}.");
                return;
            }

            _levelAbilityScoreAdjustments.TryGetValue(ability, out var current);
            _levelAbilityScoreAdjustments[ability] = current + 1;
            RefreshAbilityScoreAdjustmentsSummary();
            RefreshAbilityScoreModifiers();
            RefreshCharacterBuild();
        }

        private void RemoveAbilityScoreAdjustment_Click(object sender, RoutedEventArgs e)
        {
            if (AbilityScoreAdjustmentComboBox.SelectedItem is not string ability
                || !_levelAbilityScoreAdjustments.TryGetValue(ability, out var current)
                || current <= 0)
            {
                return;
            }

            if (current == 1)
            {
                _levelAbilityScoreAdjustments.Remove(ability);
            }
            else
            {
                _levelAbilityScoreAdjustments[ability] = current - 1;
            }

            RefreshAbilityScoreAdjustmentsSummary();
            RefreshAbilityScoreModifiers();
            RefreshCharacterBuild();
        }

        private void RefreshAbilityScoreAdjustmentsSummary()
        {
            if (AbilityScoreAdjustmentsSummaryText is null)
            {
                return;
            }

            var maxAdjustments = Math.Max(0, (ReadIntOrNull(TargetLevelTextBox?.Text) ?? 1) / 4);
            var usedAdjustments = _levelAbilityScoreAdjustments.Values.Sum();
            var details = _levelAbilityScoreAdjustments.Count == 0
                ? "None"
                : string.Join(", ", _levelAbilityScoreAdjustments.OrderBy(x => x.Key).Select(x => $"{x.Key} +{x.Value * 2}"));
            AbilityScoreAdjustmentsSummaryText.Text = $"Used {usedAdjustments}/{maxAdjustments}: {details}";
        }

        private void RefreshAbilityScoreModifiers()
        {
            if (StrengthTextBox is null
                || DexterityTextBox is null
                || ConstitutionTextBox is null
                || IntelligenceTextBox is null
                || WisdomTextBox is null
                || CharismaTextBox is null
                || StrengthModText is null
                || DexterityModText is null
                || ConstitutionModText is null
                || IntelligenceModText is null
                || WisdomModText is null
                || CharismaModText is null
                || StrengthAbilityLabel is null
                || DexterityAbilityLabel is null
                || ConstitutionAbilityLabel is null
                || IntelligenceAbilityLabel is null
                || WisdomAbilityLabel is null
                || CharismaAbilityLabel is null)
            {
                return;
            }

            var character = CreateAbilityScorePreviewCharacter();
            if (character is null)
            {
                StrengthModText.Text = FormatModifier(ReadIntOrNull(StrengthTextBox.Text));
                DexterityModText.Text = FormatModifier(ReadIntOrNull(DexterityTextBox.Text));
                ConstitutionModText.Text = FormatModifier(ReadIntOrNull(ConstitutionTextBox.Text));
                IntelligenceModText.Text = FormatModifier(ReadIntOrNull(IntelligenceTextBox.Text));
                WisdomModText.Text = FormatModifier(ReadIntOrNull(WisdomTextBox.Text));
                CharismaModText.Text = FormatModifier(ReadIntOrNull(CharismaTextBox.Text));
                return;
            }

            _updatingAbilityScoreDisplay = true;
            try
            {
                ApplyAbilityScoreUi(character, "Strength", "STR", StrengthAbilityLabel, StrengthTextBox, StrengthModText);
                ApplyAbilityScoreUi(character, "Dexterity", "DEX", DexterityAbilityLabel, DexterityTextBox, DexterityModText);
                ApplyAbilityScoreUi(character, "Constitution", "CON", ConstitutionAbilityLabel, ConstitutionTextBox, ConstitutionModText);
                ApplyAbilityScoreUi(character, "Intelligence", "INT", IntelligenceAbilityLabel, IntelligenceTextBox, IntelligenceModText);
                ApplyAbilityScoreUi(character, "Wisdom", "WIS", WisdomAbilityLabel, WisdomTextBox, WisdomModText);
                ApplyAbilityScoreUi(character, "Charisma", "CHA", CharismaAbilityLabel, CharismaTextBox, CharismaModText);
            }
            finally
            {
                _updatingAbilityScoreDisplay = false;
            }
        }

        private Character? CreateAbilityScorePreviewCharacter()
        {
            if (StrengthTextBox is null
                || DexterityTextBox is null
                || ConstitutionTextBox is null
                || IntelligenceTextBox is null
                || WisdomTextBox is null
                || CharismaTextBox is null)
            {
                return null;
            }

            var scores = new AbilityScores(
                GetBaseAbilityScore(StrengthTextBox),
                GetBaseAbilityScore(DexterityTextBox),
                GetBaseAbilityScore(ConstitutionTextBox),
                GetBaseAbilityScore(IntelligenceTextBox),
                GetBaseAbilityScore(WisdomTextBox),
                GetBaseAbilityScore(CharismaTextBox));

            var character = Character.CreateLevelOne(
                CharacterNameTextBox?.Text?.Trim() ?? "New Hero",
                scores,
                totalCp: 24,
                size: GetSelectedSize(),
                race: GetSelectedRaceDefinition(),
                raceAbilityCatalog: _raceAbilityCatalog);
            ApplyLevelAbilityScoreAdjustments(character, ReadIntOrNull(TargetLevelTextBox?.Text) ?? 1);
            return character;
        }

        private static void ApplyAbilityScoreUi(
            Character character,
            string ability,
            string abbreviation,
            FrameworkElement label,
            TextBox scoreBox,
            TextBlock modifierText)
        {
            var breakdown = character.GetAbilityScoreBreakdown(ability);
            if (scoreBox.Tag is null)
            {
                scoreBox.Tag = breakdown.BaseScore;
            }

            if (!string.Equals(scoreBox.Text, breakdown.Total.ToString(), StringComparison.Ordinal))
            {
                scoreBox.Text = breakdown.Total.ToString();
            }

            modifierText.Text = FormatSigned(breakdown.Modifier);

            var tooltip = FormatAbilityScoreTooltip(character, ability, abbreviation);
            label.ToolTip = tooltip;
            scoreBox.ToolTip = tooltip;
            modifierText.ToolTip = tooltip;
        }

        private static int GetBaseAbilityScore(TextBox textBox)
        {
            if (textBox.Tag is int tagged)
            {
                return Math.Max(1, tagged);
            }

            return Math.Max(1, ReadIntOrNull(textBox.Text) ?? 10);
        }

        private static void SetBaseAbilityScore(TextBox textBox, int score)
        {
            textBox.Tag = Math.Max(1, score);
        }

        private void SetAbilityScoreTextAndBase(TextBox textBox, int score)
        {
            _updatingAbilityScoreDisplay = true;
            try
            {
                SetBaseAbilityScore(textBox, score);
                textBox.Text = Math.Max(1, score).ToString();
            }
            finally
            {
                _updatingAbilityScoreDisplay = false;
            }
        }

        private static string FormatAbilityScoreTooltip(Character character, string ability, string abbreviation)
        {
            var breakdown = character.GetAbilityScoreBreakdown(ability);
            var lines = new List<string>
            {
                $"{abbreviation} {breakdown.Total}",
                $"{breakdown.BaseScore}: base score / roll",
            };

            lines.AddRange(breakdown.Contributions.Select(FormatAbilityScoreContributionTooltipLine));
            lines.Add($"Modifier: {FormatSigned(breakdown.Modifier)}");
            return string.Join(Environment.NewLine, lines);
        }

        private void RefreshDerivedCombatAndSaves()
        {
            if (BabText is null
                || MeleeBabText is null
                || RangedBabText is null
                || ArmorClassText is null
                || GrappleText is null
                || SpaceReachText is null
                || FortSaveText is null
                || RefSaveText is null
                || WillSaveText is null)
            {
                return;
            }

            var character = _lastCalculatedCharacter;
            var bab = character?.Warcraft ?? 0;
            var sizeProfile = CharacterSizeProfile.Get(character?.Size ?? GetSelectedSize());
            var fortBase = character?.SaveBonuses.TryGetValue(SaveType.Fortitude, out var fort) == true ? fort : 0;
            var refBase = character?.SaveBonuses.TryGetValue(SaveType.Reflex, out var reflex) == true ? reflex : 0;
            var willBase = character?.SaveBonuses.TryGetValue(SaveType.Will, out var will) == true ? will : 0;

            var scores = character?.AbilityScores
                ?? new AbilityScores(
                    ReadIntOrNull(StrengthTextBox?.Text) ?? 10,
                    ReadIntOrNull(DexterityTextBox?.Text) ?? 10,
                    ReadIntOrNull(ConstitutionTextBox?.Text) ?? 10,
                    ReadIntOrNull(IntelligenceTextBox?.Text) ?? 10,
                    ReadIntOrNull(WisdomTextBox?.Text) ?? 10,
                    ReadIntOrNull(CharismaTextBox?.Text) ?? 10);
            var strengthMod = AbilityScores.GetModifier(scores.Strength);
            var dexterityMod = AbilityScores.GetModifier(scores.Dexterity);
            var constitutionMod = AbilityScores.GetModifier(scores.Constitution);
            var wisdomMod = AbilityScores.GetModifier(scores.Wisdom);

            var sizeAttack = sizeProfile.AcAttackModifier;
            BabText.Text = FormatSigned(bab + sizeAttack);
            MeleeBabText.Text = FormatSigned(bab + strengthMod + sizeAttack);
            RangedBabText.Text = FormatSigned(bab + dexterityMod + sizeAttack);
            ArmorClassText.Text = (10 + dexterityMod + sizeProfile.AcAttackModifier).ToString();
            GrappleText.Text = FormatSigned(bab + strengthMod + sizeProfile.GrappleModifier);
            SpaceReachText.Text = $"{sizeProfile.Space} / {sizeProfile.Reach}";
            FortSaveText.Text = FormatSigned(fortBase + constitutionMod);
            RefSaveText.Text = FormatSigned(refBase + dexterityMod);
            WillSaveText.Text = FormatSigned(willBase + wisdomMod);

            BabText.ToolTip = BuildTotalBreakdown("Attack Bonus", ("Warcraft / BAB", bab), ($"{sizeProfile.Size} size", sizeAttack));
            MeleeBabText.ToolTip = BuildTotalBreakdown("Melee Attack", ("Warcraft / BAB", bab), ("Strength", strengthMod), ($"{sizeProfile.Size} size", sizeAttack));
            RangedBabText.ToolTip = BuildTotalBreakdown("Ranged Attack", ("Warcraft / BAB", bab), ("Dexterity", dexterityMod), ($"{sizeProfile.Size} size", sizeAttack));
            ArmorClassText.ToolTip = BuildTotalBreakdown("Armor Class", ("Base", 10), ("Dexterity", dexterityMod), ($"{sizeProfile.Size} size", sizeProfile.AcAttackModifier));
            GrappleText.ToolTip = BuildTotalBreakdown("Grapple", ("Warcraft / BAB", bab), ("Strength", strengthMod), ($"{sizeProfile.Size} size", sizeProfile.GrappleModifier));
            SpaceReachText.ToolTip = $"Dimension: {sizeProfile.Dimension}{Environment.NewLine}Weight: {sizeProfile.Weight}";
            FortSaveText.ToolTip = BuildTotalBreakdown("Fortitude Save", ("Base save", fortBase), ("Constitution", constitutionMod));
            RefSaveText.ToolTip = BuildTotalBreakdown("Reflex Save", ("Base save", refBase), ("Dexterity", dexterityMod));
            WillSaveText.ToolTip = BuildTotalBreakdown("Will Save", ("Base save", willBase), ("Wisdom", wisdomMod));
        }

        private static string BuildTotalBreakdown(string label, params (string Name, int Value)[] components)
        {
            var total = components.Sum(x => x.Value);
            var lines = new List<string>
            {
                $"{label}: {FormatSigned(total)}",
            };

            lines.AddRange(components.Select(x => $"{x.Name}: {FormatSigned(x.Value)}"));
            lines.Add($"Total: {FormatSigned(total)}");
            return string.Join(Environment.NewLine, lines);
        }

        private static int? ReadIntOrNull(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return int.TryParse(text.Trim(), out var value) ? value : null;
        }

        private static string FormatModifier(int? score)
        {
            if (score is null)
            {
                return "";
            }

            var modifier = AbilityScores.GetModifier(score.Value);
            return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
        }

        private void ApplyTemplate_Click(object sender, RoutedEventArgs e)
        {
            RefreshCharacterBuild(showStatus: true);
        }

        private void RefreshCharacterBuild(bool showStatus = false)
        {
            if (StatBlockTextBox is null)
            {
                return;
            }

            if (!TryReadCharacterInputs(out var inputs, quiet: true))
            {
                StatBlockTextBox.Text = "Complete the character fields to calculate a statblock.";
                return;
            }

            if (_classLevels.Count == 0)
            {
                var selectedRace = GetSelectedRaceDefinition();
                _lastCalculatedCharacter = Character.CreateLevelOne(
                    inputs.Name,
                    inputs.AbilityScores,
                    totalCp: 24,
                    size: inputs.Size,
                    race: selectedRace,
                    raceAbilityCatalog: _raceAbilityCatalog);
                ApplyLevelAbilityScoreAdjustments(_lastCalculatedCharacter, inputs.TargetLevel);
                DiagnosticsListBox.ItemsSource = null;
                ClassesSummaryText.Text = "No class levels added.";
                StatBlockTextBox.Text = _statBlockFormatter.BuildEmpty(
                    inputs,
                    _lastCalculatedCharacter,
                    PlayerNameTextBox.Text.Trim(),
                    RulesetProfile.Get(_heroSettings.RulesetId).DisplayName,
                    _selectedFeats,
                    _skillRows);
                RefreshAbilityScoreModifiers();
                RefreshDerivedCombatAndSaves();
                RefreshClassDetailTabs();
                RefreshLevelDependentSummaries();
                RefreshSkillDerivedFields();
                return;
            }

            try
            {
                var selectedRace = GetSelectedRaceDefinition();
                var build = _buildAssembler.CreateBuild(
                    inputs,
                    selectedRace,
                    _raceAbilityCatalog,
                    _levelAbilityScoreAdjustments,
                    _classLevels,
                    _selectedFeats,
                    _skillRows);

                var rulesConfig = _buildReplayFactory.CreateRulesConfig(_heroSettings);
                var result = _buildReplayFactory.Replay(build, rulesConfig);

                StatBlockTextBox.Text = _statBlockFormatter.Build(result, inputs, _currentTemplate, _classLevels, _selectedFeats, _skillRows);
                _lastCalculatedCharacter = result.Character;
                DiagnosticsListBox.ItemsSource = result.Diagnostics
                    .Select(x => $"[{x.Severity}] {x.Code}: {x.Message}")
                    .ToList();

                RefreshClassesSummary();
                RefreshAbilityScoreModifiers();
                RefreshDerivedCombatAndSaves();
                RefreshClassDetailTabs();
                RefreshLevelDependentSummaries();
                RefreshSkillDerivedFields();

                if (showStatus)
                {
                    SetStatus(result.HasErrors
                        ? "Character recalculated with diagnostics."
                        : "Character recalculated.");
                }
            }
            catch (Exception ex)
            {
                StatBlockTextBox.Text = $"Could not calculate character: {ex.Message}";
                if (showStatus)
                {
                    SetStatus($"Could not calculate character: {ex.Message}");
                }
            }
        }

        private void RefreshClassesSummary()
        {
            if (ClassesSummaryText is null)
            {
                return;
            }

            var classes = _classLevels
                .GroupBy(x => x.TemplateName)
                .Select(x => $"{x.Key} {x.Count()}")
                .ToList();
            ClassesSummaryText.Text = classes.Count == 0
                ? "No class levels added."
                : $"Level {_classLevels.Count}: {string.Join(", ", classes)}";
        }

        private void RefreshClassDetailTabs()
        {
            if (CharacterBuildTabs is null)
            {
                return;
            }

            var groupedLevels = _classLevels
                .GroupBy(x => x.TemplateId)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var removedId in _classDetailTabs.Keys.Where(id => !groupedLevels.ContainsKey(id)).ToList())
            {
                CharacterBuildTabs.Items.Remove(_classDetailTabs[removedId]);
                _classDetailTabs.Remove(removedId);
            }

            var insertAt = GetClassDetailTabInsertIndex();
            foreach (var group in groupedLevels.Values.OrderBy(x => x.Min(row => row.CharacterLevel)))
            {
                var templateId = group[0].TemplateId;
                if (!_classDetailTabs.TryGetValue(templateId, out var tab))
                {
                    tab = new TabItem { Tag = templateId };
                    _classDetailTabs.Add(templateId, tab);
                    CharacterBuildTabs.Items.Insert(Math.Min(insertAt, CharacterBuildTabs.Items.Count), tab);
                    insertAt++;
                }

                tab.Header = group[0].TemplateName;
                tab.Content = BuildClassDetailTabContent(group);
            }
        }

        private int GetClassDetailTabInsertIndex()
        {
            var classesIndex = 0;
            for (var i = 0; i < CharacterBuildTabs.Items.Count; i++)
            {
                if (CharacterBuildTabs.Items[i] is TabItem { Header: string header }
                    && string.Equals(header, "Classes", StringComparison.OrdinalIgnoreCase))
                {
                    classesIndex = i;
                    break;
                }
            }

            var index = classesIndex + 1;
            while (index < CharacterBuildTabs.Items.Count
                && CharacterBuildTabs.Items[index] is TabItem { Tag: Guid })
            {
                index++;
            }

            return index;
        }

        private UIElement BuildClassDetailTabContent(IReadOnlyList<CharacterClassLevelRow> levels)
        {
            var first = levels[0];
            var sb = new StringBuilder();
            sb.AppendLine(first.TemplateName);
            sb.AppendLine($"Source: {first.Source}");
            sb.AppendLine($"Levels taken: {levels.Count}");
            sb.AppendLine($"Character levels: {string.Join(", ", levels.Select(x => x.CharacterLevel))}");
            if (!string.IsNullOrWhiteSpace(first.HitDieLabel))
            {
                sb.AppendLine($"Hit die: {first.HitDieLabel.Trim('(', ')')}");
            }

            sb.AppendLine();
            sb.AppendLine("Class Details");
            sb.AppendLine(first.ClassDescription);
            sb.AppendLine();
            sb.AppendLine("Level Features");
            foreach (var level in levels.OrderBy(x => x.CharacterLevel))
            {
                sb.AppendLine($"Character level {level.CharacterLevel} / {first.TemplateName} {level.TemplateLevel}");
                foreach (var purchase in level.Purchases)
                {
                    sb.AppendLine($"  - {purchase.Description}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Configuration");
            var bonusFeats = levels.SelectMany(x => x.Purchases)
                .Where(x => string.Equals(x.Kind, "Bonus Feat", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (bonusFeats.Count > 0)
            {
                sb.AppendLine($"Bonus feats available from this class: {bonusFeats.Count}");
                sb.AppendLine("Feat selection controls will live here.");
            }
            else if (levels.SelectMany(x => x.Purchases).Any(x => x.Kind.Contains("Magic", StringComparison.OrdinalIgnoreCase)))
            {
                sb.AppendLine("Spell preparation and spell selection controls will live here.");
            }
            else
            {
                sb.AppendLine("No class-specific configuration is available yet.");
            }

            return new TextBox
            {
                Text = sb.ToString(),
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                IsReadOnly = true,
                Margin = new Thickness(12),
            };
        }

        private bool ValidateTemplateLevelForCharacterLevel(int characterLevel)
        {
            var maxConfigurableLevel = GetMaxContiguousTemplateLevel();
            if (maxConfigurableLevel >= characterLevel)
            {
                return true;
            }

            var highestSavedLevel = _templateLevels.Count == 0
                ? "none"
                : _templateLevels.Max(x => x.Level).ToString();

            SetStatus($"Define contiguous template levels through character level {characterLevel} before applying the template. Highest saved template level: {highestSavedLevel}.");
            MessageBox.Show(
                $"The character is being configured at level {characterLevel}, but the current template only has contiguous definitions through level {maxConfigurableLevel}. Define template level {maxConfigurableLevel + 1} before continuing.",
                "Template Level Required",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return false;
        }

        private void Experience_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshLevelDependentSummaries();
        }

        private void PathfinderProgression_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PathfinderProgressionComboBox.SelectedItem is PathfinderXpProgression progression)
            {
                _pathfinderProgression = progression;
                RefreshLevelDependentSummaries();
            }
        }

        private void RefreshLevelDependentSummaries()
        {
            RefreshHitPointTotalFromClassLevels();
            RefreshSkillPointsSummary();
            RefreshFeatsSummary();
        }

        private int GetEffectiveCharacterLevel()
        {
            if (_classLevels.Count > 0)
            {
                return _classLevels.Count;
            }

            var xp = Math.Max(0, ReadIntOrNull(ExperienceTextBox?.Text) ?? 0);
            var requestedLevel = ComputeLevelFromXp(xp, GetSelectedLevelTable());
            var maxConfigurableLevel = GetMaxContiguousTemplateLevel();
            return maxConfigurableLevel == 0
                ? Math.Max(1, requestedLevel)
                : Math.Min(requestedLevel, maxConfigurableLevel);
        }

        private void RefreshHitPointTotalFromClassLevels()
        {
            if (_classLevels.Count == 0 || HitPointsTextBox is null || ConstitutionTextBox is null)
            {
                return;
            }

            var baseHp = _classLevels.Sum(row => ReadIntOrNull(row.HpNote) ?? 0);
            var conMod = AbilityScores.GetModifier(ReadIntOrNull(ConstitutionTextBox.Text) ?? 10);
            HitPointsTextBox.Text = (baseHp + (_classLevels.Count * conMod)).ToString();
        }

        private int GetMaxContiguousTemplateLevel()
        {
            var savedLevels = _templateLevels
                .Select(x => x.Level)
                .Where(x => x > 0)
                .ToHashSet();

            var level = 0;
            while (savedLevels.Contains(level + 1))
            {
                level++;
            }

            return level;
        }

        private IReadOnlyList<LevelProgressionLevelDocument> GetSelectedLevelTable()
        {
            var empty = (IReadOnlyList<LevelProgressionLevelDocument>)Array.Empty<LevelProgressionLevelDocument>();
            if (_levelProgressionCatalog is null)
            {
                return empty;
            }

            var ruleset = _levelProgressionCatalog.Rulesets.FirstOrDefault(x => x.RulesetId == _currentTemplate.RulesetId);
            if (ruleset is null)
            {
                return empty;
            }

            LevelProgressionTableDocument? table = null;
            if (_currentTemplate.RulesetId == RulesetId.Pathfinder1E)
            {
                table = ruleset.Tables.FirstOrDefault(x => x.Progression == _pathfinderProgression);
            }
            else
            {
                table = ruleset.Tables.FirstOrDefault();
            }

            return (table?.Levels ?? new List<LevelProgressionLevelDocument>()).OrderBy(x => x.Level).ToList();
        }

        private static int ComputeLevelFromXp(int xpTotal, IReadOnlyList<LevelProgressionLevelDocument> table)
        {
            if (table is null || table.Count == 0)
            {
                return 1;
            }

            var level = 1;
            foreach (var row in table.Where(x => x != null).OrderBy(x => x.Level))
            {
                var required = row.XpTotal.GetValueOrDefault(row.Level == 1 ? 0 : int.MaxValue);
                if (required <= xpTotal)
                {
                    level = Math.Max(level, row.Level);
                }
            }

            return level;
        }

        private void RefreshSkillPointsSummary()
        {
            if (SkillPointsSummaryText is null)
            {
                return;
            }

            var level = GetEffectiveCharacterLevel();
            var baseSp = GetTemplateSkillPointsThroughLevel(level);
            var intScore = ReadIntOrNull(IntelligenceTextBox?.Text) ?? 10;
            var intMod = AbilityScores.GetModifier(intScore);
            var bonus = GetIntSkillPointBonusThroughLevel(intMod, level);
            var racialBonus = GetCurrentBonusSkillPoints(level);
            var total = baseSp + bonus + racialBonus;
            var spent = _skillRows.Sum(x => x.SkillPointsSpent);
            var remaining = total - spent;

            var intText = intMod >= 0 ? $"+{intMod}" : intMod.ToString();
            SkillPointsSummaryText.Text = $"Skill Points: {spent} spent / {total} total ({remaining} remaining). INT mod {intText}; base from template {baseSp}, INT bonus {bonus}, racial bonus {racialBonus} (through level {level}).";
        }

        private void RefreshFeatsSummary()
        {
            if (FeatsSummaryText is null)
            {
                return;
            }

            var level = GetEffectiveCharacterLevel();
            var featsFromLevels = GetFeatsGrantedByLevel(level);
            var bonusFeats = GetCurrentBonusFeats();
            var selectedCount = _selectedFeats.Sum(x => x.Count);
            FeatsSummaryText.Text = $"Feats: {selectedCount} selected / {featsFromLevels + bonusFeats} available (by level {level}; bonus feats {bonusFeats}).";
        }

        private int GetFeatsGrantedByLevel(int level)
        {
            var table = GetSelectedLevelTable();
            if (table.Count == 0)
            {
                // Fallback: 1 feat at level 1, +1 every 3 levels (D&D-ish).
                return 1 + (Math.Max(1, level) - 1) / 3;
            }

            return table.Where(x => x.Level >= 1 && x.Level <= level).Count(x => x.GrantsFeat);
        }

        private int GetRemainingSkillPoints()
        {
            var level = GetEffectiveCharacterLevel();
            var baseSp = GetTemplateSkillPointsThroughLevel(level);
            var intScore = ReadIntOrNull(IntelligenceTextBox?.Text) ?? 10;
            var intMod = AbilityScores.GetModifier(intScore);
            var bonus = GetIntSkillPointBonusThroughLevel(intMod, level);
            var total = baseSp + bonus + GetCurrentBonusSkillPoints(level);
            var spent = _skillRows.Sum(x => x.SkillPointsSpent);
            return total - spent;
        }

        private int GetCurrentBonusFeats()
        {
            return (_lastCalculatedCharacter ?? CreateAbilityScorePreviewCharacter())?.BonusFeats ?? 0;
        }

        private int GetCurrentBonusSkillPoints(int level)
        {
            var character = (_lastCalculatedCharacter ?? CreateAbilityScorePreviewCharacter())?.Clone();
            if (character is null)
            {
                return 0;
            }

            var bonus = character.BonusSkillPoints;
            while (character.Level < level)
            {
                character.AdvanceLevel(0);
                bonus = character.BonusSkillPoints;
            }

            return bonus;
        }

        private int GetTemplateSkillPointsThroughLevel(int level)
        {
            if (_classLevels.Count > 0)
            {
                return _classLevels
                    .Where(l => l.CharacterLevel >= 1 && l.CharacterLevel <= level)
                    .SelectMany(l => l.Purchases)
                    .Where(p => p.Document?.Kind == TemplatePurchaseKind.SkillRanks
                                && IsUnassignedSkillPointPurchase(p.Document))
                    .Sum(p => p.CostCp);
            }

            // For now, treat any template skill purchase with name "Unassigned Skill Points" as points granted.
            var points = _templateLevels
                .Where(l => l.Level >= 1 && l.Level <= level)
                .SelectMany(l => l.Purchases)
                .Where(p => p.Document?.Kind == TemplatePurchaseKind.SkillRanks
                            && string.Equals(p.Document.SkillName, "Unassigned Skill Points", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.Document!.SkillRanks.GetValueOrDefault(0));

            // Include current draft if editing a level within the range (draft replaces that saved level in totals elsewhere).
            if (_editingLevel >= 1 && _editingLevel <= level)
            {
                // Remove saved purchases from editing level and add draft purchases instead.
                var savedEditing = _templateLevels.FirstOrDefault(l => l.Level == _editingLevel)?.Purchases ?? Array.Empty<TemplatePurchaseRow>();
                var savedPoints = savedEditing
                    .Where(p => p.Document?.Kind == TemplatePurchaseKind.SkillRanks
                                && string.Equals(p.Document.SkillName, "Unassigned Skill Points", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Document!.SkillRanks.GetValueOrDefault(0));

                var draftPoints = _templatePurchases
                    .Where(p => p.Document?.Kind == TemplatePurchaseKind.SkillRanks
                                && string.Equals(p.Document.SkillName, "Unassigned Skill Points", StringComparison.OrdinalIgnoreCase))
                    .Sum(p => p.Document!.SkillRanks.GetValueOrDefault(0));

                points = points - savedPoints + draftPoints;
            }

            return points;
        }

        private int GetIntSkillPointBonusThroughLevel(int intModifier, int level)
        {
            if (_currentTemplate.RulesetId == RulesetId.Pathfinder1E)
            {
                return intModifier * Math.Max(0, level);
            }

            if (level < 1)
            {
                return 0;
            }

            // Negative INT mod reduces skill points; we allow it to reduce total.
            var perLevel = intModifier;
            var bonus = perLevel * Math.Max(0, level - 1);
            bonus += perLevel * 4; // level 1 multiplier
            return bonus;
        }

        private void ApplyLevelAbilityScoreAdjustments(Character character, int targetLevel)
        {
            var maxAdjustments = targetLevel / 4;
            var usedAdjustments = 0;
            foreach (var entry in _levelAbilityScoreAdjustments.Where(x => x.Value > 0))
            {
                var grants = Math.Min(entry.Value, Math.Max(0, maxAdjustments - usedAdjustments));
                if (grants <= 0)
                {
                    break;
                }

                character.AddAbilityScoreContribution(
                    entry.Key,
                    grants * 2,
                    BonusType.Untyped,
                    grants == 1
                        ? "Level 4 ability score adjustment"
                        : $"{grants} level-based ability score adjustments");
                usedAdjustments += grants;
            }
        }

        private static string FormatBonusType(BonusType type)
        {
            return type == BonusType.Untyped ? "untyped bonus" : $"{type.ToString().ToLowerInvariant()} bonus";
        }

        private static string FormatAbilityScoreContributionTooltipLine(AbilityScoreContribution contribution)
        {
            return $"{FormatSigned(contribution.Amount)}: {FormatContributionKind(contribution)} from {contribution.Source}";
        }

        private static string FormatContributionKind(AbilityScoreContribution contribution)
        {
            if (contribution.Amount < 0)
            {
                return contribution.Type == BonusType.Untyped
                    ? "untyped penalty"
                    : $"{contribution.Type.ToString().ToLowerInvariant()} penalty";
            }

            return FormatBonusType(contribution.Type);
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private bool TryReadCharacterInputs(out CharacterInputs inputs, bool quiet = false)
        {
            inputs = default!;
            var name = CharacterNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                if (!quiet)
                {
                    SetStatus("Character name is required.");
                }

                return false;
            }

            if (!TryReadPositiveInt(TargetLevelTextBox, "Target level", out var targetLevel)
                || !TryReadNonNegativeInt(HitPointsTextBox, "Hit points", out var hitPoints)
                || !TryReadNonNegativeInt(StartingGoldTextBox, "Starting gold", out var startingGold)
                || !TryReadAbilityScore(StrengthTextBox, "Strength", out _)
                || !TryReadAbilityScore(DexterityTextBox, "Dexterity", out _)
                || !TryReadAbilityScore(ConstitutionTextBox, "Constitution", out _)
                || !TryReadAbilityScore(IntelligenceTextBox, "Intelligence", out _)
                || !TryReadAbilityScore(WisdomTextBox, "Wisdom", out _)
                || !TryReadAbilityScore(CharismaTextBox, "Charisma", out _))
            {
                return false;
            }

            inputs = new CharacterInputs(
                name,
                targetLevel,
                hitPoints,
                startingGold,
                new AbilityScores(
                    GetBaseAbilityScore(StrengthTextBox),
                    GetBaseAbilityScore(DexterityTextBox),
                    GetBaseAbilityScore(ConstitutionTextBox),
                    GetBaseAbilityScore(IntelligenceTextBox),
                    GetBaseAbilityScore(WisdomTextBox),
                    GetBaseAbilityScore(CharismaTextBox)),
                GetSelectedRaceName(),
                GetSelectedSize(),
                GetSelectedAlignmentName(),
                DeityTextBox.Text.Trim(),
                _knownLanguages.ToList());
            return true;
        }

        private bool TryGetPurchaseLevel(out int level)
        {
            level = _editingLevel;
            return true;
        }

        private bool TryReadAbilityScore(TextBox textBox, string label, out int value)
        {
            if (!int.TryParse(textBox.Text, out value) || value < 1)
            {
                SetStatus($"{label} must be a number greater than 0.");
                return false;
            }

            return true;
        }

        private bool TryReadPositiveInt(TextBox textBox, string label, out int value)
        {
            if (!int.TryParse(textBox.Text, out value) || value < 1)
            {
                SetStatus($"{label} must be a number greater than 0.");
                return false;
            }

            return true;
        }

        private bool TryReadRequiredText(TextBox textBox, string label, out string value)
        {
            value = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                SetStatus($"{label} is required.");
                return false;
            }

            return true;
        }

        private bool TryReadNonNegativeInt(TextBox textBox, string label, out int value)
        {
            if (!int.TryParse(textBox.Text, out value) || value < 0)
            {
                SetStatus($"{label} must be a number greater than or equal to 0.");
                return false;
            }

            return true;
        }

        private void SetStatus(string message)
        {
            StatusText.Text = message;
        }

        private void RefreshTemplateCpSummary()
        {
            if (TemplateCpSummaryText is null)
            {
                return;
            }

            var progression = new EclipseCpProgression();
            var draftSpent = _templatePurchases.Sum(x => x.CostCp);
            var totalSpent = GetTotalTemplateSpentCp();
            var availableAtEditing = progression.GetTotalCpAtLevel(Math.Max(1, _editingLevel));
            var maxLevel = Math.Max(
                _editingLevel,
                _templateLevels.Count == 0 ? 1 : _templateLevels.Max(x => x.Level));
            var availableAtMax = progression.GetTotalCpAtLevel(Math.Max(1, maxLevel));

            TemplateCpSummaryText.Text = $"Draft L{_editingLevel}: {draftSpent} CP; Total: {totalSpent} / {availableAtMax} CP (through level {maxLevel}).";
        }

        private TemplateCostBreakdown GetTemplateCostBreakdown()
        {
            // Use same "draft replaces saved for current editing level" behavior as GetTotalTemplateSpentCp.
            var rows = _templateLevels
                .Where(x => x.Level != _editingLevel)
                .SelectMany(x => x.Purchases)
                .Concat(_templatePurchases)
                .ToList();

            var hitDie = 0;
            var saves = 0;
            var warcraft = 0;
            var magicLevels = 0;
            var baseCasterLevel = 0;
            var abilities = 0;
            var proficiencies = 0;
            var skills = 0;
            foreach (var row in rows)
            {
                var document = row.Document;
                if (document is null)
                {
                    continue;
                }

                switch (document.Kind)
                {
                    case TemplatePurchaseKind.HitDie:
                        hitDie += row.CostCp;
                        break;
                    case TemplatePurchaseKind.SaveBonus:
                        saves += row.CostCp;
                        break;
                    case TemplatePurchaseKind.Warcraft:
                        warcraft += row.CostCp;
                        break;
                    case TemplatePurchaseKind.MagicLevels:
                        magicLevels += row.CostCp;
                        break;
                    case TemplatePurchaseKind.BaseCasterLevel:
                        baseCasterLevel += row.CostCp;
                        break;
                    case TemplatePurchaseKind.Ability:
                    case TemplatePurchaseKind.BonusFeat:
                        abilities += row.CostCp;
                        break;
                    case TemplatePurchaseKind.Proficiencies:
                        proficiencies += row.CostCp;
                        break;
                    case TemplatePurchaseKind.SkillRanks:
                        skills += row.CostCp;
                        break;
                }
            }

            return new TemplateCostBreakdown(hitDie, saves, warcraft, magicLevels, baseCasterLevel, abilities, proficiencies, skills);
        }

        private int GetTotalTemplateSpentCp()
        {
            var otherLevels = _templateLevels
                .Where(x => x.Level != _editingLevel)
                .SelectMany(x => x.Purchases)
                .Sum(x => x.CostCp);

            var draft = _templatePurchases.Sum(x => x.CostCp);
            return otherLevels + draft;
        }

        private readonly struct TemplateCostBreakdown
        {
            public TemplateCostBreakdown(int hitDieCp, int savesCp, int warcraftCp, int magicLevelsCp, int baseCasterLevelCp, int abilitiesCp, int proficienciesCp, int skillsCp)
            {
                HitDieCp = hitDieCp;
                SavesCp = savesCp;
                WarcraftCp = warcraftCp;
                MagicLevelsCp = magicLevelsCp;
                BaseCasterLevelCp = baseCasterLevelCp;
                AbilitiesCp = abilitiesCp;
                ProficienciesCp = proficienciesCp;
                SkillsCp = skillsCp;
            }

            public int HitDieCp { get; }
            public int SavesCp { get; }
            public int WarcraftCp { get; }
            public int MagicLevelsCp { get; }
            public int BaseCasterLevelCp { get; }
            public int AbilitiesCp { get; }
            public int ProficienciesCp { get; }
            public int SkillsCp { get; }
        }

        private int GetPurchaseCostCp(TemplatePurchaseDocument? document)
        {
            if (document is null)
            {
                return 0;
            }

            switch (document.Kind)
            {
                case TemplatePurchaseKind.HitDie:
                    if (document.HitDie is null)
                    {
                        return 0;
                    }

                    var raw = ((int)document.HitDie.Value) - 4;
                    return raw < 0 ? 0 : raw;

                case TemplatePurchaseKind.Warcraft:
                    return 6 * document.Warcraft.GetValueOrDefault(0);

                case TemplatePurchaseKind.SaveBonus:
                    return 3 * document.SaveBonus.GetValueOrDefault(0);

                case TemplatePurchaseKind.SkillRanks:
                    return document.SkillRanks.GetValueOrDefault(0);

                case TemplatePurchaseKind.BonusFeat:
                    return 6 * document.BonusFeats.GetValueOrDefault(1);

                case TemplatePurchaseKind.BaseCasterLevel:
                    return 6 * document.BaseCasterLevels.GetValueOrDefault(0);

                case TemplatePurchaseKind.MagicLevels:
                    if (document.MagicProgression is null)
                    {
                        return 0;
                    }

                    return MagicProgressionCatalog.Get(document.MagicProgression.Value).ChartCostPerLevelCp
                        * document.MagicLevels.GetValueOrDefault(0);

                case TemplatePurchaseKind.Ability:
                    if (_ruleset is null || string.IsNullOrWhiteSpace(document.AbilityId))
                    {
                        return 0;
                    }

                    var ability = _ruleset.Abilities.GetById(document.AbilityId.Trim());
                    var optionIds = (document.AbilityOptionIds ?? new List<string>())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                    var selectedOptions = optionIds
                        .Select(id => ability.GetOptionById(id.Trim()))
                        .ToList();
                    return ability.CostCp + selectedOptions.Sum(x => x.CostCp);

                case TemplatePurchaseKind.Proficiencies:
                    return document.ProficiencyCostCp.GetValueOrDefault(0);

                default:
                    return 0;
            }
        }
    }

    public sealed class AbilityListItem
    {
        public AbilityListItem(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public AbilityDefinition Definition { get; }
        public string DisplayName => $"{Definition.Name} ({Definition.CostCp} CP)";
    }

    public sealed class AbilityOptionSelection : INotifyPropertyChanged
    {
        private bool _isSelected;

        public AbilityOptionSelection(AbilityOptionDefinition definition)
        {
            Definition = definition;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public AbilityOptionDefinition Definition { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Definition.CostExpression))
                {
                    return $"{Definition.Name} ({Definition.CostExpression})";
                }

                return Definition.CostCp == 0
                    ? Definition.Name
                    : $"{Definition.Name} (+{Definition.CostCp} CP)";
            }
        }
    }

    public sealed class TemplatePurchaseRow
    {
        public TemplatePurchaseRow(int level, string kind, string description, Func<IPurchase> createPurchase, TemplatePurchaseDocument? document, int costCp)
        {
            Level = level;
            Kind = kind;
            Description = description;
            CreatePurchase = createPurchase;
            Document = document;
            CostCp = costCp;
        }

        public int Level { get; }
        public string Kind { get; }
        public int CostCp { get; }
        public string Description { get; }
        public Func<IPurchase> CreatePurchase { get; }
        public TemplatePurchaseDocument? Document { get; }
    }

    public sealed class TemplateSegmentRow : INotifyPropertyChanged
    {
        private int _targetLevel;

        public TemplateSegmentRow(string templateName, int classLevel, int targetLevel, IReadOnlyList<TemplatePurchaseRow> purchases)
        {
            TemplateName = templateName ?? throw new ArgumentNullException(nameof(templateName));
            ClassLevel = classLevel;
            _targetLevel = targetLevel;
            Purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public string TemplateName { get; }
        public int ClassLevel { get; }
        public IReadOnlyList<TemplatePurchaseRow> Purchases { get; }

        public int TargetLevel
        {
            get => _targetLevel;
            set
            {
                if (_targetLevel == value)
                {
                    return;
                }

                _targetLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetLevel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
            }
        }

        public string DisplayName => $"{TemplateName} L{ClassLevel} -> Character L{TargetLevel} ({Purchases.Count} purchase{(Purchases.Count == 1 ? "" : "s")})";
    }

    public sealed class TemplateLevelRow : INotifyPropertyChanged
    {
        private List<TemplatePurchaseRow> _purchases;

        public TemplateLevelRow(int level, IEnumerable<TemplatePurchaseRow> purchases)
        {
            Level = level;
            _purchases = purchases.ToList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public int Level { get; }
        public IReadOnlyList<TemplatePurchaseRow> Purchases => _purchases.AsReadOnly();
        public string DisplayName => $"Level {Level} ({_purchases.Count} purchase{(_purchases.Count == 1 ? "" : "s")})";

        public void ReplacePurchases(IEnumerable<TemplatePurchaseRow> purchases)
        {
            _purchases = purchases.ToList();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Purchases)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
        }
    }

    public sealed class CharacterInputs
    {
        public CharacterInputs(
            string name,
            int targetLevel,
            int hitPoints,
            int startingGold,
            AbilityScores abilityScores,
            string race,
            CharacterSize size,
            string alignment,
            string deity,
            IReadOnlyList<string> languages)
        {
            Name = name;
            TargetLevel = targetLevel;
            HitPoints = hitPoints;
            StartingGold = startingGold;
            AbilityScores = abilityScores;
            Race = string.IsNullOrWhiteSpace(race) ? "Choose Race" : race;
            Size = size;
            Alignment = string.IsNullOrWhiteSpace(alignment) ? "Choose Alignment" : alignment;
            Deity = deity ?? "";
            Languages = languages ?? Array.Empty<string>();
        }

        public string Name { get; }
        public int TargetLevel { get; }
        public int HitPoints { get; }
        public int StartingGold { get; }
        public AbilityScores AbilityScores { get; }
        public string Race { get; }
        public CharacterSize Size { get; }
        public string Alignment { get; }
        public string Deity { get; }
        public IReadOnlyList<string> Languages { get; }
    }

    public sealed class CharacterClassLevelRow : INotifyPropertyChanged
    {
        private int _characterLevel;
        private string _hpNote = "";
        private string _favoredBonus = "+1 Hit Point";
        private IReadOnlyList<TemplatePurchaseRow> _purchases;

        public CharacterClassLevelRow(
            int characterLevel,
            Guid templateId,
            string templateName,
            int templateLevel,
            string source,
            string classDescription,
            IReadOnlyList<TemplatePurchaseRow> purchases)
        {
            _characterLevel = characterLevel;
            TemplateId = templateId;
            TemplateName = templateName ?? throw new ArgumentNullException(nameof(templateName));
            TemplateLevel = templateLevel;
            Source = source ?? "";
            ClassDescription = string.IsNullOrWhiteSpace(classDescription) ? TemplateName : classDescription;
            _purchases = purchases ?? Array.Empty<TemplatePurchaseRow>();
            if (IsFirstLevel && MaxHitPoints > 0)
            {
                _hpNote = MaxHitPoints.ToString();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public Guid TemplateId { get; }
        public string TemplateName { get; }
        public int TemplateLevel { get; }
        public string Source { get; }
        public string ClassDescription { get; }
        public IReadOnlyList<TemplatePurchaseRow> Purchases => _purchases;
        public string DisplayName => $"{TemplateName} {TemplateLevel}";
        public bool IsFirstLevel => CharacterLevel == 1;
        public int MaxHitPoints
        {
            get
            {
                var hitDie = _purchases
                    .Select(x => x.Document?.HitDie)
                    .FirstOrDefault(x => x is not null);
                return hitDie is null ? 0 : (int)hitDie.Value;
            }
        }

        public string HpToolTip => IsFirstLevel
            ? "First level hit points are fixed at the maximum allowed by the template hit die."
            : "Hit points for this class level";

        public string HitDieLabel
        {
            get
            {
                var hitDie = _purchases
                    .Select(x => x.Document?.HitDie)
                    .FirstOrDefault(x => x is not null);
                return hitDie is null ? "" : $"({FormatHitDie(hitDie.Value)})";
            }
        }

        public int CharacterLevel
        {
            get => _characterLevel;
            set
            {
                if (_characterLevel == value)
                {
                    return;
                }

                _characterLevel = value;
                if (IsFirstLevel && MaxHitPoints > 0)
                {
                    _hpNote = MaxHitPoints.ToString();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpNote)));
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharacterLevel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFirstLevel)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpToolTip)));
            }
        }

        public string HpNote
        {
            get => _hpNote;
            set
            {
                if (IsFirstLevel && MaxHitPoints > 0)
                {
                    value = MaxHitPoints.ToString();
                }

                value ??= "";
                if (string.Equals(_hpNote, value, StringComparison.Ordinal))
                {
                    return;
                }

                _hpNote = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpNote)));
            }
        }

        public string FavoredBonus
        {
            get => _favoredBonus;
            set
            {
                value ??= "";
                if (string.Equals(_favoredBonus, value, StringComparison.Ordinal))
                {
                    return;
                }

                _favoredBonus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FavoredBonus)));
            }
        }

        public void ReplacePurchases(IReadOnlyList<TemplatePurchaseRow> purchases)
        {
            _purchases = purchases ?? Array.Empty<TemplatePurchaseRow>();
            if (IsFirstLevel && MaxHitPoints > 0)
            {
                _hpNote = MaxHitPoints.ToString();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpNote)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Purchases)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HitDieLabel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxHitPoints)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HpToolTip)));
        }

        private static string FormatHitDie(HitDieType hitDie)
        {
            return $"d{(int)hitDie}";
        }
    }

    public sealed class SkillSpecializationDefinition
    {
        public SkillSpecializationDefinition(string baseSkillName, string specialization, string? baseDescription)
        {
            BaseSkillName = baseSkillName ?? throw new ArgumentNullException(nameof(baseSkillName));
            Specialization = specialization ?? throw new ArgumentNullException(nameof(specialization));
            Description = BuildDescription(baseDescription);
        }

        public string BaseSkillName { get; }
        public string Specialization { get; }
        public string DisplayName => $"{BaseSkillName} ({Specialization})";
        public string Description { get; }

        public static IReadOnlyList<SkillSpecializationDefinition> CreateDefaults(IReadOnlyDictionary<string, string> baseDescriptions)
        {
            string DescriptionFor(string name) =>
                baseDescriptions.TryGetValue(name, out var description) ? description : "";

            var items = new List<SkillSpecializationDefinition>();
            Add(items, "Craft", DescriptionFor("Craft"), new[]
            {
                "alchemy",
                "armor",
                "basketweaving",
                "blacksmithing",
                "bookbinding",
                "bowmaking",
                "calligraphy",
                "carpentry",
                "cloth",
                "clothing",
                "clockwork",
                "conveyance",
                "crystal carving",
                "dollmaking",
                "firearms",
                "gemcutting",
                "glass",
                "jewelry",
                "leather",
                "locks",
                "mapmaking",
                "mechanical",
                "musical instruments",
                "other",
                "painting",
                "poison",
                "pottery",
                "rope",
                "sculpture",
                "ships",
                "shoes",
                "siege engines",
                "stonemasonry",
                "tattoo",
                "traps",
                "weapons",
                "weaving",
                "woodworking",
            });
            Add(items, "Knowledge", DescriptionFor("Knowledge"), new[]
            {
                "arcana",
                "architecture and engineering",
                "dungeoneering",
                "geography",
                "history",
                "local",
                "nature",
                "nobility and royalty",
                "planes",
                "religion",
            });
            Add(items, "Perform", DescriptionFor("Perform"), new[]
            {
                "act",
                "comedy",
                "dance",
                "keyboard instruments",
                "oratory",
                "percussion instruments",
                "sing",
                "string instruments",
                "wind instruments",
            });
            Add(items, "Profession", DescriptionFor("Profession"), new[]
            {
                "apothecary",
                "barrister",
                "brewer",
                "cook",
                "driver",
                "engineer",
                "farmer",
                "fisher",
                "gambler",
                "herbalist",
                "innkeeper",
                "librarian",
                "merchant",
                "midwife",
                "miner",
                "porter",
                "sailor",
                "scribe",
                "shepherd",
                "siege engineer",
                "soldier",
                "stable master",
                "tanner",
                "trapper",
                "woodcutter",
            });

            return items
                .OrderBy(x => x.BaseSkillName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Specialization, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static void Add(List<SkillSpecializationDefinition> items, string baseName, string baseDescription, IEnumerable<string> specializations)
        {
            items.AddRange(specializations.Select(x => new SkillSpecializationDefinition(baseName, x, baseDescription)));
        }

        private string BuildDescription(string? baseDescription)
        {
            var lines = new List<string>
            {
                DisplayName,
                "",
            };

            if (!string.IsNullOrWhiteSpace(baseDescription))
            {
                lines.Add(baseDescription.Trim());
            }
            else
            {
                lines.Add($"{BaseSkillName} requires a specialization. This row tracks {Specialization} separately from other {BaseSkillName} specialties.");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    public sealed class SkillAllocationRow : INotifyPropertyChanged
    {
        private int _skillPointsSpent;
        private int _attributeModifier;
        private int _classSkillBonus;
        private int _sizeModifier;
        private bool _isClassSkill;
        private decimal _rankMultiplier = 1m;

        public SkillAllocationRow(string name, string? specialization, SkillAttribute attribute, bool requiresSpecialization)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Specialization = specialization;
            Attribute = attribute;
            RequiresSpecialization = requiresSpecialization;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; }
        public string? Specialization { get; }
        public SkillAttribute Attribute { get; }
        public bool RequiresSpecialization { get; }

        public string DisplayName
        {
            get
            {
                if (RequiresSpecialization && string.IsNullOrWhiteSpace(Specialization))
                {
                    return $"{Name} (*)";
                }

                return string.IsNullOrWhiteSpace(Specialization)
                    ? Name
                    : $"{Name} ({Specialization})";
            }
        }

        public string AttributeShort => Attribute switch
        {
            SkillAttribute.Strength => "Str",
            SkillAttribute.Dexterity => "Dex",
            SkillAttribute.Constitution => "Con",
            SkillAttribute.Intelligence => "Int",
            SkillAttribute.Wisdom => "Wis",
            SkillAttribute.Charisma => "Cha",
            _ => "—",
        };

        public int AttributeModifier
        {
            get => _attributeModifier;
            set
            {
                if (_attributeModifier == value)
                {
                    return;
                }

                _attributeModifier = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttributeModifier)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttributeModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public string AttributeModifierText => _attributeModifier >= 0 ? $"+{_attributeModifier}" : _attributeModifier.ToString();

        public bool IsClassSkill
        {
            get => _isClassSkill;
            set
            {
                if (_isClassSkill == value)
                {
                    return;
                }

                _isClassSkill = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsClassSkill)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public int ClassSkillBonus
        {
            get => _classSkillBonus;
            set
            {
                if (_classSkillBonus == value)
                {
                    return;
                }

                _classSkillBonus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassSkillBonus)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassSkillBonusText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public string ClassSkillBonusText => _classSkillBonus >= 0 ? $"+{_classSkillBonus}" : _classSkillBonus.ToString();

        public int SizeModifier
        {
            get => _sizeModifier;
            set
            {
                if (_sizeModifier == value)
                {
                    return;
                }

                _sizeModifier = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeModifier)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SizeModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public string SizeModifierText => _sizeModifier >= 0 ? $"+{_sizeModifier}" : _sizeModifier.ToString();

        public decimal RankMultiplier
        {
            get => _rankMultiplier;
            set
            {
                if (_rankMultiplier == value)
                {
                    return;
                }

                _rankMultiplier = value < 0 ? 0 : value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankMultiplier)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranks)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RanksText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public int SkillPointsSpent
        {
            get => _skillPointsSpent;
            set
            {
                var normalized = value < 0 ? 0 : value;
                if (_skillPointsSpent == normalized)
                {
                    return;
                }

                _skillPointsSpent = normalized;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkillPointsSpent)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanDecrease)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranks)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RanksText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierText)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalModifierBreakdown)));
            }
        }

        public bool CanDecrease => _skillPointsSpent > 0;

        public decimal Ranks => _skillPointsSpent * _rankMultiplier;
        public string RanksText => FormatNumber(Ranks);

        public string TotalModifierText
        {
            get
            {
                var effectiveRanksForRoll = decimal.Truncate(Ranks);
                var total = effectiveRanksForRoll + _attributeModifier + _classSkillBonus + _sizeModifier;
                return total >= 0 ? $"+{FormatNumber(total)}" : FormatNumber(total);
            }
        }

        public string TotalModifierBreakdown
        {
            get
            {
                var effectiveRanksForRoll = decimal.Truncate(Ranks);
                var total = effectiveRanksForRoll + _attributeModifier + _classSkillBonus + _sizeModifier;
                var lines = new List<string>
                {
                    $"{DisplayName}: {(total >= 0 ? "+" : "")}{FormatNumber(total)}",
                    $"Class skill: {(IsClassSkill ? "Yes" : "No")}",
                    $"Ranks: {FormatNumber(effectiveRanksForRoll)}",
                    $"{AttributeShort}: {AttributeModifierText}",
                };

                if (_classSkillBonus != 0)
                {
                    lines.Add($"Class skill: {ClassSkillBonusText}");
                }

                if (_sizeModifier != 0)
                {
                    lines.Add($"Size: {SizeModifierText}");
                }

                if (Ranks != effectiveRanksForRoll)
                {
                    lines.Add($"Purchased ranks: {RanksText}");
                    lines.Add("Fractional rank does not add to rolls until it reaches a whole rank.");
                }

                lines.Add($"Total: {(total >= 0 ? "+" : "")}{FormatNumber(total)}");
                return string.Join(Environment.NewLine, lines);
            }
        }

        private static string FormatNumber(decimal value)
        {
            return value % 1m == 0m
                ? ((int)value).ToString()
                : value.ToString("0.#");
        }
    }

    public sealed class FeatListItem
    {
        public FeatListItem(FeatDefinitionDocument definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public FeatDefinitionDocument Definition { get; }

        public string DisplayName
        {
            get
            {
                var flags = new List<string>();
                if (Definition.FighterBonusFeat)
                {
                    flags.Add("fighter");
                }

                if (Definition.Stacks)
                {
                    flags.Add("stacks");
                }
                else if (Definition.Repeatable)
                {
                    flags.Add("repeatable");
                }

                var suffix = flags.Count == 0 ? "" : $" [{string.Join(", ", flags)}]";
                return Definition.Name + suffix;
            }
        }
    }

    public sealed class RaceListItem
    {
        public RaceListItem(RaceDefinitionDocument definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public RaceDefinitionDocument Definition { get; }
        public string DisplayName => Definition.Name;
    }

    public sealed class RaceAbilityDisplayRow
    {
        public RaceAbilityDisplayRow(string name, string description)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Ability" : name.Trim();
            Description = description ?? "";
        }

        public string Name { get; }
        public string Description { get; }
    }

    public sealed class SelectedFeatRow : INotifyPropertyChanged
    {
        private int _count = 1;

        public SelectedFeatRow(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; }

        public int Count
        {
            get => _count;
            set
            {
                var normalized = value < 1 ? 1 : value;
                if (_count == normalized)
                {
                    return;
                }

                _count = normalized;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }
    }
}
