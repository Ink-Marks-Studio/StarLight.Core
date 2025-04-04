using System.IO.Compression;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Forge;

namespace StarLight_Core.Models.Processor.NeoForge;

internal class NeoForgeModInfo: ForgeModInfoModern
{

    
    public NeoForgeModInfo(ZipArchive zip, string fileName) : base(zip, fileName,@"META-INF/neoforge.mods.toml")
    {
        LoaderType = LoaderType.NeoForge;
    }
}