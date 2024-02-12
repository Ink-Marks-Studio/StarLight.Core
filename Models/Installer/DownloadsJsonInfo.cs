using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class DownloadsJsonInfo
{
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
    
    [JsonPropertyName("size")]
    public ulong Size { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}