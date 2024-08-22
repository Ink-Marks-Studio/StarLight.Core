using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace StarLight_Core.Utilities
{
    public static class JsonUtil
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        
        public static T Deserialize<T>(this string json, JsonTypeInfo<T> jsonType) => JsonSerializer.Deserialize(json, jsonType);

        public static T ToJsonEntry<T>(this string json) => JsonSerializer.Deserialize<T>(json, Options);
        
        public static string Serialize(this object obj) => JsonSerializer.Serialize(obj, Options);
        
        public static JsonNode ToJsonNode(this string json) => JsonNode.Parse(json);
    }
}