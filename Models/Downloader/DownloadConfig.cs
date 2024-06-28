using StarLight_Core.Downloader;

namespace StarLight_Core.Models.Downloader
{
    public class DownloadConfig
    {
        public static DownloadConfiguration DownloadOptions { get; set; } = new DownloadConfiguration()
        {
            BufferBlockSize = 10240,
            ChunkCount = 4,
            MaxTryAgainOnFailover = 5,
            MaximumMemoryBufferBytes = 1024 * 1024 * 50,
            ParallelDownload = true,
            Timeout = 1000,
            MinimumSizeOfChunking = 1024,
            ReserveStorageSpaceBeforeStartingDownload = true,
        };
    }
}