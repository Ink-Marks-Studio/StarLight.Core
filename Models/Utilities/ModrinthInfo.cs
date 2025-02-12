using System.ComponentModel;
using System.Text.Json.Serialization;
using StarLight_Core.Enum;

namespace StarLight_Core.Models.Utilities;

public class ModrinthInfo
{
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; } // 允许 null

        [JsonPropertyName("client_side")]
        public ClientSideType ClientSide { get; set; }

        [JsonPropertyName("server_side")]
        public ServerSideType ServerSide { get; set; }

        [JsonPropertyName("license")]
        public License license { get; set; }

        [JsonPropertyName("authors")]
        public IEnumerable<string> Authors { get; set; }

        [JsonPropertyName("date_created")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonPropertyName("downloads")]
        public int Downloads { get; set; }
        
    public class License
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}