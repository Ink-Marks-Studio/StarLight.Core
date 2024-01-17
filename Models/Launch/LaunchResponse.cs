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
        
        public List<string> Args { get; set; }
        
        public LaunchResponse(LaunchStatus launchStatus, Stopwatch stopwatch, Process process, List<string> args, Exception exception)
        {
            LaunchStatus = launchStatus;
            RunTime = stopwatch;
            Process = process;
            Args = args;
            Exception = exception;

            if (LaunchStatus == LaunchStatus.Success)
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                ProcessInfo = new ProcessInfo
                {
                    Name = Process.ProcessName,
                    Pid = Process.Id
                };
            }
        }
        
        public LaunchResponse(LaunchStatus launchStatus, Stopwatch stopwatch, Process process, Exception exception)
        {
            LaunchStatus = launchStatus;
            RunTime = stopwatch;
            Process = process;
            Exception = exception;
        }
    }
}