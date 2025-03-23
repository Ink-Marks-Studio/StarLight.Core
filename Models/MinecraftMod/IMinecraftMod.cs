using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO.Compression;
using StarLight_Core.Models.MinecraftMod.Enum;
using StarLight_Core.Models.MinecraftMod.Fabric;
using StarLight_Core.Models.MinecraftMod.Forge;

namespace StarLight_Core.Models.MinecraftMod;

/// <summary>
/// 单个模组信息
/// </summary>
public interface IMinecraftMod
{
    /// <summary>
    /// 模组的模组加载器类型
    /// </summary>
    ModLoaderEnum LoaderType { get; }

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
    /// 模组作者列表(大多数情况只有一个(？）)
    /// </summary>
    IEnumerable<string>? Authors { get; }

    /// <summary>
    /// 模组当前版本
    /// </summary>
    string ModVersion { get; }

    /// <summary>
    /// 必须的依赖模组
    /// </summary>

    IEnumerable<string>? Depends { get; }

    /// <summary>
    /// 软依赖
    /// </summary>
    IEnumerable<string>? SoftDepends { get; }

    /// <summary>
    /// 依赖次模组的Mod(用于一键禁用)
    /// </summary>
    IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    /*/// <summary>
    /// 异步分析方法,便于异步执行提升效率
    /// </summary>
    /// <param name="modZip">代表当前模组的流</param>
    /// <returns></returns>
    Task<IMinecraftMod> BuildAsync(ZipArchive modZip,string fileName);*/

    /// <summary>
    /// 从文件夹读取文件列表
    /// </summary>
    /// <param name="directoryPath">mods文件夹</param>
    /// <returns></returns>
    static async IAsyncEnumerable<IMinecraftMod> GetModsInfoFromDirectory(string directoryPath)
    {
        var taskList = Directory.GetFiles(directoryPath).Select(jarFile => GetModInfo(jarFile)).ToList();
        while (taskList.Count > 0)
        {
            var task = await Task.WhenAny(taskList);
            taskList.Remove(task);
            yield return await task;
        }
    }

    /// <summary>
    /// 获取单个模组的Info
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    static  Task<IMinecraftMod> GetModInfo(string filePath)
    {
        try
        {
            return FabricModInfo.BuildAsync(filePath);
        }
        catch (Exception e)
        {
            // TODO 识别错误
        }
        
        
        try
        {
            return ForgeModInfoLegacy.BuildAsync(filePath);
        }
        catch (Exception e)
        {
            // TODO 识别错误
        }

        try
        {
            return ForgeModInfoModern.BuildAsync(filePath);
        }
        catch (Exception e)
        {
            var nestedDirectories = string.Empty;
            var zipArchive = new ZipArchive(File.OpenRead(filePath), ZipArchiveMode.Read);
            LetInit:
            //递归扫包
            foreach (var entry in zipArchive.Entries)
            {
                if (!entry.Name.Contains("metadata.json")) continue;
                //包含"/"
                nestedDirectories = entry.FullName[..(entry.FullName.LastIndexOf('/') + 1)];
                break;
            }

            try
            {
                foreach (var entryINeed in zipArchive.Entries.Where(ent =>
                             ent.FullName.Contains(nestedDirectories) && ent.Name.Contains(".jar")))
                {
                    try
                    {
                        var archive = new ZipArchive(entryINeed.Open(), ZipArchiveMode.Read);
                        return Task.FromResult((IMinecraftMod)new ForgeModInfoModern(archive, filePath));
                    }
                    catch (Exception exception)
                    {
                        continue;
                        // TODO 分析
                    }
                    //TODO 其他逻辑
                }
            }
            catch (Exception e2)
            {
                //TODO 递归逻辑
            }


            throw;
            // TODO 还是不行 识别错误
        }
    }
}