using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class LatestInfo
{
    [JsonPropertyName("release")]
    public string Release { get; set; }

    [JsonPropertyName("snapshot")]
    public string Snapshot { get; set; }
}