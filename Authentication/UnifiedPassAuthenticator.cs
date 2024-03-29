using System.Text;
using System.Text.Json;
using StarLight_Core.Utilities;
using StarLight_Core.Models;
using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Authentication
{
    // 统一通行证验证类
    public class UnifiedPassAuthenticator
    {
        private const string UnifiedPassBaseUrl = "https://auth.mc-user.com:233/";

        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string ServerId { get; set; }
        
        public string BaseUrl { get; set; }
        
        public UnifiedPassAuthenticator(string username, string password, string serverId, string baseUrl = UnifiedPassBaseUrl)
        {
            Username = username;
            Password = password;
            ServerId = serverId;
            BaseUrl = baseUrl;
        }
        
        // 统一通行证异步验证方法
        public async Task<UnifiedPassAccount> UnifiedPassAuthAsync()
        {
            Uri authenticateUri = new Uri(new Uri(BaseUrl), ServerId +"/authserver/authenticate");

            var requestData = new
            {
                agent = new { name = "StarLight.Core", version = StarLightInfo.Version },
                username = Username,
                password = Password,
                clientToken = null as string,
                requestUser = true
            };  

            string jsonData = JsonSerializer.Serialize(requestData);
            
            string response;
            try
            {
                response = await HttpUtil.SendHttpPostRequest(authenticateUri.ToString(), jsonData,"application/json");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("[SL]身份验证时出错: ", ex);
            }

            var authResponse = JsonSerializer.Deserialize<UnifiedPassResponse>(response);

            // 创建字典对象并返回
            UnifiedPassAccount result = new UnifiedPassAccount
            {
                Name = authResponse.SelectedProfile.Name,
                Uuid = authResponse.SelectedProfile.Uuid,
                AccessToken = authResponse.AccessToken,
                ClientToken = authResponse.ClientToken,
                ServerId = ServerId
            };

            return result;
        }

        public async Task<UnifiedPassAccount> RefreshUnifiedPassAsync(string clientToken, string refreshToken)
        {
            var refreshPostData = new
            {
                accessToken = refreshToken,
                clientToken = clientToken,
                requestUser = true
            };
            
            string jsonData = JsonSerializer.Serialize(refreshPostData);
            string response;
            try
            {
                response = await HttpUtil.SendHttpPostRequest(BaseUrl + "authserver/refresh", jsonData,"application/json");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("[SL]刷新令牌时出错: ", ex);
            }
            
            var authResponse = JsonSerializer.Deserialize<UnifiedPassRefreshResponse>(response);
            return new UnifiedPassAccount
            {
                Name = authResponse.SelectedProfile.Name,
                Uuid = authResponse.SelectedProfile.Uuid,
                AccessToken = authResponse.AccessToken,
                ClientToken = authResponse.ClientToken,
                ServerId = ServerId
            };
        }
        
        // 获取玩家皮肤信息。
        public static async Task<Dictionary<string, string>> RetrieveSkinInfo(string baseUrl, string profileId)
        {
            return new Dictionary<string, string>();
        }
    }
}