using System.IO.Compression;
using StarLight_Core.Enum;

namespace StarLight_Core.Models.Processor;

/// <summary>
/// 高版本forge使用的toml
/// </summary>

internal class ForgeModInfoModern: IMinecraftMod
{
    public LoaderType LoaderType { get; }
    public string Description { get; }
    public string ModPath { get; }
    public string ModId { get; }
    public string DisplayName { get; }
    public IEnumerable<string>? Authors { get; }
    public string ModVersion { get; }
    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    public ForgeModInfoModern(ZipArchive zip, string fileName)
    {
        var textFile = zip.GetEntry(@"META-INF/mods.toml")?? throw new InvalidOperationException();
        var textStream = new StreamReader(textFile.Open());
        var list = new List<string>();
        //读取
        while (!textStream.EndOfStream)
        {
            list.Add(textStream.ReadLine()??string.Empty);
        }
        //初始化
        var modInfo = new ForgeModernToml(list);
        ModPath = fileName;
        ModId = modInfo.ModId;
        Description = modInfo.Description;
        DisplayName = modInfo.DisplayName;
        Authors = modInfo.Author;
        ModVersion = modInfo.ModVersion;
        Depends = modInfo.Depends;
        
        
        if (modInfo.IconLogoPath != null)
        {
            // TODO 以后加图标用用
        }

        LoaderType = LoaderType.Forge;
        //释放资源 
        zip.Dispose();
        textStream.Dispose();
    }
    /// <summary>
    /// 异步构建一个ModInfo
    /// </summary>
    /// 
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Task<IMinecraftMod> BuildAsync(string fileName)
    {
        var zipArchive = new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read);
        return Task.FromResult((IMinecraftMod)new ForgeModInfoModern(zipArchive,fileName));
    }
    //控制台测试用
    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {GetType()}";
    }
}