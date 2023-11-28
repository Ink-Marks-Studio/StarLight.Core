using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static async ValueTask<GetTokenResponse> GetTokenResponse(RetrieveDeviceCode deviceCodeInfo) 
        {
            using (HttpClient client = new()) {
                string tenant = "/consumers";
                TimeSpan pollingInterval = TimeSpan.FromSeconds(5);
                DateTimeOffset codeExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15);

                while (DateTimeOffset.UtcNow < codeExpiresOn) {
                    var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["device_code"] = deviceCodeInfo.DeviceCode,
                        ["client_id"] = deviceCodeInfo.ClientId,
                        ["tenant"] = tenant
                    });

                    var tokenRes = await client.PostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token", content);
                    string tokenJson = await tokenRes.Content.ReadAsStringAsync();

                    if (tokenRes.IsSuccessStatusCode)
                    {
                        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenJson);
                        if (tokenData.ContainsKey("access_token"))
                        {
                            return new GetTokenResponse()
                            {
                                ExpiresIn = int.Parse(tokenData["expires_in"]),
                                RefreshToken = tokenData["refresh_token"],
                                AccessToken = tokenData["access_token"],
                                ClientId = deviceCodeInfo.ClientId
                            };
                        }
                    }
                    // 添加错误处理逻辑,我懒得写了

                    await Task.Delay(pollingInterval);
                }
                throw new TimeoutException("登录已超时,请重试");
            }
        }

        public static async ValueTask<MicrosoftAccount> MicrosoftAuthAsync(GetTokenResponse tokenInfo,Action<string> action)
        {
            action("开始获取Microsoft账号信息");
            
            
            // 获取XBL令牌
            action("正在获取XBL令牌");
            var rpsTicketValue = $"d={tokenInfo.AccessToken}";
            var xboxLoginContent = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = rpsTicketValue
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            string xblLoginContentString = JsonConvert.SerializeObject(xboxLoginContent);
            string xblAuthToken = "null";
            
            var xboxResponseString = await HttpUtil.SendHttpPostRequest(xblLoginContentString,
                "https://user.auth.xboxlive.com/user/authenticate");

            var xboxResponseData = JsonConvert.DeserializeObject<dynamic>(xboxResponseString);

            xblAuthToken = xboxResponseData.Token;
            string userHash = xboxResponseData.DisplayClaims.xui[0].uhs;

            
            // 获取XSTS令牌
            action("正在获取XSTS令牌");
            var getXSTSJsonData = new
            {
                Properties = new
                {
                    SandboxId = "RETAIL",
                    UserTokens = new string[] { xblAuthToken }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };

            string xstspostData = JsonConvert.SerializeObject(getXSTSJsonData);
            string xstsResponse = await HttpUtil.SendHttpPostRequest(xstspostData, "https://xsts.auth.xboxlive.com/xsts/authorize");
            
            var xstsResponseData = JsonConvert.DeserializeObject<dynamic>(xstsResponse);
            string xstsToken = xstsResponseData.Token;
            string xstsDisplayClaimsuhs = xstsResponseData.DisplayClaims.xui[0].uhs;
            
            
            // Minecraft 身份验证
            action("正在获取 Minecraft 账户信息");
            string url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            string accountResponseData = $"XBL3.0 x={xstsDisplayClaimsuhs};{xstsToken}";
            
            string accountResponse = await HttpUtil.SendHttpPostRequest(accountResponseData, url);
            var accountData = JsonConvert.DeserializeObject<dynamic>(accountResponse);
            string accessToken = accountData.access_Token;
            
            
            // 检查游戏所有权
            action("正在检查游戏所有权");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://api.minecraftservices.com/entitlements/mcstore");
            string responseContent = "null";
            
            if (response.IsSuccessStatusCode)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }
            
            var gameAccountJsonData = JObject.Parse(responseContent);

            bool ownTheGame = false;
            
            var itemsArray = gameAccountJsonData["items"] as JArray;
            
            if (itemsArray.Count != null) {
                ownTheGame = itemsArray.Count > 0 ? true : false;
            } else {
                ownTheGame = true;
            }
            
        }
    }
}
