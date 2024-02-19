using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Installer;

public class GameDownloadJsonEntity : GameCoreVersionsJson
{
    public List<Library> Libraries { get; set; }
}