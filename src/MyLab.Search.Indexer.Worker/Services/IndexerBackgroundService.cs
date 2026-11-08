using MyLab.Log.Dsl;

namespace MyLab.Search.Indexer.Worker.Services;

public class IndexerBackgroundService : BackgroundService
{
    readonly IDslLogger _log;

    public IndexerBackgroundService(ILogger<IndexerBackgroundService> logger)
    {
        _log = logger.Dsl();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.Action("Worker started").Write();

        while (!stoppingToken.IsCancellationRequested)
        {   
            
        }
            
        _log.Action("Worker stopped").Write();
    }
}