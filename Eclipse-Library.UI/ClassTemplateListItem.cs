using System;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class ClassTemplateListItem
    {
        public ClassTemplateListItem(ClassTemplateDocument document, string source)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public ClassTemplateDocument Document { get; }
        public string Source { get; }
        public string Name => Document.Name;
        public TemplateTag Tag => Document.Tag;
        public int? MaxClassLevels => Document.MaxClassLevels;
        public RulesetId RulesetId => Document.RulesetId;
        public string DisplayName => $"{Document.Name} [{Document.Tag}, {Source}]";
    }
}
