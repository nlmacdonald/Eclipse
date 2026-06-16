using System;
using System.IO;
using System.Text.Json;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class CharacterBuildDocumentStore
    {
        private static readonly JsonSerializerOptions SaveOptions = new()
        {
            WriteIndented = true,
        };

        public CharacterBuildDocument Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Character file path is required.", nameof(path));
            }

            var document = JsonSerializer.Deserialize<CharacterBuildDocument>(File.ReadAllText(path))
                ?? throw new InvalidOperationException("Character file was empty.");
            document.NormalizeCollections();
            return document;
        }

        public void Save(string path, CharacterBuildDocument document)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Character file path is required.", nameof(path));
            }

            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.NormalizeCollections();
            File.WriteAllText(path, JsonSerializer.Serialize(document, SaveOptions));
        }
    }
}
