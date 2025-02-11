using System.Text.Json;
using StarLight_Core.Models.Mod;
using StarLight_Core.Models.Modrinth;
using StarLight_Core.Utilities;

namespace StarLight_Core.Modrinth;

public static class GetMods
{
    public async static Task<IEnumerable<ModrinthItems.ModrinthItem>> GetRandomMods(int count)
    {
        if (count > 100 | count == null | count <= 0)
        {
            throw new ArgumentException("获取数量错误，不得为空、≤0、≥100。", nameof(count));
        }
        try
        {
            Dictionary<string,string> headers = new();
            headers.Add("count",count.ToString());
            string? json = null;
            switch (ModSourceConfig.ModSource)
            {
                case 0:
                    json = await HttpUtil.SendHttpGetRequestWithHeaders("https://api.modrinth.com/v2/projects_random",headers);
                    break;
                case 1:
                    json = await HttpUtil.SendHttpGetRequestWithHeaders("https://api.modrinth.com/v2/projects_random",headers);
                    break;
            }
            var result = json.ToJsonEntry<IEnumerable<ModrinthItems.ModrinthItem>>().Select(manifest =>
                new ModrinthItems.ModrinthItem
                {
                    id = manifest.id,
                    slug = manifest.slug,
                    followers = manifest.followers,
                    published = manifest.published,
                    team = manifest.team,
                    updated = manifest.updated
                }).ToList();
            return result;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在获取随机Mod时发生异常", ex);
        }
        
    }

    public async static Task<ModrinthItems.ModrinthItem> GetSingleMod(string id)
    {
        try
        {
            var json = await HttpUtil.SendHttpGetRequest($"https://api.modrinth.com/v2/project/{id}");
            ModrinthItems.ModrinthItem modrinthItem = JsonSerializer.Deserialize<ModrinthItems.ModrinthItem>(json);
            return modrinthItem;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("在获取随机Mod时发生异常", ex);
        }
    }
}