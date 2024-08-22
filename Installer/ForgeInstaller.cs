using System.Text.Json;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class ForgeInstaller
    {
        private string Root { get; set; }
        
        private string GameVersion { get; set; }
        
        private string FabricVersion { get; set; }

        private CancellationToken CancellationToken { get; set; }
        
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public Action<string>? OnSpeedChanged { get; set; }
        
        public ForgeInstaller(string gameVersion, string fabricVersion, string root = ".minecraft", CancellationToken cancellationToken = default, Action<string>? onSpeedChanged = null, Action<string,int>? onProgressChanged = null)
        {
            GameVersion = gameVersion;
            FabricVersion = fabricVersion;
            OnSpeedChanged = onSpeedChanged;
            OnProgressChanged = onProgressChanged;
            CancellationToken = cancellationToken;
            Root = FileUtil.IsAbsolutePath(root)
                ? Path.Combine(root)
                : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), root);
        }
        
        public ForgeInstaller(string gameVersion, string fabricVersion, CancellationToken cancellationToken = default)
        {
            GameVersion = gameVersion;
            FabricVersion = fabricVersion;
            CancellationToken = cancellationToken;
            Root = Path.Combine(FileUtil.GetCurrentExecutingDirectory(), ".minecraft");
        }

        public async Task<FabricInstallResult> InstallAsync(string? customId = null)
        {
            throw new NotImplementedException();
        }
        
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