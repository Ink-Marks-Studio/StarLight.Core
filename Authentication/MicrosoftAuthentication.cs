using Newtonsoft.Json;
using StarLight.Core.Models.Authentication;
using StarLight.Core.Utilities;

namespace StarLight.Core.Authentication
{
    public static class MicrosoftAuthentication
    {
        private static string[] Scopes => new string[] { "XboxLive.signin", "offline_access", "openid", "profile", "email" };
        
        // 获取用户代码
        public static async ValueTask<RetrieveDeviceCode> RetrieveDeviceCodeInfo(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "ClientId为空！");

            string postData = $"client_id={clientId}&scope={string.Join(" ", Scopes)}";
            string deviceCodeUrl = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
            string responseJson = await HttpUtil.SendHttpPostRequest(deviceCodeUrl, postData);

            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
            
            var resultDict = new RetrieveDeviceCode
            {
                UserCode = responseDict["user_code"],
                DeviceCode = responseDict["device_code"],
                VerificationUri = responseDict["verification_uri"],
                Message = responseDict["message"]
            };

            return resultDict;
        }

        public static async ValueTask<Dictionary<string, string>> GetTokenResponse(string clientId, string deviceCode) 
        {
            using (HttpClient client = new()) {
                string tenant = "/consumers";
                TimeSpan pollingInterval = TimeSpan.FromSeconds(5);
                DateTimeOffset codeExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15);

                while (DateTimeOffset.UtcNow < codeExpiresOn) {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["device_code"] = deviceCode,
                        ["client_id"] = clientId,
                        ["tenant"] = tenant
                    });

                    var tokenRes = await client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", content);
                    string tokenJson = await tokenRes.Content.ReadAsStringAsync();

                    if (tokenRes.IsSuccessStatusCode)
                    {
                        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
                        if (tokenData.ContainsKey("access_token"))
                        {
                            return new Dictionary<string, string>
                            {
                                ["expires_in"] = tokenData["expires_in"],
                                ["refresh_token"] = tokenData["refresh_token"],
                                ["access_token"] = tokenData["access_token"]
                            };
                        }
                    }
                    // 在此添加错误处理逻辑,我懒得写了

                    await Task.Delay(pollingInterval);
                }
                throw new TimeoutException("登录操作已超时");
            }
        }
    }
}
