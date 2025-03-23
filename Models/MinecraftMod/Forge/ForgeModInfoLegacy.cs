using System.IO.Compression;
using System.Text.Json;
using StarLight_Core.Models.MinecraftMod.Enum;
using StarLight_Core.Models.MinecraftMod.Forge.Utils;

namespace StarLight_Core.Models.MinecraftMod.Forge;

internal class ForgeModInfoLegacy:  IMinecraftMod
{
    public ModLoaderEnum LoaderType { get; }
    public string Description { get; }
    public string ModPath { get; }
    public string ModId { get; }
    public string DisplayName { get; }
    public IEnumerable<string>? Authors { get; }
    public string ModVersion { get; set; }
    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }
    /*不用管的*/
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    /*/// <summary>
    /// 请不要删除，这是低版本C#没有静态虚方法做的取舍
    /// </summary>
    public ForgeModInfoLegacy() {/*NOTHING#1#  }*/
    private ForgeModInfoLegacy(ZipArchive zip, string fileName)
    {
        var jsonFile = zip.GetEntry("mcmod.info")?? throw new InvalidOperationException();
        //读取到的格式为[{ 内容 }]需要删除[] -> { 内容 }然后当Json读取
        var json = new StreamReader(jsonFile.Open()).ReadToEnd();
        json = json[1..(json.LastIndexOf("}", StringComparison.Ordinal)+1)];
        var modInfo = JsonSerializer.Deserialize<ForgeModLegacyJson?>(json)?? throw new InvalidOperationException();
        //开始赋值
        Description = modInfo.Description;
        ModPath = fileName;
        ModId = modInfo.ModId;
        DisplayName = modInfo.DisplayName;
        ModVersion = modInfo.ModVersion;
        Authors = modInfo.Author;
        LoaderType = ModLoaderEnum.ForgeLegacy;
        if (modInfo.Depends is not null)
        {
            Depends = modInfo.Depends;
        }

        if (modInfo.IconLogoPath is not null)
        {
            // TODO 以后添加图标要做
        }
        //释放资源
        zip.Dispose();
    }
    public static Task<IMinecraftMod> BuildAsync(string fileName)
    {

        return Task.FromResult(
            (IMinecraftMod)new ForgeModInfoLegacy(new ZipArchive(File.OpenRead(fileName), ZipArchiveMode.Read), fileName));
    }

    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {GetType()}";
    }
    
}