using System.Linq.Expressions;
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

    // 参数构建器
    public List<string> Build()
    {
        List<string> arguments = new List<string>();
        
        arguments.Add(BuildMemoryArgs());
        
        arguments.Add(BuildLibrariesArgs());
        
        return arguments;
    }

    // 内存参数
    private string BuildMemoryArgs()
    {
        List<string> args = new List<string>();
        
        args.Add("-Xmx" + JavaConfig.MaxMemory + "M");
        args.Add("-Xmn" + JavaConfig.MinMemory + "M");

        return string.Join(" ",args);
    }

    private string BuildJvmArgs()
    {
        var allArguments = BuildArgsData.DefaultGcArguments.Concat(BuildArgsData.DefaultAdvancedArguments);

        if (!JavaConfig.DisabledOptimizationGcArgs)
        {
            allArguments = allArguments.Concat(BuildArgsData.OptimizationGcArguments);
        }
        if (!JavaConfig.DisabledOptimizationAdvancedArgs)
        {
            allArguments = allArguments.Concat(BuildArgsData.OptimizationAdvancedArguments);
        }
        
        return string.Join(" ", allArguments);
    }
    
    // 构建 ClassPath 参数
    private string BuildLibrariesArgs()
    {
        try
        {
            GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);
            string versionPath = Path.Combine(coreInfo.root, $"{VersionId}.json");
            string librariesPath = FileUtil.IsAbsolutePath(Root) ? 
                Path.Combine(Root, "libraries") : 
                Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root, "libraries");

            var cps = new List<string>();

            string InheritFromPath = coreInfo.InheritsFrom != null ?
                Path.Combine(Root, "versions", coreInfo.InheritsFrom, $"{coreInfo.InheritsFrom}.json") : 
                null;

            if (InheritFromPath != null)
            {
                cps.AddRange(ProcessLibraryPath(InheritFromPath, librariesPath));
            }

            cps.AddRange(ProcessLibraryPath(versionPath, librariesPath));

            return string.Join(";", cps);
        }
        catch (Exception ex)
        {
            throw new Exception($"[SL]构建Library参数错误: + {ex.Message}");
        }
    }

    private IEnumerable<string> ProcessLibraryPath(string filePath, string librariesPath)
    {
        var jsonData = File.ReadAllText(filePath);
        var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);

        foreach (var lib in argsLibraries.Libraries)
        {
            if (lib.Downloads.Classifiers == null || lib.Downloads.Classifiers.Count == 0)
            {
                var path = BuildFromName(lib.Name, librariesPath);
                if (!path.Contains("ca"))
                {
                    yield return path;
                }
            }
        }
    }

    
    private string BuildFromName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3) throw new ArgumentException("[SL]名称格式无效,获取错误");

        return Path.Combine(root, parts[0].Replace('.', '\\'), parts[1], parts[2], $"{parts[1]}-{parts[2]}.jar");
    }
}