using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

public class ArgsBuildLibraryJson
{
    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; }
}

public class Library
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("downloads")]
    public Download Downloads { get; set; }
    
    [JsonPropertyName("rules")]
    public LibraryJsonRule[] Rule { get; set; }
    
    [JsonPropertyName("natives")]
    public Dictionary<string, string> Natives { get; set; }
}   

public class Os
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Download
{
    [JsonPropertyName("artifact")] 
    public Artifact Artifact { get; set; }

    [JsonPropertyName("classifiers")] 
    public Dictionary<string, Native> Classifiers { get; set; }
}

public class Native
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Artifact
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
