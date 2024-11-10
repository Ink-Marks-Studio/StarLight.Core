using System.Text.RegularExpressions;

namespace StarLight_Core.Authentication;

/// <summary>
/// 验证基类
/// </summary>
public abstract class BaseAuthentication
{
    /// <summary>
    /// 客户端令牌
    /// </summary>
    protected string ClientToken { get; set; }

    /// <summary>
    /// 验证 Uuid 是否合法
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    protected static bool IsValidUuid(string uuid)
    {
        const string pattern = "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
        var regex = new Regex(pattern);

        return regex.IsMatch(uuid);
    }
}