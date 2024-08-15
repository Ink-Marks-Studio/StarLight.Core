using StarLight_Core.Enum;

namespace StarLight_Core.Models.Installer
{
    public class CheckResult
    {
        private Status Status { get; set; }
        
        public string? Exception { get; set; }
        
        public CheckResult(Status status)
        {
            Status = status;

            if (Status == Status.Cancel)
                Exception = "已取消操作";
        }

        public CheckResult(Status status, string exception)
        {
            Status = status;
            Exception = exception;
        }
    }
}