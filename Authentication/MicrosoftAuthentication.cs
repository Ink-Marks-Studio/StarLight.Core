using System.Net.Http.Headers;
using System.Text.Json;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Utilities;

namespace StarLight_Core.Authentication
{
    public class MicrosoftAuthentication
    {
        private static string[] Scopes => new string[] { "XboxLive.signin", "offline_access", "openid", "profile", "email" };

        public string ClientId { get; set; }
        
        public MicrosoftAuthentication(string clientId)
        {
            ClientId = clientId;
        }
        
        // 获取用户代码
        public async ValueTask<RetrieveDeviceCode> RetrieveDeviceCodeInfo()
        {
            if (string.IsNullOrEmpty(ClientId))
                throw new ArgumentNullException(nameof(ClientId), "ClientId为空！");

            string postData = $"client_id={ClientId}&scope={string.Join(" ", Scopes)}";
            string deviceCodeUrl = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
            string responseJson = await HttpUtil.SendHttpPostRequest(deviceCodeUrl, postData);
            
            var responseDict = JsonSerializer.Deserialize<RetrieveDeviceCode>(responseJson);

            var resultDict = new RetrieveDeviceCode
            {
                ClientId = ClientId,
                UserCode = responseDict.UserCode,
                DeviceCode = responseDict.DeviceCode,
                VerificationUri = responseDict.VerificationUri,
                Message = responseDict.Message
            };

            return resultDict;
        }

        // 轮询获取 Token
        public async ValueTask<GetTokenResponse> GetTokenResponse(RetrieveDeviceCode deviceCodeInfo)
        {
            using (HttpClient client = new HttpClient())
            {
                string tenant = "/consumers";
                TimeSpan pollingInterval = TimeSpan.FromSeconds(5);
                DateTimeOffset codeExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15);

                while (DateTimeOffset.UtcNow < codeExpiresOn)
                {
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
                        var tokenData = JsonSerializer.Deserialize<GetTokenResponse>(tokenJson);
                        if (!string.IsNullOrEmpty(tokenData.AccessToken))
                        {
                            return new GetTokenResponse()
                            {
                                ExpiresIn = tokenData.ExpiresIn,
                                RefreshToken = tokenData.RefreshToken,
                                AccessToken = tokenData.AccessToken,
                                ClientId = deviceCodeInfo.ClientId
                            };
                        }
                    }

                    await Task.Delay(pollingInterval);
                }
                throw new TimeoutException("登录已超时,请重试");
            }
        }

        public async ValueTask<MicrosoftAccount> MicrosoftAuthAsync(GetTokenResponse tokenInfo, Action<string> action)
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
            
            string xblLoginContentString = JsonSerializer.Serialize(xboxLoginContent);

            string xblAuthToken = "null";
            string xboxResponseString = "null";

            try
            {
                xboxResponseString = await HttpUtil.SendHttpPostRequest("https://user.auth.xboxlive.com/user/authenticate",
                    xblLoginContentString, "application/json");
            }
            catch (Exception e)
            {
                throw new Exception("获取XBL令牌错误: " + e.Message);
            }
            
            var xboxResponseData = JsonSerializer.Deserialize<XboxResponse>(xboxResponseString);
            xblAuthToken = xboxResponseData.AuthToken;
            string userHash = xboxResponseData.DisplayClaims.Xui[0].UserHash;
    
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
            
            string xstspostData = JsonSerializer.Serialize(getXSTSJsonData);

            string xstsResponse = await HttpUtil.SendHttpPostRequest("https://xsts.auth.xboxlive.com/xsts/authorize",
                xstspostData, "application/json");
            
            var xstsResponseData = JsonSerializer.Deserialize<XboxResponse>(xstsResponse);
            string xstsToken = xstsResponseData.AuthToken;
            string xstsDisplayClaimsuhs = xstsResponseData.DisplayClaims.Xui[0].UserHash;

            // Minecraft 身份验证
            action("正在获取 Minecraft 账户信息");
            string url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            var accountResponseData = new
            {
                identityToken = $"XBL3.0 x={userHash};{xstsToken}"
            };
            
            string accountPostData = JsonSerializer.Serialize(accountResponseData);
            
            string accountResponse = await HttpUtil.SendHttpPostRequest(url, accountPostData, "application/json");
            var accountData = JsonSerializer.Deserialize<MinecraftAccountData>(accountResponse);
            string accessToken = accountData.AccessToken;

            // 检查游戏所有权
            action("正在检查游戏所有权");
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://api.minecraftservices.com/entitlements/mcstore");
            bool ownTheGame = false;

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                
                var gameAccountJsonData = JsonDocument.Parse(responseContent);

                if (gameAccountJsonData.RootElement.TryGetProperty("items", out var itemsArray))
                {
                    ownTheGame = itemsArray.GetArrayLength() > 0;
                }
            }
            else
            {
                throw new Exception("请求失败: " + response.StatusCode);
            }

            if (ownTheGame)
            {
                action("开始获取玩家档案");
                string profileContent = "null";
                var profileHttpClient = new HttpClient();
                profileHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var profileresponse = await httpClient.GetAsync("https://api.minecraftservices.com/minecraft/profile");

                if (profileresponse.IsSuccessStatusCode)
                {
                    profileContent = await profileresponse.Content.ReadAsStringAsync();
                }
                
                var jsonObject = JsonDocument.Parse(profileContent);
                
                var minecraftProfile = JsonSerializer.Deserialize<MinecraftProfile>(profileContent);
                string uuid = minecraftProfile.Uuid;
                string name = minecraftProfile.Name;
                string skinUrl = minecraftProfile.Skins[0].Url;

                action("微软登录完成");

                return new MicrosoftAccount
                {
                    Uuid = uuid,
                    Name = name,
                    ClientToken = Guid.NewGuid().ToString("N"),
                    AccessToken = xblAuthToken,
                    RefreshToken = tokenInfo.RefreshToken,
                    SkinUrl = skinUrl,
                    DateTime = DateTime.Now
                };
            }
            else
            {
                throw new Exception("未购买 Minecraft!");
            }

            throw new Exception("验证失败！");
        }
    }
}