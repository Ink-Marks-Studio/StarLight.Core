using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class VersionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("time")]
    public string Time { get; set; }
    
    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; }
    
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }
}