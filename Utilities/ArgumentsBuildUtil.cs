using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
    
    private string userType;
    
    public ArgumentsBuildUtil(GameWindowConfig gameWindowConfig, GameCoreConfig gameCoreConfig, JavaConfig javaConfig, BaseAccount baseAccount)
    {
        GameWindowConfig = gameWindowConfig;
        GameCoreConfig = gameCoreConfig;
        JavaConfig = javaConfig;
        BaseAccount = baseAccount;
        VersionId = gameCoreConfig.Version;
        Root = gameCoreConfig.Root;
        userType = "msa";
    }

    // 参数构建器
    public List<string> Build()
    {
        List<string> arguments = new List<string>();
        
        arguments.Add(BuildMemoryArgs());
        arguments.Add(BuildJvmArgs());
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

    // Jvm 参数
    private string BuildJvmArgs()
    {
        List<string> args = new List<string>();
        
        args.Add(BuildGcAndAdvancedArguments());
        
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);

        var jvmPlaceholders = new Dictionary<string, string>
        {
            { "${natives_directory}", Root + "\\versions\\" + VersionId + "\\natives" },
            { "${launcher_name}", "StarLight" },
            { "${launcher_version}", "1" },
            { "${classpath}", BuildLibrariesArgs() }
        };

        string jvmArgumentTemplate = "";
        
        List<string> jvmArgumentsTemplate = new List<string>
        {
            "-Djava.library.path=${natives_directory}",
            "-Dminecraft.launcher.brand=${launcher_name}",
            "-Dminecraft.launcher.version=${launcher_version}",
            "-cp",
            "${classpath}"
        };
        
        if (coreInfo.IsNewVersion)
        {
            jvmArgumentsTemplate.Clear();
            foreach (var element in coreInfo.Arguments.Jvm)
            {
                if (!ElementContainsRules(element))
                {
                    jvmArgumentsTemplate.Add(element.ToString());
                }
            }
            
            jvmArgumentTemplate = string.Join(" ", jvmArgumentsTemplate);
        }
        else
        {
            jvmArgumentTemplate = string.Join(" ", jvmArgumentsTemplate);
        }
        
        args.Add(ReplacePlaceholders(jvmArgumentTemplate, jvmPlaceholders));
        
        return string.Join(" ", args);
    }
    
    // 游戏参数
    private string BuildGameArgs()
    {
        
        
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);
        
        var gamePlaceholders = new Dictionary<string, string>
        {
            { "${auth_player_name}", BaseAccount.Name },
            { "${version_name}", VersionId },
            { "${game_directory}", Path.Combine(CurrentExecutingDirectory(Root), "versions", VersionId) },
            { "${assets_root}", Path.Combine(CurrentExecutingDirectory(Root), "assets") },
            { "${assets_index_name}", coreInfo.Assets },
            { "${auth_uuid}", BaseAccount.Uuid },
            { "${auth_access_token}", BaseAccount.AccessToken },
            { "${clientid}", "${clientid}" },
            { "${auth_xuid}", "${auth_xuid}" },
            { "${user_type}", userType },
            { "${version_type}", "SL" }
        };
        
        string gameArgumentTemplate = "";
        
        List<string> gameArgumentsTemplate = new List<string>();
        
        if (coreInfo.IsNewVersion)
        {
            foreach (var element in coreInfo.Arguments.Game)
            {
                if (!ElementContainsRules(element))
                {
                    if (!ElementContainsRules(element))
                    {
                        gameArgumentsTemplate.Add(element.ToString());
                    }
                }
            }
            
            gameArgumentTemplate = string.Join(" ", gameArgumentsTemplate);
        }
        else
        {
            gameArgumentTemplate = coreInfo.MinecraftArguments;
        }

        return ReplacePlaceholders(gameArgumentTemplate, gamePlaceholders);
    }

    // Gc 与 Advanced 参数
    private string BuildGcAndAdvancedArguments()
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

    private static bool ElementContainsRules(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            return element.TryGetProperty("rules", out _);
        }
        return false;
    }
    
    static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
    {
        foreach (var placeholder in placeholders)
        {
            template = template.Replace(placeholder.Key, placeholder.Value);
        }

        return template;
    }
    
    private string BuildFromName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3) throw new ArgumentException("[SL]名称格式无效,获取错误");

        return Path.Combine(root, parts[0].Replace('.', '\\'), parts[1], parts[2], $"{parts[1]}-{parts[2]}.jar");
    }

    private string CurrentExecutingDirectory(string path)
    {
        return FileUtil.IsAbsolutePath(Root) ? 
            Path.Combine(Root) : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
    }
}