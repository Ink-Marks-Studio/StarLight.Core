namespace StarLight.Core.Models.Authentication;

public class RetrieveDeviceCode
{
    // 访问令牌
    public string DeviceCode { get; set; }
    
    // 验证代码
    public string UserCode { get; set; }
    
    // 验证地址
    public string VerificationUri { get; set; }
    
    // 基本描述
    public string Message { get; set; }
}