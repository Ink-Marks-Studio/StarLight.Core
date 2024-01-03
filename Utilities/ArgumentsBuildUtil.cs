using StarLight_Core.Models.Utilities;
using System.Text.Json;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;

namespace StarLight_Core.Utilities;

public class ArgumentsBuildUtil
{
    public string VersionId { get; set; }
    
    public string Root { get; set; }
    
    public BaseAccount BaseAccount { get; set; }
        
    public GameWindowConfig GameWindowConfig { get; set; }
        
    public GameCoreConfig GameCoreConfig { get; set; }
        
    public JavaConfig JavaConfig { get; set; }

    
    public ArgumentsBuildUtil(GameWindowConfig gameWindowConfig, GameCoreConfig gameCoreConfig, JavaConfig javaConfig, BaseAccount baseAccount)
    {
        GameWindowConfig = gameWindowConfig;
        GameCoreConfig = gameCoreConfig;
        JavaConfig = javaConfig;
        BaseAccount = baseAccount;
        VersionId = gameCoreConfig.Version;
        Root = gameCoreConfig.Root;
    }
    
    public string BuildLibrariesArgs()
    {
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);
        string versionPath = coreInfo.root + "\\" + VersionId + ".json";
        string librariesPath = "null";
        string InheritFromPath = "null";
        if (FileUtil.IsAbsolutePath(Root))
        {
            librariesPath = Root + "\\libraries";
            InheritFromPath = Root + "\\versions\\" + coreInfo.InheritsFrom + "\\" + coreInfo.InheritsFrom + ".json";
        }
        else
        {
            librariesPath = FileUtil.GetCurrentExecutingDirectory() + "\\" + Root + "\\libraries";
            InheritFromPath = FileUtil.GetCurrentExecutingDirectory() + "\\" + Root + "\\versions\\" + coreInfo.InheritsFrom + "\\" + coreInfo.InheritsFrom + ".json";
        }
        
        if (coreInfo.InheritsFrom != null)
        {
            var fromJsonData = File.ReadAllText(InheritFromPath);
            var fromArgsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(fromJsonData);
            var cps = new List<string>();
            
            foreach (var lib in fromArgsLibraries.Libraries)
            {
                if (lib.Downloads.Classifiers == null || lib.Downloads.Classifiers.Count == 0)
                {
                    var path = BuildUrlFromName(lib.Name, librariesPath);
                    if (!path.Contains("ca"))
                    {
                        cps.Add(path);
                    }
                }
            }
            
            var jsonData = File.ReadAllText(versionPath);
            var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);
            foreach (var lib in argsLibraries.Libraries)
            {
                var path = BuildUrlFromName(lib.Name, librariesPath);
                cps.Add(path);
            }
            return string.Join(";", cps);
        }
        else
        {
            var jsonData = File.ReadAllText(versionPath);
            var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);
            var cps = new List<string>();

            foreach (var lib in argsLibraries.Libraries)
            {
                if (lib.Downloads.Classifiers == null || lib.Downloads.Classifiers.Count == 0)
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
    private string BuildUrlFromName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3) throw new ArgumentException("[SLC]名称格式无效,获取错误");
        
        return $"{root}\\{parts[0].Replace('.', '\\')}\\{parts[1]}\\{parts[2]}\\{parts[1]}-{parts[2]}.jar";
    }
}