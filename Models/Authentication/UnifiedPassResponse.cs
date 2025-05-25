using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

/// <summary>
/// 统一通行证登录返回信息类
/// </summary>
public class UnifiedPassResponse
{
    /// <summary>
    /// 资源令牌
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    /// <summary>
    /// 客户端令牌
    /// </summary>
    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }

    /// <summary>
    /// 游戏账户信息
    /// </summary>
    [JsonPropertyName("selectedProfile")]
    public UnifiedPassProfile SelectedProfile { get; set; }

    /// <summary>
    /// 统一通行证账户信息
    /// </summary>
    [JsonPropertyName("user")]
    public User User { get; set; }
}

/// <summary>
/// 统一通行证账户信息类
/// </summary>
public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}