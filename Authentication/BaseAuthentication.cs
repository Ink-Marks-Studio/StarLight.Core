using System.Text.RegularExpressions;

namespace StarLight_Core.Authentication;

public abstract class BaseAuthentication
{
    protected string ClientToken { get; set; }

    // 验证 Uuid 是否合法
    protected static bool IsValidUuid(string uuid)
    {
        var pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
        var regex = new Regex(pattern);

        return regex.IsMatch(uuid);
    }
}