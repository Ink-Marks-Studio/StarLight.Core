using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class GameCoreJsonEntity
{
    [JsonPropertyName("latest")]
    public LatestInfo Latest { get; set; }

    [JsonPropertyName("versions")]
    public List<VersionInfo> Version { get; set; }
}