using StarLight_Core.Enum;

namespace StarLight_Core.Models.Utilities;

public class DownloadStatus : BaseStatus
{
    public DownloadStatus(Status status, Exception? exception = null)
    {
        Status = status;
        Exception = exception;
    }

    public Exception? Exception { get; set; }
}