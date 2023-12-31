using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class XboxResponse
{
    [JsonPropertyName("Token")]
    public string AuthToken { get; set; }
    

    [JsonPropertyName("DisplayClaims")]
    public DisplayClaims DisplayClaims { get; set; }
}

public class DisplayClaims
{
    [JsonPropertyName("xui")]
    public Xui[] Xui { get; set; }
}

public class Xui
{
    [JsonPropertyName("uhs")]
    public string UserHash { get; set; }
}