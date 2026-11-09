namespace MyLab.Search.Indexer.Application.Config;

public record IndexConfig(string Name, string SqlQuery, IndexParams Params);