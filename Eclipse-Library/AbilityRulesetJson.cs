using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Eclipse_Library
{
    public static class AbilityRulesetJson
    {
        public static AbilityRulesetDocument Deserialize(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var serializer = new DataContractJsonSerializer(typeof(AbilityRulesetDocument));
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return (AbilityRulesetDocument)(serializer.ReadObject(ms)
                ?? throw new InvalidOperationException("Failed to deserialize AbilityRulesetDocument."));
        }

        public static string Serialize(AbilityRulesetDocument document, bool indent = true)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(AbilityRulesetDocument), settings);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, document);
            var json = Encoding.UTF8.GetString(ms.ToArray());

            // DataContractJsonSerializer doesn't support pretty-printing; keep a stable output format.
            return json;
        }

        public static AbilityRulesetDocument LoadFromFile(string path)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Deserialize(File.ReadAllText(path, Encoding.UTF8));
        }

        public static void SaveToFile(string path, AbilityRulesetDocument document)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            File.WriteAllText(path, Serialize(document), Encoding.UTF8);
        }
    }
}

