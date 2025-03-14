using System.Collections.Concurrent;
using System.Diagnostics;
using StarLight_Core.Models.Downloader;
using StarLight_Core.Models.Utilities;
using StarLight_Core.Utilities;

namespace StarLight_Core.Downloader;

/// <summary>
/// 多文件下载器
/// </summary>
public class MultiFileDownloader : DownloaderBase
{
    private readonly HttpClient _httpClient = new();
    
    /// <summary>
    /// 下载失败时触发的事件
    /// </summary>
    public Action<DownloadItem>? DownloadFailed;
    
    /// <summary>
    /// 下载进度变化时触发的事件
    /// </summary>
    /// <remarks>
    /// 第一个参数为已下载文件数，第二个参数为总文件数
    /// </remarks>
    public Action<int, int>? ProgressChanged;

    /// <summary>
    /// 多文件下载器构造函数
    /// </summary>
    /// <param name="onSpeedChanged">下载速度变化时的回调函数，参数为当前速度（字节/秒）</param>
    /// <param name="progressChanged">下载进度变化时的回调函数，第一个参数为已下载文件数，第二个参数为总文件数</param>
    /// <param name="downloadFailed">下载失败时的回调函数，参数为下载失败的文件项</param>
    public MultiFileDownloader(Action<double>? onSpeedChanged = null, Action<int, int>? progressChanged = null,
        Action<DownloadItem>? downloadFailed = null)
    {
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DownloaderConfig.UserAgent);

        OnSpeedChanged = onSpeedChanged;
        ProgressChanged = progressChanged;
        DownloadFailed = downloadFailed;
    }

    /// <summary>
    /// 下载多个文件
    /// </summary>
    /// <param name="downloadItems">要下载的文件项集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步下载操作的任务</returns>
    public async Task DownloadFiles(IEnumerable<DownloadItem> downloadItems,
        CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(MaxThreads);
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
                var elapsed = reportStopwatch.ElapsedMilliseconds;
                if (elapsed >= 1000)
                {
                    var totalSpeed = totalDownloadedBytes * 1000 / elapsed;
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
                    var response = await _httpClient.GetAsync(downloadItem.Url,
                        HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var dirPath = Path.GetDirectoryName(downloadItem.SaveAsPath);
                    if (!string.IsNullOrEmpty(dirPath))
                        FileUtil.IsDirectory(dirPath, true);

                    await using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    await using (var fileStream = new FileStream(downloadItem.SaveAsPath, FileMode.Create,
                                     FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(new Memory<byte>(buffer), cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancellationToken);
                            Interlocked.Add(ref totalDownloadedBytes, bytesRead);
                        }
                    }

                    Interlocked.Increment(ref filesDownloaded);
                    ProgressChanged?.Invoke(filesDownloaded, totalFiles);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref filesDownloaded);
                    ProgressChanged?.Invoke(filesDownloaded, totalFiles);
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

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}