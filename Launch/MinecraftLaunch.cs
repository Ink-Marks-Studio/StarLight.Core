using StarLight_Core.Models.Launch;

namespace StarLight_Core.Launch
{
    public class MinecraftLaunch
    {
        public GameWindowConfig GameWindowConfig { get; set; }
        
        public GameCoreConfig GameCoreConfig { get; set; }
        
        public JavaConfig JavaConfig { get; set; }
        
        public MinecraftLaunch(GameWindowConfig gameWindowConfig, GameCoreConfig gameCoreConfig, JavaConfig javaConfig)
        {
            GameWindowConfig = gameWindowConfig;
            GameCoreConfig = gameCoreConfig;
            JavaConfig = javaConfig;
        }
    }
}

