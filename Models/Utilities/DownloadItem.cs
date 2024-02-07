namespace StarLight_Core.Models.Utilities
{
    namespace StarLight_Core.Models.Utilities
    {
        public class DownloadItem
        {
            public string Url { get; set; }
        
            public string OutputPath { get; set; }

            public DownloadItem(string url, string outputPath = null)
            {
                Url = url;
                OutputPath = outputPath;
            }
        }
    }
}