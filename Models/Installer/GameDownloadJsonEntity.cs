using System.Text.Json.Serialization;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Installer;

internal abstract class GameDownloadJsonEntity : GameCoreVersionsJson
{
    [JsonPropertyName("libraries")]
    internal abstract List<Library> Libraries { get; set; }
}