using System.Text.Json.Serialization;
using PocketBaseSharp.Models;

namespace PocketBaseTrailReader.Models;

public class Comment:BaseModel
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("trail")]
    public string Trail { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
}