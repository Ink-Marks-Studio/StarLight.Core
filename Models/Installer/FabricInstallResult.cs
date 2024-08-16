using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class FabricInstallResult
{
    public Status Status { get; set; }
        
    public string GameVersion { get; set; }
    
    public string FabricVersion { get; set; }
    
    public string CustomId { get; set; }
    
    public Exception? Exception { get; set; }
        
    public FabricInstallResult(Status status, string gameVersion, string fabricVersion, string? customId)
    {
        Status = status;
        GameVersion = gameVersion;
        FabricVersion = fabricVersion;
        CustomId = customId ?? $"{gameVersion}-fabric-loader-{fabricVersion}";
        
        if (Status == Status.Cancel)
            Exception = new Exception("已取消安装");
    }

    public FabricInstallResult(Status status, string gameVersion, string fabricVersion, string? customId, Exception exception)
    {
        Status = status;
        GameVersion = gameVersion;
        FabricVersion = fabricVersion;
        CustomId = customId ?? $"{gameVersion}-fabric-loader-{fabricVersion}";
        Exception = exception;
    }
}