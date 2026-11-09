namespace MyLab.Search.Indexer.Application.Options;

public class WorkerOptions
{
    public const string SectionName = "Worker";
    public string ConfigPath { get; set; } = "/etc/mylab-indexer/indices";
}