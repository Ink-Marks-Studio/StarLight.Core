using Newtonsoft.Json.Linq;
using StarLight.Core.Utilities;

namespace StarLight.Core.Authentication
{
    public class MicrosoftAuthentication
    {
        private const string ClientID = "e1e383f9-59d9-4aa2-bf5e-73fe83b15ba0"; // 替换为您的Client ID
        private const string DeviceCodeUrl = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
        private const string TokenUrl = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";

        public static async Task StartOAuthLogin()
        {
            var scope = "XboxLive.signin offline_access";
            var postData = $"client_id={ClientID}&scope={Uri.EscapeDataString(scope)}";

            try
            {
                // 发送请求获取device code和user code
                var response = await HttpUtil.SendHttpPostRequest(DeviceCodeUrl, postData);
                var json = JObject.Parse(response);

                var userCode = json.Value<string>("user_code");
                var deviceCode = json.Value<string>("device_code");
                var verificationUri = json.Value<string>("verification_uri");

                Console.WriteLine($"请访问 {verificationUri} 并输入用户代码: {userCode} 来进行登录。");

                // 等待用户完成登录
                var token = await WaitForUserToLogin(deviceCode);

                Console.WriteLine("登录成功！");
                Console.WriteLine($"访问令牌: {token}");
            }
            catch (Exception e)
            {
                Console.WriteLine("在OAuth登录过程中发生错误:");
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<string> WaitForUserToLogin(string deviceCode)
        {
            var pollingInterval = TimeSpan.FromSeconds(5); // 根据OAuth服务推荐的间隔设置
            var postData = $"client_id={ClientID}&device_code={deviceCode}&grant_type=http://oauth.net/grant_type/device/1.0";

            while (true)
            {
                // 暂停一段时间后再发送请求，以避免过于频繁的轮询
                await Task.Delay(pollingInterval);

                try
                {
                    // 使用用户的device_code来获取access_token
                    var response = await HttpUtil.SendHttpPostRequest(TokenUrl, postData);

                    // 检查是否有错误响应
                    var json = JObject.Parse(response);
                    if (json.TryGetValue("error", out JToken errorToken))
                    {
                        var error = errorToken.ToString();
                        // 如果错误是“authorization_pending”，则继续轮询
                        // 如果是其他错误，则抛出异常
                        if (error != "authorization_pending")
                        {
                            throw new Exception($"OAuth error: {error}");
                        }
                    }
                    else if (json.TryGetValue("access_token", out JToken token))
                    {
                        // 如果获取到了access_token，那么登录成功
                        return token.ToString();
                    }
                }
                catch (Exception e)
                {
                    // 输出异常信息，并且可以选择是否继续尝试
                    Console.WriteLine("在检查登录状态时发生错误:");
                    Console.WriteLine(e.Message);
                    // 可以在这里添加逻辑决定是否要继续轮询或者退出方法
                }
            }
        }
    }
}
