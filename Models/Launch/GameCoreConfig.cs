namespace StarLight_Core.Models.Launch
{
    public class GameCoreConfig
    {
        public string Root { get; set; } = ".minecraft";
    
        public string Version { get; set; }

        public bool IsVersionIsolation { get; set; } = true;
    
        public string Ip { get; set; }
    
        public string Port { get; set; }
        
        public string UnifiedPassServerId { get; set; }
        
        public string Nide8authPath { get; set; }
    
        public IEnumerable<string> GameArguments { get; set; }
    }
}