using System.IO.Compression;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Fabric;
using StarLight_Core.Models.Processor.Forge;
using StarLight_Core.Models.Processor.NeoForge;
using StarLight_Core.Models.Processor.Quick;

namespace StarLight_Core.Models.Processor;

/// <summary>
/// 单个模组信息
/// </summary>
public interface IMinecraftMod
{ 
    /// <summary>
    /// 获取与设置模组的禁用状态 默认为false
    /// </summary>
    bool IsDisable { get; set; }
    /// <summary>
    /// 用于判断模组是否能识别(暂时无用)
    /// </summary>
    bool IsOk { get;  } 
    
    /// <summary>
    /// 模组的模组加载器类型(可能包含多个)
    /// </summary>
    LoaderType LoaderType { get; internal set; }

    /// <summary>
    /// 模组内置描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 模组位置(包含后缀名，会随禁用状态更改而改变)
    /// </summary>
    string ModPath { get; }

    /// <summary>
    /// 加载器中唯一名称(模组ID)
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



    /// <summary>
    /// 获取单个模组的信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    static IMinecraftMod? GetModInfo(string filePath)
    {
        
        var jarFile = new ZipArchive(File.OpenRead(filePath), ZipArchiveMode.Read);
        
        IMinecraftMod[] loadList = [
            new FabricModInfo(jarFile,filePath),
            new ForgeModInfoLegacy(jarFile,filePath),
            new ForgeModInfoModern(jarFile,filePath),
            new NeoForgeModInfo(jarFile,filePath),
            new QuiltModInfo(jarFile,filePath)
        ];

        var okList = loadList.Where(v => v.IsOk).ToArray();
        foreach (var item in okList)
        {
            okList[0].LoaderType |= item.LoaderType;
        }

        return okList.Length ==0 ? null : okList[0];
    }
    
}



