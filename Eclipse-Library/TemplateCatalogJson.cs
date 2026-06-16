using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Eclipse_Library
{
    public static class TemplateCatalogJson
    {
        public static TemplateCatalogDocument Deserialize(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(TemplateCatalogDocument), settings);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return (TemplateCatalogDocument)(serializer.ReadObject(ms)
                ?? throw new InvalidOperationException("Failed to deserialize TemplateCatalogDocument."));
        }

        public static string Serialize(TemplateCatalogDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(TemplateCatalogDocument), settings);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, document);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static TemplateCatalogDocument LoadFromFile(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Deserialize(File.ReadAllText(path, Encoding.UTF8));
        }

        public static void SaveToFile(string path, TemplateCatalogDocument document)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllText(path, Serialize(document), Encoding.UTF8);
        }
    }
}

