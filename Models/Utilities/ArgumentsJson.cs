using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

public class ArgumentsJson
{
    [JsonPropertyName("game")]
    public List<JsonElement> Game { get; set; }

    [JsonPropertyName("jvm")]
    public List<JsonElement> Jvm { get; set; }
}