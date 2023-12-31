using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

public class GetTokenResponse
{
    // 验证令牌
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    // 刷新令牌
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    
    // 客户端Id
    public string ClientId { get; set; }
    
    // 过期时间
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}