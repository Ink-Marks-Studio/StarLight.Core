namespace StarLight_Core.Models.Downloader
{
    public static class DownloaderConfig
    {
        public static int MaxThreads { get; set; } = 64;

        public static string UserAgent { get; set; } = "StarLight/" + StarLightInfo.Version;
    }
}