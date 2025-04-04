using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Processor.Utility;

internal class FabricModJson
{
    [JsonPropertyName("id")] 
    public string ModId { get; set; }

    [JsonPropertyName("version")] 
    public string ModVersion { get; set; }
    
    [JsonPropertyName("name")] 
    public string DisplayName { get; set; }
    
    [JsonPropertyName("authors")] 
    public object[]? Authors { get; set; }
    
    [JsonPropertyName("icon")] 
    public string? ModIconPath { get; set; }
    
    [JsonPropertyName("description")] 
    public string Description { get; set; }
    
    [JsonPropertyName("depends")] 
    public Dictionary<string, object>? Depends { get; set; }
}