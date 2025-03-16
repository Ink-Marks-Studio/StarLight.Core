using System.IO.Compression;
using System.Text.Json;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities;

// 模组工具
public class ModUtil
{
    public static IEnumerable<ModInfo> GetModsInfo(string root)
    {
        FileUtil.IsDirectory(root, true);

        if (!FileUtil.IsAbsolutePath(root)) root = FileUtil.GetCurrentExecutingDirectory() + root;

        var modInfos = new List<ModInfo>();
        var files = Directory.GetFiles(root, "*.jar", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                using var archive = ZipFile.OpenRead(file);
                
                // 尝试读取Fabric模组信息
                var fabricEntry = archive.GetEntry("fabric.mod.json");
                if (fabricEntry != null)
                {
                    using var reader = new StreamReader(fabricEntry.Open());
                    var jsonContent = reader.ReadToEnd();
                    var modJson = JsonSerializer.Deserialize<ModJsonEntity.ModInfo>(jsonContent);
                    if (modJson != null)
                    {
                        modInfos.Add(new ModInfo
                        {
                            Name = modJson.Name,
                            Id = modJson.Id,
                            Version = modJson.Version,
                            Author = modJson.Author,
                            displayName = modJson.DisplayName
                        });
                        continue;
                    }
                }

                // 尝试读取Forge模组信息
                var forgeEntry = archive.GetEntry("META-INF/mods.toml");
                if (forgeEntry != null)
                {
                    // TODO: 实现Forge模组信息的解析
                    // 由于Forge使用TOML格式，需要添加TOML解析库
                }
            }
            catch
            {
                // 忽略无法解析的模组文件
                continue;
            }
        }

        return modInfos;
    }
}