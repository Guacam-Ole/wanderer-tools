namespace PocketBaseTrailReader;

public class State
{
    public DateTime? LastChecked { get; set; }
    public List<RunData> Runs { get; set; } = [];

}

public class RunData
{
    public DateTime Created { get; set; }
    public int FilesCount { get; set; }
    public long SavedBytes { get; set; }
}