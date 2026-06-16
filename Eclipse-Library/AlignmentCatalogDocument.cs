using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class AlignmentCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "alignments", IsRequired = true)]
        public List<AlignmentDefinitionDocument> Alignments { get; set; } = new();

        public IReadOnlyList<AlignmentDefinition> ToDefinitions()
            => Alignments
                .Where(x => x is not null)
                .Select(x => x.ToDefinition())
                .ToList();
    }

    [DataContract]
    public sealed class AlignmentDefinitionDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = "";

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "abbreviation", IsRequired = false, EmitDefaultValue = false)]
        public string Abbreviation { get; set; } = "";

        [DataMember(Name = "parts", IsRequired = true)]
        public List<string> Parts { get; set; } = new();

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string Description { get; set; } = "";

        public AlignmentDefinition ToDefinition()
            => new(Id, Name, Abbreviation, Parts.Select(ParsePart), Description);

        private static AlignmentPart ParsePart(string part)
            => System.Enum.TryParse<AlignmentPart>(part, ignoreCase: true, out var parsed)
                ? parsed
                : AlignmentPart.Neutral;
    }
}
