using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class DownloadAPI
{
    public string Root { get; set; }
    public string VersionManifest { get; set; }
    public string Assets { get; set; }
    public string Maven { get; set; }
    
    public string FabricRoot { get; set; }
    public string FabricMaven { get; set; }
    
    public DownloadSource Source { get; set; }

    public DownloadAPI(string root, string versionManifest, string assets, string maven, string fabricRoot, string fabricMaven, DownloadSource source)
    {
        Root = root;
        VersionManifest = versionManifest;
        Assets = assets;
        Maven = maven;
        FabricRoot = fabricRoot;
        FabricMaven = fabricMaven;
        Source = source;
    }
}

public static class DownloadAPIs
{
    public static DownloadAPI Current { get; set; } = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net",
        "https://meta.fabricmc.net",
        "https://maven.fabricmc.net",
        DownloadSource.Official
    );

    public static readonly DownloadAPI Official = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net",
        "https://meta.fabricmc.net",
        "https://maven.fabricmc.net",
        DownloadSource.Official
    );

    private static readonly DownloadAPI BmclApi = new DownloadAPI(
        "https://bmclapi2.bangbang93.com",
        "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        "https://bmclapi2.bangbang93.com/assets",
        "https://bmclapi2.bangbang93.com/maven",
        "https://bmclapi2.bangbang93.com/fabric-meta",
        "https://bmclapi2.bangbang93.com/maven",
        DownloadSource.BmclApi
    );

    public static void SwitchDownloadSource(DownloadSource source) => Current = source switch
    {
        DownloadSource.Official => Official,
        DownloadSource.BmclApi => BmclApi,
        _ => throw new ArgumentException("[SL]未找到下载源")
    };
}