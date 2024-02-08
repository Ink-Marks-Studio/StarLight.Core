namespace StarLight_Core.Models.Utilities
{
    public class DownloadItem
    {
        public string Url { get; set; }
        
        public string? SaveAsPath { get; set; }

        public DownloadItem(string url, string? saveAsPath = null)
        {
            Url = url;
            SaveAsPath = saveAsPath;
        }
    }
}