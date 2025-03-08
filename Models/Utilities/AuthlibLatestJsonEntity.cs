using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

public class AuthlibLatestJsonEntity
{
    [JsonPropertyName("build_number")]
    public int BuildNumber { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
    
    [JsonPropertyName("release_time")]
    public string ReleaseTime { get; set; }
    
    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; }
    
    [JsonPropertyName("checksums")]
    public Dictionary<string, string> Checksums { get; set; }
}