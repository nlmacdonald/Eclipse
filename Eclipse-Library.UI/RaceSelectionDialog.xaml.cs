using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public partial class RaceSelectionDialog : Window
    {
        private readonly IReadOnlyList<RaceListItem> _races;
        private readonly ObservableCollection<RaceListItem> _visibleRaces = new();
        private readonly string? _currentRaceName;

        public RaceSelectionDialog(IEnumerable<RaceListItem> races, string? currentRaceName)
        {
            InitializeComponent();

            _races = races.ToList();
            _currentRaceName = currentRaceName;
            RacesListBox.ItemsSource = _visibleRaces;
            RefreshRaces();
        }

        public RaceListItem? SelectedRace { get; private set; }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshRaces();
        }

        private void RefreshRaces()
        {
            var query = SearchTextBox.Text?.Trim() ?? "";
            var previousSelection = RacesListBox.SelectedItem as RaceListItem;

            _visibleRaces.Clear();
            foreach (var item in _races
                         .Where(x => string.IsNullOrWhiteSpace(query)
                             || x.Definition.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || (x.Definition.PluralName ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(x => x.Definition.Name, StringComparer.OrdinalIgnoreCase))
            {
                _visibleRaces.Add(item);
            }

            RaceCountText.Text = $"({_visibleRaces.Count} race{(_visibleRaces.Count == 1 ? "" : "s")})";

            var selection = _visibleRaces.FirstOrDefault(x => ReferenceEquals(x, previousSelection))
                ?? _visibleRaces.FirstOrDefault(x => string.Equals(x.Definition.Name, _currentRaceName, StringComparison.OrdinalIgnoreCase))
                ?? _visibleRaces.FirstOrDefault();

            if (selection is not null)
            {
                RacesListBox.SelectedItem = selection;
            }
            else
            {
                RaceDescriptionTextBox.Text = "";
            }
        }

        private void RacesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRace = RacesListBox.SelectedItem as RaceListItem;
            RaceDescriptionTextBox.Text = SelectedRace is null ? "" : BuildDescription(SelectedRace.Definition);
        }

        private static string BuildDescription(RaceDefinitionDocument race)
        {
            var sb = new StringBuilder();
            sb.AppendLine(race.Name);

            if (!string.IsNullOrWhiteSpace(race.SourceText.Overview))
            {
                sb.AppendLine();
                sb.AppendLine(race.SourceText.Overview.Trim());
            }

            AppendSections(sb, race.SourceText.Sections);
            AppendRules(sb, race.Rules);
            return sb.ToString();
        }

        private static void AppendSections(StringBuilder sb, Dictionary<string, string> sections)
        {
            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section.Value))
                {
                    continue;
                }

                sb.AppendLine();
                sb.AppendLine(section.Key);
                sb.AppendLine(section.Value.Trim());
            }
        }

        private static void AppendRules(StringBuilder sb, RaceRulesDocument rules)
        {
            sb.AppendLine();
            sb.AppendLine("Rules");

            if (rules.AbilityScoreModifiers is { Count: > 0 })
            {
                sb.AppendLine("Ability Scores: " + string.Join(", ", rules.AbilityScoreModifiers.Select(FormatAbilityModifier)));
            }

            if (!string.IsNullOrWhiteSpace(rules.Size))
            {
                sb.AppendLine($"Size: {rules.Size}");
            }

            if (rules.SpeedFeet is not null)
            {
                sb.AppendLine($"Speed: {rules.SpeedFeet} feet");
            }

            if (rules.Vision is { Count: > 0 })
            {
                sb.AppendLine("Vision: " + string.Join(", ", rules.Vision.Select(FormatVision)));
            }

            if (rules.Abilities is { Count: > 0 })
            {
                sb.AppendLine("Abilities: " + string.Join(", ", rules.Abilities.Select(FormatAbilityReference)));
            }

            if (rules.AutomaticLanguages is { Count: > 0 })
            {
                sb.AppendLine("Automatic Languages: " + string.Join(", ", rules.AutomaticLanguages));
            }

            if (rules.BonusLanguages is { Count: > 0 })
            {
                sb.AppendLine("Bonus Languages: " + string.Join(", ", rules.BonusLanguages));
            }

            if (!string.IsNullOrWhiteSpace(rules.FavoredClass))
            {
                sb.AppendLine($"Favored Class: {rules.FavoredClass}");
            }

            foreach (var trait in rules.Traits)
            {
                sb.AppendLine();
                sb.AppendLine(trait.Name);
                sb.AppendLine(trait.Description);
            }
        }

        private static string FormatAbilityModifier(RaceAbilityScoreModifierDocument modifier)
        {
            var value = modifier.Modifier is null
                ? ""
                : modifier.Modifier.Value >= 0 ? $"+{modifier.Modifier.Value}" : modifier.Modifier.Value.ToString();
            return modifier.Choose ? $"{value} {modifier.Ability} (choice)" : $"{value} {modifier.Ability}";
        }

        private static string FormatVision(RaceVisionDocument vision)
        {
            return vision.RangeFeet is null ? vision.Kind : $"{vision.Kind} {vision.RangeFeet} ft.";
        }

        private static string FormatAbilityReference(RaceAbilityReferenceDocument reference)
        {
            if (reference.Configuration is null || reference.Configuration.Count == 0)
            {
                return reference.AbilityId;
            }

            return $"{reference.AbilityId} ({string.Join(", ", reference.Configuration.Select(x => $"{x.Key}: {x.Value}"))})";
        }

        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            SelectedRace = RacesListBox.SelectedItem as RaceListItem;
            if (SelectedRace is null)
            {
                return;
            }

            DialogResult = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
