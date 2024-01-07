namespace StarLight_Core.Models.Launch
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public int Pid { get; set; }
        public string WindowTitle { get; set; }
        public TimeSpan RunTime { get; set; } 
        public int ExitCode { get; set; }
    }

}