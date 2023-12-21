using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;

namespace StarLight_Core.Launch
{
    public class MinecraftLaunch
    {
        public BaseAccount BaseAccount { get; set; }
        
        public GameWindowConfig GameWindowConfig { get; set; }
        
        public GameCoreConfig GameCoreConfig { get; set; }
        
        public JavaConfig JavaConfig { get; set; }
        
        public static readonly IEnumerable<string> DefaultGCArguments = new string[]
        {
            "-XX:+UseG1GC",
            "-XX:+UnlockExperimentalVMOptions",
            "-XX:G1NewSizePercent=20",
            "-XX:G1ReservePercent=20",
            "-XX:MaxGCPauseMillis=50",
            "-XX:G1HeapRegionSize=16m",
            "-XX:-UseAdaptiveSizePolicy"
        };
        
        public static readonly IEnumerable<string> DefaultAdvancedArguments = new string[] { 
            "-XX:-OmitStackTraceInFastThrow",
            "-XX:-DontCompileHugeMethods",
            "-Dfml.ignoreInvalidMinecraftCertificates=true",
            "-Dfml.ignorePatchDiscrepancies=true",
            "-Djava.rmi.server.useCodebaseOnly=true",
            "-MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump"
        };
        
        public MinecraftLaunch(GameWindowConfig gameWindowConfig, GameCoreConfig gameCoreConfig, JavaConfig javaConfig, BaseAccount baseAccount)
        {
            GameWindowConfig = gameWindowConfig;
            GameCoreConfig = gameCoreConfig;
            JavaConfig = javaConfig;
            BaseAccount = baseAccount;
        }

        public async Task LaunchAsync()
        {

        }
    }
}

