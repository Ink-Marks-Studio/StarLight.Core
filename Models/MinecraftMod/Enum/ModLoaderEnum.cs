namespace StarLight_Core.Models.MinecraftMod.Enum;


/// <summary>
/// 模组的模组加载器类型
/// </summary>
public enum ModLoaderEnum
{
    /// <summary>
    /// fabric端模组
    /// </summary>
    Fabric,
    /// <summary>
    /// 老版本forge模组(1.13-)
    /// </summary>
    ForgeLegacy,
    /// <summary>
    /// 高版本forge模组
    /// </summary>
    ForgeModern
}