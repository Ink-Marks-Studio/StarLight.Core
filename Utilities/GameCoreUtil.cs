using System.Text.Json;
using StarLight_Core.Models.Utilities;

namespace StarLight_Core.Utilities
{
    public class GameCoreUtil
    {
        public static void GetGameCores(string root = ".minecraft")
        {
            string rootPath = root + "\\versions";
            var versions = Directory.GetDirectories(root);

            foreach (var version in versions)
            {
                var versionFolder = new DirectoryInfo(version);
                var versionFile = Path.Combine(versionFolder.FullName, versionFolder.Name + ".json");

                if (File.Exists(versionFile))
                {
                    try
                    {
                        var jsonData = File.ReadAllText(versionFile);
                        var versionsJson = JsonSerializer.Deserialize<GameCoreVersionsJson>(jsonData);

                        if (versionsJson != null)
                        {
                            var versionId = versionsJson.Id;
                        }
                    }
                    catch (JsonException ex)
                    {
                        throw new Exception($"JSON 解析错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"发生错误: {ex.Message}");
                    }
                }
            }
        }
    }
}
