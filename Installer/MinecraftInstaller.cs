using System.ComponentModel;
using System.Net.Mime;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using StarLight_Core.Downloader;
using StarLight_Core.Enum;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;
using Download = StarLight_Core.Downloader.Download;

namespace StarLight_Core.Installer
{
    public class MinecraftInstaller
    {
        private string GameId { get; set; }
        
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public Action<string>? OnSpeedChanged { get; set; }
        
        private string Root { get; set; }
        
        private string GamePath { get; set; }
        
        private DownloadService _downloadService;
        
        public MinecraftInstaller(string gameId, string root = ".minecraft", Action<string,int>? onProgressChanged = null, Action<string>? onSpeedChanged = null)
        {
            Root = root;
            GameId = gameId;
            OnProgressChanged = onProgressChanged;
            OnSpeedChanged = onSpeedChanged;
            _downloadService = CreateDownloadService(DownloadConfig.DownloadOptions);
        }
        
        // 创建下载服务
        private DownloadService CreateDownloadService(DownloadConfiguration config)
        {
            var downloadService = new DownloadService(config);
            downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
            return downloadService;
        }
        
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(e.BytesPerSecondSpeed));
        }

        public async Task InstallAsync(string? gameCoreName = null, bool mandatory = false, CancellationToken cancellationToken = default)
        {
            try
            {
                OnProgressChanged?.Invoke("开始安装", 0);
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                
                if (gameCoreName == null)
                {
                    gameCoreName = GameId;
                }
                
                GamePath = FileUtil.IsAbsolutePath(Root)
                    ? Path.Combine(Root)
                    : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("初始化游戏安装错误: " + e.Message, 0);
                return;
            }

            string varPath = Path.Combine(GamePath, "versions", gameCoreName);
            string jsonPath = Path.Combine(varPath, gameCoreName + ".json");
            
            try
            {
                OnProgressChanged?.Invoke("下载版本索引文件", 10);
                var versionsJson = await InstallUtil.GetGameCoreAsync(GameId);

                if (!FileUtil.IsDirectory(varPath, true) || !FileUtil.IsFile(jsonPath))
                {
                    string gameCoreJson;
                    
                    if (DownloadAPIs.Current.Source == DownloadSource.Official)
                         gameCoreJson = await HttpUtil.GetJsonAsync(versionsJson.Url);
                    else
                        gameCoreJson = await HttpUtil.GetJsonAsync($"{DownloadAPIs.Current.Root}/version/{GameId}/json");
                    
                    await File.WriteAllTextAsync(jsonPath, gameCoreJson, cancellationToken);
                    
                    if (!HashUtil.VerifySha1(gameCoreJson, versionsJson.Sha1))   
                    {
                        OnProgressChanged?.Invoke("下载版本索引文件失败", 10);
                        return;
                    }
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    var gameCoreData = JsonSerializer.Deserialize<Dictionary<string, object>>(gameCoreJson, options);
                    
                    if (gameCoreData != null && gameCoreData.ContainsKey("id"))
                        gameCoreData["id"] = gameCoreName;
                    
                    string modifiedJson = JsonSerializer.Serialize(gameCoreData, options);
                    await File.WriteAllTextAsync(jsonPath, modifiedJson, cancellationToken);
                }
                else
                {
                    if (mandatory)
                    {
                        try
                        {
                            File.Delete(jsonPath);
                            string gameCoreJson;
                    
                            if (DownloadAPIs.Current.Source == DownloadSource.Official)
                                gameCoreJson = await HttpUtil.GetJsonAsync(versionsJson.Url);
                            else
                                gameCoreJson = await HttpUtil.GetJsonAsync($"{DownloadAPIs.Current.Root}/version/{GameId}/json");
                    
                            await File.WriteAllTextAsync(jsonPath, gameCoreJson, cancellationToken);
                    
                            if (!HashUtil.VerifySha1(gameCoreJson, versionsJson.Sha1))   
                            {
                                OnProgressChanged?.Invoke("下载版本索引文件失败", 10);
                                return;
                            }
                    
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                WriteIndented = true
                            };
                            var gameCoreData = JsonSerializer.Deserialize<Dictionary<string, object>>(gameCoreJson, options);
                    
                            if (gameCoreData != null && gameCoreData.ContainsKey("id"))
                                gameCoreData["id"] = gameCoreName;
                            
                            string modifiedJson = JsonSerializer.Serialize(gameCoreData, options);
                            await File.WriteAllTextAsync(jsonPath, modifiedJson, cancellationToken);
                        }
                        catch (IOException e)
                        {
                            OnProgressChanged?.Invoke("无法删除文件: " + e.Message, 10);
                            return;
                        }
                    }
                    else
                    {
                        OnProgressChanged?.Invoke($"版本已存在", 10);
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke($"下载版本索引文件失败: {e}", 10);
                return;
            }

            try
            {
                OnProgressChanged?.Invoke("下载游戏核心", 20);
                
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                
                GameCoreVersionsJson gameCoreVersionsJson = JsonSerializer.Deserialize<GameCoreVersionsJson>(File.ReadAllText(jsonPath));
                string coreJarSha1 = gameCoreVersionsJson.Downloads.Client.Sha1;
                string jarFilePath = Path.Combine(varPath, gameCoreName + ".jar");
                if (!FileUtil.IsFile(jarFilePath))
                {
                    string jarDownloadPath = gameCoreVersionsJson.Downloads.Client.Url;

                    if (DownloadAPIs.Current.Source == DownloadSource.Official)
                        jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";

                    await _downloadService.DownloadFileTaskAsync(jarDownloadPath, jarFilePath, cancellationToken);
                }
                else if (!HashUtil.VerifyFileHash(jarFilePath, coreJarSha1, SHA1.Create()))
                {
                    string jarDownloadPath = gameCoreVersionsJson.Downloads.Client.Url;

                    if (DownloadAPIs.Current.Source == DownloadSource.Official)
                        jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";
                
                    await _downloadService.DownloadFileTaskAsync(jarDownloadPath, jarFilePath, cancellationToken);
                
                    _downloadService.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏核心错误: " + e.Message, 20);
                return;
            }

            var failedList = new List<DownloadItem>();
            
            try
            {
                OnProgressChanged?.Invoke("下载游戏核心文件", 40);
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                string jsonContent = File.ReadAllText(jsonPath);
                var versionEntity = JsonSerializer.Deserialize<GameDownloadJsonEntity>(jsonContent);

                var librariesDownloader = new DownloadsUtil();
                var downloadList = new List<DownloadItem>();
                int i = 0;
                
                foreach (var versionDownload in versionEntity.Libraries)
                {
                    if (versionDownload?.Downloads != null)
                    {
                        if (ShouldIncludeLibrary(versionDownload.Rule))    
                        {
                            if (versionDownload.Downloads.Artifact != null)
                            {
                                var basePath = MinecraftInstallerModel.BuildFromName(versionDownload.Name, Path.DirectorySeparatorChar.ToString());
                                var jarFilePath = Path.Combine(GamePath, "libraries") + basePath;
                                var jarDownloadPath = DownloadAPIs.Current.Maven + basePath.Replace("\\", "/");

                                if (!FileUtil.IsFile(jarFilePath))
                                    downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                                else if (DownloaderConfig.VerificationFile)
                                {
                                    if (!HashUtil.VerifyFileHash(jarFilePath, versionDownload.Downloads.Artifact.Sha1, SHA1.Create())) 
                                        downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                                }
                                else if (FileUtil.GetFileSize(jarFilePath) != versionDownload.Downloads.Artifact.Size)
                                    downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                            }
                            
                            if (versionDownload.Natives != null)
                            {
                                string nativeTemplate = versionDownload.Natives["windows"];
                                string native = nativeTemplate.Replace("${arch}", SystemUtil.GetOperatingSystemBit().ToString());
                                
                                Native nativesWindows = versionDownload.Downloads.Classifiers[native];
                                
                                var jarFilePathClassifiers = Path.Combine(GamePath, "libraries", nativesWindows.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));
                                var jarDownloadPathClassifiers = DownloadAPIs.Current.Maven + "/" + nativesWindows.Path;
                                
                                if (!FileUtil.IsFile(jarFilePathClassifiers))
                                    downloadList.Add(new DownloadItem(jarDownloadPathClassifiers, jarFilePathClassifiers));
                                else if (DownloaderConfig.VerificationFile)
                                {
                                    if (!HashUtil.VerifyFileHash(jarFilePathClassifiers, nativesWindows.Sha1, SHA1.Create()))
                                        downloadList.Add(new DownloadItem(jarDownloadPathClassifiers, jarFilePathClassifiers));
                                }
                                else if (FileUtil.GetFileSize(jarFilePathClassifiers) != nativesWindows.Size)
                                {
                                    downloadList.Add(new DownloadItem(jarDownloadPathClassifiers, jarFilePathClassifiers));
                                }
                            }
                        }
                    }
                }
                
                librariesDownloader.OnSpeedChanged = (double speed) =>
                {
                    OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed));
                };
                
                librariesDownloader.ProgressChanged = (int downloaded, int total) =>
                {
                    OnProgressChanged?.Invoke($"下载游戏核心文件: {downloaded}/{total}", 40);
                };
                
                librariesDownloader.DownloadFailed = (DownloadItem item) =>
                {
                    failedList.Add(item);
                };

                await librariesDownloader.DownloadFiles(downloadList, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏核心文件错误: " + e.Message, 40);
            }
            
            try
            {
                OnProgressChanged?.Invoke("下载游戏资源索引", 60);
                
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                string jsonContent = File.ReadAllText(jsonPath);
                var assetsEntity = JsonSerializer.Deserialize<AssetsJsonEntity>(jsonContent);
                string assetsJsonContent;
                if (DownloadAPIs.Current.Source == DownloadSource.Official)
                    assetsJsonContent = await HttpUtil.GetJsonAsync(assetsEntity.AssetIndex.Url);
                else
                    assetsJsonContent = await HttpUtil.GetJsonAsync(assetsEntity.AssetIndex.Url.Replace(DownloadAPIs.Official.Assets, DownloadAPIs.Current.Assets));
                
                var assetsPath = Path.Combine(GamePath, "assets");
                var assetsIndex = Path.Combine(assetsPath, "indexes");
                var assetsJsonPath = Path.Combine(assetsIndex, assetsEntity.AssetIndex.Id + ".json");
                
                if (!FileUtil.IsDirectory(assetsIndex, true) || !FileUtil.IsFile(jsonPath))
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(assetsJsonContent), options);
                    
                    await File.WriteAllTextAsync(assetsJsonPath, formattedJson, cancellationToken);
                }
                else
                {
                    File.Delete(assetsJsonPath);
                    
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string formattedJson = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(assetsJsonContent), options);
                    
                    await File.WriteAllTextAsync(assetsJsonPath, formattedJson, cancellationToken);
                }
                
                var assetsInfo = JsonSerializer.Deserialize<AssetData>(assetsJsonContent);

                OnProgressChanged?.Invoke("下载游戏资源", 80);
                
                var assetsDownloader = new DownloadsUtil();
                var assetsDownloadList = new List<DownloadItem>();
                
                var seenAssets = new HashSet<string>();

                if (assetsInfo != null && assetsInfo.Objects != null)
                {
                    foreach (var kvp in assetsInfo.Objects)
                    {
                        string baseAssetsPath = $"{kvp.Value.Hash.Substring(0, 2)}/{kvp.Value.Hash}";
                        string downloadUrl = DownloadAPIs.Current.Assets + "/" + baseAssetsPath;
                        string localPath = Path.Combine(GamePath, "assets", "objects", baseAssetsPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        
                        // 去重
                        if (!seenAssets.Contains(downloadUrl))
                        {
                            seenAssets.Add(downloadUrl);
                            if (!FileUtil.IsFile(localPath))
                                assetsDownloadList.Add(new DownloadItem(downloadUrl, localPath));
                            else if (DownloaderConfig.VerificationFile)
                            {
                                if (!HashUtil.VerifyFileHash(localPath, kvp.Value.Hash, SHA1.Create()))
                                {
                                    assetsDownloadList.Add(new DownloadItem(downloadUrl, localPath));
                                }
                            }
                            else if (FileUtil.GetFileSize(localPath) != kvp.Value.Size)
                                assetsDownloadList.Add(new DownloadItem(downloadUrl, localPath));
                        }
                    }
                }
                
                assetsDownloader.OnSpeedChanged = (double speed) =>
                    OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed));
                
                assetsDownloader.ProgressChanged = (int downloaded, int total) =>
                    OnProgressChanged?.Invoke($"下载游戏资源文件: {downloaded}/{total}", 80);
                
                assetsDownloader.DownloadFailed = (DownloadItem item) => 
                    failedList.Add(item);

                await assetsDownloader.DownloadFiles(assetsDownloadList, cancellationToken);
                
                OnProgressChanged?.Invoke("安装已完成 版本 : " + GameId, 100);
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏资源文件错误: " + e.Message, 80);
            }
        }
        
        private bool ShouldIncludeLibrary(LibraryJsonRule[] rules)
        {
            if (rules == null || rules.Length == 0)
            {
                return true;
            }

            bool isAllow = false;
            bool isDisallowForOsX = false;
            bool isDisallowForLinux = false;

            foreach (var rule in rules)
            {
                if (rule.Action == "allow")
                {
                    if (rule.Os == null || (rule.Os.Name.ToLower() != "linux" && rule.Os.Name.ToLower() != "osx"))
                    {
                        isAllow = true;
                    }
                }
                else if (rule.Action == "disallow")
                {
                    if (rule.Os != null && rule.Os.Name.ToLower() == "linux")
                    {
                        isDisallowForLinux = true;
                    }
                    if (rule.Os != null && rule.Os.Name.ToLower() == "osx")
                    {
                        isDisallowForOsX = true;
                    }
                }
            }
            return !isDisallowForLinux && (isDisallowForOsX || isAllow);
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

