namespace StarLight_Core.Models.Installer;

public class MinecraftInstallerModel
{
    public static string BuildFromName(string name, string root)
    {
        Console.WriteLine(name);
        var parts = name.Split(':');
        if (parts.Length != 3)
        {
            throw new ArgumentException("[SL]名称格式无效,获取错误");
        }
    
        string groupIdPath = parts[0].Replace('.', Path.DirectorySeparatorChar);
        string artifactId = parts[1];
        string version = parts[2];
    
        return Path.Combine(root, groupIdPath, artifactId, version, $"{artifactId}-{version}.jar");
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