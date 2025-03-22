using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

internal abstract class LibraryJsonRule
{
    [JsonPropertyName("action")]
    internal abstract string Action { get; set; }

    [JsonPropertyName("os")]
    internal abstract Os Os { get; set; }
}