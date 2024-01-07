namespace StarLight_Core.Models.Launch
{
    public class GameCoreConfig
    {
        public string Root { get; set; } = ".minecraft";
    
        public string Version { get; set; }
    
        public string Ip { get; set; }
    
        public string Port { get; set; }
    
        public IEnumerable<string> GameArguments { get; set; }
    }
}