using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Aurora_Star.Core.Utilities
{
    public class DownloadUtil
    {
        public event Action<int> ProgressChanged;
        public event Action<double> SpeedChanged;

        private string url;
        private string destination;
        private int threadCount;
        private long totalSize;
        private long totalDownloaded;
        private double speed;

        public DownloadUtil(string url, string destination, int threadCount)
        {
            this.url = url;
            this.destination = destination;
            this.threadCount = threadCount;
        }

        public void StartDownload()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                totalSize = resp.ContentLength;
            }

            long partSize = totalSize / threadCount;
            long leftOver = totalSize % threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                long from = i * partSize;
                long to = (i < threadCount - 1) ? from + partSize - 1 : from + partSize + leftOver - 1;

                Thread thread = new Thread(() => DownloadPart(from, to, i));
                thread.Start();
            }
        }

        private void DownloadPart(long from, long to, int partIndex)
        {
            int retryCount = 0;
            int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AddRange(from, to);

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (FileStream fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        fileStream.Position = from;
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        long totalRead = 0;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            Interlocked.Add(ref totalDownloaded, bytesRead);

                            if (stopwatch.ElapsedMilliseconds > 1000)
                            {
                                speed = totalRead / (stopwatch.ElapsedMilliseconds / 1000.0);
                                SpeedChanged?.Invoke(speed);
                                ProgressChanged?.Invoke((int)((totalDownloaded * 100) / totalSize));
                                stopwatch.Restart();
                            }
                        }

                        break; // Break the loop if download is successful
                    }
                }
                catch (IOException ex)
                {
                    retryCount++;
                    Thread.Sleep(2000); // Wait for 2 seconds before retrying
                    if (retryCount >= maxRetries)
                    {
                        throw; // Re-throw the exception if max retries are exceeded
                    }
                }
            }
        }

    }
}
