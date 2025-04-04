using System.Collections.Generic;
using System.IO.Compression;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor;

namespace StarLight_Core.Processor;

/// <summary>
/// 模组处理类
/// 实现 IMinecraftMod 接口
/// </summary>
public class ModProcessor : IMinecraftMod
{
    /// <summary>
    /// 模组处理类构造函数
    /// </summary>
    public ModProcessor(string modPath, LoaderType loaderType, string modId, string displayName, string description, string modVersion, IEnumerable<string>? authors, IEnumerable<string>? depends, IEnumerable<string>? softDepends, IEnumerable<IMinecraftMod>? dependedOnMods)
    {
        ModPath = modPath;
        LoaderType = loaderType;
        ModId = modId;
        DisplayName = displayName;
        Description = description;
        ModVersion = modVersion;
        Authors = authors;
        Depends = depends;
        SoftDepends = softDepends;
        DependedOnMods = dependedOnMods;
    }

    /// <inheritdoc />
    public LoaderType LoaderType { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string ModPath { get; }

    /// <inheritdoc />
    public string ModId { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public IEnumerable<string>? Authors { get; }

    /// <inheritdoc />
    public string ModVersion { get; }

    /// <inheritdoc />
    public IEnumerable<string>? Depends { get; }

    /// <inheritdoc />
    public IEnumerable<string>? SoftDepends { get; }

    /// <inheritdoc />
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    /// <summary>
    /// 从文件夹读取所有 Mod 信息
    /// </summary>
    /// <param name="directoryPath">mods 文件夹路径</param>
    /// <returns>异步返回 Mod 信息的流</returns>
    public static async IAsyncEnumerable<IMinecraftMod> GetModsInfoFromDirectory(string directoryPath)
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
    /// 获取单个 Mod 信息
    /// </summary>
    /// <param name="filePath">Mod 文件路径</param>
    /// <returns>返回解析后的 Mod 信息</returns>
    public static Task<IMinecraftMod> GetModInfo(string filePath)
    {
        try
        {
            return FabricModInfo.BuildAsync(filePath);
        }
        catch
        {
            // TODO: 识别错误
        }

        try
        {
            return ForgeModInfoLegacy.BuildAsync(filePath);
        }
        catch
        {
            // TODO: 识别错误
        }

        try
        {
            return ForgeModInfoModern.BuildAsync(filePath);
        }
        catch
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
            catch
            {
                // TODO: 递归逻辑
            }

            throw;
            // TODO: 还是不行，识别错误
        }
    }
}