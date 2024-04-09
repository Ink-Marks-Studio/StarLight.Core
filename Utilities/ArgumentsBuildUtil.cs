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

    private string jarPath;
    
    public ArgumentsBuildUtil(GameWindowConfig gameWindowConfig, GameCoreConfig gameCoreConfig, JavaConfig javaConfig, BaseAccount baseAccount)
    {
        GameWindowConfig = gameWindowConfig;
        GameCoreConfig = gameCoreConfig;
        JavaConfig = javaConfig;
        BaseAccount = baseAccount;
        VersionId = gameCoreConfig.Version;
        Root = gameCoreConfig.Root;
        userType = "Mojang";
        jarPath = GetVersionJarPath();
    }

    // 参数构建器
    public List<string> Build()
    {
        List<string> arguments = new List<string>();
        
        arguments.Add(BuildMemoryArgs());
        arguments.Add(BuildJvmArgs());
        arguments.Add(BuildGameArgs());
        arguments.Add(BuildWindowArgs());
        
        return arguments;
    }

    // 内存参数
    private string BuildMemoryArgs()
    {
        List<string> args = new List<string>();
        
        args.Add("-Xmn" + JavaConfig.MinMemory + "M");
        args.Add("-Xmx" + JavaConfig.MaxMemory + "M");

        return string.Join(" ",args);
    }

    // Jvm 参数
    private string BuildJvmArgs()
    {
        ProcessAccount();
        
        List<string> args = new List<string>();
        
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);

        if (BaseAccount is UnifiedPassAccount)
        {
            string authPath = FileUtil.IsAbsolutePath(GameCoreConfig.Nide8authPath) ? 
                Path.Combine(GameCoreConfig.Nide8authPath) : 
                Path.Combine(FileUtil.GetCurrentExecutingDirectory(), GameCoreConfig.Nide8authPath);
            args.Add("-javaagent:\"" + authPath + "\"=" + GameCoreConfig.UnifiedPassServerId);
        }

        if (coreInfo.IsNewVersion)
        {
            args.Add(BuildClientJarArgs());
        }
        
        if (SystemUtil.IsOperatingSystemGreaterThanWin10())
        {
            args.Add(BuildSystemArgs());
        }
        
        args.Add(BuildGcAndAdvancedArguments());
        
        string rootPath = FileUtil.IsAbsolutePath(Root) ? 
            Path.Combine(Root) : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
        string nativesPath = Path.Combine(rootPath, "versions", VersionId, "natives");
        
        if (!Directory.Exists(nativesPath))
        {
            string[] nativesDirectories = Directory.GetDirectories(coreInfo.root, "*natives*", SearchOption.AllDirectories);
            
            if (nativesDirectories.Length > 0)
            {
                nativesPath = nativesDirectories[0];
            }
        }
        
        var jvmPlaceholders = new Dictionary<string, string>
        {
            { "${natives_directory}", nativesPath },
            { "${launcher_name}", "StarLight" },
            { "${launcher_version}", "1" },
            { "${classpath}", BuildLibrariesArgs() },
            { "${version_name}", coreInfo.Id},
            { "${library_directory}", Path.Combine(rootPath, "libraries") },
            { "${classpath_separator}", ";" }
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
            
            List<string> updatedJvmArguments = new List<string>();
            foreach (var argument in jvmArgumentsTemplate)
            {
                updatedJvmArguments.Add(argument.Replace(" ", ""));
            }
            
            jvmArgumentsTemplate = updatedJvmArguments;
            jvmArgumentTemplate = string.Join(" ", jvmArgumentsTemplate);
        }
        else
        {
            jvmArgumentTemplate = string.Join(" ", jvmArgumentsTemplate);
        }
        
        args.Add(ReplacePlaceholders(jvmArgumentTemplate, jvmPlaceholders));
        
        return string.Join(" ", args);
    }
    
    private string BuildClientJarArgs() => "-Dminecraft.client.jar=" + jarPath;
    
    // 系统参数
    private string BuildSystemArgs()
    {
        List<string> args = new List<string>();
        
        if (SystemUtil.IsOperatingSystemGreaterThanWin10())
        {
            args.Add("-Dos.name=\"Windows 10\"");
            args.Add("-Dos.version=10.0");
        }

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
            { "${assets_root}", Path.Combine(CurrentExecutingDirectory(Root), "assets") },
            { "${assets_index_name}", coreInfo.Assets },
            { "${auth_uuid}", BaseAccount.Uuid.Replace("-","") },
            { "${auth_access_token}", BaseAccount.AccessToken },
            { "${clientid}", "${clientid}" },
            { "${auth_xuid}", "${auth_xuid}" },
            { "${user_type}", userType },
            { "${version_type}", $"\"SL/{TextUtil.ToTitleCase(coreInfo.Type)}\"" }
        };

        string gameDirectory = GameCoreConfig.IsVersionIsolation ?
            Path.Combine(CurrentExecutingDirectory(Root), "versions", VersionId) :
            Path.Combine(CurrentExecutingDirectory(Root));

        gamePlaceholders.Add("${game_directory}", gameDirectory);
        
        string gameArgumentTemplate = "";
        
        List<string> gameArgumentsTemplate = new List<string>();
        
        gameArgumentsTemplate.Add(coreInfo.MainClass);
        
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
        }
        else
        {
            gameArgumentsTemplate.Add(coreInfo.MinecraftArguments);
        }
        
        return ReplacePlaceholders(string.Join(" ", gameArgumentsTemplate), gamePlaceholders);
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

            if (coreInfo.InheritsFrom != null && coreInfo.InheritsFrom != "null")
            {
                cps.Add(Path.Combine(Root, "versions", coreInfo.InheritsFrom, $"{coreInfo.InheritsFrom}.jar"));
            }
            else
            {
                cps.Add(Path.Combine(coreInfo.root, $"{VersionId}.jar"));
            }

            return string.Join(";", cps);
        }
        catch (Exception ex)
        {
            throw new Exception($"[SL]构建Library参数错误: + {ex}");
        }
    }

    // 窗口参数
    private string BuildWindowArgs()
    {
        List<string> args = new List<string>();
        
        args.Add("--width " + GameWindowConfig.Width);
        args.Add("--height " + GameWindowConfig.Height);
        if (GameWindowConfig.IsFullScreen)
        {
            args.Add("--fullscreen");
        }

        return string.Join(" ", args);
    }
    
    private IEnumerable<string> ProcessLibraryPath(string filePath, string librariesPath)
    {
        var jsonData = File.ReadAllText(filePath);
        var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);

        foreach (var lib in argsLibraries.Libraries)
        {
            if (lib == null || lib.Downloads == null)
            {
                var path = BuildFromName(lib.Name, librariesPath);
                if (path != null)
                {
                    yield return path;
                }
                continue;
            }
            
            if (lib.Downloads.Classifiers == null || lib.Downloads.Classifiers.Count == 0)
            {
                if (ShouldIncludeLibrary(lib.Rule))
                {
                    var path = BuildFromName(lib.Name, librariesPath);
                    if (path != null)
                    {
                        yield return path;
                    }
                }
            }
        }
    }

    private bool ShouldIncludeLibrary(LibraryJsonRule[] rules)
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

    private bool ElementContainsRules(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            return element.TryGetProperty("rules", out _);
        }
        return false;
    }
    
    private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
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
        if (parts.Length == 3)
        {
            string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
            string artifactId = parts[1];
            string version = parts[2];
        
            return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}.jar");
        }
        else if (parts.Length == 4)
        {
            string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
            string artifactId = parts[1];
            string version = parts[2];
            string natives = parts[3];
        
            return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}-{natives}.jar");
        }
        return null;
    }
    
    public static string BuildNativesName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3)
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }

        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];
        
        return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}-natives-windows.jar");
    }
    
    public static string BuildNewNativesName(string name, string root)
    {
        var parts = name.Split(':');

        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];

        string path = Path.Combine(root, groupIdPath, artifactId, version);
        
        if (parts.Length > 3 && parts[3].StartsWith("natives-"))
        {
            string classifier = parts[3];
            return Path.Combine(path, $"{artifactId}-{version}-{classifier}.jar");
        }
        else
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }
    }

    // 完整路径
    private string CurrentExecutingDirectory(string path)
    {
        return FileUtil.IsAbsolutePath(Root) ? 
            Path.Combine(Root) : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
    }
    
    // 判断账户
    private void ProcessAccount()
    {
        if (BaseAccount is MicrosoftAccount)
        {
            userType = "msa";
        }
        else
        {
            userType = "Mojang";
        }
    }
    
    // 获取版本 Jar 实际路径
    private string GetVersionJarPath()
    {
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);
        string versionPath = FileUtil.IsAbsolutePath(Root) ? 
            Path.Combine(Root, "versions") : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root, "versions");
        return Path.Combine(versionPath, coreInfo.Version, $"{coreInfo.Version}.jar");
    }
}