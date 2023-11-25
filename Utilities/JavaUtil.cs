using System.Diagnostics;
using Aurora_Star.Core.Models;
using Microsoft.VisualBasic;

namespace Aurora_Star.Core.Utilities
{
    public class JavaUtil
    {
        public static JavaInfo GetJavaInfo(string javaPath)
        {
            try
            {
                var javaExePath = Path.Combine(javaPath, "java.exe");
                var javawExePath = Path.Combine(javaPath, "javaw.exe");

                var javaFileInfo = File.Exists(javaExePath) ? new FileInfo(javaExePath) : new FileInfo(javawExePath);

                if (!javaFileInfo.Exists) return null;

                var fileVersionInfo = FileVersionInfo.GetVersionInfo(javaFileInfo.FullName);

                return new JavaInfo
                {
                    Is64Bit = GetIs64Bit(javaFileInfo.FullName),
                    JavaLibraryPath = javaFileInfo.Directory.FullName,
                    JavaSlugVersion = fileVersionInfo.ProductMajorPart,
                    JavaVersion = fileVersionInfo.ProductVersion,
                    JavaPath = javaFileInfo.FullName,
                };
            }
            catch (Exception ex)
            {
                return null!;
            }
        }

        private static bool GetIs64Bit(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    if (binaryReader.ReadUInt16() != 0x5A4D) // 检查MZ头
                        return false;

                    fileStream.Seek(0x3C, SeekOrigin.Begin); // PE头偏移量位于0x3C
                    var peHeaderOffset = binaryReader.ReadUInt32();
                    fileStream.Seek(peHeaderOffset, SeekOrigin.Begin);

                    if (binaryReader.ReadUInt32() != 0x00004550) // 检查PE头
                        return false;

                    fileStream.Seek(4, SeekOrigin.Current); // 跳过机器类型之前的4个字节

                    var machineType = binaryReader.ReadUInt16();
                    return machineType == 0x8664; // 0x8664 表示文件是64位
                }
            }
            catch
            {
                return false; // 如果读取失败，假定为非64位
            }
        }

        public static IEnumerable<JavaInfo> GetJavas()
        {
            List<string> results = new List<string>();
            var environmentPaths = Environment.GetEnvironmentVariable("Path").Split(Path.PathSeparator);

            foreach (var path in environmentPaths) {
                var trimmedPath = path.Trim().Trim('"');
                var fullPath = Path.Combine(trimmedPath, "javaw.exe");
                if (File.Exists(fullPath)) {
                    results.Add(trimmedPath);
                }
            }

            foreach (var drive in DriveInfo.GetDrives()) {
                SearchJavaInFolder(new DirectoryInfo(drive.Name), results);
            }

            SearchJavaInFolder(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), results);
            SearchJavaInFolder(new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase), results, true);

            var filteredResults = results.Where(path => !path.Contains("javapath_target_")).ToList();
            filteredResults.Sort();

            foreach (var path in filteredResults) {
                var javaInfo = GetJavaInfo(path);
                yield return new JavaInfo {
                    Is64Bit = javaInfo.Is64Bit,
                    JavaLibraryPath = path,
                    JavaSlugVersion = javaInfo.JavaSlugVersion,
                    JavaVersion = javaInfo.JavaVersion,
                    JavaPath = Path.Combine(path, "javaw.exe")
                };
            }

            string ExtractDirectoryName(string path)
            {
                if (path.EndsWith(":\\") || path.EndsWith(":\\\\"))
                {
                    return path.Substring(0, 1);
                }

                path = path.TrimEnd(new char[] { '\\', '/' });

                return ExtractFileName(path);
            }

            string ExtractFileName(string path)
            {
                if (path.EndsWith("\\") || path.EndsWith("/"))
                {
                    throw new Exception("路径不包含文件名: " + path);
                }

                path = path.Split(new char[] { '?', '\\', '/' }).Last();

                return path;
            }


            void SearchJavaInFolder(DirectoryInfo directory, List<string> results, bool isFullSearch = false)
            {
                try
                {
                    if (!directory.Exists)
                    {
                        return;
                    }

                    var javaExecutablePath = Path.Combine(directory.FullName, "javaw.exe");
                    if (File.Exists(javaExecutablePath))
                    {
                        results.Add(directory.FullName);
                    }

                    foreach (var subDirectory in directory.EnumerateDirectories())
                    {
                        if (!subDirectory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            var directoryNameLower = ExtractDirectoryName(subDirectory.Name).ToLower();
                            var searchTerms = new List<string>
                            {
                                "android", "bin", "cache", "client", "corretto", "craft", "data", "eclipse", "env", "file", "game",
                                "lib", "idea", "tools", "runtime", "jdk", "jre","pcl","html","users","software", "server", "hotspot",
                                "java", "jdk", "jbr", "jre", "jvm", "mc", "microsoft", "mojang", "net", "oracle", "program", "roaming", "run",
                                "services", "必备", "官启", "客户", "新建文件夹", "服务", "应用", "整合", "游戏", "环境", "软件", "运行", "前置", "世界"
                            };
                            if (isFullSearch || subDirectory.Parent?.Name.ToLower() == "users" || searchTerms.Any(term => directoryNameLower.Contains(term)))
                            {
                                SearchJavaInFolder(subDirectory, results);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // 异常处理
                }
            }
        }
    }
}

