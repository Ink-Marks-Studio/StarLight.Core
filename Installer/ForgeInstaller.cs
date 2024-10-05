using System.Text.Json;
using StarLight_Core.Models.Installer;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    /// <summary>
    /// Forge 安装器
    /// </summary>
    public class ForgeInstaller : InstallerBase
    {
        private string GameVersion { get; set; }
        
        private string ForgeVersion { get; set; }

        private CancellationToken CancellationToken { get; set; }
        
        /// <summary>
        /// 带有进度报告的 Forge 安装器
        /// </summary>
        /// <param name="gameVersion">游戏版本</param>
        /// <param name="forgeVersion">加载器版本</param>
        /// <param name="root">游戏根目录</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="onSpeedChanged">速度报告</param>
        /// <param name="onProgressChanged">进度报告</param>
        public ForgeInstaller(string gameVersion, string forgeVersion, string root = ".minecraft", CancellationToken cancellationToken = default, Action<string>? onSpeedChanged = null, Action<string,int>? onProgressChanged = null)
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
        /// Forge 异步安装方法
        /// </summary>
        /// <param name="customId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ForgeInstallResult> InstallAsync(string? customId = null)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 获取指定 Minecraft 版本的所有 Forge 版本列表
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public static async Task<IEnumerable<ForgeVersionEntity>> FetchForgeVersionsAsync(string version)
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync($"https://bmclapi2.bangbang93.com/forge/minecraft/{version}");
                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException("[SL]版本列表为空");

                return json.ToJsonEntry<IEnumerable<ForgeVersionEntity>>();
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
        
        static string CalcMemoryMensurableUnit(double bytes)
        {
            double kb = bytes / 1024;
            double mb = kb / 1024;
            double gb = mb / 1024;
            double tb = gb / 1024;

            string result =
                tb > 1 ? $"{tb:0.##}TB" :
                gb > 1 ? $"{gb:0.##}GB" :
                mb > 1 ? $"{mb:0.##}MB" :
                kb > 1 ? $"{kb:0.##}KB" :
                $"{bytes:0.##}B";

            result = result.Replace("/", ".");
            return result;
        }
    }
}