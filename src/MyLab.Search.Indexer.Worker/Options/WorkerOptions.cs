namespace MyLab.Search.Indexer.Worker.Options;

public class WorkerOptions
{
    public const string SectionName = "Worker";
    public string ConfigPath { get; set; } = "/etc/mylab-indexer/indices";
}