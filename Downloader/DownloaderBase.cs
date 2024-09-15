using StarLight_Core.Models.Downloader;

namespace StarLight_Core.Downloader
{
    public class DownloaderBase
    {
        public Action<double>? OnSpeedChanged;
        protected int MaxThreads { get; private set; } // 最大线程数

        protected DownloaderBase()
        {
            MaxThreads = DownloaderConfig.MaxThreads;
            DownloaderConfig.MaxThreadsChanged += (_, _) => MaxThreads = DownloaderConfig.MaxThreads;
        }
        
        /// <summary>
        /// 获取下载文件大小
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url">文件下载地址</param>
        /// <returns>下载文件大小</returns>
        protected static async Task<long> GetFileSizeAsync(HttpClient httpClient, string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength ?? 0;
        }
    }
}