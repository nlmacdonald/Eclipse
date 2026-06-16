using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public partial class LanguageSelectionDialog : Window
    {
        private readonly IReadOnlyList<LanguageListItem> _languages;
        private readonly ObservableCollection<LanguageListItem> _visibleLanguages = new();

        public LanguageSelectionDialog(IEnumerable<LanguageDefinitionDocument> languages, IEnumerable<string> knownLanguages)
        {
            InitializeComponent();

            var known = new HashSet<string>(knownLanguages ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _languages = languages
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .Where(x => !known.Contains(x.Name) || string.Equals(x.Name, "Custom Language", StringComparison.OrdinalIgnoreCase))
                .Select(x => new LanguageListItem(x))
                .ToList();

            LanguagesListBox.ItemsSource = _visibleLanguages;
            CategoryFilterComboBox.ItemsSource = new[] { "Everything" }
                .Concat(_languages.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                .ToList();
            CategoryFilterComboBox.SelectedIndex = 0;
            RefreshLanguages();
        }

        public LanguageDefinitionDocument? SelectedLanguage { get; private set; }
        public bool AddAnother { get; private set; }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshLanguages();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshLanguages();
        }

        private void LanguagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLanguage = (LanguagesListBox.SelectedItem as LanguageListItem)?.Definition;
            LanguageDescriptionTextBox.Text = SelectedLanguage is null
                ? ""
                : BuildDescription(SelectedLanguage);
        }

        private void RefreshLanguages()
        {
            var query = SearchTextBox.Text?.Trim() ?? "";
            var category = CategoryFilterComboBox.SelectedItem as string ?? "Everything";

            _visibleLanguages.Clear();
            foreach (var item in _languages.Where(x =>
                         (category == "Everything" || string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase))
                         && (string.IsNullOrWhiteSpace(query) || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                     .OrderBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                _visibleLanguages.Add(item);
            }

            CountText.Text = $"({_visibleLanguages.Count} items)";
            if (_visibleLanguages.Count > 0 && LanguagesListBox.SelectedItem is null)
            {
                LanguagesListBox.SelectedIndex = 0;
            }
        }

        private static string BuildDescription(LanguageDefinitionDocument language)
        {
            return string.Join(Environment.NewLine, new[]
            {
                language.Name,
                $"Category: {language.Category ?? "Uncategorized"}",
                $"Source: {language.Source ?? "Unknown"}",
                "",
                language.Description ?? "",
            });
        }

        private void AddClose_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = (LanguagesListBox.SelectedItem as LanguageListItem)?.Definition;
            if (SelectedLanguage is null)
            {
                return;
            }

            AddAnother = false;
            DialogResult = true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = (LanguagesListBox.SelectedItem as LanguageListItem)?.Definition;
            if (SelectedLanguage is null)
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

    public sealed class LanguageListItem
    {
        public LanguageListItem(LanguageDefinitionDocument definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public LanguageDefinitionDocument Definition { get; }
        public string Name => Definition.Name;
        public string Category => Definition.Category ?? "Uncategorized";
        public string DisplayName => string.IsNullOrWhiteSpace(Definition.Source)
            ? Definition.Name
            : $"{Definition.Name}    {Definition.Source}";
    }
}
