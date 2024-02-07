using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class MinecraftInstaller
    {
        public string GameId { get; set; }
        
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public string Root { get; set; }
        
        public MinecraftInstaller(string gameId, Action<string,int>? onProgressChanged = null, string root = ".minecraft")
        {
            GameId = gameId;
            OnProgressChanged = onProgressChanged;
        }

        public async Task InstallAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                OnProgressChanged?.Invoke("下载版本索引文件", 10);
                var versionsJson = await InstallUtil.GetGameCoreAsync(GameId);
                
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载版本索引文件错误",10);
            }
        }
    }
}

