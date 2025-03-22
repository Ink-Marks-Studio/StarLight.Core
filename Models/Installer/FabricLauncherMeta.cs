using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Models.Installer;

public abstract class FabricLauncherMeta
{
    public abstract JsonNode MainClass { get; set; }
    
    //TODO: public abstract Dictionary<string, List<Library>> Libraries { get; set; }
}