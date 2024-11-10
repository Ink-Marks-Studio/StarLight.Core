using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace StarLight_Core.Utilities;

public static class JsonUtil
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static T Deserialize<T>(this string json, JsonTypeInfo<T> jsonType)
    {
        return JsonSerializer.Deserialize(json, jsonType);
    }

    public static T ToJsonEntry<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static string Serialize(this object obj)
    {
        return JsonSerializer.Serialize(obj, Options);
    }

    public static JsonNode ToJsonNode(this string json)
    {
        return JsonNode.Parse(json);
    }
}