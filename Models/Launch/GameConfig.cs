using StarLight_Core.Models.Authentication;

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

public class Account
{
    public BaseAccount BaseAccount { get; set; }
}

public class GameWindowConfig
{
    public int Height { get; set; } = 0;

    public int Width { get; set; } = 0;

    public bool IsFullScreen { get; set; } = false;
}

public class GameCoreConfig
{
    public string Root { get; set; }
    
    public string Version { get; set; }
    
    public string Ip { get; set; }
    
    public string Port { get; set; }
    
    public IEnumerable<string> GameArguments { get; set; }
}

public class JavaConfig
{
    public string JavaPath { get; set; }

    public int MaxMemory { get; set; } = 2048;

    public int MinMemory { get; set; } = 256;
    
    public bool DisabledOptimizationAdvancedArgs { get; set; } = false;
    
    public bool DisabledOptimizationGcArgs { get; set; } = false;
    
    public IEnumerable<string> AdvancedArguments { get; set; }
    
    public IEnumerable<string> GCArguments { get; set; }
}