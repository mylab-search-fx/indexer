namespace MyLab.Search.Indexer.Application.Config;

public record IndexConfig(
        int PageSize = 500,
        int IdleSec = 5 * 60,
        string SeedField = "Seed"
    );