using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aurora_Star.Core.Utilities
{
    /* 生怕你们不会用
     static async Task Main()
    {
        string url = "https://example.com/file.zip";
        string destinationPath = "downloadedFile.zip";

        var downloader = new MultithreadedDownloader(url, destinationPath);

        // 订阅下载进度事件
        downloader.ProgressChanged += (sender, progress) =>
        {
            Console.WriteLine($"Download progress: {progress:P}");
        };

        // 订阅实时下载速度事件
        downloader.SpeedChanged += (sender, speed) =>
        {
            Console.WriteLine($"Download speed: {speed:F} KB/s");
        };

        await downloader.StartDownloadAsync();
    }
    */
    public class DownloadsUtil
    {
        public event EventHandler<double> ProgressChanged;
        public event EventHandler<double> SpeedChanged;

        private readonly string _url;
        private readonly string _destinationPath;

        public DownloadsUtil(string url, string destinationPath)
        {
            _url = url;
            _destinationPath = destinationPath;
        }

        public async Task StartDownloadAsync()
        {
            await DownloadFileAsync(_url, _destinationPath);
        }

        private async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalSize = response.Content.Headers.ContentLength ?? -1L;
                    var progressDict = new Dictionary<int, double>();
                    var speedDict = new Dictionary<int, double>();

                    var tasks = new List<Task>();

                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        int threadIndex = i;
                        tasks.Add(Task.Run(async () =>
                        {
                            using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                          fileStream = new FileStream($"{destinationPath}.part{threadIndex}", FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                var totalRead = 0L;
                                var buffer = new byte[8192];
                                var isMoreToRead = true;

                                do
                                {
                                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                    if (read == 0)
                                    {
                                        isMoreToRead = false;
                                    }
                                    else
                                    {
                                        await fileStream.WriteAsync(buffer, 0, read);

                                        totalRead += read;

                                        // 计算下载进度
                                        var progress = totalRead * 1d / totalSize;
                                        progressDict[threadIndex] = progress;

                                        // 计算实时下载速度
                                        var speed = read / 1024d;
                                        speedDict[threadIndex] = speed;

                                        // 获取总体下载进度
                                        var overallProgress = progressDict.Values.Average();
                                        OnProgressChanged(overallProgress);

                                        // 获取总体实时下载速度
                                        var overallSpeed = speedDict.Values.Average();
                                        OnSpeedChanged(overallSpeed);
                                    }
                                } while (isMoreToRead);
                            }
                        }));
                    }

                    // 等待所有任务完成
                    await Task.WhenAll(tasks);

                    // 合并下载的文件块
                    using (var combinedFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        for (int i = 0; i < Environment.ProcessorCount; i++)
                        {
                            var partFilePath = $"{destinationPath}.part{i}";
                            using (var partFileStream = new FileStream(partFilePath, FileMode.Open, FileAccess.Read))
                            {
                                await partFileStream.CopyToAsync(combinedFileStream);
                            }

                            // 删除临时文件块
                            File.Delete(partFilePath);
                        }
                    }
                }
            }
        }

        protected virtual void OnProgressChanged(double progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        protected virtual void OnSpeedChanged(double speed)
        {
            SpeedChanged?.Invoke(this, speed);
        }
    }
}

