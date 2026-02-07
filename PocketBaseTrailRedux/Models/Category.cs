using System.Text.Json.Serialization;
using PocketBaseSharp.Models;

namespace PocketBaseTrailReader.Models;

public class Category:BaseModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}