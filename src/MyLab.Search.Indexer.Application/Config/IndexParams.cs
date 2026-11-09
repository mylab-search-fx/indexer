namespace MyLab.Search.Indexer.Application.Config;

public record IndexParams(int PageSize = 500, string SeepField = "Seed");