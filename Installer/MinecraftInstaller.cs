using System.Security.Cryptography;
using System.Text.Json;
using StarLight_Core.Downloader;
using StarLight_Core.Enum;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class MinecraftInstaller : InstallerBase
    {
        private string GameId { get; set; }
        
        private string GamePath { get; set; }
        
        public MinecraftInstaller(string gameId, string root = ".minecraft", Action<string,int>? onProgressChanged = null, Action<string>? onSpeedChanged = null)
        {
            Root = root;
            GameId = gameId;
            OnProgressChanged = onProgressChanged;
            OnSpeedChanged = onSpeedChanged;
            GamePath = FileUtil.IsAbsolutePath(Root)
                ? Path.Combine(Root)
                : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
        }

        public async Task<InstallResult> InstallAsync(string? gameCoreName = null, bool mandatory = false, CancellationToken cancellationToken = default)
        {
            var multiThreadedDownloader = new MultiThreadedFileDownloader(x =>
            {
                OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(x));
            }, cancellationToken);
            
            try
            {
                OnProgressChanged?.Invoke("开始安装", 0);
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();

                gameCoreName ??= GameId;
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("初始化游戏安装错误", 0);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
            }

            var varPath = Path.Combine(GamePath, "versions", gameCoreName);
            var jsonPath = Path.Combine(varPath, gameCoreName + ".json");
            
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
                        return new InstallResult(Status.Failed, GameId, gameCoreName, new Exception("下载版本索引文件失败"));
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
                                return new InstallResult(Status.Failed, GameId, gameCoreName, new Exception("下载版本索引文件失败"));
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
                            return new InstallResult(Status.Failed, GameId, gameCoreName, e);
                        }
                    }
                    else
                    {
                        OnProgressChanged?.Invoke("版本已存在", 10);
                        return new InstallResult(Status.Failed, GameId, gameCoreName, new Exception("版本已存在"));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke($"下载版本索引文件失败: {e}", 10);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
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

                    await multiThreadedDownloader.DownloadFileWithMultiThread(jarDownloadPath, jarFilePath);
                }
                else if (!HashUtil.VerifyFileHash(jarFilePath, coreJarSha1, SHA1.Create()))
                {
                    string jarDownloadPath = gameCoreVersionsJson.Downloads.Client.Url;

                    if (DownloadAPIs.Current.Source == DownloadSource.Official)
                        jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";
                
                    await multiThreadedDownloader.DownloadFileWithMultiThread(jarDownloadPath, jarFilePath);
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏核心错误", 20);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
            }

            var failedList = new List<DownloadItem>();
            
            try
            {
                OnProgressChanged?.Invoke("下载游戏核心文件", 30);
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                string jsonContent = await File.ReadAllTextAsync(jsonPath, cancellationToken);
                var versionEntity = JsonSerializer.Deserialize<GameDownloadJsonEntity>(jsonContent);

                var librariesDownloader = new MultiFileDownloader();
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
                
                librariesDownloader.OnSpeedChanged = speed =>
                {
                    OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed));
                };
                
                librariesDownloader.ProgressChanged = (downloaded, total) =>
                {
                    OnProgressChanged?.Invoke($"下载游戏核心文件: {downloaded}/{total}", 30);
                };
                
                librariesDownloader.DownloadFailed = item =>
                {
                    failedList.Add(item);
                };

                await librariesDownloader.DownloadFiles(downloadList, cancellationToken);
                librariesDownloader.Dispose();
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏核心文件错误", 30);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
            }
            
            try
            {
                OnProgressChanged?.Invoke("下载游戏资源索引", 50);
                
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                string jsonContent = await File.ReadAllTextAsync(jsonPath, cancellationToken);
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

                OnProgressChanged?.Invoke("下载游戏资源", 60);
                
                var assetsDownloader = new MultiFileDownloader();
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
                        if (!seenAssets.Add(downloadUrl)) continue;
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
                
                assetsDownloader.OnSpeedChanged = speed =>
                    OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed));
                
                assetsDownloader.ProgressChanged = (downloaded, total) =>
                    OnProgressChanged?.Invoke($"下载游戏资源文件: {downloaded}/{total}", 60);
                
                assetsDownloader.DownloadFailed = item => 
                    failedList.Add(item);

                await assetsDownloader.DownloadFiles(assetsDownloadList, cancellationToken);
                assetsDownloader.Dispose();
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏资源文件错误", 60);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
            }
            
            try
            {
                OnProgressChanged?.Invoke("补全游戏资源", 80);
                
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                failedList.Clear();
                var assetsDownloader = new MultiFileDownloader
                {
                    OnSpeedChanged = speed =>
                        OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed)),
                    ProgressChanged = (downloaded, total) =>
                        OnProgressChanged?.Invoke($"补全游戏资源文件: {downloaded}/{total}", 80),
                    DownloadFailed = item => 
                        failedList.Add(item)
                };

                await assetsDownloader.DownloadFiles(failedList, cancellationToken);
                assetsDownloader.Dispose();
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("补全游戏资源文件错误", 80);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
            }
            
            try
            {
                OnProgressChanged?.Invoke("安装结束前检查", 90);
                
                if (cancellationToken != default)
                    cancellationToken.ThrowIfCancellationRequested();
                
                var assetsDownloader = new MultiFileDownloader
                {
                    OnSpeedChanged = speed =>
                        OnSpeedChanged?.Invoke(CalcMemoryMensurableUnit(speed))
                };

                await assetsDownloader.DownloadFiles(failedList, cancellationToken);
                assetsDownloader.Dispose();
                
                OnProgressChanged?.Invoke("安装已完成", 100);
                return new InstallResult(Status.Succeeded, GameId, gameCoreName);
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return new InstallResult(Status.Cancel, GameId, gameCoreName);
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("安装结束前检查文件错误", 90);
                return new InstallResult(Status.Failed, GameId, gameCoreName, e);
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
    }
}

