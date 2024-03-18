using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;
using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class MinecraftInstaller
    {
        public string GameId { get; set; }
        
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public Action<double>? OnSpeedChanged { get; set; }
        
        public string Root { get; set; }
        
        private string GamePath { get; set; }
        
        public MinecraftInstaller(string gameId, string root = ".minecraft", Action<string,int>? onProgressChanged = null, Action<double>? onSpeedChanged = null)
        {
            Root = root;
            GameId = gameId;
            OnProgressChanged = onProgressChanged;
            OnSpeedChanged = onSpeedChanged;
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
                    
                    if (DownloadAPIs.Current == DownloadAPIs.Mojang)
                    {
                         gameCoreJson = await HttpUtil.GetJsonAsync(versionsJson.Url);
                    }
                    else
                    {
                        gameCoreJson = await HttpUtil.GetJsonAsync($"{DownloadAPIs.Current.Root}/version/{GameId}/json");
                    }
                    
                    await File.WriteAllTextAsync(jsonPath, gameCoreJson, cancellationToken);
                    
                    if (!HashUtil.VerifySha1(gameCoreJson, versionsJson.Sha1))   
                    {
                        OnProgressChanged?.Invoke("下载版本索引文件失败", 10);
                        return;
                    }
                    
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var gameCoreData = JsonSerializer.Deserialize<Dictionary<string, object>>(gameCoreJson, options);
                    
                    if (gameCoreData != null && gameCoreData.ContainsKey("id"))
                    {
                        gameCoreData["id"] = gameCoreName;
                    }
                    
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
                    
                            if (DownloadAPIs.Current == DownloadAPIs.Mojang)
                            {
                                gameCoreJson = await HttpUtil.GetJsonAsync(versionsJson.Url);
                            }
                            else
                            {
                                gameCoreJson = await HttpUtil.GetJsonAsync($"{DownloadAPIs.Current.Root}/version/{GameId}/json");
                            }
                    
                            await File.WriteAllTextAsync(jsonPath, gameCoreJson, cancellationToken);
                    
                            if (!HashUtil.VerifySha1(gameCoreJson, versionsJson.Sha1))   
                            {
                                OnProgressChanged?.Invoke("下载版本索引文件失败", 10);
                                return;
                            }
                    
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var gameCoreData = JsonSerializer.Deserialize<Dictionary<string, object>>(gameCoreJson, options);
                    
                            if (gameCoreData != null && gameCoreData.ContainsKey("id"))
                            {
                                gameCoreData["id"] = gameCoreName;
                            }
                    
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

                    if (DownloadAPIs.Current != DownloadAPIs.Mojang)
                    {
                        jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";
                    }
                
                    var jarDownloader = new DownloadsUtil();
                
                    Action<double> progressChanged = (double speed) =>
                    {
                        OnSpeedChanged?.Invoke(speed / 1024);
                    };
                    var jarDownloadstatus =
                        await jarDownloader.DownloadAsync(new DownloadItem(jarDownloadPath, jarFilePath), null,
                            progressChanged);
                
                    if (jarDownloadstatus.Status != Status.Succeeded)
                    {
                        OnProgressChanged?.Invoke("下载游戏核心失败", 20);
                        return;
                    }
                }
                else if (!HashUtil.VerifyFileHash(jarFilePath, coreJarSha1, SHA1.Create()))
                {
                    string jarDownloadPath = gameCoreVersionsJson.Downloads.Client.Url;

                    if (DownloadAPIs.Current != DownloadAPIs.Mojang)
                    {
                        jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";
                    }
                
                    var jarDownloader = new DownloadsUtil();
                
                    Action<double> progressChanged = (double speed) =>
                    {
                        OnSpeedChanged?.Invoke(speed / 1024);
                    };
                    var jarDownloadstatus =
                        await jarDownloader.DownloadAsync(new DownloadItem(jarDownloadPath, jarFilePath), null,
                            progressChanged);
                
                    if (jarDownloadstatus.Status != Status.Succeeded)
                    {
                        OnProgressChanged?.Invoke("下载游戏核心失败", 20);
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
                OnProgressChanged?.Invoke("下载游戏核心错误: " + e.Message, 20);
                return;
            }

            try
            {
                OnProgressChanged?.Invoke("下载游戏核心文件", 20);
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                
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
                            var basePath = MinecraftInstallerModel.BuildFromName(versionDownload.Name, Path.DirectorySeparatorChar.ToString());
                            var jarFilePath = Path.Combine(GamePath, "libraries") + basePath;
                            var jarDownloadPath = DownloadAPIs.Current.Maven + basePath.Replace("\\", "/");

                            if (!FileUtil.IsFile(jarFilePath))
                            {
                                downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                            }
                            else if (!HashUtil.VerifyFileHash(jarFilePath, versionDownload.Downloads.Artifact.Sha1, SHA1.Create()))
                            {
                                downloadList.Add(new DownloadItem(jarDownloadPath, jarFilePath));
                            }
                            
                            if (versionDownload.Downloads.Classifiers != null)
                            {
                                Native nativesWindows = versionDownload.Downloads.Classifiers["natives-windows"];
                                
                                var jarFilePathClassifiers = Path.Combine(GamePath, "libraries", nativesWindows.Path.Replace("/", Path.DirectorySeparatorChar.ToString()));
                                var jarDownloadPathClassifiers = DownloadAPIs.Current.Maven + "/" + nativesWindows.Path;
                                
                                if (!FileUtil.IsFile(jarFilePathClassifiers))
                                {
                                    downloadList.Add(new DownloadItem(jarDownloadPathClassifiers, jarFilePathClassifiers));
                                }
                                else if (!HashUtil.VerifyFileHash(jarFilePathClassifiers, nativesWindows.Sha1, SHA1.Create()))
                                {
                                    downloadList.Add(new DownloadItem(jarDownloadPathClassifiers, jarFilePathClassifiers));
                                }
                            }
                        }
                    }
                }
                
                Action<double> progressChanged = (double speed) =>
                {
                    OnSpeedChanged?.Invoke(speed / 1024);
                };
                
                Action<int, int> downloadCompleted = (int downloaded, int total) =>
                {
                    OnProgressChanged?.Invoke($"下载游戏核心文件: {downloaded}/{total}", 20);
                };
                
                Action<string> downloadFailed = (string path) =>
                {
                    OnProgressChanged?.Invoke($"下载游戏核心文件: {path}", 20);
                };
                
                var jarDownloadstatus = await librariesDownloader.DownloadFilesAsync(downloadList, null, progressChanged, downloadCompleted, downloadFailed);
                if (jarDownloadstatus.Status != Status.Succeeded)
                {
                    OnProgressChanged?.Invoke("下载游戏核心文件失败" + jarDownloadstatus.Exception, 20);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏核心文件错误: " + e.Message, 20);
            }
            
            try
            {
                OnProgressChanged?.Invoke("下载游戏资源", 20);
                var assetsJson = HttpUtil.GetJsonAsync("");
                if (cancellationToken != default)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                OnProgressChanged?.Invoke("已取消安装", 0);
                return;
            }
            catch (Exception e)
            {
                OnProgressChanged?.Invoke("下载游戏资源文件错误: " + e.Message, 20);
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

