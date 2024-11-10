using System.Text.Json;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Utilities;

namespace StarLight_Core.Authentication;

/// <summary>
/// 外置验证类
/// </summary>
/// <a href="https://mohen.wiki/Authentication/Yggdrasil.html">查看文档</a>
public class YggdrasilAuthenticator : BaseAuthentication
{
    /// <summary>
    /// 外置验证器
    /// </summary>
    /// <param name="url"></param>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="clientToken"></param>
    /// <a href="https://mohen.wiki/Authentication/Yggdrasil.html">查看文档</a>
    public YggdrasilAuthenticator(string url, string email, string password, string clientToken = "")
    {
        Url = Url == "LittleSkin" ? "https://littleskin.cn/api/yggdrasil" : url;
        Email = email;
        Password = password;
        ClientToken = clientToken;
    }

    /// <summary>
    /// 外置验证器
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="clientToken"></param>
    /// <a href="https://mohen.wiki/Authentication/Yggdrasil.html">查看文档</a>
    public YggdrasilAuthenticator(string email, string password, string clientToken = "")
    {
        Url = "https://littleskin.cn/api/yggdrasil";
        Email = email;
        Password = password;
        ClientToken = clientToken;
    }

    private string Url { get; }

    private string Email { get; }

    private string Password { get; }

    /// <summary>
    /// 异步验证方法
    /// </summary>
    /// <returns></returns>
    /// <a href="https://mohen.wiki/Authentication/Yggdrasil.html">查看文档</a>
    public async ValueTask<IEnumerable<YggdrasilAccount>> YggdrasilAuthAsync()
    {
        var requestJson = new
        {
            clientToken = IsValidUuid(ClientToken) ? ClientToken : Guid.NewGuid().ToString("N"),
            username = Email,
            password = Password,
            requestUser = false,
            agent = new
            {
                name = "Minecraft",
                version = 1
            }
        };

        var baseUrl = string.IsNullOrEmpty(Url) ? "https://authserver.mojang.com" : Url;
        var requestUrl = $"{baseUrl}/authserver/authenticate";

        var postResponseContent =
            await HttpUtil.SendHttpPostRequest(requestUrl, JsonSerializer.Serialize(requestJson), "application/json");

        var accountMessage = JsonSerializer.Deserialize<YggdrasilResponse>(postResponseContent);

        return accountMessage.UserAccounts.Select(userAccount => new YggdrasilAccount
            {
                AccessToken = accountMessage.AccessToken,
                ClientToken = accountMessage.ClientToken,
                Name = userAccount.Name,
                Uuid = Guid.Parse(userAccount.Uuid).ToString(),
                ServerUrl = Url ?? string.Empty,
                Email = Email ?? string.Empty,
                Password = Password ?? string.Empty
            })
            .ToList();
    }
}