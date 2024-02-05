using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        private HttpClient httpClient = new HttpClient();
        private long totalBytesReceived = 0;
        private long totalExpectedBytes = 0;

        public DownloadsUtil()
        {
            httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task DownloadFilesAsync(IEnumerable<string> urls, string outputFolder, Action<double, double> progressChanged)
        {
            var sw = Stopwatch.StartNew();
            var downloadTasks = urls.Select(url => DownloadFileAsync(url, outputFolder)).ToList();

            // Start a timer to periodically report progress
            using var timer = new Timer(_ =>
            {
                var progress = totalExpectedBytes > 0 ? (double)totalBytesReceived / totalExpectedBytes * 100 : 0;
                var speed = totalBytesReceived / sw.Elapsed.TotalSeconds;
                progressChanged(progress, speed);
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

            var tasksWithExpectedBytes = await Task.WhenAll(downloadTasks);
            totalExpectedBytes = tasksWithExpectedBytes.Sum(task => task.ExpectedBytes);

            timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
            sw.Stop();

            // Final progress update
            var finalProgress = totalExpectedBytes > 0 ? 100.0 : 0; // Assuming all downloads are complete
            var finalSpeed = totalBytesReceived / sw.Elapsed.TotalSeconds;
            progressChanged(finalProgress, finalSpeed);
        }

        private async Task<(long ExpectedBytes, long DownloadedBytes)> DownloadFileAsync(string url, string outputFolder)
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
            var outputPath = Path.Combine(outputFolder, fileName);
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

            return (totalBytes, bytesReceived);
        }
    }
}