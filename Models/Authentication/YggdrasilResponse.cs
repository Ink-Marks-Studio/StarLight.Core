using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class YggdrasilResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; init; }
    
    [JsonPropertyName("availableProfiles")]
    public List<AvailableProfiles> UserAccounts { get; init; }
}

public class AvailableProfiles 
{
    [JsonPropertyName("id")]
    public string Uuid { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}