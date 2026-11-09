namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexWorkerFactory
{
    Task<IReadOnlyList<IIndexWorker>> CreateAllAsync(CancellationToken cancellationToken);
}