using MyLab.Log.Dsl;
using MyLab.Search.Indexer.Application.Abstraction;

namespace MyLab.Search.Indexer.Worker.Services;

public class IndexerBackgroundService : BackgroundService
{
    readonly IIndexWorkerOrchestrator _orchestrator;
    readonly IDslLogger? _log;

    public IndexerBackgroundService(IIndexWorkerOrchestrator orchestrator, ILogger<IndexerBackgroundService>? logger = null)
    {
        _orchestrator = orchestrator;
        _log = logger?.Dsl();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.Action("Worker started").Write();

        await _orchestrator.RunAsync(stoppingToken);
            
        _log?.Action("Worker stopped").Write();
    }
}