using System.Diagnostics;
using System.Net;

namespace StarLight_Core.Utilities 
{
    public class DownloadsUtil 
    {
        public event Action<int> ProgressChanged;
        public event Action<double> SpeedChanged;
        
        private string url; // 地址
          
        private string destination; // 目标位置
          
        private int threadCount; // 并行下载线程数
          
        private long totalSize; // 总数据量
          
        private long totalDownloaded; // 数据量
          
        private double speed; // 下载速度
        
        public DownloadsUtil(string url, string destination, int threadCount)
        {
            this.url = url; // 地址 
            this.destination = destination; // 目标位置
            this.threadCount = threadCount; // 并行下载线程数  
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

                    break;
                }
                catch (IOException ex)
                {
                    retryCount++;
                    Thread.Sleep(3000);
                    if (retryCount >= maxRetries)
                    {
                        throw;
                    }
                }
            }
            
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

                totalRead = 0;
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