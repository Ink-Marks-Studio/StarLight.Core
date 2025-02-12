using System.Text.Json;
using StarLight_Core.Downloader;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer;

/// <summary>
/// Forge 安装类
/// </summary>
public class ForgeInstaller : InstallerBase
{
    /// <summary>
    /// Forge 安装器
    /// </summary>
    /// <param name="gameVersion">游戏版本</param>
    /// <param name="forgeVersion">加载器版本</param>
    public ForgeInstaller(string gameVersion, string forgeVersion)
    {
        GameVersion = gameVersion;
        ForgeVersion = forgeVersion;
        CancellationToken = default;
        Root = Path.Combine(FileUtil.GetCurrentExecutingDirectory(), ".minecraft");
    }

    /// <summary>
    /// Forge 安装器
    /// </summary>
    /// <param name="gameVersion">游戏版本</param>
    /// <param name="forgeVersion">加载器版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    public ForgeInstaller(string gameVersion, string forgeVersion, CancellationToken cancellationToken = default)
    {
        GameVersion = gameVersion;
        ForgeVersion = forgeVersion;
        CancellationToken = cancellationToken;
        Root = Path.Combine(FileUtil.GetCurrentExecutingDirectory(), ".minecraft");
    }

    /// <summary>
    /// 带有进度报告的 Forge 安装器
    /// </summary>
    /// <param name="gameVersion">游戏版本</param>
    /// <param name="forgeVersion">加载器版本</param>
    /// <param name="root">游戏根目录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="onSpeedChanged">速度报告</param>
    /// <param name="onProgressChanged">进度报告</param>
    public ForgeInstaller(string gameVersion, string forgeVersion, string root = ".minecraft",
        Action<string>? onSpeedChanged = null, Action<string, int>? onProgressChanged = null,
        CancellationToken cancellationToken = default)
    {
        GameVersion = gameVersion;
        ForgeVersion = forgeVersion;
        OnSpeedChanged = onSpeedChanged;
        OnProgressChanged = onProgressChanged;
        CancellationToken = cancellationToken;
        Root = FileUtil.IsAbsolutePath(root)
            ? Path.Combine(root)
            : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), root);
    }

    private string GameVersion { get; }

    private string ForgeVersion { get; }

    private CancellationToken CancellationToken { get; }

    /// <summary>
    /// Forge 异步安装方法
    /// </summary>
    /// <param name="customId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ForgeInstallResult> InstallAsync(string? customId = null)
    {
        var multiThreadedDownloader =
            new MultiThreadedFileDownloader(x => { OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(x)); },
                CancellationToken);

        var versionId = customId ?? $"{GameVersion}-Forge_{ForgeVersion}";
        var varPath = Path.Combine(Root, "versions", versionId);
        var jsonPath = Path.Combine(varPath, versionId + ".json");
        var forgeJarPath = Path.Combine(FileUtil.GetTempDirectory(), "StarLight.Core", $"forge-{ForgeVersion}");

        // DEV
        Console.WriteLine(forgeJarPath);

        FileUtil.IsDirectory(varPath, true);

        try
        {
            OnProgressChanged?.Invoke("开始安装 Forge 加载器", 0);

            if (FileUtil.IsFile(jsonPath))
                return new ForgeInstallResult(Status.Failed, GameVersion, ForgeVersion, versionId,
                    new Exception("版本已存在"));

            OnProgressChanged?.Invoke("下载加载器安装文件", 20);
            if (CancellationToken != default)
                CancellationToken.ThrowIfCancellationRequested();

            FileUtil.IsDirectory(forgeJarPath, true);
            Console.WriteLine(
                $"{DownloadAPIs.Current.ForgeMaven}/net/minecraftforge/forge/{GameVersion}-{ForgeVersion}/forge-{GameVersion}-{ForgeVersion}-installer.jar");
            try
            {
                await multiThreadedDownloader.DownloadFileWithMultiThread(
                    $"{DownloadAPIs.Current.ForgeMaven}/net/minecraftforge/forge/{GameVersion}-{ForgeVersion}/forge-{GameVersion}-{ForgeVersion}-installer.jar",
                    Path.Combine(forgeJarPath, "forge-installer.jar"));
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载加载器安装文件错误: " + e.Message, 0);
                Console.WriteLine(e);
                return new ForgeInstallResult(Status.Failed, GameVersion, ForgeVersion, customId, e);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            await ZipUtil.ExtractSpecificFileFromZipAsync(Path.Combine(forgeJarPath, "forge-installer.jar"),
                "install_profile.json",
                Path.Combine(forgeJarPath, "install_profile.json"));
            
            var installProfileJson = await File.ReadAllTextAsync(Path.Combine(forgeJarPath, "install_profile.json"), CancellationToken);
            
        }
        catch (OperationCanceledException)
        {
            OnProgressChanged?.Invoke("已取消安装", 0);
            return new ForgeInstallResult(Status.Cancel, GameVersion, ForgeVersion, customId);
        }
        catch (Exception e)
        {
            OnProgressChanged?.Invoke("安装 Forge 加载器错误: " + e.Message, 0);
            return new ForgeInstallResult(Status.Failed, GameVersion, ForgeVersion, customId, e);
        }

        OnProgressChanged?.Invoke("Forge 加载器安装完成", 100);
        return new ForgeInstallResult(Status.Succeeded, GameVersion, ForgeVersion, customId);
    }

    /// <summary>
    /// 获取指定 Minecraft 版本的所有 Forge 版本列表
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public static async Task<IEnumerable<ForgeVersionEntity>?> FetchForgeVersionsAsync(string version)
    {
        try
        {
            var json = await HttpUtil.GetJsonAsync($"https://bmclapi2.bangbang93.com/forge/minecraft/{version}");
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("版本列表为空");

            return json.ToJsonEntry<IEnumerable<ForgeVersionEntity>>();
        }
        catch (JsonException je)
        {
            throw new Exception("版本列表解析失败：" + je.Message, je);
        }
        catch (HttpRequestException hre)
        {
            throw new Exception("下载版本列表失败：" + hre.Message, hre);
        }
        catch (Exception e)
        {
            throw new Exception("获取版本列表时发生未知错误：" + e.Message, e);
        }
    }
}