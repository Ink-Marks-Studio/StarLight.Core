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
            var json = await HttpUtil.GetJsonAsync("https://piston-meta.mojang.com/mc/game/version_manifest.json");

            var gameCoresInfo = JsonSerializer.Deserialize<GameCoreJsonEntity>(json);

            if (gameCoresInfo != null)
            {
                return gameCoresInfo.Version.Select(x => new GameCoreDownloadInfo
                {
                    Id = x.Id,
                    Type = x.Type,
                    Url = x.Url,
                    Time = x.Time,
                    ReleaseTime = x.ReleaseTime
                });
            }
            else
            {
                return null;
            }
        }
    }
}

