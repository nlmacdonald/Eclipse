using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class TemplateCatalogStore
    {
        private const string OfficialCatalogFileName = "templates.official.json";
        private const string CustomCatalogFileName = "templates.custom.json";
        private readonly CatalogPathResolver _pathResolver;

        public TemplateCatalogStore()
            : this(new CatalogPathResolver())
        {
        }

        public TemplateCatalogStore(CatalogPathResolver pathResolver)
        {
            _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        }

        public IReadOnlyList<ClassTemplateListItem> LoadTemplates(
            RulesetId rulesetId,
            IReadOnlyCollection<string> includedSources)
        {
            var officialPath = _pathResolver.FindDocsFile(OfficialCatalogFileName);
            var customPath = _pathResolver.FindDocsFile(CustomCatalogFileName);
            var templates = new List<ClassTemplateListItem>();

            if (File.Exists(officialPath))
            {
                templates.AddRange(LoadCatalogItems(officialPath, "official", rulesetId, includedSources));
            }

            if (File.Exists(customPath))
            {
                templates.AddRange(LoadCatalogItems(customPath, "custom", rulesetId, includedSources));
            }
            else
            {
                TemplateCatalogJson.SaveToFile(
                    customPath,
                    new TemplateCatalogDocument
                    {
                        SchemaVersion = 1,
                        Templates = new List<ClassTemplateDocument>(),
                    });
            }

            return templates
                .OrderBy(x => x.Source, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public ClassTemplateDocument SaveCustomTemplate(ClassTemplateDocument template)
        {
            if (template is null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var customPath = _pathResolver.FindDocsFile(CustomCatalogFileName);
            var catalog = File.Exists(customPath)
                ? TemplateCatalogJson.LoadFromFile(customPath)
                : new TemplateCatalogDocument { SchemaVersion = 1, Templates = new List<ClassTemplateDocument>() };

            catalog.Templates ??= new List<ClassTemplateDocument>();
            var existingIndex = catalog.Templates.FindIndex(x => x.Id == template.Id);
            if (existingIndex >= 0)
            {
                catalog.Templates[existingIndex] = template;
            }
            else
            {
                catalog.Templates.Add(template);
            }

            TemplateCatalogJson.SaveToFile(customPath, catalog);
            return template;
        }

        private static IEnumerable<ClassTemplateListItem> LoadCatalogItems(
            string path,
            string catalogSource,
            RulesetId rulesetId,
            IReadOnlyCollection<string> includedSources)
        {
            return (TemplateCatalogJson.LoadFromFile(path).Templates ?? new List<ClassTemplateDocument>())
                .Where(template => template.RulesetId == rulesetId)
                .Select(template => new ClassTemplateListItem(template, GetTemplateSourceId(template, catalogSource)))
                .Where(item => ShouldShowTemplateSource(item.Source, includedSources));
        }

        private static bool ShouldShowTemplateSource(string source, IReadOnlyCollection<string> includedSources)
        {
            return IsRequiredResourceSource(source)
                || includedSources.Count == 0
                || includedSources.Contains(source);
        }

        private static bool IsRequiredResourceSource(string source)
        {
            return string.Equals(source, ResourceSourceIds.CoreRules, StringComparison.OrdinalIgnoreCase)
                || string.Equals(source, ResourceSourceIds.Eclipse, StringComparison.OrdinalIgnoreCase)
                || string.Equals(source, ResourceSourceIds.PlayersHandbook, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetTemplateSourceId(ClassTemplateDocument template, string catalogSource)
        {
            if (template.Source is not null && !string.IsNullOrWhiteSpace(template.Source.Id))
            {
                return template.Source.Id.Trim();
            }

            return string.Equals(catalogSource, "official", StringComparison.OrdinalIgnoreCase)
                ? ResourceSourceIds.PlayersHandbook
                : ResourceSourceIds.Custom;
        }
    }
}
