using StarLight_Core.Models.Authentication;
using System.Text.Json;
using StarLight_Core.Utilities;

namespace StarLight_Core.Authentication
{
    public class YggdrasilAuthenticator
    {
        public string Url { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ClientToken { get; private set; }
        
        public YggdrasilAuthenticator(string url, string email, string password)
        {
            if (Url == "LittleSkin")
            {
                Url = "https://littleskin.cn/api/yggdrasil";
            }
            else
            {
                Url = url;
            }
            Email = email;
            Password = password;
        }
        
        public YggdrasilAuthenticator(string email, string password)
        {
            Url = "https://littleskin.cn/api/yggdrasil";
            Email = email;
            Password = password;
        }
        
        public async ValueTask<IEnumerable<YggdrasilAccount>> YggdrasilAuthAsync()
        {
            var requestJson = new
            {
                clientToken = Guid.NewGuid().ToString("N"),
                username = Email,
                password = Password,
                requestUser = false,
                agent = new
                {
                    name = "Minecraft",
                    version = 1
                }
            };

            string baseUrl = string.IsNullOrEmpty(Url) ? "https://authserver.mojang.com" : Url;
            string requestUrl = $"{baseUrl}/authenticate";

            var postResponseContent = await HttpUtil.SendHttpPostRequest(requestUrl, JsonSerializer.Serialize(requestJson), "application/json");

            var accountMessage = JsonSerializer.Deserialize<YggdrasilResponse>(postResponseContent);

            List<YggdrasilAccount> accounts = new();
            foreach (var userAccount in accountMessage.UserAccounts)
            {
                accounts.Add(new YggdrasilAccount
                {
                    AccessToken = accountMessage.AccessToken,
                    ClientToken = accountMessage.ClientToken,
                    Name = userAccount.Name,
                    Uuid = Guid.Parse(userAccount.Uuid).ToString(),
                    ServerUrl = Url ?? string.Empty,
                    Email = Email ?? string.Empty,
                    Password = Password ?? string.Empty
                });
            }

            return accounts;
        }
    }
}
