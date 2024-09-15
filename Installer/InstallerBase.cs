namespace StarLight_Core.Installer
{
    public class InstallerBase
    {
        public Action<string,int>? OnProgressChanged { get; set; }
        
        public Action<string>? OnSpeedChanged { get; set; }
        
        protected string Root { get; set; }
    
        protected static string CalcMemoryMensurableUnit(double bytes)
        {
            double kb = bytes / 1024;
            double mb = kb / 1024;
            double gb = mb / 1024;
            double tb = gb / 1024;

            string result =
                tb > 1 ? $"{tb:0.##}TB" :
                gb > 1 ? $"{gb:0.##}GB" :
                mb > 1 ? $"{mb:0.##}MB" :
                kb > 1 ? $"{kb:0.##}KB" :
                $"{bytes:0.##}B";

            result = result.Replace("/", ".");
            return result;
        }
    }
}