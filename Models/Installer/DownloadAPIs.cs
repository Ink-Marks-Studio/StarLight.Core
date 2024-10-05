using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

/// <summary>
/// 下载源
/// </summary>
public class DownloadAPI
{
    public string Root { get; set; }
    public string VersionManifest { get; set; }
    public string Assets { get; set; }
    public string Maven { get; set; }
    
    public string FabricRoot { get; set; }
    public string FabricMaven { get; set; }
    
    public string ForgeRoot { get; set; }
    public string ForgeMaven { get; set; }
    
    public DownloadSource Source { get; set; }

    /// <summary>
    /// 下载源
    /// </summary>
    /// <param name="root"></param>
    /// <param name="versionManifest"></param>
    /// <param name="assets"></param>
    /// <param name="maven"></param>
    /// <param name="fabricRoot"></param>
    /// <param name="fabricMaven"></param>
    /// <param name="forgeRoot"></param>
    /// <param name="forgeMaven"></param>
    /// <param name="source"></param>
    public DownloadAPI(string root, string versionManifest, string assets, string maven, string fabricRoot, string fabricMaven, string forgeRoot, string forgeMaven, DownloadSource source)
    {
        Root = root;
        VersionManifest = versionManifest;
        Assets = assets;
        Maven = maven;
        FabricRoot = fabricRoot;
        FabricMaven = fabricMaven;
        ForgeRoot = forgeRoot;
        ForgeMaven = forgeMaven;
        Source = source;
    }
}

/// <summary>
/// 下载源
/// </summary>
public static class DownloadAPIs
{
    internal static DownloadAPI Current { get; set; } = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net",
        "https://meta.fabricmc.net",
        "https://maven.fabricmc.net",
        "https://files.minecraftforge.net",
        "https://files.minecraftforge.net/maven",
        DownloadSource.Official
    );
    
    internal static readonly DownloadAPI Official = new DownloadAPI(
        "https://launcher.mojang.com",
        "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        "https://resources.download.minecraft.net",
        "https://libraries.minecraft.net",
        "https://meta.fabricmc.net",
        "https://maven.fabricmc.net",
        "https://files.minecraftforge.net",
        "https://files.minecraftforge.net/maven",
        DownloadSource.Official
    );

    private static readonly DownloadAPI BmclApi = new DownloadAPI(
        "https://bmclapi2.bangbang93.com",
        "https://bmclapi2.bangbang93.com/mc/game/version_manifest_v2.json",
        "https://bmclapi2.bangbang93.com/assets",
        "https://bmclapi2.bangbang93.com/maven",
        "https://bmclapi2.bangbang93.com/fabric-meta",
        "https://bmclapi2.bangbang93.com/maven",
        "https://bmclapi2.bangbang93.com/maven",
        "https://bmclapi2.bangbang93.com/maven",
        DownloadSource.BmclApi
    );

    /// <summary>
    /// 切换下载源
    /// </summary>
    /// <param name="source"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void SwitchDownloadSource(DownloadSource source) => Current = source switch
    {
        DownloadSource.Official => Official,
        DownloadSource.BmclApi => BmclApi,
        _ => throw new ArgumentException("[SL]未找到下载源")
    };
}