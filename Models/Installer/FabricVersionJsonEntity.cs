using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

internal class FabricVersionJsonEntity
{
    [JsonPropertyName("intermediary")]
    public FabricMavenItem Intermediary { get; set; }

    [JsonPropertyName("loader")]
    public FabricMavenItem Loader { get; set; }

    [JsonPropertyName("launcherMeta")]
    public FabricLauncherMetaEntity LauncherMetaEntity { get; set; }
}