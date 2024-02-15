using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer;

public class DownloadsJsonEntity
{
    [JsonPropertyName("client")]
    public DownloadsJsonInfo Client { get; set; }
    
    [JsonPropertyName("server")]
    public DownloadsJsonInfo server { get; set; }
}