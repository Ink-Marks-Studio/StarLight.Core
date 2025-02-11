namespace StarLight_Core.Models.Modrinth;

public class ModrinthItems
{
    public class ModrinthItem
    {
        public string id { get; set; }
        public string slug { get; set; }
        public string team { get; set; }
        public string published { get; set; }
        public string updated { get; set; }
        public string followers { get; set; }
    }
}