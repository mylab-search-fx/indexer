namespace MyLab.Search.Indexer.Application.Abstraction;

public interface IIndexLocker
{
    Task<IIndexLock?> TryLockAsync(string indexId, CancellationToken ct);
}

public interface IIndexLock : IAsyncDisposable
{
    Task<bool> HeartbeatAsync(CancellationToken ct);
}