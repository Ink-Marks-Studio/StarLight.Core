using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Skin
{
    public class Textures
    {
        [JsonPropertyName("SKIN")]
        public SkinInfoJsonEntity Skin { get; set; }
    }
}