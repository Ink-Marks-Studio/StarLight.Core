namespace StarLight_Core.Models.Launch;

public class GameWindowConfig
{
    public int Height { get; set; } = 0;

    public int Width { get; set; } = 0;

    public bool IsFullScreen { get; set; } = false;
}

public class JavaConfig
{
    public string JavaPath { get; set; }
    
    public int MaxMemory { get; set; }
    
    public int MinMemory { get; set; }
}