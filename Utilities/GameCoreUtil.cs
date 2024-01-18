using System.Text.Json;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class GameCoreUtil
    {
        
        // 获取游戏核心信息
        public static IEnumerable<GameCoreInfo> GetGameCores(string root = ".minecraft")
        {
            string rootPath = "null";
            
            FileUtil.IsDirectory(root, true);
            
            if (FileUtil.IsAbsolutePath(root))
            {
                rootPath = root + "\\versions";
            }
            else
            {
                rootPath = FileUtil.GetCurrentExecutingDirectory() + root + "\\versions";
            }

            FileUtil.IsDirectory(rootPath, true);
            
            var versions = Directory.GetDirectories(rootPath);
            List<GameCoreInfo> gameCores = new List<GameCoreInfo>();
            
            foreach (var version in versions)
            {
                var versionFolder = new DirectoryInfo(version);
                var versionFile = Path.Combine(versionFolder.FullName, versionFolder.Name + ".json");

                if (File.Exists(versionFile))
                {
                    try
                    {
                        var jsonData = File.ReadAllText(versionFile);
                        var gameCore = JsonSerializer.Deserialize<GameCoreVersionsJson>(jsonData);
                        GameCoreInfo gameCoreInfo = new GameCoreInfo();
                        
                        if (gameCore != null)
                        {
                            gameCoreInfo = new GameCoreInfo {
                                
                                Id = gameCore.Id,
                                Type = gameCore.Type,
                                JavaVersion = gameCore.JavaVersion?.MajorVersion ?? 0,
                                MainClass = gameCore.MainClass,
                                InheritsFrom = gameCore.InheritsFrom,
                                ReleaseTime = gameCore.ReleaseTime,
                                Time = gameCore.Time,
                                root = rootPath + "\\" + gameCore.Id,
                                Version = gameCore.ClientVersion
                            };
                        }
                        
                        if (gameCoreInfo.InheritsFrom != null && gameCoreInfo.InheritsFrom != "null")
                        {
                            gameCoreInfo.Version = gameCoreInfo.InheritsFrom;
                        }
                        else if (gameCoreInfo.Version == null)
                        {
                            gameCoreInfo.Version = gameCoreInfo.Id;
                        }
                        
                        try
                        {
                            if (gameCore.Arguments.Game != null)
                            {
                                gameCoreInfo.IsNewVersion = true;
                            }
                            else
                            {
                                gameCoreInfo.IsNewVersion = false;
                            }
                        }
                        catch (Exception e)
                        {
                            gameCoreInfo.IsNewVersion = false;
                        }
                        
                        gameCores.Add(gameCoreInfo);
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception($"[SL]版本 JSON 解析错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[SL]发生错误: {ex.Message}");
                    }
                }
            }
            return gameCores;
        }
        
        // 获取指定版本的游戏核心信息
        public static GameCoreInfo GetGameCore(string versionId, string root = ".minecraft")
        {
            string rootPath = "null";

            FileUtil.IsDirectory(root, true);
            
            if (FileUtil.IsAbsolutePath(root))
            {
                rootPath = root + "\\versions";
            }
            else
            {
                rootPath = FileUtil.GetCurrentExecutingDirectory() + root + "\\versions";
            }
            
            var versions = Directory.GetDirectories(rootPath);

            foreach (var version in versions)
            {
                var versionFolder = new DirectoryInfo(version);
                var versionFile = Path.Combine(versionFolder.FullName, versionFolder.Name + ".json");

                if (File.Exists(versionFile))
                {
                    try
                    {
                        var jsonData = File.ReadAllText(versionFile);
                        var gameCore = JsonSerializer.Deserialize<GameCoreVersionsJson>(jsonData);

                        if (gameCore != null && gameCore.Id == versionId)
                        {
                            GameCoreInfo gameCoreInfo = new GameCoreInfo
                            {
                                Id = gameCore.Id,
                                Type = gameCore.Type,
                                JavaVersion = gameCore.JavaVersion?.MajorVersion ?? 0,
                                MainClass = gameCore.MainClass,
                                InheritsFrom = gameCore.InheritsFrom,
                                ReleaseTime = gameCore.ReleaseTime,
                                Time = gameCore.Time,
                                IsNewVersion = gameCore.Arguments?.Game != null,
                                root = rootPath + "\\" + gameCore.Id,
                                Assets = gameCore.Assets,
                                MinecraftArguments = gameCore.MinecraftArguments,
                                Version = gameCore.ClientVersion
                            };

                            try
                            {
                                if (gameCoreInfo.InheritsFrom != null)
                                {
                                    gameCoreInfo.Assets = GetGameCore(gameCoreInfo.InheritsFrom, root).Assets;
                                }
                            }
                            catch (Exception e)
                            {
                                //
                            }

                            if (gameCoreInfo.InheritsFrom != null && gameCoreInfo.InheritsFrom != "null")
                            {
                                gameCoreInfo.Version = gameCoreInfo.InheritsFrom;
                            }
                            else if (gameCoreInfo.Version == null)
                            {
                                gameCoreInfo.Version = gameCoreInfo.Id;
                            }
                            
                            try
                            {
                                if (gameCore.Arguments.Game != null)
                                {
                                    gameCoreInfo.IsNewVersion = true;
                                    gameCoreInfo.Arguments = gameCore.Arguments;
                                }
                                else
                                {
                                    gameCoreInfo.IsNewVersion = false;
                                    gameCoreInfo.MinecraftArguments = gameCore.MinecraftArguments;
                                }
                            }
                            catch (Exception e)
                            {
                                gameCoreInfo.IsNewVersion = false;
                            }

                            return gameCoreInfo;
                        }
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception($"[SL]版本 JSON 解析错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[SL]发生错误: {ex.Message}");
                    }
                }
            }

            return null!;
        }
    }
}
