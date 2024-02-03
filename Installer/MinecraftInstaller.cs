namespace StarLight_Core.Installer
{
    public class MinecraftInstaller
    {
        public string GameId { get; set; }
        
        public MinecraftInstaller(string gameId)
        {
            GameId = gameId;
        }
    }
}

