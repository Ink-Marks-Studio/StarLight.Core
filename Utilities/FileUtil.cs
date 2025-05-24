using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using StarLight_Core.Enum;
using StarLight_Core.Models.Launch;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

/// <summary>
/// 文件工具类
/// </summary>
public static class FileUtil
{
    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件大小</returns>
    public static long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath)) return 0;
        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    /// <summary>
    /// 对称加密写入文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileMode">文件类型</param>
    /// <param name="key">密钥</param>
    /// <param name="content">内容</param>
    /// <exception cref="Exception"></exception>
    public static Status CryptoWriteFile(string filePath, FileMode fileMode,byte[] key,string content)
    {
        try
        {
            using (FileStream fileStream = new("TestData.txt", fileMode))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    byte[] iv = aes.IV;
                    fileStream.Write(iv, 0, iv.Length);
                    using (CryptoStream cryptoStream = new(
                               fileStream,
                               aes.CreateEncryptor(),
                               CryptoStreamMode.Write))
                    {
                        using (StreamWriter encryptWriter = new(cryptoStream))
                        {
                            encryptWriter.WriteLine(content);
                        }
                    }
                }
            }

            return Status.Succeeded;
        }
        catch (Exception ex)
        {
            throw ex.InnerException ?? ex;
        }
    }
    
    /// <summary>
    /// 对称加密读取文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="key">密钥</param>
    /// <exception cref="Exception"></exception>
    public async static Task<string> CryptoeReadFile(string filePath,byte[] key)
    {
        try
        {
            using (FileStream fileStream = new(filePath, FileMode.Open))
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] iv = new byte[aes.IV.Length];
                    int numBytesToRead = aes.IV.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                        if (n == 0) break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }

                    using (CryptoStream cryptoStream = new(
                               fileStream,
                               aes.CreateDecryptor(key, iv),
                               CryptoStreamMode.Read))
                    {
                        using (StreamReader decryptReader = new(cryptoStream))
                        {
                            return await decryptReader.ReadToEndAsync();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    /// <summary>
    /// 检测是否为相对路径
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>是否为相对路径</returns>
    public static bool IsAbsolutePath(string path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// 获取文件的完整路径
    /// </summary>
    /// <param name="relativePath">文件相对路径</param>
    /// <returns>文件的完整路径</returns>
    public static string GetFullPath(string relativePath)
    {
        return Path.GetFullPath(relativePath);
    }

    /// <summary>
    /// 获取 AppData 路径
    /// </summary>
    /// <returns>AppData 路径</returns>
    public static string GetAppDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }

    /// <summary>
    /// 获取当前运行路径
    /// </summary>
    /// <returns>当前运行路径</returns>
    public static string GetCurrentExecutingDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// 获取系统临时文件夹路径
    /// </summary>
    /// <returns>临时文件夹路径</returns>
    public static string GetTempDirectory()
    {
        return Path.GetTempPath();
    }

    /// <summary>
    /// 是否为文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否为文件</returns>
    public static bool IsFile(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 是否为文件夹
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="isCreate">是否创建</param>
    /// <returns>是否为文件夹</returns>
    public static bool IsDirectory(string path, bool isCreate = false)
    {
        if (isCreate && !Directory.Exists(path))
            Directory.CreateDirectory(path);

        return Directory.Exists(path);
    }

    /// <summary>
    /// 获取文件所在目录
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件所在目录</returns>
    public static string? GetFileDirectory(string filePath) => Path.GetDirectoryName(filePath);


    /// <summary>
    /// 重命名文件
    /// </summary>
    /// <param name="oldFileName">文件名</param>
    /// <param name="newFileName">新文件名</param>
    public static void RenameFile(string oldFileName, string newFileName)
    {
        if (File.Exists(oldFileName))
            File.Move(oldFileName, newFileName);
    }

    /// <summary>
    /// 重命名文件夹
    /// </summary>
    /// <param name="oldDirectoryName">文件夹名</param>
    /// <param name="newDirectoryName">新文件夹名</param>
    public static void RenameDirectory(string oldDirectoryName, string newDirectoryName)
    {
        if (Directory.Exists(oldDirectoryName))
            Directory.Move(oldDirectoryName, newDirectoryName);
    }

    // 修改语言
    internal static void ModifyLangValue(string filePath, string gameId, string root,
        GameLanguage language = GameLanguage.zh_cn)
    {
        try
        {
            var gameInfo = GameCoreUtil.GetGameCore(gameId, root);
            var versionMember = GameCoreUtil.GetMajorVersion(gameInfo.Assets);

            var langCode = language.ToString().ToLower();
            if (versionMember <= 10 && versionMember != 0)
                langCode = string.Concat(langCode.AsSpan(0, langCode.Length - 2), langCode[^2..].ToUpper());

            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);
                var updatedContent = Regex.Replace(content, @"lang:\w+", $"lang:{langCode}");
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

    // 解压 Natives
    internal static async Task DecompressionNatives(GameCoreConfig coreConfig)
    {
        try
        {
            var versionPath = Path.Join(coreConfig.Root, "versions", coreConfig.Version,
                $"{coreConfig.Version}-natives");

            IsDirectory(versionPath, true);

            var coreInfo = GameCoreUtil.GetGameCore(coreConfig.Version, coreConfig.Root);
            var versionJsonPath = Path.Combine(coreInfo.root, $"{coreConfig.Version}.json");
            var librariesPath = IsAbsolutePath(coreConfig.Root)
                ? Path.Combine(coreConfig.Root, "libraries")
                : Path.Combine(GetCurrentExecutingDirectory(), coreConfig.Root, "libraries");

            var natives = ProcessNativesPath(versionJsonPath, librariesPath);


            foreach (var nativePath in natives) await ZipUtil.ExtractNativesFilesAsync(nativePath, versionPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"处理 Natives 文件错误: + {ex}");
        }
    }

    private static IEnumerable<string> ProcessNativesPath(string filePath, string librariesPath)
    {
        var jsonData = File.ReadAllText(filePath);
        var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);

        foreach (var lib in argsLibraries.Libraries)
            if (lib.Downloads != null)
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
                        if (parts.Length <= 3) continue;
                        var path = ArgumentsBuildUtil.BuildNewNativesName(lib.Name, librariesPath);
                        if (string.IsNullOrEmpty(path)) continue;
                        yield return path;
                    }
                }
    }

    internal static bool ShouldIncludeLibrary(LibraryJsonRule[] rules)
    {
        if (rules == null || rules.Length == 0) return true;

        bool isAllowed = false;
        foreach (var rule in rules)
        {
            // 检查当前规则是否匹配操作系统
            bool isRuleMatched = CheckIfRuleMatchesCurrentOs(rule.Os);
            if (isRuleMatched)
            {
                // 匹配的规则直接决定结果（最后一个匹配的规则生效）
                isAllowed = (rule.Action == "allow");
            }
        }

        return isAllowed;
    }

    private static bool CheckIfRuleMatchesCurrentOs(Os osRule)
    {
        if (osRule == null) return true; // 无 OS 限制的规则始终匹配

        string currentOs = GetCurrentOsName(); // 返回小写的 "windows", "linux", "osx"
        return osRule.Name.ToLower() == currentOs;
    }

    private static string GetCurrentOsName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "osx";
        return "unknown";
    }
}