using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Installer;

internal abstract class FabricLauncherMetaEntity
{
    [JsonPropertyName("mainClass")]
    internal abstract JsonNode MainClass { get; set; }

    [JsonPropertyName("libraries")]
    internal abstract Dictionary<string, List<Library>> Libraries { get; set; }
}