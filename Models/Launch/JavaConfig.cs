namespace StarLight_Core.Models.Launch;

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