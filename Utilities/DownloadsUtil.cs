using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using StarLight_Core.Enum;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private long _totalBytesReceived = 0;
        private int _totalFiles = 0;
        private int _downloadedFiles = 0;
        
        public DownloadsUtil()
        {
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        
        public async Task<DownloadStatus> DownloadAsync(DownloadItem downloadItem, string? outputFolder = null, Action<double>? speedChanged = null, Action<int, int>? downloadCompleted = null, Action<string>? downloadFailed = null)
        {
            try
            {
                var downloadItems = new List<DownloadItem> { downloadItem };
                var result = await DownloadFilesAsync(downloadItems, outputFolder, speedChanged, downloadCompleted, downloadFailed);
                return result;
            }
            catch (Exception e)
            {
                return new DownloadStatus(Status.Failed, e);
            }
        }

        public async Task<DownloadStatus> DownloadFilesAsync(IEnumerable<DownloadItem> downloadItems, string? outputFolder = null, Action<double>? progressChanged = null, Action<int, int>? downloadCompleted = null, Action<string>? downloadFailed = null)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                List<DownloadItem> downloadItemList = downloadItems.ToList();
                _totalFiles = downloadItemList.Count;

                if (outputFolder == null)
                {
                    foreach (var item in downloadItemList)
                    {
                        if (item.SaveAsPath == null)
                        {
                            throw new Exception("[SL]未设置保存路径");
                        }
                    }
                }

                downloadCompleted?.Invoke(_downloadedFiles, _totalFiles);

                var downloadTasks = downloadItemList.Select(item => DownloadFileAsync(item.Url, outputFolder, item.SaveAsPath, progressChanged, downloadCompleted, downloadFailed)).ToList();

                await Task.WhenAll(downloadTasks);

                sw.Stop();

                return new DownloadStatus(Status.Succeeded);
            }
            catch (Exception e)
            {
                return new DownloadStatus(Status.Failed, e);
            }
        }

        private async Task DownloadFileAsync(string url, string? outputFolder, string? saveAsPath, Action<double>? progressChanged, Action<int, int>? downloadCompleted, Action<string>? downloadFailed)
        {
            const int maxRetryAttempts = 3;
            int retryCount = 0;

            long previousBytesReceived = 0;
            long currentBytesReceived = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            using var timer = new Timer(_ =>
            {
                var elapsed = stopwatch.Elapsed.TotalSeconds;
                if (elapsed >= 1)
                {
                    var currentSpeed = (currentBytesReceived - previousBytesReceived) / elapsed / 1024.0; // KB/s
                    progressChanged?.Invoke(currentSpeed);

                    previousBytesReceived = currentBytesReceived;
                    stopwatch.Restart();
                }
            }, null, 0, 1000);

            while (true)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;

                    var fileName = Path.GetFileName(new Uri(url).AbsolutePath); // 获取文件名
                    var outputPath = string.IsNullOrEmpty(saveAsPath) ? Path.Combine(outputFolder, fileName) : saveAsPath;

                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException());

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            Interlocked.Add(ref _totalBytesReceived, bytesRead);
                            currentBytesReceived += bytesRead;
                        }
                    }

                    // 下载完成
                    _downloadedFiles++;
                    downloadCompleted?.Invoke(_downloadedFiles, _totalFiles);

                    return;
                }
                catch (HttpRequestException e) when (retryCount < maxRetryAttempts)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    downloadFailed?.Invoke($"下载失败：{ex.Message}");
                    throw new Exception($"[SL]下载失败，无法重试：{ex.Message}");
                }
            }
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