using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Utility;

namespace StarLight_Core.Models.Processor.Fabric;


internal class FabricModInfo : IMinecraftMod
{
    public bool IsDisable
    {
        get => ModPath.EndsWith(".jar.disabled");
        set
        {
            switch (value)
            {
                case true when ModPath.EndsWith(".jar"):
                    File.Move(ModPath,ModPath+".disabled");
                    ModPath = ModPath + ".disabled";
                    break;
                case true when ModPath.EndsWith(".jar.disabled"):
                    File.Move(ModPath,ModPath[..ModPath.LastIndexOf(".disabled", StringComparison.Ordinal)]);
                    ModPath = ModPath[..ModPath.LastIndexOf(".disabled", StringComparison.Ordinal)];
                    break;
            }
        }
    }
    /*不用动*/
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }


    /*需要管理的*/
    public bool IsOk { get; } = false;
    public LoaderType LoaderType { get; set; }
    public string Description { get;  }
    public string ModPath { get; protected set; }
    public string ModId { get; }
    public string DisplayName { get; }
    public string ModVersion { get; }
    public IEnumerable<string>? Authors { get; }

    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }

    
    //主方法
    protected FabricModInfo(ZipArchive fs, string fileName, string fileEntry)
    {
        //开始分析Jar包
        //说明不是Fabric版本
        ZipArchiveEntry? jsonString =null;
        foreach (var iEntry in fs.Entries)
        {
            if (fileEntry != iEntry.Name) continue;
            jsonString = iEntry;
            break;
        }
        if (jsonString is null)
        {
            return;
        }
        /*Console.WriteLine(new StreamReader(jsonString.Open()).ReadToEnd());*/
        
            
        FabricModJson? modInfo;
        //正常逻辑
        try
        {
            modInfo = JsonSerializer.Deserialize<FabricModJson>(jsonString.Open());
        }
        //如果有sb作者直接在json里换行，进行处理合并
        catch
        {
            const string pattern = @"(""description"":\s*""\s*)([^""\\]*(?:\\.[^""\\]*)*)(""[^""]*"")";
            // 替换匹配到的换行符为 \\n
            var json = Regex.Replace(new StreamReader(jsonString.Open()).ReadToEnd(), pattern, m =>
            {
                var descriptionValue = m.Groups[2].Value;
                var escapedDescription = Regex.Replace(descriptionValue, @"\r?\n", "\\\\n"); // 转义换行符
                return m.Groups[1].Value + escapedDescription + m.Groups[3].Value;
            });
            modInfo = JsonSerializer.Deserialize<FabricModJson>(json);
        }
        //为Null返回
        if (modInfo is null)
        {
            return;
        }
        ModPath = fileName;
        ModId = modInfo.ModId;
        DisplayName = modInfo.DisplayName;
        Authors = modInfo.Authors?.Cast<string>() ?? [];
        ModVersion = modInfo.ModVersion;
        Description = modInfo.Description;
        LoaderType = LoaderType.Fabric;
        if (modInfo.Depends is not null)
        {
            Depends = modInfo.Depends.Keys;
        }

        if (modInfo.ModIconPath is not null)
        {
            // TODO 以后添加图标用得上
        }

        IsOk = true;
        //释放资源

    }
    /*/// <summary>
    /// 请不要删除，这是低版本C#没有静态虚方法做的取舍
    /// </summary>
    public FabricModInfo(){/*NOTHING#1#}*/
    public FabricModInfo(ZipArchive fs, string fileName):this(fs, fileName, "fabric.mod.json")
    {
        
    }
    
    
    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {LoaderType.ToNormalString()} 路径:{ModPath}";
    }
    
    
}