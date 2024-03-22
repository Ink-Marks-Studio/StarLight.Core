using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class AssetIndex
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
    
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}