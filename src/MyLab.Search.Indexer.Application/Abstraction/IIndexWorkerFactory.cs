namespace MyLab.Search.Indexer.Application.Abstraction
{
    public interface IIndexWorkerFactory
    {
        IReadOnlyList<IIndexWorker> CreateAll();
    }
}
