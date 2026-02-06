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

    [JsonPropertyName("created")] 
    public string CreatedStr { get; set; } = "";
    
    [JsonPropertyName("updated")]
    public string? UpdatedStr { get; set; }

    [JsonIgnore]
    public DateTime? Updated
    {
        get
        {
            if (UpdatedStr == null) return null;
            return DateTime.Parse(UpdatedStr);
        }
    }

    [JsonIgnore]
    public DateTime Created => DateTime.Parse(CreatedStr);

    public override string ToString()
    {
        return $"ID: {Id}\nName: {Name}\nAuthor: {Author}\nGPX: {Gpx}\n";
    }
}
