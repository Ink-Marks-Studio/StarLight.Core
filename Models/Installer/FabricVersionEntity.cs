using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class FabricVersionEntity
{
    public FabricMavenItem Intermediary { get; set; }

    public FabricMavenItem Loader { get; set; }

    //TODO: public FabricLauncherMeta LauncherMetaEntity { get; set; }

    public string GameVersion { get; set; }

    public string Version { get; set; }

    public LoaderType Type => LoaderType.Fabric;
}