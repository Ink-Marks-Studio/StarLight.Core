namespace StarLight_Core.Models.Utilities;

public class DownloadItem
{
    public DownloadItem(string url, string saveAsPath)
    {
        Url = url;
        SaveAsPath = saveAsPath;
    }

    public string Url { get; set; }

    public string SaveAsPath { get; set; }
}