using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Skin;

public class SkinInfoJsonEntity
{
    [JsonPropertyName("url")]
    public string Url { get; set; }
}