using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

public class JavaVersionJsonEntity
{
    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }
}