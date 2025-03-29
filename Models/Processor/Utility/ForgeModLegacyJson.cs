using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Processor.Utility;

internal class ForgeModLegacyJson
{
    [JsonPropertyName("dependencies")] 
    public string[]? Depends { get; set; }
    
    [JsonPropertyName("modid")] 
    public string? ModId { get; set; }
    
    [JsonPropertyName("name")] 
    public string? DisplayName { get; set; }
    
    [JsonPropertyName("description")] 
    public string? Description { get; set; }
    
    [JsonPropertyName("authorList")] 
    public string[]? Author { get; set; }
    
    [JsonPropertyName("version")] 
    public string? ModVersion { get; set; }
    
    [JsonPropertyName("logoFile")] 
    public object? IconLogoPath { get; set; }
    
    [JsonPropertyName("modList")]
    public ForgeModLegacyJson[]? ModList2 { get; set; }
}