using System.Text.Json;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class FabricInstaller
    {
        private string Root { get; set; }
        
        private string GameVersion { get; set; }
        
        private string FabricVersion { get; set; }

        private CancellationToken CancellationToken { get; set; }
        
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public Action<string>? OnSpeedChanged { get; set; }
        
        public FabricInstaller(string gameVersion, string fabricVersion, string root = ".minecraft", CancellationToken cancellationToken = default, Action<string>? onSpeedChanged = null, Action<string,int>? onProgressChanged = null)
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
        
        public FabricInstaller(string gameVersion, string fabricVersion, CancellationToken cancellationToken = default)
        {
            GameVersion = gameVersion;
            FabricVersion = fabricVersion;
            CancellationToken = cancellationToken;
            Root = Path.Combine(FileUtil.GetCurrentExecutingDirectory(), ".minecraft");
        }
        
        public async Task<FabricInstallResult> InstallAsync(string? customId = null)
        {
            string versionId = customId ?? $"{GameVersion}-fabric-loader-{FabricVersion}";
            string varPath = Path.Combine(Root, "versions", versionId);
            string jsonPath = Path.Combine(varPath, versionId + ".json");
            FileUtil.IsDirectory(varPath, true);
            
            try
            {
                OnProgressChanged?.Invoke("开始安装 Fabric 加载器", 0);

                if (FileUtil.IsFile(jsonPath))
                    return new FabricInstallResult(Status.Failed, GameVersion, FabricVersion, versionId,
                        new Exception("版本已存在"));
                
                OnProgressChanged?.Invoke("下载加载器索引文件", 20);
                if (CancellationToken != default)
                    CancellationToken.ThrowIfCancellationRequested();

                string fabricLoaderJson;
                try
                {
                    fabricLoaderJson = await HttpUtil.GetJsonAsync(DownloadAPIs.Current.FabricRoot + $"/v2/versions/loader/{GameVersion}/{FabricVersion}/profile/json");
                }
                catch (Exception e)
                {
                    OnProgressChanged?.Invoke("下载加载器索引文件错误: " + e.Message, 0);
                    return new FabricInstallResult(Status.Failed, GameVersion, FabricVersion, customId, e);
                }
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var gameCoreData = JsonSerializer.Deserialize<Dictionary<string, object>>(fabricLoaderJson, options);
                    
                if (gameCoreData != null && gameCoreData.ContainsKey("id"))
                    gameCoreData["id"] = versionId;
                            
                string modifiedJson = JsonSerializer.Serialize(gameCoreData, options);
                await File.WriteAllTextAsync(jsonPath, modifiedJson, CancellationToken);
                
                var fabricLoaderEntity = JsonSerializer.Deserialize<GameDownloadJsonEntity>(fabricLoaderJson);
                
                OnProgressChanged?.Invoke("下载加载器核心文件", 60);
                if (CancellationToken != default)
                    CancellationToken.ThrowIfCancellationRequested();

                var librariesDownloader = new DownloadsUtil();
                var downloadList = new List<DownloadItem>();
                var failedList = new List<DownloadItem>();

                foreach (var versionDownload in fabricLoaderEntity.Libraries)
                {
                    var basePath = MinecraftInstallerModel.BuildFromName(versionDownload.Name, Path.DirectorySeparatorChar.ToString());
                    var jarFilePath = Path.Combine(Root, "libraries") + basePath;
                    var jarDownloadPath = DownloadAPIs.Current.FabricMaven + basePath.Replace("\\", "/");
                    
                    if (!FileUtil.IsFile(jarFilePath))
                        downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                    
                    librariesDownloader.OnSpeedChanged = (double speed) =>
                    {
                        OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed));
                    };
                
                    librariesDownloader.ProgressChanged = (int downloaded, int total) =>
                    {
                        OnProgressChanged?.Invoke($"下载加载器核心文件: {downloaded}/{total}", 60);
                    };
                
                    librariesDownloader.DownloadFailed = (DownloadItem item) =>
                    {
                        failedList.Add(item);
                    };

                    await librariesDownloader.DownloadFiles(downloadList, CancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new FabricInstallResult(Status.Cancel, GameVersion, FabricVersion, customId);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("安装 Fabric 加载器错误: " + e.Message, 0);
                return new FabricInstallResult(Status.Failed, GameVersion, FabricVersion, customId, e);
            }
            
            OnProgressChanged?.Invoke("Fabric 加载器安装完成", 100);
            return new FabricInstallResult(Status.Succeeded, GameVersion, FabricVersion, customId);
        }
        
        public static async Task<IEnumerable<FabricVersionEntity>> FetchFabricVersionsAsync(string version)
        {
            try
            {
                var json = await HttpUtil.GetJsonAsync(DownloadAPIs.Current.FabricRoot + $"/v2/versions/loader/{version}");
                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidOperationException("[SL]版本列表为空");
                
                var result = json.ToJsonEntry<IEnumerable<FabricVersionJsonEntity>>().Select(manifest =>
                    new FabricVersionEntity
                    {
                        Intermediary = manifest.Intermediary,
                        Loader = manifest.Loader,
                        LauncherMeta = manifest.LauncherMeta,
                        GameVersion = manifest.Intermediary.Version,
                        Version = manifest.Loader.Version
                    }).ToList();

                return result;
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