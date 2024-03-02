using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using StarLight_Core.Models.Launch;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

public class FileUtil
{
    // 检测是否为相对路径
    public static bool IsAbsolutePath(string path)
    {
        return Path.IsPathRooted(path);
    }

    // 获取文件的完整路径
    public static string GetFullPath(string relativePath)
    {
        return Path.GetFullPath(relativePath);
    }
    
    // 获取当前运行路径
    public static string GetCurrentExecutingDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }
    
    public static bool IsFile(string filePath)
    {
        return File.Exists(filePath);
    }
    
    // 检测是否为文件夹
    public static bool IsDirectory(string path, bool isCreate = false)
    {
        bool isDir = Directory.Exists(path);
        
        if (isCreate) 
        {
            Directory.CreateDirectory(path);
        }

        return isDir;
    }
    
    // 修改语言
    public static void ModifyLangValue(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                string updatedContent = Regex.Replace(content, @"lang:\w+", "lang:zh_cn");

                File.WriteAllText(filePath, updatedContent);
            }
            else
            {
                File.WriteAllText(filePath, "lang:zh_cn");
            }
        }
        catch (Exception x)
        {
            throw new("修改设置异常: " + x);
        }
    }
    
    // 重命名文件
    public static void RenameFile(string oldFileName, string newFileName)
    {
        if (File.Exists(oldFileName))
        {
            File.Move(oldFileName, newFileName);
        }
    }
    
    // 重命名文件夹
    public static void RenameDirectory(string oldDirectoryName, string newDirectoryName)
    {
        if (Directory.Exists(oldDirectoryName))
        {
            Directory.Move(oldDirectoryName, newDirectoryName);
        }
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
            string librariesPath = FileUtil.IsAbsolutePath(coreConfig.Root) ? 
                Path.Combine(coreConfig.Root, "libraries") : 
                Path.Combine(FileUtil.GetCurrentExecutingDirectory(), coreConfig.Root, "libraries");

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
                if (lib.Downloads.Classifiers != null)
                {
                    if (ShouldIncludeLibrary(lib.Rule))
                    {
                        var path = ArgumentsBuildUtil.BuildNativesName(lib.Name, librariesPath);
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