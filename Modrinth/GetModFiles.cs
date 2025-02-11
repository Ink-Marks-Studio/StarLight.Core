using StarLight_Core.Models.Mod;
using StarLight_Core.Models.Modrinth;
using StarLight_Core.Utilities;

namespace StarLight_Core.Modrinth;

public static class GetModFiles
{
    public static async Task<IEnumerable<ModrinthItems.ModrinthItem>> GetModFile(string slug)
    {
        if (slug == null || slug == "")
        {
            throw new ArgumentException("slug cannot be null or empty",nameof(slug));
        }
        try
        {
            string? json = null;
            switch (ModSourceConfig.ModSource)
            {
                case 0:
                    json = await HttpUtil.SendHttpGetRequest($"https://api.modrinth.com/v2/project/{slug}/version");
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
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}