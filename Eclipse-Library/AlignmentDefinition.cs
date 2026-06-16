using System;
using System.Collections.Generic;
using System.Linq;

namespace Eclipse_Library
{
    public sealed class AlignmentDefinition
    {
        public AlignmentDefinition(
            string id,
            string name,
            string abbreviation,
            IEnumerable<AlignmentPart> parts,
            string description)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Alignment id is required.", nameof(id)) : id.Trim();
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Alignment name is required.", nameof(name)) : name.Trim();
            Abbreviation = string.IsNullOrWhiteSpace(abbreviation) ? Name : abbreviation.Trim();
            Parts = (parts ?? throw new ArgumentNullException(nameof(parts))).Distinct().ToList().AsReadOnly();
            Description = description ?? "";
        }

        public string Id { get; }
        public string Name { get; }
        public string Abbreviation { get; }
        public IReadOnlyList<AlignmentPart> Parts { get; }
        public string Description { get; }

        public bool HasPart(AlignmentPart part) => Parts.Contains(part);

        public override string ToString() => Name;
    }
}
