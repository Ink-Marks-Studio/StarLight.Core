using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text.Json;
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

        public async Task InstallAsync(string? gameCoreName = null, CancellationToken cancellationToken = default)
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
                    OnProgressChanged?.Invoke($"版本已存在", 10);
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
                string jarDownloadPath = gameCoreVersionsJson.Downloads.Client.Url;
                string jarFilePath = Path.Combine(varPath, gameCoreName + ".jar");
                if (DownloadAPIs.Current != DownloadAPIs.Mojang)
                {
                    jarDownloadPath = $"{DownloadAPIs.Current.Root}/version/{GameId}/client";
                }

                Console.WriteLine(jarDownloadPath);
                
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
                    
                    var versionEntity = JsonSerializer.Deserialize<GameDownloadJsonEntity>(File.ReadAllText(jsonPath));
                    foreach (var versionDownload in versionEntity.Libraries)
                    {
                        
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
                OnProgressChanged?.Invoke("下载游戏核心文件错误: " + e.Message, 20);
            }
        }
    }
}

