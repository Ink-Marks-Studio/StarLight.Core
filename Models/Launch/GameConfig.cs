using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Models.Launch;

public class LaunchConfig
{
    public Account Account { get; set; }

    public GameWindowConfig GameWindowConfig { get; set; } = new();

    public GameCoreConfig GameCoreConfig { get; set; }

    public JavaConfig JavaConfig { get; set; }
}