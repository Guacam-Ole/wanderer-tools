using System.Text.Json.Serialization;
using PocketBaseSharp.Models;

namespace PocketBaseTrailReader.Models;

public class Trail:BaseModel
{

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("gpx")]
    public string Gpx { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string CategoryId { get; set; }
    

    public override string ToString()
    {
        return $"ID: {Id}\nName: {Name}\nAuthor: {Author}\nGPX: {Gpx}\n";
    }
}
