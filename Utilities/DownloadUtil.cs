using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StarLight.Core.Utilities 
{
    public class DownloadUtil 
    {
        public event Action<int> ProgressChanged;
        public event Action<double> SpeedChanged;
        
        // 地址  
        private string url;

        // 目标位置  
        private string destination;

        // 并行下载线程数  
        private int threadCount;

        // 总数据量  
        private long totalSize;

        // 数据量  
        private long totalDownloaded;

        // 下载速度  
        private double speed;
        
        public DownloadUtil(string url, string destination, int threadCount)
        {
            // 地址  
            this.url = url;

            // 目标位置  
            this.destination = destination;

            // 并行下载线程数  
            this.threadCount = threadCount;
        }

        public void StartDownload()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // 设置请求的HTTP方法为HEAD，即只获取请求头而不获取具体内容  
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

                // 创建一个新的线程来执行下载任务，传入需要下载的数据范围的起始位置和结束位置，以及线程的标识符  
                Thread thread = new Thread(() => DownloadPart(from, to, i));
                // 启动线程执行下载任务  
                thread.Start();
            }
        }
        
        // 下载部分
        private void DownloadPart(long from, long to, int partIndex)
        {
            int retryCount = 0;
            int maxRetries = 3;
            long totalRead = 0; // 追踪当前线程读取的总字节数

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
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            Interlocked.Add(ref totalDownloaded, bytesRead);

                            UpdateProgress(stopwatch, ref totalRead);
                        }
                    }

                    break; // 成功下载后退出循环
                }
                catch (IOException ex)
                {
                    retryCount++;
                    Thread.Sleep(2000);
                    if (retryCount >= maxRetries)
                    {
                        throw;
                    }
                }
            }

            // 下载完成时的最终更新
            UpdateProgress(new Stopwatch(), ref totalRead, true);
        }

        private void UpdateProgress(Stopwatch stopwatch, ref long totalRead, bool forceUpdate = false)
        {
            if (stopwatch.ElapsedMilliseconds > 0)
            {
                double speed = totalRead / (stopwatch.ElapsedMilliseconds / 1000.0);
                SpeedChanged?.Invoke(speed);
                ProgressChanged?.Invoke((int)((totalDownloaded * 100) / totalSize));
                stopwatch.Restart();

                totalRead = 0; // 重置 totalRead 为下一次测量准备
            }
            else if (forceUpdate)
            {
                // 如果是强制更新但计时器时间为零，可以考虑跳过速度更新或设置默认值
                SpeedChanged?.Invoke(0); 
                ProgressChanged?.Invoke((int)((totalDownloaded * 100) / totalSize));
            }
        }
    }
}
