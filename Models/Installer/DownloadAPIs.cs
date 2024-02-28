using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class DownloadAPI
{
    public string Root { get; set; }
    
    public string VersionManifest { get; set; }
    
    public string Assets { get; set; }
    
    public string Maven { get; set; }

    public DownloadAPI(string root, string versionManifest, string assets, string maven)
    {
        Root = root;
        VersionManifest = versionManifest;
        Assets = assets;
        Maven = maven;
    }
}

public static class DownloadAPIs
{
    public static DownloadAPI Current { get; set; } = new DownloadAPI(
        "https://bmclapi2.bangbang93.com",
        "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        "https://bmclapi2.bangbang93.com/assets",
        "https://bmclapi2.bangbang93.com/maven"
    );

    public static readonly DownloadAPI Mojang = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net"
    );

    public static readonly DownloadAPI BmclApi = new DownloadAPI(
        "https://bmclapi2.bangbang93.com",
        "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        "https://bmclapi2.bangbang93.com/assets",
        "https://bmclapi2.bangbang93.com/maven"
    );
    
    //public static readonly DownloadAPI Mcbbs = new DownloadAPI(
    //    "https://download.mcbbs.net",
    //    "https://download.mcbbs.net/mc/game/version_manifest_v2.json",
    //    "https://download.mcbbs.net/assets",
    //    "https://download.mcbbs.net/maven"
    //);

    public static void SwitchDownloadSource(DownloadSource source)
    {
        switch (source)
        {
            case DownloadSource.Current:
                Current = Current;
                break;
            case DownloadSource.Mojang:
                Current = Mojang;
                break;
            case DownloadSource.BmclApi:
                Current = BmclApi;
                break;
            default:
                throw new ArgumentException("[SL]未找到下载源");
        }
    }
}