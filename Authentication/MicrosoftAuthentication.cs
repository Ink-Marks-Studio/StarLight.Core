﻿using System.Net.Http.Headers;
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
                ClientId = clientId,
                UserCode = responseDict["user_code"],
                DeviceCode = responseDict["device_code"],
                VerificationUri = responseDict["verification_uri"],
                Message = responseDict["message"]
            };

            return resultDict;
        }

        //轮询获取 Token
        public static async ValueTask<GetTokenResponse> GetTokenResponse(RetrieveDeviceCode deviceCodeInfo) 
        {
            Console.WriteLine("开始获取Token");
            Console.WriteLine(deviceCodeInfo.DeviceCode + " 验证 " + deviceCodeInfo.ClientId);
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
                    
                    string contentString = await content.ReadAsStringAsync();
                    Console.WriteLine(contentString);
                    
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
                    else
                    {
                        Console.WriteLine("错误响应: " + tokenJson);
                    }

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
            Console.WriteLine(xblLoginContentString);

            string xboxResponseString = "null";
            
            try
            {
                xboxResponseString = await HttpUtil.SendHttpPostRequest("https://user.auth.xboxlive.com/user/authenticate",
                    xblLoginContentString,"application/json");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


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
            string xstsResponse = await HttpUtil.SendHttpPostRequest("https://xsts.auth.xboxlive.com/xsts/authorize", 
                xstspostData,"application/json");
            
            var xstsResponseData = JsonConvert.DeserializeObject<dynamic>(xstsResponse);
            string xstsToken = xstsResponseData.Token;
            string xstsDisplayClaimsuhs = xstsResponseData.DisplayClaims.xui[0].uhs;
            Console.WriteLine("uhs: " + userHash + "token: " + xstsToken);
            
            // Minecraft 身份验证
            action("正在获取 Minecraft 账户信息");
            string url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            var accountResponseData = new
            {
                identityToken = $"XBL3.0 x={userHash};{xstsToken}"
            };
            string accountPostData = JsonConvert.SerializeObject(accountResponseData);
            Console.WriteLine(accountPostData);
            string accountResponse = await HttpUtil.SendHttpPostRequest(url, accountPostData,"application/json");
            var accountData = JsonConvert.DeserializeObject<dynamic>(accountResponse);
            Console.WriteLine(accountData);
            string accessToken = accountData.access_token;
            
            
            // 检查游戏所有权
            action("正在检查游戏所有权");
            Console.WriteLine(accessToken);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://api.minecraftservices.com/entitlements/mcstore");
            bool ownTheGame = false;
            
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
    
                var gameAccountJsonData = JObject.Parse(responseContent);
                
                if (gameAccountJsonData["items"] != null)
                {
                    var itemsArray = gameAccountJsonData["items"] as JArray;
                    ownTheGame = itemsArray != null && itemsArray.Count > 0;
                }

                Console.WriteLine(ownTheGame ? "账户拥有Minecraft" : "账户不拥有Minecraft或者是XGP用户");
            }
            else
            {
                Console.WriteLine("请求失败: " + response.StatusCode);
            }

            if (ownTheGame) {
                action("开始获取玩家档案");
                string profileContent = "null";
                var profileHttpClient = new HttpClient();
                profileHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var profileresponse = await httpClient.GetAsync("https://api.minecraftservices.com/minecraft/profile");

                if (profileresponse.IsSuccessStatusCode)
                {
                    profileContent = await profileresponse.Content.ReadAsStringAsync();
                }
                
                var jsonObject = JObject.Parse(profileContent);
                string uuid = jsonObject["id"].ToString();
                string name = jsonObject["name"].ToString();
                string skinUrl = jsonObject["skins"][0]["url"].ToString();
                
                action("微软登录完成");

                return new MicrosoftAccount {
                    Uuid = uuid,
                    Name = name,
                    ClientToken = Guid.NewGuid().ToString("N"),
                    AccessToken = xblAuthToken,
                    RefreshToken = tokenInfo.RefreshToken,
                    SkinUrl = skinUrl,
                    DateTime = DateTime.Now
                };
            } else {
                throw new("未购买 Minecraft！");
            }

            throw new("验证失败！");
        }
    }
}