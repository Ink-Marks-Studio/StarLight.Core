using System.Text.Json;
using System.Text.Json.Serialization;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

public static class ModrinthUtil
{
    public static async Task<IEnumerable<ModrinthInfo>?> GetRandomMod(int count)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 自动转换 snake_case
            Converters = { new JsonStringEnumConverter() } // 枚举与字符串转换
        };
        try
        {
            var response = await HttpUtil.SendHttpGetRequest($"https://api.modrinth.com/v2/projects_random?count={count}");
            IEnumerable<ModrinthInfo> projects = JsonSerializer.Deserialize<IEnumerable<ModrinthInfo>>(response, options);
            return projects;
        }
        catch (Exception ex)
        {
            throw new Exception("获取Mod时发生错误", ex);
        }
    }
}