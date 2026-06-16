using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Eclipse_Library.UI
{
    public partial class TemplateSelectionDialog : Window
    {
        private readonly IReadOnlyList<ClassTemplateListItem> _templates;
        private readonly ObservableCollection<ClassTemplateListItem> _visibleTemplates = new();
        private readonly int _levelsRemaining;
        private int _levelsToAdd = 1;

        public TemplateSelectionDialog(IEnumerable<ClassTemplateListItem> templates, int levelsRemaining)
        {
            InitializeComponent();

            _templates = templates.ToList();
            _levelsRemaining = Math.Max(1, levelsRemaining);
            TemplatesListBox.ItemsSource = _visibleTemplates;
            SourceFilterComboBox.ItemsSource = new[] { "Everything" }
                .Concat(_templates.Select(x => x.Source).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x))
                .ToList();
            SourceFilterComboBox.SelectedIndex = 0;
            LevelsRemainingText.Text = $"{_levelsRemaining} level{(_levelsRemaining == 1 ? "" : "s")} left to add";
            RefreshLevelsToAdd();
            RefreshTemplates();
        }

        public ClassTemplateListItem? SelectedTemplate { get; private set; }
        public int LevelsToAdd => _levelsToAdd;
        public bool AddAnother { get; private set; }
        public bool CreateCustomTemplateRequested { get; private set; }
        public bool EditTemplateRequested { get; private set; }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshTemplates();
        }

        private void SourceFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshTemplates();
        }

        private void RefreshTemplates()
        {
            var query = SearchTextBox.Text?.Trim() ?? "";
            var source = SourceFilterComboBox.SelectedItem as string ?? "Everything";

            _visibleTemplates.Clear();
            foreach (var item in _templates.Where(x =>
                         (source == "Everything" || string.Equals(x.Source, source, StringComparison.OrdinalIgnoreCase))
                         && (string.IsNullOrWhiteSpace(query) || x.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)))
                     .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
            {
                _visibleTemplates.Add(item);
            }

            if (_visibleTemplates.Count > 0 && TemplatesListBox.SelectedItem is null)
            {
                TemplatesListBox.SelectedIndex = 0;
            }
        }

        private void TemplatesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTemplate = TemplatesListBox.SelectedItem as ClassTemplateListItem;
            TemplateDescriptionTextBox.Text = SelectedTemplate is null
                ? ""
                : BuildDescription(SelectedTemplate);
        }

        private void DecreaseLevelsToAdd_Click(object sender, RoutedEventArgs e)
        {
            _levelsToAdd = Math.Max(1, _levelsToAdd - 1);
            RefreshLevelsToAdd();
        }

        private void IncreaseLevelsToAdd_Click(object sender, RoutedEventArgs e)
        {
            _levelsToAdd = Math.Min(_levelsRemaining, _levelsToAdd + 1);
            RefreshLevelsToAdd();
        }

        private void RefreshLevelsToAdd()
        {
            _levelsToAdd = Math.Clamp(_levelsToAdd, 1, _levelsRemaining);
            LevelsToAddText.Text = _levelsToAdd.ToString();
        }

        private static string BuildDescription(ClassTemplateListItem item)
        {
            var doc = item.Document;
            var sb = new StringBuilder();
            sb.AppendLine(doc.Name);
            sb.AppendLine($"Source: {item.Source}");
            sb.AppendLine($"Ruleset: {doc.RulesetId}");
            sb.AppendLine($"Max levels: {doc.MaxClassLevels?.ToString() ?? "Unspecified"}");
            sb.AppendLine($"Alignment: {doc.Alignment ?? "Any"}");
            sb.AppendLine($"Starting gold: {doc.StartingGold ?? "Unspecified"}");
            sb.AppendLine();
            if (doc.NotesByRuleset is not null && doc.NotesByRuleset.TryGetValue(doc.RulesetId, out var notes) && !string.IsNullOrWhiteSpace(notes))
            {
                sb.AppendLine(notes);
                sb.AppendLine();
            }

            sb.AppendLine("Levels");
            foreach (var level in (doc.Levels ?? new List<Eclipse_Library.ClassTemplateLevelDocument>()).OrderBy(x => x.ClassLevel))
            {
                sb.AppendLine($"L{level.ClassLevel}: {(level.Purchases?.Count ?? 0)} purchase(s)");
            }

            return sb.ToString();
        }

        private void AddClose_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplate = TemplatesListBox.SelectedItem as ClassTemplateListItem;
            if (SelectedTemplate is null)
            {
                return;
            }

            AddAnother = false;
            DialogResult = true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplate = TemplatesListBox.SelectedItem as ClassTemplateListItem;
            if (SelectedTemplate is null)
            {
                return;
            }

            AddAnother = true;
            DialogResult = true;
        }

        private void CreateCustomTemplate_Click(object sender, RoutedEventArgs e)
        {
            CreateCustomTemplateRequested = true;
            DialogResult = true;
        }

        private void EditSelectedTemplate_Click(object sender, RoutedEventArgs e)
        {
            SelectedTemplate = TemplatesListBox.SelectedItem as ClassTemplateListItem;
            if (SelectedTemplate is null)
            {
                return;
            }

            EditTemplateRequested = true;
            DialogResult = true;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
