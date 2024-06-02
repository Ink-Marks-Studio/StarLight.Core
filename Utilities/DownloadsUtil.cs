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

        // 下载文件
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

        // 下载多个文件
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
            
                var downloadTasks = downloadItemList.Select(item => DownloadFileAsync(item.Url, outputFolder, item.SaveAsPath, downloadCompleted, downloadFailed)).ToList();
            
                using var timer = new Timer(_ =>
                {
                    var speed = _totalBytesReceived / sw.Elapsed.TotalSeconds;
                    progressChanged?.Invoke(speed);
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

                var tasksWithExpectedBytes = await Task.WhenAll(downloadTasks);
                var totalExpectedBytes = tasksWithExpectedBytes.Sum(task => task.ExpectedBytes);

                timer.Change(Timeout.Infinite, Timeout.Infinite);
                sw.Stop();
            
                var finalSpeed = _totalBytesReceived / sw.Elapsed.TotalSeconds;
                progressChanged?.Invoke(finalSpeed);
                
                return new DownloadStatus(Status.Succeeded);
            }
            catch (Exception e)
            {
                return new DownloadStatus(Status.Failed, e);
            }
        }

        // 下载单个文件
        private async Task<(long ExpectedBytes, long DownloadedBytes)> DownloadFileAsync(string url, string? outputFolder, string? saveAsPath, Action<int, int>? downloadCompleted, Action<string>? downloadFailed)
        {
            const int maxRetryAttempts = 3;
            int retryCount = 0;

            while (true)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;

                    var fileName = Path.GetFileName(new Uri(url).AbsolutePath); // 获取文件名
                    var outputPath = string.IsNullOrEmpty(saveAsPath) ? Path.Combine(outputFolder, fileName) : saveAsPath;

                    FileUtil.IsDirectory(Path.GetDirectoryName(outputPath), true);
                    
                    var bytesReceived = 0L;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            Interlocked.Add(ref _totalBytesReceived, bytesRead);
                            bytesReceived += bytesRead;
                        }
                    }

                    // 下载完成
                    _downloadedFiles++;
                    downloadCompleted?.Invoke(_downloadedFiles, _totalFiles);
                    downloadFailed?.Invoke(outputPath);

                    return (totalBytes, bytesReceived);
                }
                catch (HttpRequestException e) when (retryCount < maxRetryAttempts)
                {
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
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
                    throw new Exception("无法获取文件大小，服务器未返回 Content-Length 头部字段");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取文件大小失败：{ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}