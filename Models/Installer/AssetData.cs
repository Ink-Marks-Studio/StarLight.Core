using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class AssetData
{
    [JsonPropertyName("objects")]
    public Dictionary<string, AssetInfo> Objects { get; set; }
}