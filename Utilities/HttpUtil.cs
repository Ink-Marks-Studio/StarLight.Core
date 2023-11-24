using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Aurora_Star.Core.Utilities
{
    public static class HttpUtil
    {
        // GET 请求
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
                    throw new HttpRequestException($"HTTP GET 请求失败，状态代码 {response.StatusCode}");
                }
            }
        }
        
        // POST 请求
        public static async Task<string> SendHttpPostRequest(string url, string postData, string contentType = "application/x-www-form-urlencoded")
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(postData, Encoding.UTF8, contentType);

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP POST 请求失败，状态代码 {response.StatusCode}");
                }
            }
        }
    }
}

