using System.Text.Json.Serialization;

namespace StarLight_Core.Models.MinecraftMod.Forge.Utils;

internal class ForgeModLegacyJson
{
    [JsonPropertyName("dependencies")] public string[]? Depends { get; set; }
    [JsonPropertyName("modid")] public string ModId { get; set; }
    [JsonPropertyName("name")] public string DisplayName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("authorList")] public string[] Author { get; set; }
    [JsonPropertyName("version")] public string ModVersion { get; set; }
    [JsonPropertyName("logoFile")] public string? IconLogoPath { get; set; }
}