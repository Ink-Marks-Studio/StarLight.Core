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

        // public DownloadUtil(string url, string destination, int threadCount) 是一个构造函数，用于创建DownloadUtil类的实例  
        // 它接受三个参数：url、destination和threadCount  
        // 在构造函数中，我们将这三个参数分别赋值给DownloadUtil类中的url、destination和threadCount属性
        public DownloadUtil(string url, string destination, int threadCount)
        {
            // 将传入的url赋值给this.url属性，表示要下载的URL地址  
            this.url = url;

            // 将传入的destination赋值给this.destination属性，表示下载内容的目标路径或目标位置  
            this.destination = destination;

            // 将传入的threadCount赋值给this.threadCount属性，表示用于并行下载的线程数量  
            this.threadCount = threadCount;
        }

        public void StartDownload()
        {
            // 创建一个HttpWebRequest对象，根据传入的url来构造一个Web请求  
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // 设置请求的HTTP方法为HEAD，即只获取请求头而不获取具体内容  
            request.Method = "HEAD";

            // 使用WebRequest.Create(url)创建的请求对象获取响应，并将响应对象赋值给HttpWebResponse对象  
            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                // 从响应对象中获取内容长度，并赋值给totalSize，用于记录需要下载的总数据量  
                totalSize = resp.ContentLength;
            }

            // 根据总数据量和线程数量计算每一部分下载的数据量，并赋值给partSize  
            long partSize = totalSize / threadCount;

            // 计算总数据量除以线程数量后的余数，并赋值给leftOver，用于最后一部分下载的数据量可能有所不同  
            long leftOver = totalSize % threadCount;

            // 根据线程数量进行循环，为每个线程分配一个下载任务  
            for (int i = 0; i < threadCount; i++)
            {
                // 计算每个线程需要下载的数据范围的起始位置和结束位置  
                long from = i * partSize;
                long to = (i < threadCount - 1) ? from + partSize - 1 : from + partSize + leftOver - 1;

                // 创建一个新的线程来执行下载任务，将下载任务封装在DownloadPart方法中，并通过Lambda表达式传入需要下载的数据范围的起始位置和结束位置，以及线程的标识符  
                Thread thread = new Thread(() => DownloadPart(from, to, i));
                // 启动线程执行下载任务  
                thread.Start();
            }
        }
        
        // 下载部分，接受起始位置、结束位置和部分索引作为参数
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
                SpeedChanged?.Invoke(0); // 或者根据实际情况设置其他默认值
                ProgressChanged?.Invoke((int)((totalDownloaded * 100) / totalSize));
            }
        }
    }
}
