namespace StarLight_Core.Installer
{
    /// <summary>
    /// 安装器基类
    /// </summary>
    public abstract class InstallerBase
    {
        /// <summary>
        /// 进度报告方法
        /// </summary>
        public Action<string,int>? OnProgressChanged { get; set; }
        
        /// <summary>
        /// 速度报告方法
        /// </summary>
        public Action<string>? OnSpeedChanged { get; set; }
        
        /// <summary>
        /// 游戏根目录
        /// </summary>
        protected string Root { get; set; }
    
        /// <summary>
        /// 速度转换方法
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
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