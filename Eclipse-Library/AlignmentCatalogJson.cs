using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Eclipse_Library
{
    public static class AlignmentCatalogJson
    {
        public static AlignmentCatalogDocument Deserialize(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(AlignmentCatalogDocument), settings);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return (AlignmentCatalogDocument)(serializer.ReadObject(ms)
                ?? throw new InvalidOperationException("Failed to deserialize AlignmentCatalogDocument."));
        }

        public static string Serialize(AlignmentCatalogDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(AlignmentCatalogDocument), settings);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, document);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static AlignmentCatalogDocument LoadFromFile(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Deserialize(File.ReadAllText(path, Encoding.UTF8));
        }
    }
}
