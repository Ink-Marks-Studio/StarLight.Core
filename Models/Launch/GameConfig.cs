namespace StarLight_Core.Models.Launch;

public class GameWindowConfig
{
    public int Height { get; set; } = 0;

    public int Width { get; set; } = 0;

    public bool IsFullScreen { get; set; } = false;
}

public class GameCoreConfig
{
    public string Ip { get; set; }
    
    public string Port { get; set; }
    
    public string GameArguments { get; set; }
}

public class JavaConfig
{
    public string JavaPath { get; set; }

    public int MaxMemory { get; set; } = 2048;

    public int MinMemory { get; set; } = 256;
    
    public string AdvancedArguments { get; set; }
    
    public string GCArguments { get; set; }
}