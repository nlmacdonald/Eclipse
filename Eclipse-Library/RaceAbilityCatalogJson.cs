using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Eclipse_Library
{
    public static class RaceAbilityCatalogJson
    {
        public static RaceAbilityCatalogDocument Deserialize(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(RaceAbilityCatalogDocument), settings);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return (RaceAbilityCatalogDocument)(serializer.ReadObject(ms)
                ?? throw new InvalidOperationException("Failed to deserialize RaceAbilityCatalogDocument."));
        }

        public static string Serialize(RaceAbilityCatalogDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(RaceAbilityCatalogDocument), settings);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, document);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static RaceAbilityCatalogDocument LoadFromFile(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Deserialize(File.ReadAllText(path, Encoding.UTF8));
        }

        public static void SaveToFile(string path, RaceAbilityCatalogDocument document)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllText(path, Serialize(document), Encoding.UTF8);
        }
    }
}
