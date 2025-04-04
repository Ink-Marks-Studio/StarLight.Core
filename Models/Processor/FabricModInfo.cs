using System.IO.Compression;
using System.Text.Json;
using StarLight_Core.Enum;

namespace StarLight_Core.Models.Processor;


internal class FabricModInfo : IMinecraftMod
{
    /*不用动*/
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }


    /*需要管理的*/
    public LoaderType LoaderType { get; }
    public string Description { get;  }
    public string ModPath { get; }
    public string ModId { get; }
    public string DisplayName { get; }
    public string ModVersion { get; }
    public IEnumerable<string>? Authors { get; }

    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }

    /*/// <summary>
    /// 请不要删除，这是低版本C#没有静态虚方法做的取舍
    /// </summary>
    public FabricModInfo(){/*NOTHING#1#}*/
    private FabricModInfo(ZipArchive fs, string fileName)
    {
        
        //开始分析Jar包
        //说明不是Fabric版本
        var jsonString = fs.GetEntry("fabric.mod.json") ?? throw new InvalidOperationException();
        /*Console.WriteLine(new StreamReader(jsonString.Open()).ReadToEnd());*/
        var modInfo = JsonSerializer.Deserialize<FabricModJson>(jsonString.Open());
        ModPath = fileName;
        ModId = modInfo.ModId;
        DisplayName = modInfo.DisplayName;
        Authors = modInfo.Authors.Cast<string>();
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
        //释放资源
        fs.Dispose();
    }
    
    public static Task<IMinecraftMod> BuildAsync(string fileName)
    {
        return Task.FromResult((IMinecraftMod)new FabricModInfo(new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read), fileName));
    }

    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {GetType()}";
    }
    
    
}