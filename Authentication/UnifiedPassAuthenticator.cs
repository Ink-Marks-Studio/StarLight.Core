using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarLight_Core.Utilities;
using StarLight_Core.Models;
using StarLight_Core.Enum;
using StarLight_Core.Models.Authentication;

namespace StarLight_Core.Authentication
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
        public Profile SelectedProfile { get; set; }
        public List<Profile> AvailableProfiles { get; set; }
        public User User { get; set; }
    }

    public class Profile
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class User
    {
        public string Id { get; set; }
    }


    // 统一通行证验证类
    public static class UnifiedPassAuthenticator
    {
        private const string UnifiedPassBaseUrl = "https://auth.mc-user.com:233/";
        public static async Task<UnifiedPassAccount> Authenticate(string username, string password, string serverId, string baseUrl = UnifiedPassBaseUrl)
        {
            Uri authenticateUri = new Uri(new Uri(baseUrl), serverId +"/authserver/authenticate");

            var requestData = new
            {
                agent = new { name = "StarLight.Core", version = StarLightInfo.Version },
                username,
                password,
                clientToken = null as string,
                requestUser = true
            };

            string jsonData = JsonConvert.SerializeObject(requestData);

            string response;
            try
            {
                response = await HttpUtil.SendHttpPostRequest(authenticateUri.ToString(), jsonData,"application/json");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors)
                throw new ApplicationException("[SL]身份验证时出错: ", ex);
            }

            var authResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(response);

            // 创建字典对象并返回
            UnifiedPassAccount result = new UnifiedPassAccount
            {
                Type = AuthType.UnifiedPass.ToString(),
                Name = authResponse.SelectedProfile?.Name,
                Uuid = authResponse.SelectedProfile?.Id,
                AccessToken = authResponse.AccessToken,
                ClientToken = authResponse.ClientToken,
                ServerId = serverId
            };

            return result;
        }
        
        // 获取玩家皮肤信息。
        public static async Task<Dictionary<string, string>> RetrieveSkinInfo(string baseUrl, string profileId)
        {
            Uri skinQueryUri = new Uri(new Uri(baseUrl), $"sessionserver/session/minecraft/profile/{profileId}");

            string response;
            try
            {
                response = await HttpUtil.SendHttpGetRequest(skinQueryUri.ToString());
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving skin info", ex);
            }

            var skinData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

            string skinUrl = ExtractSkinUrl(skinData);

            return new Dictionary<string, string> { { "skinUrl", skinUrl } };
        }

        private static string ExtractSkinUrl(Dictionary<string, object> skinData)
        {
            if (skinData.TryGetValue("properties", out var propertiesValue) && propertiesValue is JArray propertiesArray 
                                                                            && propertiesArray.Count > 0 && propertiesArray[0] is JObject property)
            {
                string encodedValue = property.Value<string>("value");
                string decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue));
                var texturesData = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedJson);

                if (texturesData.TryGetValue("textures", out var texturesValue) && texturesValue is JObject textures 
                    && textures.TryGetValue("SKIN", out var skinValue) && skinValue is JObject skin)
                {
                    return skin.Value<string>("url");
                }
            }

            return string.Empty;
        }

    }
}