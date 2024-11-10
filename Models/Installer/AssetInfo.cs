using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class AssetInfo
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}