using System.Diagnostics;
using System.Net.Http.Headers;
using StarLight_Core.Utilities;

namespace StarLight_Core.Downloader;

/// <summary>
/// 多线程文件下载器类
/// </summary>
public class MultiThreadedFileDownloader : DownloaderBase
{
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// 多线程文件下载器构造函数
    /// </summary>
    /// <param name="onSpeedChanged"></param>
    /// <param name="cancellationToken"></param>
    public MultiThreadedFileDownloader(Action<double> onSpeedChanged, CancellationToken cancellationToken = default)
    {
        OnSpeedChanged = onSpeedChanged;
        _cancellationToken = cancellationToken;
    }

    /// <summary>
    /// 多线程文件下载器
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="filePath">保存路径</param>
    public async Task DownloadFileWithMultiThread(string url, string filePath)
    {
        if (FileUtil.IsFile(filePath))
            return;
        FileUtil.IsDirectory(FileUtil.GetFileDirectory(filePath), true);

        using var client = new HttpClient();
        var totalSize = await GetFileSizeAsync(client, url);

        var partSize = totalSize / MaxThreads;
        var downloadTasks = new List<Task>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        long totalDownloaded = 0;
        var lockObj = new object();

        for (var i = 0; i < MaxThreads; i++)
        {
            var startRange = i * partSize;
            var endRange = i == MaxThreads - 1 ? totalSize - 1 : startRange + partSize - 1;

            downloadTasks.Add(Task.Run(async () =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(startRange, endRange);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    _cancellationToken);
                response.EnsureSuccessStatusCode();
                await using var contentStream = await response.Content.ReadAsStreamAsync(_cancellationToken);
                await using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write,
                    FileShare.Write, 8192, true);
                fileStream.Seek(startRange, SeekOrigin.Begin);
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationToken);

                    // 更新总字节数
                    lock (lockObj)
                    {
                        totalDownloaded += bytesRead;
                    }

                    _cancellationToken.ThrowIfCancellationRequested();
                }
            }, _cancellationToken));
        }

        // 下载速度
        var monitorTask = Task.Run(() =>
        {
            long previousDownloaded = 0;
            while (!Task.WhenAll(downloadTasks).IsCompleted)
            {
                Thread.Sleep(1000);
                lock (lockObj)
                {
                    var downloadedThisSecond = totalDownloaded - previousDownloaded;
                    previousDownloaded = totalDownloaded;
                    OnSpeedChanged?.Invoke(downloadedThisSecond);
                }

                if (_cancellationToken.IsCancellationRequested)
                    _cancellationToken.ThrowIfCancellationRequested();
            }
        }, _cancellationToken);

        try
        {
            await Task.WhenAll(downloadTasks);
            await monitorTask;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}