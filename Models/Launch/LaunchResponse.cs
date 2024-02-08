using System.Diagnostics;
using StarLight_Core.Enum;

namespace StarLight_Core.Models.Launch
{
    public class LaunchResponse
    {
        public Status Status { get; set; }
        
        public Stopwatch RunTime { get; set; }
        
        public Process Process { get; set; }
        
        public Exception Exception { get; set; }
        
        public ProcessInfo ProcessInfo { get; set; }
        
        public List<string> Args { get; set; }
        
        public LaunchResponse(Status Status, Stopwatch stopwatch, Process process, List<string> args, Exception exception)
        {
            Status = Status;
            RunTime = stopwatch;
            Process = process;
            Args = args;
            Exception = exception;

            if (Status == Status.Succeeded)
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
        
        public LaunchResponse(Status Status, Stopwatch stopwatch, Process process, Exception exception)
        {
            Status = Status;
            RunTime = stopwatch;
            Process = process;
            Exception = exception;
        }
    }
}