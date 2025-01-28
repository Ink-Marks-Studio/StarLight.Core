using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using StarLight_Core.Models.Installer;

namespace StarLight_Core.Utilities;

/// <summary>
/// 安装工具
/// </summary>
public static class InstallUtil
{
    /// <summary>
    /// 获取指定版本
    /// </summary>
    /// <param name="id">版本 Id</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">版本不存在</exception>
    public static async Task<GameCoreDownloadInfo> GetGameCoreAsync(string id)
    {
        var gameCoresInfo = await GetGameCoresAsync();
        var gameCoreInfo = gameCoresInfo.FirstOrDefault(x => x.Id == id);

        if (gameCoreInfo == null) throw new InvalidOperationException("版本不存在");

        return gameCoreInfo;
    }

    /// <summary>
    /// 获取版本列表
    /// </summary>
    /// <returns>版本列表</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static async Task<IEnumerable<GameCoreDownloadInfo>> GetGameCoresAsync()
    {
        try
        {
            var json = await HttpUtil.GetJsonAsync(DownloadAPIs.Current.VersionManifest);
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidOperationException("版本列表为空");

            var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
            if (versionsManifest?.Version == null) throw new InvalidOperationException("版本列表为空");

            return versionsManifest.Version.Select(x => new GameCoreDownloadInfo
            {
                Id = x.Id,
                Type = x.Type,
                Url = x.Url,
                Time = x.Time,
                ReleaseTime = x.ReleaseTime,
                Sha1 = x.Sha1
            });
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

    /// <summary>
    /// 获取最新版本
    /// </summary>
    /// <returns>最新正式版与快照版</returns>
    /// <exception cref="InvalidOperationException">无法获取版本</exception>
    /// <exception cref="Exception"></exception>
    public static async Task<LatestGameCoreDownloadInfo> GetLatestGameCoreAsync()
    {
        try
        {
            var json = await HttpUtil.GetJsonAsync(DownloadAPIs.Current.VersionManifest);
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidOperationException("版本列表为空");

            var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
            if (versionsManifest?.Latest == null)
                throw new InvalidOperationException("版本列表为空");
            return new LatestGameCoreDownloadInfo
            {
                Release = versionsManifest.Latest.Release,
                Snapshot = versionsManifest.Latest.Snapshot
            };
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