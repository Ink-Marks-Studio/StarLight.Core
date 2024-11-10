using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Skin;

internal class ProfileJsonEntity
{
    [JsonPropertyName("properties")]
    public List<SkinInfo> Properties { get; set; }
}

internal class SkinInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}