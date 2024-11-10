using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class UnifiedPassProfile
{
    [JsonPropertyName("id")]
    public string Uuid { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}