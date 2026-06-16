using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class LanguageCatalogDocument
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)]
        public int SchemaVersion { get; set; } = 1;

        [DataMember(Name = "languages", IsRequired = true)]
        public List<LanguageDefinitionDocument> Languages { get; set; } = new();
    }

    [DataContract]
    public sealed class LanguageDefinitionDocument
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "";

        [DataMember(Name = "category", IsRequired = false, EmitDefaultValue = false)]
        public string? Category { get; set; }

        [DataMember(Name = "source", IsRequired = false, EmitDefaultValue = false)]
        public string? Source { get; set; }

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string? Description { get; set; }
    }
}
