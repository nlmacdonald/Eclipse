using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public partial class HeroConfigurationDialog : Window
    {
        private readonly ObservableCollection<HeroSourceOption> _sourceOptions = new();

        public HeroConfigurationDialog(HeroConfigurationSettings initial)
        {
            InitializeComponent();

            RulesetComboBox.ItemsSource = RulesetProfile.All;
            AbilityMethodComboBox.ItemsSource = HeroConfigurationSettings.AbilityMethods;
            SkillRankCapModeComboBox.ItemsSource = HeroConfigurationSettings.SkillRankCapModes;
            SourceOptionsItemsControl.ItemsSource = _sourceOptions;

            ApplySettings(initial);
        }

        public HeroConfigurationSettings Settings { get; private set; } = new();
        public bool OpenFileRequested { get; private set; }

        private void ApplySettings(HeroConfigurationSettings settings)
        {
            HeroNameTextBox.Text = settings.HeroName;
            PlayerNameTextBox.Text = settings.PlayerName;
            RulesetComboBox.SelectedItem = RulesetProfile.Get(settings.RulesetId);
            AbilityMethodComboBox.SelectedItem = string.IsNullOrWhiteSpace(settings.AbilityMethod)
                ? HeroConfigurationSettings.AbilityMethods[0]
                : settings.AbilityMethod;
            StartingLevelTextBox.Text = Math.Max(1, settings.StartingLevel).ToString();
            SaveDefaultsCheckBox.IsChecked = settings.SaveDefaults;
            GestaltOptionCheckBox.IsChecked = settings.UseGestalt;
            MaxHpFirstLevelCheckBox.IsChecked = settings.MaxHpFirstLevel;
            UseDefaultStartingCashCheckBox.IsChecked = settings.UseDefaultStartingCash;
            SkillRankCapModeComboBox.SelectedItem = string.IsNullOrWhiteSpace(settings.SkillRankCapMode)
                ? HeroConfigurationSettings.SkillRankCapModes[0]
                : settings.SkillRankCapMode;
            SuppressIrrelevantPromotionCheckBox.IsChecked = settings.SuppressIrrelevantSkillPromotion;
            UseFirstCharacterLevelSkillPointMultiplierCheckBox.IsChecked = settings.UseFirstCharacterLevelSkillPointMultiplier;
            RefreshSourceOptions(settings.IncludedSources);
        }

        private void RulesetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSources = _sourceOptions.Where(x => x.IsSelected).Select(x => x.Id).ToList();
            RefreshSourceOptions(selectedSources);
        }

        private void RefreshSourceOptions(System.Collections.Generic.IEnumerable<string>? selectedSources)
        {
            var selected = (selectedSources ?? Array.Empty<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (selected.Count == 0)
            {
                selected.Add(ResourceSourceIds.Custom);
            }

            _sourceOptions.Clear();
            _sourceOptions.Add(new HeroSourceOption(ResourceSourceIds.CoreRules, "Core rules", true, isRequired: true));
            _sourceOptions.Add(new HeroSourceOption(ResourceSourceIds.Eclipse, "Eclipse", true, isRequired: true));
            _sourceOptions.Add(new HeroSourceOption(ResourceSourceIds.PlayersHandbook, "Player's Handbook", true, isRequired: true));
            _sourceOptions.Add(new HeroSourceOption(ResourceSourceIds.Custom, "Custom resources", selected.Contains(ResourceSourceIds.Custom)));
            _sourceOptions.Add(new HeroSourceOption("complete-arcane", "Complete Arcane", selected.Contains("complete-arcane")));
            RefreshResourceSummary();

            foreach (var source in _sourceOptions)
            {
                source.PropertyChanged += (_, __) => RefreshResourceSummary();
            }
        }

        private void RefreshResourceSummary()
        {
            var ruleset = RulesetComboBox.SelectedItem is RulesetProfile profile
                ? profile.DisplayName
                : RulesetProfile.Get(RulesetId.Dnd35).DisplayName;
            var sources = _sourceOptions.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            ResourceSummaryTextBox.Text = $"Ruleset: {ruleset}{Environment.NewLine}Sources: {(sources.Count == 0 ? "None" : string.Join(", ", sources))}";
        }

        private void IncreaseLevel_Click(object sender, RoutedEventArgs e)
        {
            StartingLevelTextBox.Text = (ReadLevel() + 1).ToString();
        }

        private void DecreaseLevel_Click(object sender, RoutedEventArgs e)
        {
            StartingLevelTextBox.Text = Math.Max(1, ReadLevel() - 1).ToString();
        }

        private int ReadLevel()
        {
            return int.TryParse(StartingLevelTextBox.Text, out var level) && level > 0 ? level : 1;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Settings = ReadSettings();
            if (Settings.SaveDefaults)
            {
                HeroConfigurationSettings.SaveDefaultsToDisk(Settings);
            }

            DialogResult = true;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileRequested = true;
            Settings = ReadSettings();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private HeroConfigurationSettings ReadSettings()
        {
            var profile = RulesetComboBox.SelectedItem as RulesetProfile ?? RulesetProfile.Get(RulesetId.Dnd35);
            return new HeroConfigurationSettings
            {
                HeroName = string.IsNullOrWhiteSpace(HeroNameTextBox.Text) ? "New Hero" : HeroNameTextBox.Text.Trim(),
                PlayerName = PlayerNameTextBox.Text.Trim(),
                RulesetId = profile.Id,
                AbilityMethod = AbilityMethodComboBox.SelectedItem as string ?? HeroConfigurationSettings.AbilityMethods[0],
                StartingLevel = ReadLevel(),
                IncludedSources = _sourceOptions.Where(x => x.IsSelected).Select(x => x.Id).ToList(),
                UseGestalt = GestaltOptionCheckBox.IsChecked == true,
                MaxHpFirstLevel = MaxHpFirstLevelCheckBox.IsChecked == true,
                UseDefaultStartingCash = UseDefaultStartingCashCheckBox.IsChecked == true,
                SkillRankCapMode = SkillRankCapModeComboBox.SelectedItem as string ?? HeroConfigurationSettings.SkillRankCapModes[0],
                SuppressIrrelevantSkillPromotion = SuppressIrrelevantPromotionCheckBox.IsChecked == true,
                UseFirstCharacterLevelSkillPointMultiplier = UseFirstCharacterLevelSkillPointMultiplierCheckBox.IsChecked == true,
                SaveDefaults = SaveDefaultsCheckBox.IsChecked == true,
            };
        }
    }

    public sealed class HeroSourceOption : INotifyPropertyChanged
    {
        private bool _isSelected;

        public HeroSourceOption(string id, string name, bool isSelected, bool isRequired = false)
        {
            Id = id;
            Name = name;
            IsRequired = isRequired;
            _isSelected = isSelected;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public string Id { get; }
        public string Name { get; }
        public bool IsRequired { get; }
        public bool CanToggle => !IsRequired;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (IsRequired)
                {
                    value = true;
                }

                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    [DataContract]
    public sealed class HeroConfigurationSettings
    {
        public static readonly string[] AbilityMethods =
        {
            "Standard array",
            "Point buy",
            "Roll 4d6 drop lowest",
            "Manual entry",
        };

        public static readonly string[] SkillRankCapModes =
        {
            "Rules default",
            "Character level +2",
            "Character level +5",
            "Character level +10",
            "Unlimited",
        };

        [DataMember] public string HeroName { get; set; } = "New Hero";
        [DataMember] public string PlayerName { get; set; } = "";
        [DataMember] public RulesetId RulesetId { get; set; } = RulesetId.Dnd35;
        [DataMember] public string AbilityMethod { get; set; } = AbilityMethods[0];
        [DataMember] public int StartingLevel { get; set; } = 1;
        [DataMember] public System.Collections.Generic.List<string> IncludedSources { get; set; } = new()
        {
            ResourceSourceIds.CoreRules,
            ResourceSourceIds.Eclipse,
            ResourceSourceIds.PlayersHandbook,
            ResourceSourceIds.Custom,
        };
        [DataMember] public bool UseGestalt { get; set; }
        [DataMember] public bool MaxHpFirstLevel { get; set; } = true;
        [DataMember] public bool UseDefaultStartingCash { get; set; } = true;
        [DataMember] public string SkillRankCapMode { get; set; } = SkillRankCapModes[0];
        [DataMember] public bool SuppressIrrelevantSkillPromotion { get; set; }
        [DataMember] public bool UseFirstCharacterLevelSkillPointMultiplier { get; set; } = true;
        [DataMember] public bool SaveDefaults { get; set; }

        public static HeroConfigurationSettings LoadDefaults()
        {
            try
            {
                var path = GetDefaultsPath();
                if (!File.Exists(path))
                {
                    return new HeroConfigurationSettings();
                }

                using var stream = File.OpenRead(path);
                var serializer = new DataContractJsonSerializer(typeof(HeroConfigurationSettings));
                return (HeroConfigurationSettings?)serializer.ReadObject(stream) ?? new HeroConfigurationSettings();
            }
            catch
            {
                return new HeroConfigurationSettings();
            }
        }

        public static void SaveDefaultsToDisk(HeroConfigurationSettings settings)
        {
            var path = GetDefaultsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var stream = File.Create(path);
            var serializer = new DataContractJsonSerializer(typeof(HeroConfigurationSettings));
            serializer.WriteObject(stream, settings);
        }

        private static string GetDefaultsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Eclipse-Library",
                "hero-defaults.json");
        }
    }
}
