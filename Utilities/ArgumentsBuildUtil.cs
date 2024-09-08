using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using StarLight_Core.Models.Utilities;
using System.Text.Json;
using StarLight_Core.Downloader;
using StarLight_Core.Models;
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
        Root = FileUtil.IsAbsolutePath(gameCoreConfig.Root) ? 
            Path.Combine(gameCoreConfig.Root) : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), gameCoreConfig.Root);
        userType = "Mojang";
        jarPath = GetVersionJarPath();
    }

    // TODO: -Dfabric.log.level=DEBUG
    // 参数构建器
    public async Task<List<string>> Build()
    {
        List<string> arguments = new List<string>();
        
        arguments.Add(BuildMemoryArgs());
        arguments.Add(await BuildJvmArgs());
        arguments.Add(BuildWindowArgs());
        arguments.Add(BuildGameArgs());
        
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
    private async Task<string> BuildJvmArgs()
    {
        ProcessAccount();
        
        List<string> args = new List<string>();
        GameCoreInfo coreInfo = GameCoreUtil.GetGameCore(VersionId, Root);
        GameCoreInfo inheritsFromInfo = new GameCoreInfo();
        if (coreInfo.InheritsFrom != null)
        {
            inheritsFromInfo = GameCoreUtil.GetGameCore(coreInfo.InheritsFrom, Root);
        }

        var appDataPath = Path.Combine(FileUtil.GetAppDataPath(), "StarLight.Core", "jar");
        var tempPath = Path.Combine(FileUtil.GetAppDataPath(), "StarLight.Core", "temp");
        
        if (BaseAccount is UnifiedPassAccount)
        {
            FileUtil.IsDirectory(appDataPath, true);
            FileUtil.IsDirectory(tempPath, true);
            
            if (!FileUtil.IsFile(GameCoreConfig.Nide8authPath))
            {
                var nidePath = Path.Combine(appDataPath + Path.DirectorySeparatorChar + "nide8auth.jar");
                var downloader = new MultiFileDownloader();
                await downloader.DownloadFiles(new List<DownloadItem>
                {
                    new ("https://login.mc-user.com:233/index/jar", nidePath)
                });
                args.Add("-javaagent:\"" + nidePath + "\"=" + GameCoreConfig.UnifiedPassServerId);
            }
            else
            {
                string authPath = FileUtil.IsAbsolutePath(GameCoreConfig.Nide8authPath) ? 
                    Path.Combine(GameCoreConfig.Nide8authPath) : 
                    Path.Combine(FileUtil.GetCurrentExecutingDirectory(), GameCoreConfig.Nide8authPath);
                args.Add("-javaagent:\"" + authPath + "\"=" + GameCoreConfig.UnifiedPassServerId);
            }
        }

        if (coreInfo.IsNewVersion)
            args.Add(BuildClientJarArgs());
        
        if (SystemUtil.IsOperatingSystemGreaterThanWin10())
            args.Add(BuildSystemArgs());
        
        args.Add(BuildGcAndAdvancedArguments());
        
        string rootPath = FileUtil.IsAbsolutePath(Root) ? 
            Path.Combine(Root) : 
            Path.Combine(FileUtil.GetCurrentExecutingDirectory(), Root);
        string nativesPath = Path.Combine(rootPath, "versions", VersionId, "natives");
        
        if (!Directory.Exists(nativesPath))
        {
            string[] nativesDirectories = Directory.GetDirectories(coreInfo.root, "*natives*", SearchOption.AllDirectories);
            
            if (nativesDirectories.Length > 0)
                nativesPath = nativesDirectories[0];
        }
        
        var jvmPlaceholders = new Dictionary<string, string>
        {
            { "${natives_directory}", $"\"{nativesPath}\"" },
            { "${launcher_name}", "StarLight" },
            { "${launcher_version}", StarLightInfo.Version },
            { "${classpath}", $"\"{BuildLibrariesArgs()}\"" },
            { "${version_name}", coreInfo.Id},
            { "${library_directory}", Path.Combine(rootPath, "libraries") },
            { "${classpath_separator}", ";" }
        };
        
        string jvmArgumentTemplate = "";
        
        if (coreInfo.IsNewVersion)
        {
            BuildArgsData.JvmArgumentsTemplate.Clear();

            if (coreInfo.InheritsFrom != null)
            {
                foreach (var element in inheritsFromInfo.Arguments.Jvm)
                {
                    if (!ElementContainsRules(element))
                    {
                        BuildArgsData.JvmArgumentsTemplate.Add(element.ToString());
                    }
                }
            }
            
            foreach (var element in coreInfo.Arguments.Jvm)
            {
                if (!ElementContainsRules(element))
                {
                    BuildArgsData.JvmArgumentsTemplate.Add(element.ToString());
                }
            }
            
            List<string> updatedJvmArguments = new List<string>();
            foreach (var argument in BuildArgsData.JvmArgumentsTemplate)
            {
                updatedJvmArguments.Add(argument.Replace(" ", ""));
            }
            
            BuildArgsData.JvmArgumentsTemplate = updatedJvmArguments;
            jvmArgumentTemplate = string.Join(" ", BuildArgsData.JvmArgumentsTemplate);
        }
        else
        {
            jvmArgumentTemplate = string.Join(" ", BuildArgsData.JvmArgumentsTemplate);
        }
        
        args.Add(ReplacePlaceholders(jvmArgumentTemplate, jvmPlaceholders));
        
        var wrapperPath = Path.Combine(appDataPath + Path.DirectorySeparatorChar + "launch_wrapper.jar");
        
        FileUtil.IsDirectory(appDataPath, true);
        FileUtil.IsDirectory(tempPath, true);
        
        if (FileUtil.IsFile(wrapperPath))
            args.Add($"-Doolloo.jlw.tmpdir=\"{tempPath}\" -jar \"{wrapperPath}\"");
        else
        {
            var downloader = new MultiFileDownloader();
            await downloader.DownloadFiles(new List<DownloadItem>
            {
                new ("http://cdn.hjdczy.top/starlight.core/launch_wrapper.jar", wrapperPath)
            });
            args.Add($"-Doolloo.jlw.tmpdir=\"{tempPath}\" -jar \"{wrapperPath}\"");
        }
        
        args.Add(coreInfo.MainClass);
        
        return string.Join(" ", args);
    }
    
    private string BuildClientJarArgs() => "-Dminecraft.client.jar=" + $"\"{jarPath}\"";
    
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
        GameCoreInfo inheritsFromInfo = new GameCoreInfo();
        if (coreInfo.InheritsFrom != null)
        {
            inheritsFromInfo = GameCoreUtil.GetGameCore(coreInfo.InheritsFrom, Root);
        }
        
        var gamePlaceholders = new Dictionary<string, string>
        {
            { "${auth_player_name}", BaseAccount.Name },
            { "${version_name}", $"\"{VersionId}\"" },
            { "${assets_root}", Path.Combine(CurrentExecutingDirectory(Root), "assets") },
            { "${assets_index_name}", coreInfo.Assets },
            { "${auth_uuid}", BaseAccount.Uuid.Replace("-","") },
            { "${auth_access_token}", BaseAccount.AccessToken },
            { "${clientid}", "${clientid}" },
            { "${auth_xuid}", "${auth_xuid}" },
            { "${user_type}", userType },
            { "${version_type}", $"\"SL/{TextUtil.ToTitleCase(coreInfo.Type)}\"" },
            { "${user_properties}", "{}"}
        };

        string gameDirectory = GameCoreConfig.IsVersionIsolation ?
            Path.Combine(CurrentExecutingDirectory(Root), "versions", VersionId) :
            Path.Combine(CurrentExecutingDirectory(Root));

        gamePlaceholders.Add("${game_directory}", $"\"{gameDirectory}\"");
        
        string gameArguments = coreInfo.IsNewVersion 
            ? string.Join(" ", coreInfo.Arguments.Game.Where(element => !ElementContainsRules(element)))
            : coreInfo.MinecraftArguments;

        if (coreInfo.InheritsFrom != null)
        {
            gameArguments += inheritsFromInfo.IsNewVersion
                ? $" {string.Join(" ", inheritsFromInfo.Arguments.Game.Where(element => !ElementContainsRules(element)))}"
                : $" {inheritsFromInfo.MinecraftArguments}";
        }

        string[] tweakClasses = new[] { "--tweakClass optifine.OptiFineForgeTweaker ", "--tweakClass optifine.OptiFineTweaker " };
        string foundTweakClass = null;
        
        foreach (var tweakClass in tweakClasses)
        {
            if (gameArguments.Contains(tweakClass))
            {
                foundTweakClass = tweakClass;
                gameArguments = gameArguments.Replace(tweakClass, "").Trim();
                break;
            }
        }
        
        if (foundTweakClass != null)
        {
            gameArguments = $"{gameArguments} {foundTweakClass}".Trim();
        }
        
        return ReplacePlaceholders(gameArguments, gamePlaceholders);
    }

    // Gc 与 Advanced 参数
    private string BuildGcAndAdvancedArguments()
    {
        var allArguments = BuildArgsData.DefaultGcArguments.Concat(BuildArgsData.DefaultAdvancedArguments);

        if (!JavaConfig.DisabledOptimizationGcArgs)
            allArguments = allArguments.Concat(BuildArgsData.OptimizationGcArguments);
        if (!JavaConfig.DisabledOptimizationAdvancedArgs)
            allArguments = allArguments.Concat(BuildArgsData.OptimizationAdvancedArguments);
        
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

            string inheritFromPath = coreInfo.InheritsFrom != null ?
                Path.Combine(Root, "versions", coreInfo.InheritsFrom, $"{coreInfo.InheritsFrom}.json") : 
                null;

            if (inheritFromPath != null)
                cps.AddRange(ProcessLibraryPath(inheritFromPath, librariesPath));

            cps.AddRange(ProcessLibraryPath(versionPath, librariesPath));
            cps.Add(jarPath);

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
            args.Add("--fullscreen");

        return string.Join(" ", args);
    }
    
    private IEnumerable<string> ProcessLibraryPath(string filePath, string librariesPath)
    {
        var jsonData = File.ReadAllText(filePath);
        var argsLibraries = JsonSerializer.Deserialize<ArgsBuildLibraryJson>(jsonData);

        var optifinePaths = new List<string>();
        var normalPaths = new List<string>();

        foreach (var lib in argsLibraries.Libraries)
        {
            if (lib == null || lib.Downloads == null)
            {
                var path = BuildFromName(lib.Name, librariesPath);
                if (path != null)
                    if (lib.Name.StartsWith("optifine", StringComparison.OrdinalIgnoreCase))
                        optifinePaths.Add(path);
                    else
                        normalPaths.Add(path);
                continue;
            }

            if (lib.Downloads.Classifiers == null || lib.Downloads.Classifiers.Count == 0)
            {
                if (ShouldIncludeLibrary(lib.Rule))
                {
                    var path = BuildFromName(lib.Name, librariesPath);
                    if (path != null)
                        if (lib.Name.StartsWith("optifine", StringComparison.OrdinalIgnoreCase))
                            optifinePaths.Add(path);
                        else
                            normalPaths.Add(path);
                }
            }
        }
        
        foreach (var path in normalPaths)
            yield return path;
        
        foreach (var optifinePath in optifinePaths)
            yield return optifinePath;
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
    
    public static string BuildNativesName(Library library, string root)
    {
        var parts = library.Name.Split(':');
        if (parts.Length != 3)
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }

        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];

        int arch = SystemUtil.GetOperatingSystemBit();
        
        string windowsNative = library.Natives["windows"].Replace("${arch}", arch.ToString());
        
        return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}-{windowsNative}.jar");
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
        if (coreInfo.InheritsFrom != null && coreInfo.InheritsFrom != "null")
            return Path.Combine(Root, "versions", coreInfo.InheritsFrom, $"{coreInfo.InheritsFrom}.jar");
        else
            return Path.Combine(versionPath, coreInfo.Id, $"{coreInfo.Id}.jar");
    }
}