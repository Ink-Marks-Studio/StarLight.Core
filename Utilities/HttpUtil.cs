using System.Text;

namespace StarLight_Core.Utilities
{
    public static class HttpUtil
    {
        // GET 请求
        public static async Task<string> SendHttpGetRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL 不能为空", nameof(url));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 配置 HttpClient（例如设置超时）
                    client.Timeout = TimeSpan.FromSeconds(30);

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
            catch (Exception ex)
            {
                throw new HttpRequestException("在发送 HTTP GET 请求时发生异常", ex);
            }
        }

        // POST 请求
        public static async Task<string> SendHttpPostRequest(string url, string postData, string contentType = "application/x-www-form-urlencoded")
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL 不能为空", nameof(url));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

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
            catch (Exception ex)
            {
                throw new HttpRequestException("在发送 HTTP POST 请求时发生异常", ex);
            }
        }

        // GET 请求(带请求头)
        public static async Task<string> SendHttpGetRequestWithHeaders(string url, Dictionary<string, string> customHeaders = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL 不能为空", nameof(url));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    if (customHeaders != null)
                    {
                        foreach (var header in customHeaders)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

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
            catch (Exception ex)
            {
                throw new HttpRequestException("在发送 HTTP GET 请求时发生异常", ex);
            }
        }

        // POST 请求(带请求头)
        public static async Task<string> SendHttpPostRequestWithHeaders(string url, string postData, string contentType = "application/x-www-form-urlencoded", Dictionary<string, string> customHeaders = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL 不能为空", nameof(url));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    
                    if (customHeaders != null)
                    {
                        foreach (var header in customHeaders)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

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
            catch (Exception ex)
            {
                throw new HttpRequestException("在发送 HTTP POST 请求时发生异常", ex);
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

