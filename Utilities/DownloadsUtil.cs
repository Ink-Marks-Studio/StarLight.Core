using System.Collections.Concurrent;
using System.Diagnostics;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        private readonly HttpClient _httpClient = new();
        private int _maxThreads = DownloaderConfig.MaxThreads; 
            
        public Action<double>? OnSpeedChanged;
        public Action<int, int>? ProgressChanged;
        public Action<DownloadItem>? DownloadFailed;
        
        public DownloadsUtil(Action<double>? onSpeedChanged = null, Action<int, int>? progressChanged = null, Action<DownloadItem>? downloadFailed = null)
        {
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DownloaderConfig.UserAgent);
            
            OnSpeedChanged = onSpeedChanged;
            ProgressChanged = progressChanged;
            DownloadFailed = downloadFailed;
        }

        public async Task DownloadFiles(IEnumerable<DownloadItem> downloadItems, CancellationToken cancellationToken = default)
        {
            var semaphore = new SemaphoreSlim(_maxThreads);
            var threadDownloadSpeeds = new ConcurrentDictionary<int, long>();
            var tasks = new ConcurrentBag<Task>();
            var cts = new CancellationTokenSource();

            var downloadItemList = downloadItems.ToList();
            var totalFiles = downloadItemList.Count;
            var filesDownloaded = 0;
            long totalDownloadedBytes = 0;
            var reportStopwatch = new Stopwatch();
            
            var reportingTask = Task.Run(async () =>
            {
                reportStopwatch.Start();
                while (!cancellationToken.IsCancellationRequested && !cts.Token.IsCancellationRequested)
                {
                    long elapsed = reportStopwatch.ElapsedMilliseconds;
                    if (elapsed >= 1000)
                    {
                        long totalSpeed = (totalDownloadedBytes * 1000) / elapsed;
                        OnSpeedChanged?.Invoke(totalSpeed);
                        reportStopwatch.Restart();
                        Interlocked.Exchange(ref totalDownloadedBytes, 0); // 重置下载字节数
                    }
                    await Task.Delay(1000 - (int)(reportStopwatch.ElapsedMilliseconds % 1000)); // 校准间隔
                }
            }, cancellationToken);

            foreach (var downloadItem in downloadItemList)
            {
                await semaphore.WaitAsync(cancellationToken);
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(downloadItem.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        FileUtil.IsDirectory(Path.GetDirectoryName(downloadItem.SaveAsPath), true);

                        await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                        await using (var fileStream = new FileStream(downloadItem.SaveAsPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                Interlocked.Add(ref totalDownloadedBytes, bytesRead);
                            }
                        }
                        Interlocked.Increment(ref filesDownloaded);
                        ProgressChanged?.Invoke(filesDownloaded, totalFiles);
                    }
                    catch (OperationCanceledException e)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        DownloadFailed?.Invoke(downloadItem);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            cts.Cancel();
            await reportingTask;
        }
        
        // 获取下载文件大小
        public async Task<long> GetFileSizeAsync(string url)
        {
            try
            {
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                response.EnsureSuccessStatusCode();
                
                var fileSize = response.Content.Headers.ContentLength;
                return fileSize.HasValue ? fileSize.Value : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"[SL]获取文件大小失败：{ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}