namespace MyLab.Search.Indexer.Application.Abstraction
{
    interface IIndexWorkerFactory
    {
        IReadOnlyList<IIndexWorker> CreateAll();
    }
}
