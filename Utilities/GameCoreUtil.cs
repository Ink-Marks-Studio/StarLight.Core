using System.Text.Json;
using StarLight_Core.Enum;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class GameCoreUtil
    {
        /// <summary>
        /// 获取游戏核心信息
        /// </summary>
        /// <param name="root"></param>
        /// <returns>指定路径的游戏列表</returns>
        /// <exception cref="Exception"></exception>
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
                                IsNewVersion = gameCore.Arguments?.Game != null,
                                Time = gameCore.Time,
                                root = rootPath + "\\" + gameCore.Id,
                                Version = gameCore.ClientVersion,
                                LoaderType = GetLoader(gameCore.MainClass)
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
                                Version = gameCore.ClientVersion,
                                LoaderType = GetLoader(gameCore.MainClass)
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
                                if (gameCore.Arguments != null)
                                {
                                    gameCoreInfo.Arguments = gameCore.Arguments;
                                }
                                else
                                {
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

        internal static LoaderType GetLoader(string mainClass)
        {
            if (mainClass == "net.fabricmc.loader.impl.launch.knot.KnotClient")
                return LoaderType.Fabric;
            if (mainClass == "net.minecraft.launchwrapper.Launch")
                return LoaderType.Forge;
            else
                return LoaderType.Vanilla;
        }
        
        internal static int GetMajorVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length > 1)
            {
                return int.Parse(parts[1]);
            }
            return 0;
        }
    }
}
