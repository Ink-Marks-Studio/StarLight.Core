using System.Text;
using StarLight_Core.Models.Skin;
using StarLight_Core.Utilities;

namespace StarLight_Core.Skin.Fetchers;

/// <summary>
/// 统一通行证皮肤获取器
/// </summary>
public class UnifiedPassSkinFetcher
{
    /// <summary>
    /// 获取统一通行证皮肤
    /// </summary>
    /// <param name="uuid">微软账户 Uuid</param>
    /// <returns>皮肤图片字节信息</returns>
    public static async Task<byte[]> GetMicrosoftUnifiedPassSkinAsync(string uuid)
    {
        const string baseUrl = "https://auth.mc-user.com:233/sessionserver/session/minecraft/profile/";
        var skinJson = await HttpUtil.GetStringAsync(baseUrl + uuid);
        var skinUrl =
            Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        skinJson.ToJsonEntry<ProfileJsonEntity>().Properties.First().Value))
                .ToJsonEntry<SkinJsonEntity>()
                .Textures.Skin.Url;
        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(skinUrl);
    }
}