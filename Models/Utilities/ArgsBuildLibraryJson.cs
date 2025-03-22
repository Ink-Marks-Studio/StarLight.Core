using System.Text.Json.Serialization;

namespace StarLight_Core.Models.Utilities;

internal class ArgsBuildLibraryJson
{
    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; }
}

internal abstract class Library
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

internal abstract class Os
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

internal abstract class Download
{
    [JsonPropertyName("artifact")]
    public Artifact Artifact { get; set; }

    [JsonPropertyName("classifiers")]
    public Dictionary<string, Native> Classifiers { get; set; }
}

internal abstract class Native
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

internal abstract class Artifact
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