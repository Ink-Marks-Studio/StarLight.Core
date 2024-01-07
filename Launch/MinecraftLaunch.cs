using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;
using StarLight_Core.Utilities;

namespace StarLight_Core.Launch
{
    public class MinecraftLaunch
    {
        public BaseAccount BaseAccount { get; set; }
        
        public GameWindowConfig GameWindowConfig { get; set; }
        
        public GameCoreConfig GameCoreConfig { get; set; }
        
        public JavaConfig JavaConfig { get; set; }
        
        public MinecraftLaunch(LaunchConfig launchConfig)
        {
            GameWindowConfig = launchConfig.GameWindowConfig;
            GameCoreConfig = launchConfig.GameCoreConfig;
            JavaConfig = launchConfig.JavaConfig;
            BaseAccount = launchConfig.Account.BaseAccount;
        }

        public async Task LaunchAsync()
        {
            try
            {
                var arguments = new ArgumentsBuildUtil(GameWindowConfig, GameCoreConfig, JavaConfig, BaseAccount).Build();
                Console.WriteLine(string.Join(" ", arguments));
            }
            catch (Exception e)
            {
                throw new Exception("启动失败,未知原因: " + e);
            }
        }
    }
}

