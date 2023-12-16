using System.Reflection;

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
}