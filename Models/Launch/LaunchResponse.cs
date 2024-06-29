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
        
        public event Action<string> OutputReceived;
        
        public event Action<string> ErrorReceived;
        
        public LaunchResponse(Status Status, Stopwatch stopwatch, Process process, List<string> args, Exception exception)
        {
            Status = Status;
            RunTime = stopwatch;
            Process = process;
            Args = args;
            Exception = exception;

            process.OutputDataReceived += (sender, e) => 
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    OutputReceived?.Invoke(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    ErrorReceived?.Invoke(e.Data);
                }
            };
            
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