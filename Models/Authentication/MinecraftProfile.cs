using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Authentication
{
    public class MinecraftProfile
    {
        [JsonPropertyName("id")]
        public string Uuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("skins")]
        public Skin[] Skins { get; set; }
    }

    public class Skin
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}