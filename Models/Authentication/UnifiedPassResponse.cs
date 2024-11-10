using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class UnifiedPassResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }

    [JsonPropertyName("selectedProfile")]
    public UnifiedPassProfile SelectedProfile { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Uuid { get; set; }
}