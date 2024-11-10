using System.Diagnostics;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

public class JavaUtil
{
    // 获取 Java 信息
    public static JavaInfo GetJavaInfo(string javaPath)
    {
        try
        {
            var javaExePath = javaPath;
            var javawExePath = javaPath;

            if (FileUtil.IsDirectory(javaPath))
            {
                javaExePath = Path.Combine(javaPath, "javaw.exe");
                javawExePath = Path.Combine(javaPath, "javaw.exe");
            }

            var javaFileInfo = File.Exists(javaExePath) ? new FileInfo(javaExePath) : new FileInfo(javawExePath);

            if (!javaFileInfo.Exists) return null;

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(javaFileInfo.FullName);

            return new JavaInfo
            {
                Is64Bit = GetIs64Bit(javaFileInfo.FullName),
                JavaLibraryPath = javaFileInfo.Directory.FullName,
                JavaSlugVersion = fileVersionInfo.ProductMajorPart,
                JavaVersion = fileVersionInfo.ProductVersion,
                JavaPath = javaFileInfo.FullName
            };
        }
        catch (Exception ex)
        {
            return null!;
        }
    }

    // 判断是否为64位
    private static bool GetIs64Bit(string filePath)
    {
        const ushort mzSignature = 23117;
        const ushort peSignature = 17744;
        const int peHeaderOffsetPosition = 0x3A;
        const int machineTypeOffset = 20;
        const ushort machineTypeX64 = 523;
        const ushort machineTypeItanium = 267;

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);

            if (binaryReader.ReadUInt16() == mzSignature)
            {
                fileStream.Seek(peHeaderOffsetPosition, SeekOrigin.Current);
                fileStream.Seek(binaryReader.ReadUInt32(), SeekOrigin.Begin);

                if (binaryReader.ReadUInt32() == peSignature)
                {
                    fileStream.Seek(machineTypeOffset, SeekOrigin.Current);
                    var machineType = binaryReader.ReadUInt16();

                    return machineType == machineTypeX64 || machineType == machineTypeItanium;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        return false;
    }

    public static IEnumerable<JavaInfo> GetJavas()
    {
        var results = new List<string>();
        var environmentPaths = Environment.GetEnvironmentVariable("Path").Split(Path.PathSeparator);

        foreach (var path in environmentPaths)
        {
            var trimmedPath = path.Trim().Trim('"');
            var fullPath = Path.Combine(trimmedPath, "javaw.exe");
            if (File.Exists(fullPath)) results.Add(trimmedPath);
        }

        foreach (var drive in DriveInfo.GetDrives()) SearchJavaInFolder(new DirectoryInfo(drive.Name), results);

        SearchJavaInFolder(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            results);
        SearchJavaInFolder(new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), results, true);

        var filteredResults = results.Where(path => !path.Contains("javapath_target_")).ToList();
        filteredResults.Sort();

        foreach (var path in filteredResults)
        {
            var javaInfo = GetJavaInfo(path);
            yield return new JavaInfo
            {
                Is64Bit = javaInfo.Is64Bit,
                JavaLibraryPath = path,
                JavaSlugVersion = javaInfo.JavaSlugVersion,
                JavaVersion = javaInfo.JavaVersion,
                JavaPath = Path.Combine(path, "javaw.exe")
            };
        }

        string ExtractDirectoryName(string path)
        {
            if (path.EndsWith(":\\") || path.EndsWith(":\\\\")) return path.Substring(0, 1);

            path = path.TrimEnd('\\', '/');

            return ExtractFileName(path);
        }

        string ExtractFileName(string path)
        {
            if (path.EndsWith("\\") || path.EndsWith("/")) throw new Exception("路径不包含文件名: " + path);

            path = path.Split('?', '\\', '/').Last();

            return path;
        }


        void SearchJavaInFolder(DirectoryInfo directory, List<string> results, bool isFullSearch = false)
        {
            try
            {
                if (!directory.Exists) return;

                var javaExecutablePath = Path.Combine(directory.FullName, "javaw.exe");
                if (File.Exists(javaExecutablePath)) results.Add(directory.FullName);

                foreach (var subDirectory in directory.EnumerateDirectories())
                    if (!subDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        var directoryNameLower = ExtractDirectoryName(subDirectory.Name).ToLower();
                        var searchTerms = new List<string>
                        {
                            "android", "bin", "cache", "client", "corretto", "craft", "data", "eclipse", "env", "file",
                            "game",
                            "lib", "idea", "tools", "runtime", "jdk", "jre", "starry", "ssml", "launch", "launcher",
                            "pcl", "hmcl", "html", "users", "software", "server", "hotspot",
                            "java", "jdk", "jbr", "jre", "jvm", "mc", "minecraft", "microsoft", "mcl", "mclauncher",
                            "mojang", "net", "oracle", "program", "roaming", "run",
                            "services", "新建文件夹", "我的世界", "必备", "依赖", "官启", "客户", "服务", "应用", "整合", "游戏", "环境", "软件",
                            "运行", "前置", "世界"
                        };
                        if (isFullSearch || subDirectory.Parent?.Name.ToLower() == "users" ||
                            searchTerms.Any(term => directoryNameLower.Contains(term)))
                            SearchJavaInFolder(subDirectory, results);
                    }
            }
            catch (UnauthorizedAccessException)
            {
                // 异常处理
            }
        }
    }
}