using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

public class ModJsonEntity
{
    public class ModInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("modid")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("authorList")]
        public List<string> Authors { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        public string Author => Authors != null ? string.Join(", ", Authors) : null;
    }
}