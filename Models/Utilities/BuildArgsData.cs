namespace StarLight_Core.Models.Utilities;

public class BuildArgsData
{
    public static readonly IEnumerable<string> DefaultGcArguments = new string[]
    {
        "-XX:+UseG1GC",
        "-XX:-UseAdaptiveSizePolicy"
    };

    public static readonly IEnumerable<string> DefaultAdvancedArguments = new string[] { 
        "-XX:-OmitStackTraceInFastThrow",
        "-Dfml.ignoreInvalidMinecraftCertificates=true",
        "-Dfml.ignorePatchDiscrepancies=true",
        "-Dlog4j2.formatMsgNoLookups=true",
        "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump"
    };
    
    public static readonly IEnumerable<string> OptimizationGcArguments = new string[] { 
        "-XX:+UnlockExperimentalVMOptions",
        "-XX:G1NewSizePercent=20",
        "-XX:G1ReservePercent=20",
        "-XX:MaxGCPauseMillis=50",
        "-XX:G1HeapRegionSize=16m"
    };

    public static readonly IEnumerable<string> OptimizationAdvancedArguments = new string[]
    {
        "-XX:-DontCompileHugeMethods",
        "-Djava.rmi.server.useCodebaseOnly=true"
    };
    
    public static List<string> JvmArgumentsTemplate = new List<string>
    {
        "-Djava.library.path=${natives_directory}",
        "-Dminecraft.launcher.brand=${launcher_name}",
        "-Dminecraft.launcher.version=${launcher_version}",
        "-cp",
        "${classpath}"
    };
    
}