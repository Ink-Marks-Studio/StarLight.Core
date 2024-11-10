using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class RetrieveDeviceCode
{
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; }

    [JsonPropertyName("user_code")]
    public string UserCode { get; set; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}