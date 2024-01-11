using System.Reflection;
using System.Text.RegularExpressions;

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
        string executablePath = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(executablePath);
    }
    
    // 检测是否为文件夹
    public static bool IsDirectory(string path, bool isCreate = false) 
    {
        if (isCreate) {
            Directory.CreateDirectory(path);
        }

        return Directory.Exists(path);
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
}