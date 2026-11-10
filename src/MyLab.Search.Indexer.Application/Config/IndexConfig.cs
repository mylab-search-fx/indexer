namespace MyLab.Search.Indexer.Application.Config;

public record IndexConfig(
        int PageSize = 500,
        int Idle = 5 * 60, //sec
        string SeepField = "Seed"
    );