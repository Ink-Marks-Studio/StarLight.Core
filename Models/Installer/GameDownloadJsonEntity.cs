using System.Text.Json.Serialization;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Installer;

internal class GameDownloadJsonEntity : GameCoreVersionsJson
{
    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; }
}