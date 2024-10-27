using StarLight_Core.Downloader;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;

namespace StarLight_Core.Installer
{
    /// <summary>
    /// 自定义安装器
    /// </summary>
    public class CustomInstaller : InstallerBase
    {
        private string GameVersion { get; set; }
        
        private string? CustomId { get; set; }    
        
        private CancellationToken CancellationToken { get; set; }
        
        /// <summary>
        /// 自定义安装器
        /// </summary>
        /// <param name="gameVersion">游戏版本</param>
        /// <param name="customId">自定义名称</param>
        /// <param name="savePath">安装路径</param>
        /// <param name="cancellationToken">取消令牌</param>
        public CustomInstaller(string gameVersion, string savePath, string? customId = null, CancellationToken cancellationToken = default)
        {
            GameVersion = gameVersion;
            Root = savePath;
            CustomId = customId ?? gameVersion;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// 游戏自定义安装器
        /// </summary>
        /// <returns></returns>
        public async Task<InstallResult> MinecraftInstall()
        {
            var multiThreadedDownloader = new MultiThreadedFileDownloader(x =>
            {
                OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(x));
            }, CancellationToken);
            
            return new InstallResult(Status.Succeeded, GameVersion, CustomId);
        }
    }
}