namespace StarLight.Core.Models.Authentication;

public class MicrosoftAccount : BaseAccount
{
    // 刷新令牌
    public string RefreshToken { get; set; }
    
    // 皮肤地址
    public string SkinUrl { get; set; }
    
    //获取时间
    public DateTime DateTime { get; set; }
}