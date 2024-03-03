namespace StarLight_Core.Models.Installer;

public class MinecraftInstallerModel
{
    public static string BuildFromName(string name, string root)
    {
        Console.WriteLine(name);
        var parts = name.Split(':');
        if (parts.Length < 3)
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }

        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];

        string path = Path.Combine(root, groupIdPath, artifactId, version);

        if (parts.Length == 3)
        {
            return Path.Combine(path, $"{artifactId}-{version}.jar");
        }
        else if (parts.Length > 3 && parts[3].StartsWith("natives-"))
        {
            string classifier = parts[3];
            return Path.Combine(path, $"{artifactId}-{version}-{classifier}.jar");
        }
        else
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }
    }
    
    public static string BuildNativesName(string name, string root)
    {
        var parts = name.Split(':');
        if (parts.Length != 3)
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }

        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];
        
        return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}-natives-windows.jar");
    }
}