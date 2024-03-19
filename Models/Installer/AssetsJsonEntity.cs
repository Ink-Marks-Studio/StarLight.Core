using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class AssetsJsonEntity
{
    [JsonPropertyName("assetIndex")]
    public AssetIndex AssetIndex { get; set; }
}