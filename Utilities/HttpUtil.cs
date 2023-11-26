using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StarLight.Core.Utilities
{
    public static class HttpUtil
    {
        // GET 请求
        public static async Task<string> SendHttpGetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage res = await client.GetAsync(url);

                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP GET 请求失败，状态代码 {res.StatusCode}");
                }
            }
        }
        
        // POST 请求
        public static async Task<string> SendHttpPostRequest(string url, string postData, string contentType = "application/x-www-form-urlencoded")
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(postData, Encoding.UTF8, contentType);

                HttpResponseMessage res = await client.PostAsync(url, content);

                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP POST 请求失败，状态代码 {res.StatusCode}");
                }
            }
        }
        
        // Json 请求下载
        public static async Task<string> GetJsonAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage res = await client.GetAsync(url);
                if (res.IsSuccessStatusCode)
                {
                    string json = await res.Content.ReadAsStringAsync();
                    return json; // 返回下载的 JSON 数据
                }
                else
                {
                    throw new HttpRequestException($"Json 下载失败，错误代码 {res.StatusCode}");
                }
            }
        }
    }
}

