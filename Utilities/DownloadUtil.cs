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
        // 定义一个私有的下载部分的方法，接受起始位置、结束位置和部分索引作为参数  
        private void DownloadPart(long from, long to, int partIndex)
        {
            // 定义重试次数和最大重试次数  
            int retryCount = 0;
            int maxRetries = 3;

            // 当重试次数小于最大重试次数时，进入循环  
            while (retryCount < maxRetries)
            {
                try
                {
                    // 根据url创建HttpWebRequest对象，并通过AddRange方法设置下载范围  
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AddRange(from, to);

                    // 发送请求并获取响应，然后获取响应流并打开文件流以写入数据  
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (FileStream fileStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                    {
                        // 将文件流的位置设置为起始位置，以便从该位置开始写入数据  
                        fileStream.Position = from;
                        // 定义一个缓冲区，用于存储读取的数据  
                        byte[] buffer = new byte[4096];
                        // 定义一个变量，用于存储每次读取的字节数  
                        int bytesRead;
                        // 定义一个变量，用于存储总共读取的字节数  
                        long totalRead = 0;

                        // 创建一个Stopwatch对象，用于计时  
                        Stopwatch stopwatch = new Stopwatch();
                        // 开始计时  
                        stopwatch.Start();

                        // 当从响应流中读取的数据大于0时进入循环，读取数据并写入文件流，同时更新总共读取的字节数、总下载量和下载速度  
                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            Interlocked.Add(ref totalDownloaded, bytesRead); // 原子性地对总下载量进行增加操作，防止多线程竞争条件下的数据竞争问题  

                            // 如果已经超过1秒，则计算当前的下载速度并通知SpeedChanged事件处理程序，同时计算进度并通知ProgressChanged事件处理程序，然后重新开始计时器  
                            if (stopwatch.ElapsedMilliseconds > 1000)
                            {
                                speed = totalRead / (stopwatch.ElapsedMilliseconds / 1000.0); // 计算下载速度，单位是字节/秒或KB/s等，这取决于总读取的字节数和经过的时间（秒）  
                                SpeedChanged?.Invoke(speed); // 如果有SpeedChanged事件处理程序注册，则通过调用事件来通知下载速度的变化  
                                ProgressChanged?.Invoke((int)((totalDownloaded * 100) / totalSize)); // 如果有ProgressChanged事件处理程序注册，则通过调用事件来通知下载进度的变化，进度是总下载量除以总大小再乘以100得到百分比形式  
                                stopwatch.Restart(); // 重置计时器，以便重新开始计时下一次循环迭代的时间间隔  
                            }
                        }

                        // 如果在循环中没有发生异常，并且已经成功下载了数据，则跳出循环（break;）  
                        break;  
                    }
                }
                // 捕获IOException异常。当try块中的代码引发IOException时，此catch块将执行。  
                catch (IOException ex)
                {
                    // retryCount变量加1。每次捕获异常时，都会增加此计数。  
                    retryCount++;
                    // Thread.Sleep(2000);让当前线程休眠2000毫秒，也就是2秒。这是为了在重试之前等待一段时间，防止对服务器产生过大的压力。  
                    Thread.Sleep(2000);
                    // 检查retryCount是否大于等于maxRetries。maxRetries是定义在方法或类级别的一个变量，表示最大的重试次数。  
                    if (retryCount >= maxRetries)
                    {
                        // 如果已超过最大重试次数，则重新抛出异常。这样，异常可以在上层调用者中再次捕获和处理。  
                        throw;
                    }
                }
            }
        }
    }
}
