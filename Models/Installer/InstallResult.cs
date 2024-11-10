using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer;

public class InstallResult
{
    public InstallResult(Status status, string version, string? customId)
    {
        Status = status;
        Version = version;
        CustomId = customId ?? version;

        if (Status == Status.Cancel)
            Exception = new Exception("已取消安装");
    }

    public InstallResult(Status status, string version, string? customId, Exception exception)
    {
        Status = status;
        Version = version;
        CustomId = customId ?? version;
        Exception = exception;
    }

    public Status Status { get; set; }

    public string Version { get; set; }

    public string CustomId { get; set; }

    public Exception? Exception { get; set; }
}