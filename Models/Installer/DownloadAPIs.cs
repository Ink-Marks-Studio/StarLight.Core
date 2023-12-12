namespace StarLight_Core.Models.Installer;

public class DownloadAPI
{
    public string Host { get; set; }
    public string VersionManifest { get; set; }
    public string Assets { get; set; }
    public string Libraries { get; set; }

    public DownloadAPI(string host, string versionManifest, string assets, string libraries)
    {
        Host = host;
        VersionManifest = versionManifest;
        Assets = assets;
        Libraries = libraries;
    }
}

public static class DownloadAPIs
{
    public static DownloadAPI Current { get; set; } = new DownloadAPI(
        "https://download.mcbbs.net",
        "https://download.mcbbs.net/mc/game/version_manifest.json",
        "https://download.mcbbs.net/assets",
        "https://download.mcbbs.net/maven"
    );

    public static readonly DownloadAPI Mojang = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://launchermeta.mojang.com/mc/game/version_manifest.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net"
    );

    public static readonly DownloadAPI BmclApi = new DownloadAPI(
        "https://bmclapi2.bangbang93.com",
        "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json",
        "https://bmclapi2.bangbang93.com/assets",
        "https://bmclapi2.bangbang93.com/maven"
    );

    public static readonly DownloadAPI Mcbbs = new DownloadAPI(
        "https://download.mcbbs.net",
        "https://download.mcbbs.net/mc/game/version_manifest.json",
        "https://download.mcbbs.net/assets",
        "https://download.mcbbs.net/maven"
    );
}