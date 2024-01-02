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

