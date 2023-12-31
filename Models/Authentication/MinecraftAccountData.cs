using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class MinecraftAccountData
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}