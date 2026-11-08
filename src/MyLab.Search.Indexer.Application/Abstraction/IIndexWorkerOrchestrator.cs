namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface IIndexWorkerOrchestrator
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
