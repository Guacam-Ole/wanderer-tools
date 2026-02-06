using System.Text.Json.Serialization;

namespace PocketBaseTrailReader.Models;

public class Actor
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("username")]
    public string Name { get; set; } = "";
}