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
        //公有
        //   声明一个名为ProgressChanged的公开事件，该事件可以触发一个Action<int>类型的回调函数  
        public event Action<int> ProgressChanged;

        //   声明一个名为SpeedChanged的公开事件，该事件可以触发一个Action<double>类型的回调函数  
        public event Action<double> SpeedChanged;
        //公有

        //私有
        //   这是一个私有字符串变量，用于存储需要下载内容的URL地址  
        private string url;

        //   这是一个私有字符串变量，用于存储下载内容的目标路径或目标位置  
        private string destination;

        //   这是一个私有整型变量，用于存储用于并行下载的线程数量  
        private int threadCount;

        //   这是一个私有长整型变量，用于存储需要下载的总数据量  
        private long totalSize;

        //   这是一个私有长整型变量，用于存储已经下载的数据量  
        private long totalDownloaded;

        //   这是一个私有双精度浮点型变量，用于存储当前的下载速度  
        private double speed;
        //私有

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
