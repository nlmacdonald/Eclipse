using System.Runtime.Serialization;

namespace Eclipse_Library
{
    [DataContract]
    public sealed class ResourceSourceDocument
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string Id { get; set; } = ResourceSourceIds.Custom;

        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; } = "Custom";

        [DataMember(Name = "system", IsRequired = false, EmitDefaultValue = false)]
        public string? System { get; set; }

        [DataMember(Name = "isCore", IsRequired = false, EmitDefaultValue = false)]
        public bool IsCore { get; set; }

        [DataMember(Name = "isOptional", IsRequired = false, EmitDefaultValue = false)]
        public bool IsOptional { get; set; }
    }
}
