namespace StarLight.Core.Models.Authentication;

public class GetTokenResponse
{
    // 验证令牌
    public string AccessToken { get; set; }
    
    // 刷新令牌
    public string RefreshToken { get; set; }
    
    // 过期时间
    public int ExpiresIn { get; set; }
}