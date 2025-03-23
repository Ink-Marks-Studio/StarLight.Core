using StarLight_Core.Enum;

namespace StarLight_Core.Models.Processor;

/// <summary>
/// 单个模组信息
/// </summary>
public interface IMinecraftMod
{
    /// <summary>
    /// 模组的模组加载器类型
    /// </summary>
    LoaderType LoaderType { get; }

    /// <summary>
    /// 模组内置描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 模组位置(包含后缀名)
    /// </summary>
    string ModPath { get; }

    /// <summary>
    /// 加载器中唯一名称
    /// </summary>
    string ModId { get; }

    /// <summary>
    /// 友好名称(建议的显示名称)
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 模组作者列表
    /// </summary>
    IEnumerable<string>? Authors { get; }

    /// <summary>
    /// 模组当前版本
    /// </summary>
    string ModVersion { get; }

    /// <summary>
    /// 依赖模组
    /// </summary>

    IEnumerable<string>? Depends { get; }

    /// <summary>
    /// 软依赖
    /// </summary>
    IEnumerable<string>? SoftDepends { get; }

    /// <summary>
    /// 依赖此模组的 Mod
    /// 用于一键禁用
    /// </summary>
    IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    /*/// <summary>
    /// 异步分析方法,便于异步执行提升效率
    /// </summary>
    /// <param name="modZip">代表当前模组的流</param>
    /// <returns></returns>
    Task<IMinecraftMod> BuildAsync(ZipArchive modZip,string fileName);*/
}