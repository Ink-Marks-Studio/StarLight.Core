using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        private HttpClient httpClient = new HttpClient();
        private long totalBytesReceived = 0;
        private long totalExpectedBytes = 0;

        public DownloadsUtil()
        {
            httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        // 下载多个文件
        public async Task DownloadFilesAsync(IEnumerable<DownloadItem> downloadItems, string outputFolder, Action<double> progressChanged, Action<string> onDownloadCompleted)
        {
            var sw = Stopwatch.StartNew();
            var downloadTasks = downloadItems.Select(item => DownloadFileAsync(item.Url, outputFolder, item.SaveAsName, onDownloadCompleted)).ToList();
            
            using var timer = new Timer(_ =>
            {
                var speed = totalBytesReceived / sw.Elapsed.TotalSeconds;
                progressChanged(speed);
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            var tasksWithExpectedBytes = await Task.WhenAll(downloadTasks);
            totalExpectedBytes = tasksWithExpectedBytes.Sum(task => task.ExpectedBytes);

            timer.Change(Timeout.Infinite, Timeout.Infinite);
            sw.Stop();
            
            var finalSpeed = totalBytesReceived / sw.Elapsed.TotalSeconds;
            progressChanged(finalSpeed);
        }

        // 下载单个文件
        private async Task<(long ExpectedBytes, long DownloadedBytes)> DownloadFileAsync(string url, string outputFolder, string saveAsName, Action<string> onDownloadCompleted)
        {
            const int maxRetryAttempts = 3;
            int retryCount = 0;

            while (true)
            {
                try
                {
                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    
                    var fileName = Path.GetFileName(new Uri(url).AbsolutePath); //名称
                    if (string.IsNullOrEmpty(saveAsName))
                    {
                        saveAsName = Path.GetFileName(new Uri(url).AbsolutePath);
                    }
                    
                    var outputPath = Path.Combine(outputFolder, fileName); // 路径
                    
                    var bytesReceived = 0L;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            Interlocked.Add(ref totalBytesReceived, bytesRead);
                            bytesReceived += bytesRead;
                        }
                    }

                    // 下载完成，调用回调函数
                    onDownloadCompleted(outputPath);

                    return (totalBytes, bytesReceived);
                }
                catch (HttpRequestException e) when (retryCount < maxRetryAttempts)
                {
                    retryCount++;
                    // Console.WriteLine($"下载失败，正在重试...（{retryCount}/{maxRetryAttempts}）");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    throw new Exception($"下载失败，无法重试。错误：{ex.Message}");
                }
            }
        }
    }
}