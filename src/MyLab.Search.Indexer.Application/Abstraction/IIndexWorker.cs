namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface IIndexWorker
    {
        string Name { get; }
        Task RunAsync(CancellationToken cancellationToken);
    }
}
