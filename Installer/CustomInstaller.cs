using StarLight_Core.Downloader;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer;

/// <summary>
/// 自定义安装器
/// </summary>
public class CustomInstaller : InstallerBase
{
    /// <summary>
    /// 自定义安装器
    /// </summary>
    /// <param name="gameVersion">游戏版本</param>
    /// <param name="customId">自定义名称</param>
    /// <param name="savePath">安装路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    public CustomInstaller(string gameVersion, string savePath, string? customId = null,
        CancellationToken cancellationToken = default)
    {
        GameVersion = gameVersion;
        Root = savePath;
        CustomId = customId ?? gameVersion;
        CancellationToken = cancellationToken;
    }

    private string GameVersion { get; }

    private string? CustomId { get; }

    private CancellationToken CancellationToken { get; }

    /// <summary>
    /// 游戏自定义安装器
    /// </summary>
    /// <returns></returns>
    public async Task<InstallResult> MinecraftInstall()
    {
        try
        {
            var multiThreadedDownloader =
                new MultiThreadedFileDownloader(x => { OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(x)); },
                    CancellationToken);

            OnProgressChanged?.Invoke("开始安装", 0);
            if (CancellationToken != default)
                CancellationToken.ThrowIfCancellationRequested();
            FileUtil.IsDirectory(FileUtil.GetFileDirectory(Root), true);
            if (!FileUtil.IsFile(Root))
                return new InstallResult(Status.Failed, GameVersion, CustomId, new Exception("文件已存在"));

            OnProgressChanged?.Invoke("下载版本索引文件", 30);
            if (CancellationToken != default)
                CancellationToken.ThrowIfCancellationRequested();
            var versionsJson = await InstallUtil.GetGameCoreAsync(GameVersion);

            string gameCoreJson;
            if (DownloadAPIs.Current.Source == DownloadSource.Official)
                gameCoreJson = await HttpUtil.GetJsonAsync(versionsJson.Url);
            else
                gameCoreJson = await HttpUtil.GetJsonAsync($"{DownloadAPIs.Current.Root}/version/{GameVersion}/json");

            OnProgressChanged?.Invoke("下载游戏核心", 70);
            if (CancellationToken != default)
                CancellationToken.ThrowIfCancellationRequested();
            var jarDownloadPath = gameCoreJson.ToJsonEntry<GameCoreVersionsJson>().Downloads.Client.Url;

            if (DownloadAPIs.Current.Source == DownloadSource.Official)
                jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameVersion}/client";

            await multiThreadedDownloader.DownloadFileWithMultiThread(jarDownloadPath, Root);

            OnProgressChanged?.Invoke("安装已完成", 100);
            return new InstallResult(Status.Succeeded, GameVersion, CustomId);
        }
        catch (OperationCanceledException)
        {
            OnProgressChanged?.Invoke("已取消安装", 0);
            return new InstallResult(Status.Cancel, GameVersion, CustomId);
        }
        catch (Exception e)
        {
            OnProgressChanged?.Invoke("初始化游戏安装错误", 0);
            return new InstallResult(Status.Failed, GameVersion, CustomId, e);
        }
    }
}