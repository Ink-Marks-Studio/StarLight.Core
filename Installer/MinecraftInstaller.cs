using System.Text.Json;
using StarLight_Core.Models.Installer;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class MinecraftInstaller
    {
        // 获取版本列表
        public static async Task<IEnumerable<GameCoreDownloadInfo>> GetGameCoresAsync()
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync("https://piston-meta.mojang.com/mc/game/version_manifest.json").ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }

                var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
                if (versionsManifest?.Version == null)
                {
                    return Enumerable.Empty<GameCoreDownloadInfo>();
                }

                return versionsManifest.Version.Select(x => new GameCoreDownloadInfo
                {
                    Id = x.Id,
                    Type = x.Type,
                    Url = x.Url,
                    Time = x.Time,
                    ReleaseTime = x.ReleaseTime
                });
            }
            catch (JsonException je)
            {
                throw new Exception("[SL]版本列表解析失败：" + je.Message, je);
            }
            catch (HttpRequestException hre)
            {
                throw new Exception("[SL]下载版本列表失败：" + hre.Message, hre);
            }
            catch (Exception e)
            {
                throw new Exception("[SL]获取版本列表时发生未知错误：" + e.Message, e);
            }
        }
        
        // 获取最新版本
        public static async Task<GameCoreDownloadInfo> GetLatestGameCoreAsync()
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync("https://piston-meta.mojang.com/mc/game/version_manifest.json").ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }

                var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
                if (versionsManifest?.Latest == null)
                {
                    return null;
                }else
                {
                    return null;
                }
            }
            catch (JsonException je)
            {
                throw new Exception("[SL]版本列表解析失败：" + je.Message, je);
            }
            catch (HttpRequestException hre)
            {
                throw new Exception("[SL]下载版本列表失败：" + hre.Message, hre);
            }
            catch (Exception e)
            {
                throw new Exception("[SL]获取版本列表时发生未知错误：" + e.Message, e);
            }
        }
    }
}

