using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class ForgeInstallResult
{
    public ForgeInstallResult(Status status, string gameVersion, string fabricVersion, string? customId)
    {
        Status = status;
        GameVersion = gameVersion;
        ForgeVersion = fabricVersion;
        CustomId = customId ?? $"{gameVersion}-forge_{fabricVersion}";

        if (Status == Status.Cancel)
            Exception = new Exception("已取消安装");
    }

    public ForgeInstallResult(Status status, string gameVersion, string fabricVersion, string? customId,
        Exception exception)
    {
        Status = status;
        GameVersion = gameVersion;
        ForgeVersion = fabricVersion;
        CustomId = customId ?? $"{gameVersion}-forge_{fabricVersion}";
        Exception = exception;
    }

    public Status Status { get; set; }

    public string GameVersion { get; set; }

    public string ForgeVersion { get; set; }

    public string CustomId { get; set; }

    public Exception? Exception { get; set; }
}