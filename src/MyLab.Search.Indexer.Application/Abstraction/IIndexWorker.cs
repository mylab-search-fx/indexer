namespace MyLab.Search.Indexer.Application.Abstraction
{
    interface IIndexWorker
    {
        string Name { get; }
        Task RunAsync(CancellationToken cancellationToken);
    }
}
