namespace StarLight_Core.Models.Utilities
{
    public class DownloadItem
    {
        public string Url { get; set; }
        
        public string SaveAsName { get; set; }

        public DownloadItem(string url, string saveAsName = null)
        {
            Url = url;
            SaveAsName = saveAsName;
        }
    }

}