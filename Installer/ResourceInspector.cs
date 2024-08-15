using StarLight_Core.Enum;
using StarLight_Core.Models.Installer;
using StarLight_Core.Utilities;

namespace StarLight_Core.Installer
{
    public class ResourceInspector
    {
        private string Root { get; set; }
        
        private string GameId { get; set; }

        private CancellationToken CancellationToken { get; set; }
        
        public Action<string>? OnSpeedChanged { get; set; }
        
        public ResourceInspector(string gameId, string root = ".minecraft", Action<string>? onSpeedChanged = null, CancellationToken cancellationToken = default)
        {
            GameId = gameId;
            CancellationToken = cancellationToken;
            OnSpeedChanged = onSpeedChanged;
            Root = FileUtil.IsAbsolutePath(root)
                ? Path.Combine(root)
                : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), root);
        }
        
        public ResourceInspector(string gameId, string root = ".minecraft", CancellationToken cancellationToken = default)
        {
            GameId = gameId;
            CancellationToken = cancellationToken;
            Root = FileUtil.IsAbsolutePath(root)
                ? Path.Combine(root)
                : Path.Combine(FileUtil.GetCurrentExecutingDirectory(), root);
        }

        public async Task<CheckResult> Check()
        {
            return new CheckResult(Status.Succeeded);
        }
    }
}