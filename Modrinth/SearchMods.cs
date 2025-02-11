using StarLight_Core.Models.Mod;
using StarLight_Core.Models.Modrinth;
using StarLight_Core.Utilities;

namespace StarLight_Core.Modrinth;

public static class SearchMods
{
    public async static Task<IEnumerable<ModrinthItems.ModrinthItem>> SearchMod(string name)
    {
        if (name == null)
        {
            throw new ArgumentException("参数不得为空", nameof(name));
        }
        try
        {
            Dictionary<string,string> headers = new();
            headers.Add("query",name);
            string? json = null;
            switch (ModSourceConfig.ModSource)
            {
                case 0:
                    json = await HttpUtil.SendHttpGetRequestWithHeaders("https://api.modrinth.com/v2/search",headers);
                    break;
                case 1:
                    json = await HttpUtil.SendHttpGetRequestWithHeaders("https://api.modrinth.com/v2/search",headers);
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
            throw new HttpRequestException("在获取Mod时发生异常", ex);
        }
        
    }
}