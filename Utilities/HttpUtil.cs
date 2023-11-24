using System.Net.Http;
using System.Threading.Tasks;

namespace Aurora_Star.Core.Utilities
{
    public static class HttpUtil
    {
        public static async Task<string> SendHttpGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP 请求失败，状态代码 {response.StatusCode}");
                }
            }
        }
    }
}

