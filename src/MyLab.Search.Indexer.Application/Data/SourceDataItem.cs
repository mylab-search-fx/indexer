namespace MyLab.Search.Indexer.Application.Data;

public class SourceDataItem
{
    public required SourceDataValue Id { get; set; }
    public required long Seed { get; set; }
    public required IReadOnlyDictionary<string, SourceDataValue> Fields { get; set; }
}