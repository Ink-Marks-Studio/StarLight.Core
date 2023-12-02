using Newtonsoft.Json.Linq;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class GameCoreUtil
    {
        public static void GetGameCores(string root = ".minecraft")
        {
            var versions = Directory.GetDirectories(root);

            foreach (var version in versions)
            {
                var versionFolder = new DirectoryInfo(version);
                var versionFile = Path.Combine(versionFolder.FullName, versionFolder.Name + ".json");

                if (File.Exists(versionFile))
                {
                    var jsonData = File.ReadAllText(versionFile);
                    var jsonObject = JObject.Parse(jsonData);
                    var versionId = jsonObject["id"]?.ToString();
                }
            }
        }
    }
}
