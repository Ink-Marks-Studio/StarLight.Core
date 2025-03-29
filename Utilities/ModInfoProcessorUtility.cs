using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using StarLight_Core.Models.Processor;

namespace StarLight_Core.Utilities;

/// <summary>
/// 获取模组信息的实用工具 封装了常用的使用方法 如需定制，需自行处理 IMinecraftMod.GetModInfo(string filePath)
/// </summary>
public static class ModInfoProcessorUtility
{
    //默认的后缀名
    private static IEnumerable<string> FileExtensions { get; set; } = [".jar", ".disabled"];
    
    /// <summary>
    /// 从文件夹中获取所有可识别模组信息
    /// </summary>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="fileExtensions">搜索的后缀名->默认".jar",".disabled"</param>
    /// <returns></returns>
    public static IEnumerable<IMinecraftMod> GetModsInfoFromDirectory(string directoryPath,IEnumerable<string>? fileExtensions = null)
    {
        List<IMinecraftMod> list = [];
        foreach (var se in Directory.GetFiles(directoryPath).Where(filePath =>
                     FileExtensions.Any(suffix => filePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))))
        {
            IMinecraftMod? minecraftMod;
            if ((minecraftMod = IMinecraftMod.GetModInfo(se)) is not null)
            {
                list.Add(minecraftMod!);
            }
            
        }
        return list;
    }
    /// <summary>
    /// 异步流 方式获取模组信息(!!!未优化，性能较差，如无需要请使用await Task.Run(()=>GetModsInfoFromDirectory(...)))
    /// </summary>
    /// <param name="directoryPath">路径</param>
    /// <param name="fileExtensions">后缀名</param>
    /// <returns></returns>
    public static async IAsyncEnumerable<IMinecraftMod> GetModsInfoFromDirectoryAsyncStream(string directoryPath,IEnumerable<string>? fileExtensions = null)
    {
        var tasks = Directory.GetFiles(directoryPath)
            .Where(filePath => 
                FileExtensions.Any(suffix => filePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            .Select(v
                => Task.Run(() => IMinecraftMod.GetModInfo(v))).ToList();
        while (tasks.Count !=0)
        {
            var whenAny = await Task.WhenAny(tasks);
            tasks.Remove(whenAny);
            if (whenAny.Result is null)
            {
                continue;
            }
            yield return whenAny.Result;
        }
    }
    
    
}