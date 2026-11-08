namespace MyLab.Search.Indexer.Application.Abstraction
{
    interface IIndexWorkerOrchestrator
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
