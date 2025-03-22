using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

internal abstract class FabricVersionJsonEntity
{
    [JsonPropertyName("intermediary")]
    internal abstract FabricMavenItem Intermediary { get; set; }

    [JsonPropertyName("loader")]
    internal abstract FabricMavenItem Loader { get; set; }

    [JsonPropertyName("launcherMeta")]
    internal abstract FabricLauncherMetaEntity LauncherMetaEntity { get; set; }
}