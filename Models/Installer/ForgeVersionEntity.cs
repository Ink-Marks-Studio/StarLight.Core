using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Installer
{
    public class ForgeVersionEntity
    {
        [JsonPropertyName("build")]
        public int Build { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("mcversion")]
        public string GameVersion { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("modified")]
        public DateTime ModifiedTime { get; set; }
    }
}