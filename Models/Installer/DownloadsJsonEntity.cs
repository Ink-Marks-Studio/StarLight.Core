using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class DownloadsJsonEntity
{
    [JsonPropertyName("client")]
    public DownloadsJsonEntity Client { get; set; }
    
    [JsonPropertyName("server")]
    public DownloadsJsonEntity server { get; set; }
}