using System.Text.Json.Serialization;

namespace PocketBaseTrailReader.Models;

public class Trail
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("gpx")]
    public string Gpx { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"ID: {Id}\nName: {Name}\nAuthor: {Author}\nGPX: {Gpx}\n";
    }
}
