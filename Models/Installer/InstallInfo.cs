namespace StarLight_Core.Models.Installer;

public class InstallInfo
{
    public string Id { get; set; }

    public string Type { get; set; }

    public string Status { get; set; }

    public Exception Exception { get; set; }
}