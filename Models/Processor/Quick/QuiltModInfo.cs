using System.IO.Compression;
using StarLight_Core.Enum;
using StarLight_Core.Models.Processor.Fabric;

namespace StarLight_Core.Models.Processor.Quick;

internal class QuiltModInfo: FabricModInfo
{
    /// <inheritdoc />
    public QuiltModInfo(ZipArchive zip, string s):base(zip,s,"quilt.mod.json")
    {
        LoaderType = LoaderType.Quilt;
    }
}