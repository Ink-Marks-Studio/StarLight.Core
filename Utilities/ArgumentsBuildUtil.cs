using StarLight_Core.Models.Utilities;
using System.Text.Json;

namespace StarLight_Core.Utilities;

public class ArgumentsBuildUtil
{
    public static string BuildLibrariesArgs(string versionId, string root)
    {
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(versionId, root);
        string versionPath = coreInfo.root + "\\" + versionId + ".json";
        Console.WriteLine(versionPath);
        string librariesPath = "null";
        if (FileUtil.IsAbsolutePath(root))
        {
            librariesPath = root + "\\libraries";
        }
        else
        {
            librariesPath = FileUtil.GetCurrentExecutingDirectory() + "\\" + root + "\\libraries";
        }
        
        if (coreInfo.InheritsFrom != null!)
        {
            
        }
        else
        {
            var jsonData = File.ReadAllText(versionPath);
            var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);
            var cps = new List<string>();

            foreach (var lib in argsLibraries.Libraries)
            {
                if (lib.Downloads.Classifiers?.Count == 0 || lib.Downloads.Classifiers == null)
                {
                    var path = BuildUrlFromName(lib.Name, librariesPath);
                    if (!path.Contains("ca"))
                    {
                        cps.Add(path);
                    }
                }
            }

            return string.Join(";", cps);
        }

        return "";
    }
    
    // 构建 Libraries 路径
    private static string BuildUrlFromName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3) throw new ArgumentException("[SLC]名称格式无效,获取错误");
        
        return $"{root}\\{parts[0].Replace('.', '\\')}\\{parts[1]}\\{parts[2]}\\{parts[1]}-{parts[2]}.jar";
    }
}