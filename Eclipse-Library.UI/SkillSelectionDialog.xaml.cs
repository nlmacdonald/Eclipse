using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Eclipse_Library.UI
{
    public partial class SkillSelectionDialog : Window
    {
        private readonly IReadOnlyList<SkillSpecializationDefinition> _skills;
        private readonly ObservableCollection<SkillSpecializationDefinition> _visibleSkills = new();

        public SkillSelectionDialog(
            IEnumerable<SkillSpecializationDefinition> skills,
            IEnumerable<string> existingDisplayNames)
        {
            InitializeComponent();

            var existing = new HashSet<string>(existingDisplayNames ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _skills = (skills ?? Array.Empty<SkillSpecializationDefinition>())
                .Where(x => !existing.Contains(x.DisplayName))
                .OrderBy(x => x.BaseSkillName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Specialization, StringComparer.OrdinalIgnoreCase)
                .ToList();

            SkillsListBox.ItemsSource = _visibleSkills;
            CategoryFilterComboBox.ItemsSource = new[] { "Everything" }
                .Concat(_skills.Select(x => x.BaseSkillName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                .ToList();
            CategoryFilterComboBox.SelectedIndex = 0;
            RefreshSkills();
        }

        public SkillSpecializationDefinition? SelectedSkill { get; private set; }
        public bool AddAnother { get; private set; }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSkills();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSkills();
        }

        private void SkillsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSkill = SkillsListBox.SelectedItem as SkillSpecializationDefinition;
            SkillDescriptionRichTextBox.Document = BuildDescriptionDocument(SelectedSkill?.Description ?? "");
        }

        private void RefreshSkills()
        {
            var query = SearchTextBox.Text?.Trim() ?? "";
            var category = CategoryFilterComboBox.SelectedItem as string ?? "Everything";

            _visibleSkills.Clear();
            foreach (var item in _skills.Where(x =>
                         (category == "Everything" || string.Equals(x.BaseSkillName, category, StringComparison.OrdinalIgnoreCase))
                         && (string.IsNullOrWhiteSpace(query)
                             || x.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || x.Description.Contains(query, StringComparison.OrdinalIgnoreCase))))
            {
                _visibleSkills.Add(item);
            }

            CountText.Text = $"({_visibleSkills.Count} items)";
            if (_visibleSkills.Count > 0 && SkillsListBox.SelectedItem is null)
            {
                SkillsListBox.SelectedIndex = 0;
            }
        }

        private void AddClose_Click(object sender, RoutedEventArgs e)
        {
            SelectedSkill = SkillsListBox.SelectedItem as SkillSpecializationDefinition;
            if (SelectedSkill is null)
            {
                return;
            }

            AddAnother = false;
            DialogResult = true;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            SelectedSkill = SkillsListBox.SelectedItem as SkillSpecializationDefinition;
            if (SelectedSkill is null)
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

        private static FlowDocument BuildDescriptionDocument(string description)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0),
            };

            var lines = description.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                AddFormattedLine(paragraph, lines[i], i == 0);
                if (i < lines.Length - 1)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }
            }

            return new FlowDocument(paragraph)
            {
                PagePadding = new Thickness(8),
            };
        }

        private static void AddFormattedLine(Paragraph paragraph, string line, bool isTitle)
        {
            if (isTitle)
            {
                paragraph.Inlines.Add(new Bold(new Run(line)));
                return;
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0 && colonIndex <= 35 && line.Take(colonIndex).All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch)))
            {
                paragraph.Inlines.Add(new Bold(new Run(line.Substring(0, colonIndex + 1))));
                paragraph.Inlines.Add(new Run(line.Substring(colonIndex + 1)));
                return;
            }

            paragraph.Inlines.Add(new Run(line));
        }
    }
}
