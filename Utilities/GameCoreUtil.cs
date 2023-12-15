using System.Text.Json;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class GameCoreUtil
    {
        
        // 获取游戏核心信息
        public static IEnumerable<GameCoreInfo> GetGameCores(string root = ".minecraft")
        {
            string rootPath = root + "\\versions";
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
                                JavaVersion = (gameCore.JavaVersion?.MajorVersion).Value,
                                MainClass = gameCore.MainClass,
                                InheritsFrom = gameCore.InheritsFrom,
                                ReleaseTime = gameCore.ReleaseTime,
                                Time = gameCore.Time,
                            };
                        }

                        // 判断是否为 1.12.2 以上版本 
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
                        
                        // 添加到列表
                        gameCores.Add(gameCoreInfo);
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception($"版本 JSON 解析错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"发生错误: {ex.Message}");
                    }
                }
            }
            return gameCores;
        }
        
        // 获取指定版本的游戏核心信息
        public static GameCoreInfo GetGameCore(string versionId, string root = ".minecraft")
        {
            string rootPath = root + "\\versions";
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
                                JavaVersion = gameCore.JavaVersion?.MajorVersion ?? 0, // 安全地处理可空类型
                                MainClass = gameCore.MainClass,
                                InheritsFrom = gameCore.InheritsFrom,
                                ReleaseTime = gameCore.ReleaseTime,
                                Time = gameCore.Time,
                                IsNewVersion = gameCore.Arguments?.Game != null // 简化判断逻辑
                            };

                            return gameCoreInfo;
                        }
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception($"版本 JSON 解析错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"发生错误: {ex.Message}");
                    }
                }
            }

            return null!; // 如果未找到指定版本，则返回 null
        }
    }
}
