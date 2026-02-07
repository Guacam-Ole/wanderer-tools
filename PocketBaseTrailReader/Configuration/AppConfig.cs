namespace PocketBaseTrailReader.Configuration;

public class AppConfig
{
    public string LokiUrl { get; set; } = string.Empty;
    public PocketBaseConfig PocketBase { get; set; } = new();
    public Dictionary<string, double> MinDistanceMeters { get; set; } = new();
    public CommentsConfig Comments { get; set; } = new();
    public int MinSizeKb { get; set; }
    public int MinReductionPercent { get; set; }
}
