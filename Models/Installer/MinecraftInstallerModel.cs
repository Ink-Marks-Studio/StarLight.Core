namespace StarLight_Core.Models.Installer;

public class MinecraftInstallerModel
{
    public static string BuildFromName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length < 3) throw new ArgumentException("名称格式无效,获取错误");

        var groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        var artifactId = parts[1];
        var version = parts[2];

        var path = Path.Combine(root, groupIdPath, artifactId, version);

        if (parts.Length == 3) return Path.Combine(path, $"{artifactId}-{version}.jar");

        if (parts.Length > 3 && parts[3].StartsWith("natives-"))
        {
            var classifier = parts[3];
            return Path.Combine(path, $"{artifactId}-{version}-{classifier}.jar");
        }

        throw new ArgumentException("名称格式无效,获取错误");
    }

    public static string BuildNativesName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3) throw new ArgumentException("名称格式无效,获取错误");

        var groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        var artifactId = parts[1];
        var version = parts[2];

        return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}-natives-windows.jar");
    }
}