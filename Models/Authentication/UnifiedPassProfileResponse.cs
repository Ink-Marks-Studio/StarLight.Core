using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication;

/// <summary>
/// 统一通行证游戏账户档案类
/// </summary>
public class UnifiedPassProfile
{
    /// <summary>
    /// 游戏账户 UUID
    /// </summary>
    [JsonPropertyName("id")]
    public string Uuid { get; set; }

    /// <summary>
    /// 游戏名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}