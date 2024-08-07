using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using StarLight_Core.Models.Launch;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class FileUtil
    {
        // 获取文件大小
        public static long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            else
            {
                return 0;
            }
        }
        
        // 检测是否为相对路径
        public static bool IsAbsolutePath(string path) => Path.IsPathRooted(path);

        // 获取文件的完整路径
        public static string GetFullPath(string relativePath) => Path.GetFullPath(relativePath);
        
        // 获取 AppData 路径
        public static string GetAppDataPath() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        // 获取当前运行路径
        public static string GetCurrentExecutingDirectory() => AppDomain.CurrentDomain.BaseDirectory;
        
        // 是否为文件
        public static bool IsFile(string filePath) => File.Exists(filePath);
        
        // 是否为文件夹
        public static bool IsDirectory(string path, bool isCreate = false)
        {
            if (isCreate && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        
            return Directory.Exists(path);
        }
        
        // 修改语言
        public static void ModifyLangValue(string filePath, string gameId, string root, GameLanguage language = GameLanguage.zh_cn)
        {
            try
            {
                var gameInfo = GameCoreUtil.GetGameCore(gameId, root);
                var versionMember = GameCoreUtil.GetMajorVersion(gameInfo.Assets);
                
                string langCode = language.ToString().ToLower();
                if (versionMember <= 10 && versionMember != 0)
                    langCode = langCode.Substring(0, langCode.Length - 2) + langCode.Substring(langCode.Length - 2).ToUpper();
                
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    string updatedContent = Regex.Replace(content, @"lang:\w+", $"lang:{langCode}");
                    File.WriteAllText(filePath, updatedContent);
                }
                else
                {
                    File.WriteAllText(filePath, $"lang:{langCode}");
                }
            }
            catch (Exception x)
            {
                throw new Exception("修改设置异常: " + x.Message);
            }
        }
        
        // 重命名文件
        public static void RenameFile(string oldFileName, string newFileName)
        {
            if (File.Exists(oldFileName))
                File.Move(oldFileName, newFileName);
        }
        
        // 重命名文件夹
        public static void RenameDirectory(string oldDirectoryName, string newDirectoryName)
        {
            if (Directory.Exists(oldDirectoryName))
                Directory.Move(oldDirectoryName, newDirectoryName);
        }
        
        // 解压 Natives
        public static async Task DecompressionNatives(GameCoreConfig coreConfig)
        {
            try
            {
                string versionPath = Path.Join(coreConfig.Root, "versions", coreConfig.Version,
                    $"{coreConfig.Version}-natives");
                
                IsDirectory(versionPath, true);
                
                GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(coreConfig.Version, coreConfig.Root);
                string versionJsonPath = Path.Combine(coreInfo.root, $"{coreConfig.Version}.json");
                string librariesPath = IsAbsolutePath(coreConfig.Root) ? 
                    Path.Combine(coreConfig.Root, "libraries") : 
                    Path.Combine(GetCurrentExecutingDirectory(), coreConfig.Root, "libraries");

                var natives = ProcessNativesPath(versionJsonPath, librariesPath);
                
                
                foreach (var nativePath in natives)
                {
                    await ZipUtil.ExtractNativesFilesAsync(nativePath, versionPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[SL]处理 Natives 文件错误: + {ex}");
            }
        }
        
        private static IEnumerable<string> ProcessNativesPath(string filePath, string librariesPath)
        {
            var jsonData = File.ReadAllText(filePath);
            var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);

            foreach (var lib in argsLibraries.Libraries)
            {
                if (lib.Downloads != null)
                {
                    if (ShouldIncludeLibrary(lib.Rule))
                    {
                        if (lib.Downloads.Classifiers != null)
                        {
                            var path = ArgumentsBuildUtil.BuildNativesName(lib, librariesPath);
                            yield return path;
                        }
                        else
                        {
                            var parts = lib.Name.Split(':');
                            if (parts.Length <= 3)
                            {
                                continue;
                            }
                            var path = ArgumentsBuildUtil.BuildNewNativesName(lib.Name, librariesPath);
                            yield return path;
                        }
                    }
                }
            }
        }
        
        private static bool ShouldIncludeLibrary(LibraryJsonRule[] rules)
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