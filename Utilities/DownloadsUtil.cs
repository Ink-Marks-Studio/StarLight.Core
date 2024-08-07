using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StarLight_Core.Enum;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Installer;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private int _maxThreads = DownloaderConfig.MaxThreads; 
            
        public Action<double>? OnSpeedChanged = null;
        public Action<int, int>? ProgressChanged = null;
        public Action<DownloadItem>? DownloadFailed = null;
        
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
            SemaphoreSlim semaphore = new SemaphoreSlim(_maxThreads);
            ConcurrentDictionary<int, long> threadDownloadSpeeds = new ConcurrentDictionary<int, long>();
            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var downloadItemList = downloadItems.ToList();
            int totalFiles = downloadItemList.Count;
            int filesDownloaded = 0;
            long totalDownloadedBytes = 0;
            Stopwatch reportStopwatch = new Stopwatch();
            
            Task reportingTask = Task.Run(async () =>
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
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        HttpResponseMessage response = await _httpClient.GetAsync(downloadItem.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        FileUtil.IsDirectory(Path.GetDirectoryName(downloadItem.SaveAsPath), true);
                        
                        using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                        using (var fileStream = new FileStream(downloadItem.SaveAsPath, FileMode.Create, FileAccess.Write, FileShare.None))
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
                if (fileSize.HasValue)
                {
                    return fileSize.Value;
                }
                else
                {
                    return 0;
                }
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