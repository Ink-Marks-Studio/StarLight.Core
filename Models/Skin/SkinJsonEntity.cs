using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Skin
{
    public class SkinJsonEntity
    {
        [JsonPropertyName("textures")]
        public Textures Textures { get; set; }
    }
}