using System.Diagnostics;
using StarLight_Core.Enum;

namespace StarLight_Core.Models.Launch
{
    public class LaunchResponse
    {
        public LaunchStatus LaunchStatus { get; set; }
        
        public Stopwatch RunTime { get; set; }
        
        public Process Process { get; set; }
        
        public Exception Exception { get; set; }
        
        public ProcessInfo ProcessInfo { get; set; }
        
        public LaunchResponse(LaunchStatus launchStatus, Stopwatch stopwatch, Process process, Exception exception)
        {
            LaunchStatus = launchStatus;
            RunTime = stopwatch;
            Process = process;
            Exception = exception;

            if (LaunchStatus == LaunchStatus.Success)
            {
                process.Start();
                ProcessInfo = new ProcessInfo()
                {
                    Name = Process.ProcessName,
                    Pid = Process.Id,
                    Title = Process.MainWindowTitle
                };
            }
        }
    }
}