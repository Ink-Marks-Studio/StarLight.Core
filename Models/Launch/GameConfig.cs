using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Launch
{
    public class LaunchConfig
    {
        public Account Account { get; set; }

        public GameWindowConfig GameWindowConfig { get; set; }

        public GameCoreConfig GameCoreConfig { get; set; }

        public JavaConfig JavaConfig { get; set; }
    }
}