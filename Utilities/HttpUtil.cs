using System.Text;

namespace StarLight_Core.Utilities;

public static class HttpUtil
{
    // GET 请求
    public static async Task<string> SendHttpGetRequest(string url)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentException("URL 不能为空", nameof(url));

        try
        {
            using (var client = new HttpClient())
            {
                // 配置 HttpClient（例如设置超时）
                client.Timeout = TimeSpan.FromSeconds(30);

                var res = await client.GetAsync(url);

                if (res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                throw new HttpRequestException($"HTTP GET 请求失败，状态代码 {res.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在发送 HTTP GET 请求时发生异常", ex);
        }
    }

    // POST 请求
    public static async Task<string> SendHttpPostRequest(string url, string postData,
        string contentType = "application/x-www-form-urlencoded")
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentException("URL 不能为空", nameof(url));

        try
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                var content = new StringContent(postData, Encoding.UTF8, contentType);

                var res = await client.PostAsync(url, content);

                if (res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                // TODO: 添加状态代码识别
                // HTTP POST 请求失败，状态代码 TooManyRequests
                throw new HttpRequestException($"HTTP POST 请求失败，状态代码 {res.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在发送 HTTP POST 请求时发生异常", ex);
        }
    }

    // GET 请求(带请求头)
    public static async Task<string> SendHttpGetRequestWithHeaders(string url,
        Dictionary<string, string>? customHeaders = null)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentException("URL 不能为空", nameof(url));

        try
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                if (customHeaders != null)
                    foreach (var header in customHeaders)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                var res = await client.GetAsync(url);

                if (res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();
                throw new HttpRequestException($"HTTP GET 请求失败，状态代码 {res.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在发送 HTTP GET 请求时发生异常", ex);
        }
    }

    // POST 请求(带请求头)
    public static async Task<string> SendHttpPostRequestWithHeaders(string url, string postData,
        string contentType = "application/x-www-form-urlencoded", Dictionary<string, string>? customHeaders = null)
    {
        if (string.IsNullOrEmpty(url)) throw new ArgumentException("URL 不能为空", nameof(url));

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            if (customHeaders != null)
                foreach (var header in customHeaders)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

            var content = new StringContent(postData, Encoding.UTF8, contentType);

            var res = await client.PostAsync(url, content);

            if (res.IsSuccessStatusCode)
                return await res.Content.ReadAsStringAsync();
            throw new HttpRequestException($"HTTP POST 请求失败，状态代码 {res.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在发送 HTTP POST 请求时发生异常", ex);
        }
    }

    /// <summary>
    /// 异步获取字符数据
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException">请求异常</exception>
    public static async Task<string> GetStringAsync(string url)
    {
        using var client = new HttpClient();
        var res = await client.GetAsync(url);
        if (res.IsSuccessStatusCode)
            return await res.Content.ReadAsStringAsync();

        throw new HttpRequestException($"请求失败: {res.StatusCode}");
    }
}