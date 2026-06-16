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
    public partial class FeatSelectionDialog : Window
    {
        private readonly IReadOnlyList<FeatListItem> _feats;
        private readonly ObservableCollection<FeatListItem> _visibleFeats = new();

        public FeatSelectionDialog(IEnumerable<FeatListItem> feats, int selectedCount, int availableCount)
        {
            InitializeComponent();

            _feats = feats.ToList();
            FeatsListBox.ItemsSource = _visibleFeats;
            FilterComboBox.ItemsSource = new[] { "Everything", "Fighter Bonus", "Repeatable", "Stacking" };
            FilterComboBox.SelectedIndex = 0;
            FeatSlotsText.Text = $"Add a Feat - {selectedCount} of {availableCount}";
            RefreshFeats();
        }

        public FeatListItem? SelectedFeat { get; private set; }
        public bool AddAnother { get; private set; }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFeats();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFeats();
        }

        private void RefreshFeats()
        {
            var query = SearchTextBox.Text?.Trim() ?? "";
            var filter = FilterComboBox.SelectedItem as string ?? "Everything";

            _visibleFeats.Clear();
            foreach (var item in _feats
                         .Where(x => MatchesFilter(x.Definition, filter))
                         .Where(x => string.IsNullOrWhiteSpace(query)
                             || x.Definition.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || (x.Definition.Notes ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(x => x.Definition.Name, StringComparer.OrdinalIgnoreCase))
            {
                _visibleFeats.Add(item);
            }

            FeatCountText.Text = $"({_visibleFeats.Count} item{(_visibleFeats.Count == 1 ? "" : "s")})";

            if (_visibleFeats.Count > 0 && FeatsListBox.SelectedItem is null)
            {
                FeatsListBox.SelectedIndex = 0;
            }
        }

        private static bool MatchesFilter(FeatDefinitionDocument feat, string filter)
        {
            return filter switch
            {
                "Fighter Bonus" => feat.FighterBonusFeat,
                "Repeatable" => feat.Repeatable,
                "Stacking" => feat.Stacks,
                _ => true,
            };
        }

        private void FeatsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedFeat = FeatsListBox.SelectedItem as FeatListItem;
            FeatDescriptionTextBox.Text = SelectedFeat is null ? "" : BuildDescription(SelectedFeat.Definition);
        }

        private static string BuildDescription(FeatDefinitionDocument feat)
        {
            var sb = new StringBuilder();
            sb.AppendLine(feat.Name);
            sb.AppendLine();
            sb.AppendLine($"Fighter bonus feat: {(feat.FighterBonusFeat ? "Yes" : "No")}");
            sb.AppendLine($"Repeatable: {(feat.Repeatable ? "Yes" : "No")}");
            sb.AppendLine($"Stacks: {(feat.Stacks ? "Yes" : "No")}");

            if (!string.IsNullOrWhiteSpace(feat.Notes))
            {
                sb.AppendLine();
                sb.AppendLine(feat.Notes.Trim());
            }

            return sb.ToString();
        }

        private void AddClose_Click(object sender, RoutedEventArgs e)
        {
            SelectedFeat = FeatsListBox.SelectedItem as FeatListItem;
            if (SelectedFeat is null)
            {
                return;
            }

            AddAnother = false;
            DialogResult = true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SelectedFeat = FeatsListBox.SelectedItem as FeatListItem;
            if (SelectedFeat is null)
            {
                return;
            }

            AddAnother = true;
            DialogResult = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
