using System.Text.Json;
using StarLight_Core.Models.Installer;

namespace StarLight_Core.Utilities
{
    public class InstallUtil
    {
        // 获取指定版本
        public static async Task<GameCoreDownloadInfo> GetGameCoreAsync(string id)
        {
            var gameCoresInfo = await GetGameCoresAsync();
            var gameCoreInfo = gameCoresInfo.FirstOrDefault(x => x.Id == id);
            
            if (gameCoreInfo == null)
            {
                throw new InvalidOperationException("[SL]版本不存在");
            }
            
            return gameCoreInfo;
        }
        
        // 获取版本列表
        public static async Task<IEnumerable<GameCoreDownloadInfo>> GetGameCoresAsync()
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }

                var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
                if (versionsManifest?.Version == null)
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }

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
        public static async Task<LatestGameCoreDownloadInfo> GetLatestGameCoreAsync()
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }

                var versionsManifest = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);
                if (versionsManifest?.Latest == null)
                {
                    throw new InvalidOperationException("[SL]版本列表为空");
                }else
                {
                    return new LatestGameCoreDownloadInfo
                    {
                        Release = versionsManifest.Latest.Release,
                        Snapshot = versionsManifest.Latest.Snapshot
                    };
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