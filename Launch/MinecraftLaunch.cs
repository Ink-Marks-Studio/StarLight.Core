using System.Diagnostics;
using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;
using StarLight_Core.Utilities;

namespace StarLight_Core.Launch
{
    public class MinecraftLaunch
    {
        public BaseAccount BaseAccount { get; set; }
        
        public GameWindowConfig GameWindowConfig { get; set; }
        
        public GameCoreConfig GameCoreConfig { get; set; }
        
        public JavaConfig JavaConfig { get; set; }
        
        public MinecraftLaunch(LaunchConfig launchConfig)
        {
            GameWindowConfig = launchConfig.GameWindowConfig;
            GameCoreConfig = launchConfig.GameCoreConfig;
            JavaConfig = launchConfig.JavaConfig;
            BaseAccount = launchConfig.Account.BaseAccount;
        }

        public async Task<ProcessInfo> LaunchAsync(Action<ProgressReport> onProgressChanged, Action<ProcessInfo> onProcessExited)
        {
            var stopwatch = new Stopwatch();
            var processInfo = new ProcessInfo();
            var progressReport = new ProgressReport();

            var tcs = new TaskCompletionSource<ProcessInfo>();
            
            try
            {
                progressReport.Description = "构建启动参数...";
                progressReport.Percentage = 10;
                onProgressChanged?.Invoke(progressReport);
                
                var arguments = new ArgumentsBuildUtil(GameWindowConfig, GameCoreConfig, JavaConfig, BaseAccount).Build();

                Console.WriteLine(string.Join(' '.ToString(), arguments));
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = JavaConfig.JavaPath,
                        Arguments = string.Join(' '.ToString(), arguments),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = GameCoreConfig.Root
                    },
                    EnableRaisingEvents = true
                };
                
                stopwatch.Start();
                process.Start();
                string processName = process.ProcessName;
                int pid = process.Id;

                progressReport.Description = "进程启动...";
                progressReport.Percentage = 100;
                onProgressChanged?.Invoke(progressReport);

                // 异步等待进程退出
                await Task.Run(() =>
                {
                    process.WaitForExit();
                    stopwatch.Stop();
                });
                
                processInfo = new ProcessInfo
                {
                    Name = processName,
                    Pid = pid,
                    RunTime = stopwatch.Elapsed,
                    ExitCode = process.ExitCode
                };
                
                onProcessExited?.Invoke(processInfo);

                return processInfo;
            }
            catch (Exception e)
            {
                throw new Exception("启动失败,未知原因: " + e);
            }
        }
    }
}

