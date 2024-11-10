namespace StarLight_Core.Models.Downloader;

public static class DownloaderConfig
{
    private static int _maxThreads = 64;

    public static int MaxThreads
    {
        get => _maxThreads;
        set
        {
            if (_maxThreads == value) return;
            _maxThreads = value;
            MaxThreadsChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static string UserAgent { get; set; } = "StarLight/" + StarLightInfo.Version;

    public static bool VerificationFile { get; set; } = false;

    public static event EventHandler? MaxThreadsChanged;
}