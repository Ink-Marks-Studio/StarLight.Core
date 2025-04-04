using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Utility;
using StarLight_Core.Processor;

namespace StarLight_Core.Models.Processor;


internal class ForgeModInfoLegacy : IMinecraftMod
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
    public string ModVersion { get; set; }
    public IEnumerable<string>? Depends { get; }
    public IEnumerable<string>? SoftDepends { get; }
    /*不用管的*/
    public IEnumerable<IMinecraftMod>? DependedOnMods { get; }

    /// <summary>
    /// TODO V2版本的.info暂时无法处理
    /// </summary>
    /// <param name="zip"></param>
    /// <param name="fileName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public ForgeModInfoLegacy(ZipArchive zip, string fileName)
    {
        var jsonFile = zip.GetEntry("mcmod.info");
        if (jsonFile is null)
        {
            return;
        }
        //读取到的格式为[{ 内容 }]需要删除[] -> { 内容 }然后当Json读取
        var json = new StreamReader(jsonFile.Open()).ReadToEnd();
        //TODO 转化
        ForgeModLegacyJson modInfo;

        try
        {
            modInfo = JsonSerializer.Deserialize<ForgeModLegacyJson[]>(json)![0] ??
                      throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            try
            {
                modInfo = JsonSerializer.Deserialize<ForgeModLegacyJson>(json) ??
                          throw new InvalidOperationException();
            }
            catch
            {
                return;
            }
        }
        

        /*catch (Exception e)
        {

            json =Regex.Replace(json, @"\n", "");
            json =Regex.Replace(json, @"\s*", "");
            Console.WriteLine(json);
            try
            {
                modInfoTemp = JsonSerializer.Deserialize<List<ForgeModLegacyJson>>(json[json.IndexOf("[", StringComparison.Ordinal)..(json.LastIndexOf("]", StringComparison.Ordinal) + 1)]);
            }
            catch (Exception exception)
            {
                Console.WriteLine(fileName);
                Console.WriteLine(json[json.IndexOf("[", StringComparison.Ordinal)..(json.LastIndexOf("]", StringComparison.Ordinal) + 1)]);
                throw;
            }
        }*/
        
        



        //开始赋值
        Description = modInfo.Description!;
        ModPath = fileName;
        ModId = modInfo.ModId!;
        DisplayName = modInfo.DisplayName!;
        ModVersion = modInfo.ModVersion!;
        Authors = modInfo.Author;
        LoaderType = LoaderType.ForgeLegacy;
        if (modInfo.Depends is not null)
        {
            Depends = modInfo.Depends;
        }

        if (modInfo.IconLogoPath is not null)
        {
            // TODO 以后添加图标要做
        }
        IsOk = true;
        //释放资源
       
    }


    public override string ToString()
    {
        return $"模组名称: {DisplayName}  模组ID: {ModId} 加载器类型: {LoaderType.ToNormalString()} 路径:{ModPath}";
    }
    
}