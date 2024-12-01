using System.Security.Cryptography;
using System.Text;
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
    
    /// <summary>
    /// 生成 UUID
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    protected static string GenerateNameUuid(string characterName)
    {
        var input = "OfflinePlayer:" + characterName;
        var inputBytes = Encoding.UTF8.GetBytes(input);
        
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(inputBytes);

        // 根据 RFC 4122 生成 UUID
        hashBytes[6] &= 0x0f;
        hashBytes[6] |= 0x30;
        hashBytes[8] &= 0x3f;
        hashBytes[8] |= 0x80;

        return new Guid(hashBytes).ToString();
    }
}