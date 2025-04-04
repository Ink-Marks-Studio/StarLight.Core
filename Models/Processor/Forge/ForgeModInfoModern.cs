using System.IO.Compression;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Utility;
using StarLight_Core.Processor;

namespace StarLight_Core.Models.Processor.Forge;

/// <summary>
/// 高版本forge使用的toml
/// </summary>

internal class ForgeModInfoModern: IMinecraftMod
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
    public bool IsOk { get; } = false;
    public LoaderType LoaderType { get; set; }
    public string Description { get; }
    public string ModPath { get; protected set; }
    public string ModId { get; }
    public string DisplayName { get; }
    public IEnumerable<string>? Authors { get; }
    public string ModVersion { get; }
    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    protected ForgeModInfoModern(ZipArchive zip, string fileName, string findEntry)
    {
        var textFile = zip.GetEntry(findEntry);
        var deepPath=string.Empty;
        if (textFile is null)
        {
            foreach (var entry in zip.Entries)
            {
                if (!entry.Name.Contains("metadata.json")) continue;
                //包含"/"
                deepPath = entry.FullName[..(entry.FullName.LastIndexOf('/') + 1)];
                break;
            }

            if (deepPath ==string.Empty)
            {
                return;
            }
            //需要循环的部分
            ZipArchive zipArchive;
            foreach (var item in zip.Entries.Where(v=>v.FullName.StartsWith(deepPath)))
            {
                if (!item.FullName.EndsWith(".jar"))
                {
                    continue;
                }
                zipArchive = new ZipArchive(item.Open(), ZipArchiveMode.Read);
                if (zipArchive.Entries.Any(v =>
                        v.FullName.Contains(findEntry)))
                {
                    textFile = zipArchive.GetEntry(findEntry);
                }
            }
        }

        if (textFile is null)
        {
            return;
        }
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
        IsOk = true;
        //释放资源 
        textStream.Dispose();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="zip">jar文件的只读zip</param>
    /// <param name="fileName">jar文件路径，只用于初始化ModPath</param>
    public ForgeModInfoModern(ZipArchive zip, string fileName):this(zip,fileName,@"META-INF/mods.toml")
    {
        
    }
    /// <summary>
    /// 异步构建一个ModInfo
    /// </summary>
    /// 
    /// <param name="fileName"></param>
    /// <returns></returns>
    //控制台测试用
    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {LoaderType.ToNormalString()} 路径:{ModPath}";
    }
}