using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Aurora_Star.Core.Utilities
{
    /* 生怕你们不会用
        var downloader = new DownloadUtil("http://example.com/largefile.zip", 4); // 分成4部分下载

        downloader.SpeedReported += (sender, speed) => 
        {
            Console.WriteLine($"Speed: {speed} bytes/sec");
        };

        downloader.ProgressReported += (sender, progress) => 
        {
            Console.WriteLine($"Progress: {progress}%");
        };

        await downloader.DownloadFileAsync("C:\\Downloads\\largefile.zip");
    */
    public class DownloadUtil
    {
        private HttpClient _client;
        private int _numberOfParts;
        private long _totalBytesDownloaded = 0;
        private DateTime _downloadStartTime;
        private long _fileSize;
        private string _fileUrl;
        private ConcurrentQueue<(long bytes, DateTime time)> _speedCalculationQueue = new ConcurrentQueue<(long, DateTime)>();

        public event EventHandler<double> SpeedReported;
        public event EventHandler<double> ProgressReported;

        public DownloadUtil(string fileUrl, int numberOfParts)
        {
            _client = new HttpClient();
            _numberOfParts = numberOfParts;
            _fileUrl = fileUrl;
        }

        public async Task DownloadFileAsync(string savePath)
        {
            _fileSize = await GetFileSizeAsync(_fileUrl);
            _downloadStartTime = DateTime.Now;

            long partSize = _fileSize / _numberOfParts;
            var tasks = new List<Task>();
            for (int i = 0; i < _numberOfParts; i++)
            {
                long start = i * partSize;
                long end = (i < _numberOfParts - 1) ? start + partSize - 1 : _fileSize;
                tasks.Add(DownloadPartAsync(_fileUrl, savePath, i, start, end));
            }

            await Task.WhenAll(tasks);
            MergeFiles(savePath);
        }

        private async Task<long> GetFileSizeAsync(string url)
        {
            using var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            response.EnsureSuccessStatusCode();
            return long.Parse(response.Content.Headers.ContentLength.ToString());
        }

        private async Task DownloadPartAsync(string url, string savePath, int partIndex, long start, long end)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            string partPath = $"{savePath}.part{partIndex}";
            await File.WriteAllBytesAsync(partPath, fileBytes);

            UpdateSpeed(fileBytes.Length);
            ReportSpeed();
            ReportProgress();
        }

        private void UpdateSpeed(long recentBytes)
        {
            var now = DateTime.Now;
            _speedCalculationQueue.Enqueue((recentBytes, now));
            _totalBytesDownloaded += recentBytes;

            // Keep only the last X seconds of data for speed calculation
            while (_speedCalculationQueue.TryPeek(out var old) && (now - old.time).TotalSeconds > 5) // for example, last 5 seconds
            {
                _speedCalculationQueue.TryDequeue(out _);
            }
        }

        private void ReportSpeed()
        {
            if (_speedCalculationQueue.IsEmpty) return;

            var now = DateTime.Now;
            long bytesInLastPeriod = 0;
            DateTime earliestTime = now;

            foreach (var (bytes, time) in _speedCalculationQueue)
            {
                bytesInLastPeriod += bytes;
                if (time < earliestTime) earliestTime = time;
            }

            var elapsedTime = now - earliestTime;
            double speed = elapsedTime.TotalSeconds > 0 ? bytesInLastPeriod / elapsedTime.TotalSeconds : 0;
            SpeedReported?.Invoke(this, speed);
        }

        private void ReportProgress()
        {
            double progress = (double)_totalBytesDownloaded / _fileSize * 100;
            ProgressReported?.Invoke(this, progress);
        }

        private void MergeFiles(string savePath)
        {
            using var outputStream = File.Create(savePath);
            for (int i = 0; i < _numberOfParts; i++)
            {
                string partPath = $"{savePath}.part{i}";
                try
                {
                    using var inputStream = File.OpenRead(partPath);
                    inputStream.CopyTo(outputStream);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (File.Exists(partPath))
                    {
                        File.Delete(partPath);
                    }
                }
            }
        }
    }
}
