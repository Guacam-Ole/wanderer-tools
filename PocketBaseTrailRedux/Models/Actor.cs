using System.Text.Json.Serialization;
using PocketBaseSharp.Models;

namespace PocketBaseTrailReader.Models;

public class Actor:BaseModel
{

    [JsonPropertyName("username")]
    public string Name { get; set; } = "";
}