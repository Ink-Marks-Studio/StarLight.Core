using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class FabricVersionJsonEntity
{
    [JsonPropertyName("intermediary")]
    public FabricMavenItem Intermediary { get; set; }

    [JsonPropertyName("loader")]
    public FabricMavenItem Loader { get; set; }

    [JsonPropertyName("launcherMeta")]
    public FabricLauncherMeta LauncherMeta { get; set; }
}