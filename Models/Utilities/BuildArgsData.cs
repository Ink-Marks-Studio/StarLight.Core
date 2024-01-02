namespace StarLight_Core.Models.Utilities;

public class BuildArgsData
{
    public readonly IEnumerable<string> DefaultGCArguments = new string[]
    {
        "-XX:+UseG1GC",
        "-XX:+UnlockExperimentalVMOptions",
        "-XX:G1NewSizePercent=20",
        "-XX:G1ReservePercent=20",
        "-XX:MaxGCPauseMillis=50",
        "-XX:G1HeapRegionSize=16m",
        "-XX:-UseAdaptiveSizePolicy"
    };

    public readonly IEnumerable<string> DefaultAdvancedArguments = new string[] { 
        "-XX:-OmitStackTraceInFastThrow",
        "-XX:-DontCompileHugeMethods",
        "-Dfml.ignoreInvalidMinecraftCertificates=true",
        "-Dfml.ignorePatchDiscrepancies=true",
        "-Djava.rmi.server.useCodebaseOnly=true",
        "-MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump"
    };
}